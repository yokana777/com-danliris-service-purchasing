using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnpaidDispositionReport;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.UnpaidDispositionReportFacades
{
    public class UnpaidDispositionReportDetailFacade : IUnpaidDispositionReportDetailFacade
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IdentityService _identityService;
        private const string IDRCurrencyCode = "IDR";

        public UnpaidDispositionReportDetailFacade(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
            _currencyProvider = serviceProvider.GetService<ICurrencyProvider>();
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        private async Task<UnpaidDispositionReportDetailViewModel> GetReportData(int accountingUnitId, int accountingCategoryId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency)
        {
            //var dateStart = dateFrom.GetValueOrDefault().ToUniversalTime();
            var dateEnd = (dateTo.HasValue ? dateTo.Value : DateTime.Now).ToUniversalTime();

            var query = from pdItems in _dbContext.PurchasingDispositionItems

                        join pds in _dbContext.PurchasingDispositions on pdItems.PurchasingDispositionId equals pds.Id into joinPurchasingDispositions
                        from pd in joinPurchasingDispositions.DefaultIfEmpty()

                        join pdDetailItems in _dbContext.PurchasingDispositionDetails on pdItems.Id equals pdDetailItems.PurchasingDispositionItemId into joinPurchasingDispositionDetails
                        from pdDetailItem in joinPurchasingDispositionDetails.DefaultIfEmpty()

                        join urnItems in _dbContext.UnitReceiptNoteItems on pdItems.EPOId equals urnItems.EPOId.ToString() into joinUnitReceiptNoteItems
                        from urnItem in joinUnitReceiptNoteItems.DefaultIfEmpty()

                        join upoItems in _dbContext.UnitPaymentOrderItems on urnItem.Id equals upoItems.URNId into joinUnitPaymentOrderItems
                        from upoItem in joinUnitPaymentOrderItems.DefaultIfEmpty()

                        join upos in _dbContext.UnitPaymentOrders on upoItem.UPOId equals upos.Id into joinUnitPaymentOrders
                        from upo in joinUnitPaymentOrders.DefaultIfEmpty()

                        join epos in _dbContext.ExternalPurchaseOrders on pdItems.EPOId equals epos.Id.ToString() into joinExternalPurchaseOrders
                        from epo in joinExternalPurchaseOrders.DefaultIfEmpty()

                        where pd.PaymentDueDate <= dateEnd && pd.IsPaid == false && epo.SupplierIsImport == isImport
                        select new
                        {
                            pdItems.Id,

                            pd.CreatedUtc,
                            pd.IsPaid,
                            pd.DispositionNo,
                            pd.PaymentDueDate,
                            pd.CurrencyId,
                            pd.CurrencyCode,
                            pd.CurrencyRate,
                            pd.CategoryId,
                            pd.CategoryCode,
                            pd.CategoryName,
                            pd.SupplierName,
                            pd.DPP,
                            pd.IncomeTaxBy,
                            pd.IncomeTaxValue,
                            pd.VatValue,

                            pdDetailItem.UnitId,
                            pdDetailItem.UnitCode,
                            pdDetailItem.UnitName,

                            urnItem.UnitReceiptNote.URNNo,

                            upoItem.UnitPaymentOrder.UPONo,
                            upoItem.UnitPaymentOrder.InvoiceNo,

                            epo.SupplierIsImport
                        };

            if (!isForeignCurrency && !isImport)
                query = query.Where(x => x.CurrencyCode == "IDR");
            else if (isForeignCurrency)
                query = query.Where(x => x.CurrencyCode != "IDR");

            var unitFilterIds = await _currencyProvider.GetUnitsIdsByAccountingUnitId(accountingUnitId);
            if (unitFilterIds.Count() > 0)
                query = query.Where(urn => unitFilterIds.Contains(urn.UnitId));

            var categoryFilterIds = await _currencyProvider.GetCategoryIdsByAccountingCategoryId(accountingCategoryId);
            if (categoryFilterIds.Count() > 0)
                query = query.Where(urn => categoryFilterIds.Contains(urn.CategoryId));

            var queryResult = query.OrderByDescending(x => x.CreatedUtc).ToList();

            var unitIds = queryResult.Select(item =>
            {
                int.TryParse(item.UnitId, out var unitId);
                return unitId;
            }).Distinct().ToList();
            var units = await _currencyProvider.GetUnitsByUnitIds(unitIds);
            var accountingUnits = await _currencyProvider.GetAccountingUnitsByUnitIds(unitIds);

            var categoryIds = queryResult.Select(item =>
            {
                int.TryParse(item.CategoryId, out var categoryId);
                return categoryId;
            }).Distinct().ToList();
            var categories = await _currencyProvider.GetCategoriesByCategoryIds(categoryIds);
            var accountingCategories = await _currencyProvider.GetAccountingCategoriesByCategoryIds(categoryIds);

            var reportResult = new UnpaidDispositionReportDetailViewModel();
            foreach (var item in queryResult)
            {
                int.TryParse(item.UnitId, out var unitId);
                var unit = units.FirstOrDefault(element => element.Id == unitId);
                var accountingUnit = new AccountingUnit();
                if (unit != null)
                {
                    accountingUnit = accountingUnits.FirstOrDefault(element => element.Id == unit.AccountingUnitId);
                }

                int.TryParse(item.CategoryId, out var categoryId);
                var category = categories.FirstOrDefault(element => element.Id == categoryId);
                var accountingCategory = new AccountingCategory();
                if (category != null)
                {
                    accountingCategory = accountingCategories.FirstOrDefault(element => element.Id == category.AccountingCategoryId);
                }

                double total = 0;
                double totalCurrency = 0;
                if (item.IncomeTaxBy == "Supplier")
                {
                    total = item.DPP + item.VatValue - item.IncomeTaxValue;
                    totalCurrency = (item.DPP + item.VatValue - item.IncomeTaxValue) * item.CurrencyRate;
                }
                else
                {
                    total = item.DPP + item.VatValue - item.IncomeTaxValue;
                    totalCurrency = (item.DPP + item.VatValue - item.IncomeTaxValue) * item.CurrencyRate;
                }


                var reportItem = new DispositionReport()
                {
                    DispositionNo = item.DispositionNo,
                    DispositionDate = item.CreatedUtc,
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    CategoryCode = item.CategoryCode,
                    CurrencyId = item.CurrencyId,
                    CurrencyCode = item.CurrencyCode,
                    CurrencyRate = item.CurrencyRate,
                    AccountingCategoryName = accountingCategory.Name,
                    AccountingCategoryCode = accountingCategory.Code,
                    AccountingLayoutIndex = accountingCategory.AccountingLayoutIndex,
                    DPP = item.DPP,
                    DPPCurrency = item.DPP * item.CurrencyRate,
                    InvoiceNo = item.InvoiceNo,
                    VAT = item.VatValue,
                    Total = total,
                    TotalCurrency = totalCurrency,
                    SupplierName = item.SupplierName,
                    UnitId = item.UnitId,
                    UnitName = item.UnitName,
                    UnitCode = item.UnitCode,
                    AccountingUnitName = accountingUnit.Name,
                    AccountingUnitCode = accountingUnit.Code,
                    UPONo = item.UPONo,
                    URNNo = item.URNNo,
                    IncomeTax = item.IncomeTaxValue,
                    IncomeTaxBy = item.IncomeTaxBy,
                    PaymentDueDate = item.PaymentDueDate
                };

                reportResult.Reports.Add(reportItem);
            }

            reportResult.UnitSummaries = reportResult.Reports
                        .GroupBy(report => new { report.AccountingUnitName })
                        .Select(report => new UnitSummary()
                        {
                            Unit = report.Key.AccountingUnitName,
                            SubTotal = report.Sum(sum => sum.Total),
                            SubTotalCurrency = report.Sum(sum => sum.TotalCurrency),
                            AccountingLayoutIndex = report.Select(item => item.AccountingLayoutIndex).FirstOrDefault()
                        }).OrderBy(order => order.AccountingLayoutIndex).ToList();
            reportResult.Reports = reportResult.Reports;
            reportResult.GrandTotal = reportResult.Reports.Sum(sum => sum.TotalCurrency);
            reportResult.UnitSummaryTotal = reportResult.UnitSummaries.Sum(categorySummary => categorySummary.SubTotalCurrency);

            return reportResult;
        }

        public async Task<MemoryStream> GenerateExcel(int accountingUnitId, int accountingCategoryId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency)
        {
            var result = await GetReport(accountingUnitId, accountingCategoryId, divisionId, dateTo, isImport, isForeignCurrency);

            var reportDataTable = GetFormatReportExcel();

            var unitDataTable = new DataTable();
            unitDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            unitDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

            if (result.Reports.Count > 0)
            {
                var data = result.Reports.GroupBy(x => x.AccountingUnitCode);
                int i = 1;
                foreach (var reports in data)
                {
                    foreach (var v in reports)
                    {
                        reportDataTable.Rows.Add(i.ToString(), v.DispositionDate.GetValueOrDefault().ToString("dd/MM/yyyy"), v.DispositionNo, v.URNNo, v.UPONo, v.InvoiceNo, v.SupplierName, v.AccountingCategoryName, v.AccountingUnitName, v.PaymentDueDate.GetValueOrDefault().ToString("dd/MM/yyyy"), v.CurrencyCode, v.Total.ToString("##0,#0"));
                        i++;
                    }
                }

                //foreach (var unitSummary in result.UnitSummaries)
                //    unitDataTable.Rows.Add(unitSummary.Unit, unitSummary.SubTotal);
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var title = "BUKU PEMBELIAN LOKAL";
                if (isForeignCurrency)
                    title = "BUKU PEMBELIAN LOKAL VALAS";
                else if (isImport)
                    title = "BUKU PEMBELIAN IMPORT";
                var period = $"Periode sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):dd/MM/yyyy}";

                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].Value = company;
                worksheet.Cells["A2"].Value = title;
                worksheet.Cells["A3"].Value = period;
                worksheet.Cells["A4"].LoadFromDataTable(reportDataTable, true);
                worksheet.Cells[$"A{4 + 3 + result.Reports.Count}"].LoadFromDataTable(unitDataTable, true);
                //worksheet.Cells[$"A{4 + result.Reports.Count + 3 + result.CategorySummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }

        private DataTable GetFormatReportExcel()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Tgl Disposisi", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Disposisi", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No SPB", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No BP", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Jatuh Tempo", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Currency", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Saldo", DataType = typeof(string) });

            return dt;
        }

        public Task<UnpaidDispositionReportDetailViewModel> GetReport(int accountingUnitId, int accountingCategoryId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency)
        {
            return GetReportData(accountingUnitId, accountingCategoryId, divisionId, dateTo, isImport, isForeignCurrency);
        }
    }
}
