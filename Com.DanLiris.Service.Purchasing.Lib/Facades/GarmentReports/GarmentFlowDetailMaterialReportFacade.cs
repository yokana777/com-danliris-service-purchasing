using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class GarmentFlowDetailMaterialReportFacade : IGarmentFlowDetailMaterialReport
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentUnitExpenditureNote> dbSet;


        public GarmentFlowDetailMaterialReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentUnitExpenditureNote>();
        }


        public IQueryable<GarmentFlowDetailMaterialViewModel> GetQuery(string category,  DateTimeOffset? DateFrom, DateTimeOffset? DateTo, int offset)
        {
            //DateTimeOffset dateFrom = DateFrom == null ? new DateTime(1970, 1, 1) : (DateTimeOffset)DateFrom;
            //DateTimeOffset dateTo = DateTo == null ? new DateTime(2100, 1, 1) : (DateTimeOffset)DateTo;

            var Query = (from a in dbContext.GarmentUnitExpenditureNoteItems 
                         join b in dbContext.GarmentUnitExpenditureNotes on a.UENId equals b.Id
                         join c in dbContext.GarmentExternalPurchaseOrderItems on a.EPOItemId equals c.Id
                         join d in dbContext.GarmentUnitDeliveryOrders on b.UnitDONo equals d.UnitDONo

                         join e in dbContext.GarmentUnitDeliveryOrderItems on d.Id equals e.UnitDOId
                         join f in dbContext.GarmentDeliveryOrderDetails on e.DODetailId equals f.Id
                         where
                         f.CodeRequirment == (string.IsNullOrWhiteSpace(category) ? f.CodeRequirment : category)
                         && a.CreatedUtc.Date >= DateFrom
                         && a.CreatedUtc.Date <= DateTo

                         orderby a.CreatedUtc descending
                         select new GarmentFlowDetailMaterialViewModel {
                             ProductCode = a.ProductCode,
                             ProductName = a.ProductName,
                             POSerialNumber = a.POSerialNumber,
                             ProductRemark = a.ProductRemark,
                             RONo = a.RONo,
                             Article = c.Article,
                             BuyerCode = a.BuyerCode,
                             RONoDO = d.RONo,
                             ArticleDO = d.Article,
                             UnitDOType = d.UnitDOType,
                             UENNo = b.UENNo,
                             ExpenditureDate = b.ExpenditureDate,
                             Quantity = a.Quantity,
                             UomUnit = a.UomUnit,
                             Total = a.Quantity * a.PricePerDealUnit* a.DOCurrencyRate

                         });


            return Query.AsQueryable();
        }

        public Tuple<List<GarmentFlowDetailMaterialViewModel>, int> GetReport(string category, DateTimeOffset? DateFrom, DateTimeOffset? DateTo, int offset, string order, int page, int size)
        {
            var Query = GetQuery( category, DateFrom, DateTo, offset);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            //if (OrderDictionary.Count.Equals(0))
            //{
            //	Query = Query.OrderByDescending(b => b.poExtDate);
            //}

            Pageable<GarmentFlowDetailMaterialViewModel> pageable = new Pageable<GarmentFlowDetailMaterialViewModel>(Query, page - 1, size);
            List<GarmentFlowDetailMaterialViewModel> Data = pageable.Data.ToList<GarmentFlowDetailMaterialViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcel(string category, string categoryname, string unit, string unitname, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int offset)
        {
            var Query = GetQuery(category, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.CreatedUtc);
            DataTable result = new DataTable();
            //No	Unit	Budget	Kategori	Tanggal PR	Nomor PR	Kode Barang	Nama Barang	Jumlah	Satuan	Tanggal Diminta Datang	Status	Tanggal Diminta Datang Eksternal


            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No. R/O", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Artikel", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Buyer", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Untuk RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Untuk Artikel", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tujuan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No.Bukti", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Quantity", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah", DataType = typeof(String) });
            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", 0, "", 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string tanggal = item.ExpenditureDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    result.Rows.Add(index, item.ProductCode, item.ProductName, item.POSerialNumber, item.ProductRemark, item.RONo,
                        item.Article, item.BuyerCode, item.RONoDO, item.ArticleDO, item.UnitDOType, item.UENNo, tanggal, NumberFormat(item.Quantity),
                        item.UomUnit, NumberFormat(item.Total));
                }
            }
            ExcelPackage package = new ExcelPackage();
            //DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom.Value.DateTime;
            //DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo.Value.DateTime;
            CultureInfo Id = new CultureInfo("id-ID");

            var sheet = package.Workbook.Worksheets.Add("Report");

            var col = (char)('A' + result.Columns.Count);
            string tglawal = dateFrom.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            string tglakhir = dateTo.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            sheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN REKAP PENGELUARAN {0}", categoryname);
            sheet.Cells[$"A1:{col}1"].Merge = true;
            sheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
            sheet.Cells[$"A2:{col}2"].Merge = true;
            sheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col}3"].Value = string.Format("KONFEKSI : {0}", unitname);
            sheet.Cells[$"A3:{col}3"].Merge = true;
            sheet.Cells[$"A3:{col}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            sheet.Cells["A5"].LoadFromDataTable(result, true, OfficeOpenXml.Table.TableStyles.Light16);

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
            //return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        String NumberFormat(double? numb)
        {

            var number = string.Format("{0:0,0.00}", numb);

            return number;
        }
    }
}
