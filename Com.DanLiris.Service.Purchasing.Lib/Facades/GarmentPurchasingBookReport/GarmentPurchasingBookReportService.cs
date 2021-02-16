using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public class GarmentPurchasingBookReportService : IGarmentPurchasingBookReportService
    {
        private readonly PurchasingDbContext _dbContext;

        public GarmentPurchasingBookReportService(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
        }

        public List<BillNoPaymentBillAutoCompleteDto> GetBillNos(string keyword)
        {
            var query = _dbContext.GarmentDeliveryOrders.Select(entity => entity.BillNo).Distinct();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(entity => entity.Contains(keyword));

            return query.Take(10).Select(entity => new BillNoPaymentBillAutoCompleteDto(entity)).ToList();
        }

        public List<BillNoPaymentBillAutoCompleteDto> GetPaymentBills(string keyword)
        {
            var query = _dbContext.GarmentDeliveryOrders.Select(entity => entity.PaymentBill).Distinct();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(entity => entity.Contains(keyword));

            return query.Take(10).Select(entity => new BillNoPaymentBillAutoCompleteDto(entity)).ToList();
        }

        public ReportDto GetReport(string billNo, string paymentBill, string garmentCategory, DateTimeOffset startDate, DateTimeOffset endDate, bool isForeignCurrency, bool isImportSupplier)
        {
            var query = from garmentDeliveryOrders in _dbContext.GarmentDeliveryOrders.Include(entity => entity.Items).ThenInclude(entity => entity.Details).AsQueryable()

                        join customsItems in _dbContext.GarmentBeacukaiItems.AsQueryable() on garmentDeliveryOrders.Id equals customsItems.GarmentDOId into doCustoms
                        from deliveryOrderCustoms in doCustoms.DefaultIfEmpty()

                        join garmentDeliveryOrderItems in _dbContext.GarmentDeliveryOrderItems.AsQueryable() on garmentDeliveryOrders.Id equals garmentDeliveryOrderItems.GarmentDOId into garmentDOItems
                        from deliveryOrderItems in garmentDOItems.DefaultIfEmpty()

                            //join garmentDeliveryOrderDetails in _dbContext.GarmentDeliveryOrderDetails.AsQueryable() on deliveryOrderItems.Id equals garmentDeliveryOrderDetails.GarmentDOItemId into garmentDODetails
                            //from deliveryOrderDetails in garmentDODetails.DefaultIfEmpty()

                        join invoiceItems in _dbContext.GarmentInvoiceItems.AsQueryable() on garmentDeliveryOrders.Id equals invoiceItems.DeliveryOrderId into garmentDOInvoiceItems
                        from deliveryOrderInvoiceItems in garmentDOInvoiceItems.DefaultIfEmpty()

                        join invoices in _dbContext.GarmentInvoices.AsQueryable() on deliveryOrderInvoiceItems.InvoiceId equals invoices.Id into garmentDOInvoices
                        from deliveryOrderInvoices in garmentDOInvoices.DefaultIfEmpty()

                        join garmentExternalPurchaseOrders in _dbContext.GarmentExternalPurchaseOrders.AsQueryable() on deliveryOrderItems.EPOId equals garmentExternalPurchaseOrders.Id into doExternalPurchaseOrders
                        from deliveryOrderExternalPurchaseOrders in doExternalPurchaseOrders.DefaultIfEmpty()

                        join internalNoteDetails in _dbContext.GarmentInternNoteDetails.AsQueryable() on garmentDeliveryOrders.Id equals internalNoteDetails.DOId into doInternalNoteDetails
                        from deliveryOrderInternalNoteDetails in doInternalNoteDetails.DefaultIfEmpty()

                        join internalNoteItems in _dbContext.GarmentInternNoteItems.AsQueryable() on deliveryOrderInternalNoteDetails.GarmentItemINId equals internalNoteItems.Id into doInternalNoteItems
                        from deliveryOrderInternalNoteItems in doInternalNoteItems.DefaultIfEmpty()

                        join internalNotes in _dbContext.GarmentInternNotes.AsQueryable() on deliveryOrderInternalNoteItems.GarmentINId equals internalNotes.Id into doInternalNotes
                        from deliveryOrderInternalNotes in doInternalNotes.DefaultIfEmpty()

                        select new ReportIndexDto(garmentDeliveryOrders, deliveryOrderCustoms, deliveryOrderItems, deliveryOrderInvoiceItems, deliveryOrderInvoices, deliveryOrderExternalPurchaseOrders, deliveryOrderInternalNoteDetails, deliveryOrderInternalNoteItems, deliveryOrderInternalNotes);

            query = query.Where(entity => entity.CustomsArrivalDate >= startDate && entity.CustomsArrivalDate <= endDate);

            if (!string.IsNullOrWhiteSpace(billNo))
            {
                query = query.Where(entity => entity.BillNo == billNo);
            }

            if (!string.IsNullOrWhiteSpace(paymentBill))
            {
                query = query.Where(entity => entity.PaymentBill == paymentBill);
            }

            if (!string.IsNullOrWhiteSpace(garmentCategory))
            {
                query = query.Where(entity => entity.PurchasingCategoryName == garmentCategory);
            }

            if (isImportSupplier)
            {
                query = query.Where(entity => entity.IsImportSupplier);
            }
            else
            {
                query = query.Where(entity => !entity.IsImportSupplier);
            }

            if (isForeignCurrency)
            {
                query = query.Where(entity => entity.CurrencyCode != "IDR");
            }

            var data = query.ToList();

            var reportCategories = data
                .GroupBy(element => element.AccountingCategoryName)
                .Select(element => new ReportCategoryDto(0, element.Key, element.Sum(sum => sum.Total)))
                .ToList();

            var reportCurrencies = data
                .GroupBy(element => element.CurrencyId)
                .Select(element => new ReportCurrencyDto(element.Key, element.FirstOrDefault(e => element.Key == e.CurrencyId).CurrencyCode, element.FirstOrDefault(e => element.Key == e.CurrencyId).CurrencyRate, element.Sum(sum => sum.Total)))
                .ToList();

            return new ReportDto(data, reportCategories, reportCurrencies);
        }

        public async Task<MemoryStream> GenerateExcel(string billNo, string paymentBill, string garmentCategory, DateTimeOffset startDate, DateTimeOffset endDate, bool isForeignCurrency, bool isImportSupplier,int timeZone)
        {
            var result = GetReport(billNo, paymentBill, garmentCategory, startDate, endDate, isForeignCurrency, isImportSupplier);
            //var Data = reportResult.Reports;
            var reportDataTable = new DataTable();
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal Bon", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No BP Besar", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No BP Kecil", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
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
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Rate", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

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
                    reportDataTable.Rows.Add(report.CustomsArrivalDate,report.SupplierName,report.ProductName,report.GarmentDeliveryOrderNo,report.BillNo, report.PaymentBill,report.InvoiceNo,report.VATNo,report.InternalNoteNo,report.PurchasingCategoryName, report.AccountingCategoryName,report.InternalNoteQuantity,report.DPPAmount,report.VATAmount,report.IncomeTaxAmount,report.Total);
                }
                foreach (var categorySummary in result.Categories)
                    categoryDataTable.Rows.Add(categorySummary.CategoryName, categorySummary.Amount);

                foreach (var currencySummary in result.Currencies)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.Amount, 0);//TODO : change to Currency TOtal Idr
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var title = "BUKU PEMBELIAN Import";
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
