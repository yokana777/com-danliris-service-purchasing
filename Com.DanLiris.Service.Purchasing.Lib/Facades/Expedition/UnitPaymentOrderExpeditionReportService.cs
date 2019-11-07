using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class UnitPaymentOrderExpeditionReportService : IUnitPaymentOrderExpeditionReportService
    {
        private readonly PurchasingDbContext _dbContext;

        public UnitPaymentOrderExpeditionReportService(PurchasingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<UnitPaymentOrderExpeditionReportViewModel> GetQuery(string no, string supplierCode, string divisionCode, int status, DateTimeOffset dateFrom, DateTimeOffset dateTo, string order)
        {
            var expeditionDocumentQuery = _dbContext.Set<PurchasingDocumentExpedition>().AsQueryable();
            var query = _dbContext.Set<UnitPaymentOrder>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(no))
            {
                query = query.Where(document => document.UPONo.Equals(no));
            }

            if (!string.IsNullOrWhiteSpace(supplierCode))
            {
                query = query.Where(document => document.SupplierCode.Equals(supplierCode));
            }

            if (!string.IsNullOrWhiteSpace(divisionCode))
            {
                query = query.Where(document => document.DivisionCode.Equals(divisionCode));
            }

            if (status != 0)
            {
                query = query.Where(document => document.Position.Equals(status));
            }

            query = query.Where(document => document.Date >= dateFrom && document.Date <= dateTo);

            var joinedQuery = from unitPaymentOrder in query
                              join expeditionDocument in expeditionDocumentQuery on unitPaymentOrder.UPONo equals expeditionDocument.UnitPaymentOrderNo into upoExpeditions
                              from upoExpedition in upoExpeditions
                              select new UnitPaymentOrderExpeditionReportViewModel()
                              {
                                  SendToVerificationDivisionDate = upoExpedition.SendToVerificationDivisionDate,
                                  VerificationDivisionDate = upoExpedition.VerificationDivisionDate,
                                  VerifyDate = upoExpedition.VerifyDate,
                                  SendDate = (upoExpedition.Position == ExpeditionPosition.CASHIER_DIVISION || upoExpedition.Position == ExpeditionPosition.SEND_TO_CASHIER_DIVISION) ? upoExpedition.SendToCashierDivisionDate : (upoExpedition.Position == ExpeditionPosition.FINANCE_DIVISION || upoExpedition.Position == ExpeditionPosition.SEND_TO_ACCOUNTING_DIVISION) ? upoExpedition.SendToAccountingDivisionDate : (upoExpedition.Position == ExpeditionPosition.SEND_TO_PURCHASING_DIVISION) ? upoExpedition.SendToPurchasingDivisionDate : null,
                                  CashierDivisionDate = upoExpedition.CashierDivisionDate,
                                  BankExpenditureNoteNo = upoExpedition.BankExpenditureNoteNo,
                                  Date = upoExpedition.UPODate,
                                  Division = new DivisionViewModel()
                                  {
                                      Code = upoExpedition.DivisionCode,
                                      Name = upoExpedition.DivisionName
                                  },
                                  DueDate = upoExpedition.DueDate,
                                  InvoiceNo = upoExpedition.InvoiceNo,
                                  No = upoExpedition.UnitPaymentOrderNo,
                                  Position = upoExpedition.Position,
                                  Supplier = new NewSupplierViewModel()
                                  {
                                      code = upoExpedition.SupplierCode,
                                      name = upoExpedition.SupplierName
                                  },
                                  LastModifiedUtc = upoExpedition.LastModifiedUtc
                              };

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            /* Default Order */
            if (OrderDictionary.Count.Equals(0))
            {
                OrderDictionary.Add("Date", "desc");

                joinedQuery = joinedQuery.OrderBy("Date desc");
            }
            /* Custom Order */
            else
            {
                string Key = OrderDictionary.Keys.First();
                string OrderType = OrderDictionary[Key];

                joinedQuery = joinedQuery.OrderBy(string.Concat(Key, " ", OrderType));
            }

            return joinedQuery;
        }

        public async Task<UnitPaymentOrderExpeditionReportWrapper> GetReport(string no, string supplierCode, string divisionCode, int status, DateTimeOffset dateFrom, DateTimeOffset dateTo, string order, int page, int size)
        {
            var joinedQuery = GetQuery(no, supplierCode, divisionCode, status, dateFrom, dateTo, order);

            return new UnitPaymentOrderExpeditionReportWrapper()
            {
                Total = await joinedQuery.CountAsync(),
                Data = await joinedQuery
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync()
            };
        }

        public async Task<MemoryStream> GetExcel(string no, string supplierCode, string divisionCode, int status, DateTimeOffset dateFrom, DateTimeOffset dateTo, string order)
        {
            var query = GetQuery(no, supplierCode, divisionCode, status, dateFrom, dateTo, order);

            var data = new List<UnitPaymentOrderExpeditionReportViewModel> { new UnitPaymentOrderExpeditionReportViewModel { Supplier = new NewSupplierViewModel(), Division = new DivisionViewModel() } };
            var listData = await query.ToListAsync();
            if (listData != null && listData.Count > 1)
            {
                data = listData;
            }

            DataTable dataTable = new DataTable();

            var headersDateType = new int[] { 1, 2, 7, 8, 9, 10, 11 };
            var headers = new string[] { "No. SPB", "Tgl SPB", "Tgl Jatuh Tempo", "Nomor Invoice", "Supplier", "Divisi", "Posisi", "Tgl Pembelian Kirim", "Verifikasi", "Verifikasi1", "Verifikasi2", "Kasir", "Kasir1" };
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                if (headersDateType.Contains(i))
                {
                    dataTable.Columns.Add(new DataColumn() { ColumnName = header, DataType = typeof(DateTime) });
                }
                else
                {
                    dataTable.Columns.Add(new DataColumn() { ColumnName = header, DataType = typeof(string) });
                }
            }

            foreach (var d in data)
            {
                dataTable.Rows.Add(d.No ?? "-", GetFormattedDate(d.Date), GetFormattedDate(d.DueDate), d.InvoiceNo ?? "-", d.Supplier.name ?? "-", d.Division.Name ?? "-", d.Position, GetFormattedDate(d.SendToVerificationDivisionDate), GetFormattedDate(d.VerificationDivisionDate), GetFormattedDate(d.VerifyDate), GetFormattedDate(d.SendDate), GetFormattedDate(d.CashierDivisionDate), d.BankExpenditureNoteNo ?? "-");
            }

            ExcelPackage package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Data");

            sheet.Cells["A3"].LoadFromDataTable(dataTable, false, OfficeOpenXml.Table.TableStyles.None);

            foreach (var i in Enumerable.Range(0, 8))
            {
                var col = (char)('A' + i);
                sheet.Cells[$"{col}1"].Value = headers[i];
                sheet.Cells[$"{col}1:{col}2"].Merge = true;
            }
            sheet.Cells["I1"].Value = headers[8];
            sheet.Cells["I1:K1"].Merge = true;
            sheet.Cells["L1"].Value = headers[11];
            sheet.Cells["L1:M1"].Merge = true;

            var subHeaders = new string[] { "Tgl Terima", "Tgl Cek", "Tgl Kirim", "Tgl Terima", "No Kuitansi" };
            foreach (var i in Enumerable.Range(0, 5))
            {
                var col = (char)('I' + i);
                sheet.Cells[$"{col}2"].Value = subHeaders[i];
            }

            sheet.Cells["A1:M2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1:M2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A1:M2"].Style.Font.Bold = true;

            foreach (var headerDateType in headersDateType)
            {
                sheet.Column(headerDateType + 1).Style.Numberformat.Format = "dd MMMM yyyy";
            }

            var widths = new int[] { 20, 20, 20, 50, 30, 20, 40, 20, 20, 20, 20, 20, 20 };
            foreach (var i in Enumerable.Range(0, widths.Length))
            {
                sheet.Column(i + 1).Width = widths[i];
            }

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
        }

        DateTime? GetFormattedDate(DateTimeOffset? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            else
            {
                return dateTime.Value.ToOffset(new TimeSpan(7, 0, 0)).DateTime;
            }
        }
    }
}
