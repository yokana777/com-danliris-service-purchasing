using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport.Excel
{
    public static class GarmentPurchasingBookReportImportExcel
    {
        public static async Task<MemoryStream> GenerateExcel(DateTimeOffset startDate, DateTimeOffset endDate, ReportDto data, int timeZone)
        {
            var result = data;
            var reportDataTable = new DataTable();
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal Bon", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No BP Besar", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No BP Kecil", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No NI", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori Pembelian", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori Pembukuan", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Quantity", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPh", DataType = typeof(decimal) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Total(IDR)", DataType = typeof(decimal) });

            var categoryDataTable = new DataTable();
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

            if (result.Data.Count > 0)
            {
                foreach (var report in result.Data)
                {
                    reportDataTable.Rows.Add(report.CustomsArrivalDate, report.SupplierName, report.ProductName, report.GarmentDeliveryOrderNo, report.BillNo, report.PaymentBill, report.InvoiceNo, report.VATNo, report.InternalNoteNo, report.PurchasingCategoryName, report.AccountingCategoryName, report.InternalNoteQuantity, report.DPPAmount, report.VATAmount, report.IncomeTaxAmount, report.Total);
                }
                foreach (var categorySummary in result.Categories)
                    categoryDataTable.Rows.Add(categorySummary.CategoryName, categorySummary.Amount);

                foreach (var currencySummary in result.Currencies)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.Amount, 0);//TODO : change to Currency TOtal Idr
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var title = "BUKU PEMBELIAN Buku Lokal";
                var period = $"Dari {startDate.AddHours(timeZone):dd/MM/yyyy} Sampai {endDate.AddHours(timeZone):dd/MM/yyyy}";

                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].Value = company;
                worksheet.Cells["A2"].Value = title;
                worksheet.Cells["A3"].Value = period;
                worksheet.Cells["A4"].LoadFromDataTable(reportDataTable, true);
                worksheet.Cells[$"A{4 + 3 + result.Data.Count}"].LoadFromDataTable(categoryDataTable, true);
                worksheet.Cells[$"A{4 + result.Data.Count + 3 + result.Data.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }
    }
}
