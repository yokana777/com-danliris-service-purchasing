using Com.DanLiris.Service.Purchasing.Lib.Enums;
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
// using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
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
            var expeditionDocumentQuery = _dbContext.Set<PurchasingDocumentExpedition>().Include(entity => entity.Items).AsQueryable();
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

            var joinedQuery = query.GroupJoin(
                expeditionDocumentQuery,
                unitPaymentOrder => unitPaymentOrder.UPONo,
                expeditionDocument => expeditionDocument.UnitPaymentOrderNo,
                (unitPaymentOrder, expeditionDocuments) => new { UnitPaymentOrder = unitPaymentOrder, ExpeditionDocuments = expeditionDocuments })
                .SelectMany(
                    joined => joined.ExpeditionDocuments.DefaultIfEmpty(),
                    (joinResult, expeditionDocument) => new UnitPaymentOrderExpeditionReportViewModel()
                    {
                        SendToVerificationDivisionDate = expeditionDocument.SendToVerificationDivisionDate,
                        VerificationDivisionDate = expeditionDocument.VerificationDivisionDate,
                        VerifyDate = expeditionDocument.VerifyDate,
                        SendDate = (expeditionDocument.Position == ExpeditionPosition.CASHIER_DIVISION || expeditionDocument.Position == ExpeditionPosition.SEND_TO_CASHIER_DIVISION) ? expeditionDocument.SendToCashierDivisionDate : (expeditionDocument.Position == ExpeditionPosition.FINANCE_DIVISION || expeditionDocument.Position == ExpeditionPosition.SEND_TO_ACCOUNTING_DIVISION) ? expeditionDocument.SendToAccountingDivisionDate : (expeditionDocument.Position == ExpeditionPosition.SEND_TO_PURCHASING_DIVISION) ? expeditionDocument.SendToPurchasingDivisionDate : null,
                        CashierDivisionDate = expeditionDocument.CashierDivisionDate,
                        BankExpenditureNoteNo = expeditionDocument.BankExpenditureNoteNo,
                        Date = expeditionDocument.UPODate.AddHours(7),
                        DueDate = expeditionDocument.DueDate.AddHours(7)
                        InvoiceNo = expeditionDocument.InvoiceNo,
                        No = expeditionDocument.UnitPaymentOrderNo,
                        Position = expeditionDocument.Position,
                        DPP = expeditionDocument.TotalPaid - expeditionDocument.Vat,
                        PPn = expeditionDocument.Vat,
                        PPh = expeditionDocument.IncomeTax,
                        TotalTax = expeditionDocument.TotalPaid + expeditionDocument.IncomeTax,
                        Supplier = new NewSupplierViewModel()
                        {
                            code = expeditionDocument.SupplierCode,
                            name = expeditionDocument.SupplierName
                        },
                        Currency = new CurrencyViewModel()
                        {
                            Code = joinResult.UnitPaymentOrder.CurrencyCode,
                            Rate = joinResult.UnitPaymentOrder.CurrencyRate
                        },
                        // TotalDay = TimeSpan.Days,
                        // TotalDay = (joinResult.UnitPaymentOrder.DueDate.Date.Date - joinResult.UnitPaymentOrder.Date.Date.Date).TotalDays,
                        //TotalDay = (joinResult.UnitPaymentOrder.DueDate.Date - joinResult.UnitPaymentOrder.Date.Date),
                        // TotalDay = SqlServerDbFunctionsExtensions.,
                        Category = new CategoryViewModel()
                        {
                            Code = expeditionDocument.CategoryCode,
                            Name = expeditionDocument.CategoryName
                        },
                        Unit = new UnitViewModel()
                        {
                            Code = expeditionDocument.Items.FirstOrDefault().UnitCode,
                            Name = expeditionDocument.Items.FirstOrDefault().UnitName
                        },
                        Division = new DivisionViewModel()
                        {
                            Code = expeditionDocument.DivisionCode,
                            Name = expeditionDocument.DivisionName
                        },
                        LastModifiedUtc = expeditionDocument.LastModifiedUtc,
                        VerifiedBy = expeditionDocument.VerificationDivisionBy,
                        //TotalDay = Math.Abs((Convert.ToDateTime(expeditionDocument.DueDate)-Convert.ToDateTime(expeditionDocument.UPODate.Date)).TotalDays)
                        // TotalDay = Math.Ceiling((expeditionDocument.DueDate - expeditionDocument.UPODate).TotalDays)
                    }
                );

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
            if (listData != null && listData.Count > 0)
            {
                data = listData;
            }

            var headers = new string[] { "No. SPB", "Tgl SPB", "Tgl Jatuh Tempo", "Nomor Invoice", "Supplier", "Kurs", "Jumlah", "Jumlah1", "Jumlah2", "Jumlah3", "Tempo", "Kategori", "Unit", "Divisi", "Posisi", "Tgl Pembelian Kirim", "Verifikasi", "Verifikasi1", "Verifikasi2", "Kasir", "Kasir1" };
            var subHeaders = new string[] { "DPP", "PPn", "PPh", "Total", "Tgl Terima", "Tgl Cek", "Tgl Kirim", "Tgl Terima", "No Kuitansi" };

            DataTable dataTable = new DataTable();

            var headersDateType = new int[] { 1, 2, 15, 16, 17, 18, 19 };
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
                var diff = ((d.DueDate).Value - (d.Date).Value).TotalDays;
                
                dataTable.Rows.Add(d.No ?? "-", GetFormattedDate(d.Date), GetFormattedDate(d.DueDate), d.InvoiceNo ?? "-", d.Supplier.name ?? "-", d.Currency.Code ?? "-", d.DPP, d.PPn, d.PPh, d.TotalTax, Math.Ceiling(diff), d.Category.Name ?? "-", d.Unit.Name ?? "-", d.Division.Name ?? "-", d.Position, GetFormattedDate(d.SendToVerificationDivisionDate), GetFormattedDate(d.VerificationDivisionDate), GetFormattedDate(d.VerifyDate), GetFormattedDate(d.SendDate), GetFormattedDate(d.CashierDivisionDate), d.BankExpenditureNoteNo ?? "-");
            }

            ExcelPackage package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Data");

            sheet.Cells["A3"].LoadFromDataTable(dataTable, false, OfficeOpenXml.Table.TableStyles.None);

            sheet.Cells["G1"].Value = headers[6];
            sheet.Cells["G1:J1"].Merge = true;
            sheet.Cells["Q1"].Value = headers[16];
            sheet.Cells["Q1:S1"].Merge = true;
            sheet.Cells["T1"].Value = headers[19];
            sheet.Cells["T1:U1"].Merge = true;

            foreach (var i in Enumerable.Range(0, 6))
            {
                var col = (char)('A' + i);
                sheet.Cells[$"{col}1"].Value = headers[i];
                sheet.Cells[$"{col}1:{col}2"].Merge = true;
            }

            foreach (var i in Enumerable.Range(0, 4))
            {
                var col = (char)('G' + i);
                sheet.Cells[$"{col}2"].Value = subHeaders[i];
            }

            foreach (var i in Enumerable.Range(0, 6))
            {
                var col = (char)('K' + i);
                sheet.Cells[$"{col}1"].Value = headers[i + 10];
                sheet.Cells[$"{col}1:{col}2"].Merge = true;
            }

            foreach (var i in Enumerable.Range(0, 5))
            {
                var col = (char)('Q' + i);
                sheet.Cells[$"{col}2"].Value = subHeaders[i + 4];
            }
            sheet.Cells["A1:U2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1:U2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A1:U2"].Style.Font.Bold = true;

            foreach (var headerDateType in headersDateType)
            {
                sheet.Column(headerDateType + 1).Style.Numberformat.Format = "dd MMMM yyyy";
            }

            var widths = new int[] { 20, 20, 20, 50, 30, 10, 20, 20, 20, 20, 20, 30, 30, 20, 40, 20, 20, 20, 20, 20, 20 };
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
