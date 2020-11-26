using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using MongoDB.Driver;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Microsoft.EntityFrameworkCore;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using MongoDB.Bson;
using System.Data.SqlClient;
using System.Globalization;
using Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Microsoft.Extensions.Caching.Distributed;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Report
{
    public class DetailCreditBalanceReportFacade : IDetailCreditBalanceReportFacade
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IdentityService _identityService;
        //private const string IDRCurrencyCode = "IDR";
        private readonly List<CategoryDto> _categories;

        public DetailCreditBalanceReportFacade(IServiceProvider serviceProvider)
        {
            var cache = serviceProvider.GetService<IDistributedCache>();
            var jsonCategories = cache.GetString(MemoryCacheConstant.Categories);

            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
            _currencyProvider = serviceProvider.GetService<ICurrencyProvider>();
            _identityService = serviceProvider.GetService<IdentityService>();

            _categories = JsonConvert.DeserializeObject<List<CategoryDto>>(jsonCategories, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }

        public async Task<DetailCreditBalanceReportViewModel> GetReportData(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency)
        {
            // var d1 = dateFrom.GetValueOrDefault().ToUniversalTime();
            var d2 = (dateTo.HasValue ? dateTo.Value : DateTime.MaxValue).ToUniversalTime();

            var query = from urnWithItem in _dbContext.UnitReceiptNoteItems

                        join urn in _dbContext.UnitReceiptNotes on urnWithItem.URNId equals urn.Id into joinUnitReceiptNotes
                        from urnItemUrn in joinUnitReceiptNotes.DefaultIfEmpty()

                        join upoItem in _dbContext.UnitPaymentOrderItems on urnItemUrn.Id equals upoItem.URNId into joinUnitPaymentOrderItems
                        from urnUPOItem in joinUnitPaymentOrderItems.DefaultIfEmpty()

                        join upo in _dbContext.UnitPaymentOrders on urnUPOItem.UPOId equals upo.Id into joinUnitPaymentOrders
                        from urnUPO in joinUnitPaymentOrders.DefaultIfEmpty()

                        join epo in _dbContext.ExternalPurchaseOrders on urnWithItem.EPOId equals epo.Id into joinExternalPurchaseOrder
                        from urnEPO in joinExternalPurchaseOrder.DefaultIfEmpty()

                        join pr in _dbContext.PurchaseRequests on urnWithItem.PRId equals pr.Id into joinPurchaseRequest
                        from urnPR in joinPurchaseRequest.DefaultIfEmpty()

                        // Additional
                        join epoDetail in _dbContext.ExternalPurchaseOrderDetails on urnWithItem.EPODetailId equals epoDetail.Id into joinExternalPurchaseOrderDetails
                        from urnEPODetail in joinExternalPurchaseOrderDetails.DefaultIfEmpty()

                        where urnItemUrn != null && urnItemUrn.ReceiptDate != null  
                        && urnEPO != null && urnEPO.PaymentDueDays != null
                        && urnItemUrn.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays)) <= d2
                        && urnUPO != null && urnUPO.IsPaid == false && urnEPO != null && urnEPO.SupplierIsImport == isImport
                        
                        select new
                        {
                            //urnWithItem.UnitReceiptNote.ReceiptDate,
                            urnUPOItem.UnitPaymentOrder.Date,
                            UPONo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.UPONo : "",
                            urnWithItem.UnitReceiptNote.URNNo,
                            InvoiceNo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.InvoiceNo : "",
                            urnWithItem.UnitReceiptNote.SupplierName,
                            urnPR.CategoryName,
                            //AccountingUnitName = 
                            DueDate = urnItemUrn != null && urnItemUrn.ReceiptDate != null && urnEPO != null ? urnItemUrn.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays)) : DateTimeOffset.Now,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyCode,
                            //TotalSaldo = urnWithItem.PricePerDealUnit * urnWithItem.ReceiptQuantity,

                            urnPR.CategoryId,
                            urnPR.DivisionName,
                            urnWithItem.UnitReceiptNote.UnitId,
                            urnPR.DivisionId,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseVat,
                            urnWithItem.ReceiptQuantity,
                            EPOPricePerDealUnit = urnEPODetail.PricePerDealUnit,
                            urnWithItem.IncomeTaxBy,
                            urnEPO.UseIncomeTax,
                            urnEPO.IncomeTaxRate,
                        };

            if (!isForeignCurrency && !isImport)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() == "IDR");
            else if (isForeignCurrency)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() != "IDR");

            if (categoryId > 0)
                query = query.Where(urn => urn.CategoryId == categoryId.ToString());

            if (accountingUnitId > 0)
            {
                var unitFilterIds = await _currencyProvider.GetUnitsIdsByAccountingUnitId(accountingUnitId);
                if (unitFilterIds.Count() > 0)
                {
                    query = query.Where(urn => unitFilterIds.Contains(urn.UnitId));
                }
            }

            if (divisionId > 0)
                query = query.Where(urn => urn.DivisionId == divisionId.ToString());

            var queryResult = query.OrderByDescending(item => item.DueDate).ToList();
            
            var currencyTuples = queryResult.Select(item => new Tuple<string, DateTimeOffset>(item.CurrencyCode, item.Date));
            var currencies = await _currencyProvider.GetCurrencyByCurrencyCodeDateList(currencyTuples);

            var unitIds = queryResult.Select(item =>
            {
                int.TryParse(item.UnitId, out var unitId);
                return unitId;
            }).Distinct().ToList();
            var units = await _currencyProvider.GetUnitsByUnitIds(unitIds);
            var accountingUnits = await _currencyProvider.GetAccountingUnitsByUnitIds(unitIds);

            var itemCategoryIds = queryResult.Select(item =>
            {
                int.TryParse(item.CategoryId, out var itemCategoryId);
                return itemCategoryId;
            }).Distinct().ToList();
            var categories = await _currencyProvider.GetCategoriesByCategoryIds(itemCategoryIds);
            var accountingCategories = await _currencyProvider.GetAccountingCategoriesByCategoryIds(itemCategoryIds);
            

            var reportResult = new DetailCreditBalanceReportViewModel();
            foreach (var item in queryResult)
            {
                var currency = currencies.FirstOrDefault(f => f.Code == item.CurrencyCode);

                int.TryParse(item.UnitId, out var unitId);
                var unit = units.FirstOrDefault(element => element.Id == unitId);
                var accountingUnit = new AccountingUnit();
                if (unit != null)
                {
                    accountingUnit = accountingUnits.FirstOrDefault(element => element.Id == unit.AccountingUnitId);
                }

                //int.TryParse(item.CategoryId, out var itemCategoryId);
                //var category = categories.FirstOrDefault(element => element.Id == itemCategoryId);
                //var accountingCategory = new AccountingCategory();
                //if (category != null)
                //{
                //    accountingCategory = accountingCategories.FirstOrDefault(element => element.Id == category.AccountingCategoryId);
                //}

                var category = _categories.FirstOrDefault(_category => _category.Id.ToString() == item.CategoryId);
                var categoryLayoutIndex = 0;
                if (category != null)
                    categoryLayoutIndex = category.ReportLayoutIndex;

                decimal dpp = 0;
                decimal dppCurrency = 0;
                decimal ppn = 0;
                decimal ppnCurrency = 0;

                double currencyRate = 1;
                var currencyCode = "IDR";

                decimal totalDebt = 0;
                decimal totalDebtIDR = 0;
                decimal incomeTax = 0;
                decimal.TryParse(item.IncomeTaxRate, out var incomeTaxRate);


                if (item.UseVat)
                    ppn = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity * 0.1);

                if (currency != null && !currency.Code.Equals("IDR"))
                {
                    currencyRate = currency.Rate.GetValueOrDefault();
                    dpp = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);
                    dppCurrency = dpp * (decimal)currencyRate;
                    ppnCurrency = ppn * (decimal)currencyRate;
                    currencyCode = currency.Code;
                }
                else
                    dpp = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);

                if (item.UseIncomeTax)
                    incomeTax = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity) * incomeTaxRate / 100;

                if (item.IncomeTaxBy == "Supplier")
                {
                    totalDebtIDR = (dpp + ppn - incomeTax) * (decimal)currencyRate;
                    totalDebt = dpp + ppn - incomeTax;
                }
                else
                {
                    totalDebtIDR = (dpp + ppn) * (decimal)currencyRate;
                    totalDebt = dpp + ppn;
                }

                var reportItem = new DetailCreditBalanceReport()
                {
                    UPODate = item.Date,
                    UPONo = item.UPONo,
                    URNNo = item.URNNo,
                    InvoiceNo = item.InvoiceNo,
                    SupplierName = item.SupplierName,
                    CategoryName = item.CategoryName,
                    AccountingUnitName = accountingUnit.Name,
                    DueDate = item.DueDate,
                    CurrencyCode = currencyCode,
                    Total = totalDebt,
                    TotalIDR = totalDebtIDR,
                    CategoryId = item.CategoryId,
                    DivisionName = item.DivisionName,
                    CategoryLayoutIndex = categoryLayoutIndex,
                    //TotalSaldo = (decimal)item.TotalSaldo
                    //CategoryCode = item.CategoryCode,
                    //AccountingCategoryName = accountingCategory.Name,
                    //AccountingCategoryCode = accountingCategory.Code,
                    //AccountingLayoutIndex = accountingCategory.AccountingLayoutIndex,
                    //CurrencyRate = (decimal)currencyRate,
                    //DONo = item.DONo,
                    //DPP = dpp,
                    //DPPCurrency = dppCurrency,
                    //VATNo = item.VatNo,
                    //IPONo = item.PONo,
                    //VAT = ppn,
                    //VATCurrency = ppnCurrency,
                    //Total = dpp * (decimal)currencyRate,
                    //ProductName = item.ProductName,
                    //SupplierCode = item.SupplierCode,
                    //UnitName = item.UnitName,
                    //UnitCode = item.UnitCode,
                    //AccountingUnitCode = accountingUnit.Code,
                    //IsUseVat = item.UseVat,
                    //PIBDate = item.PibDate,
                    //PIBNo = item.PibNo,
                    //PIBBM = (decimal)item.ImportDuty,
                    //PIBIncomeTax = (decimal)item.TotalIncomeTaxAmount,
                    //PIBVat = (decimal)item.TotalVatAmount,
                    //PIBImportInfo = item.ImportInfo,
                    //Remark = item.Remark,
                    //Quantity = item.ReceiptQuantity,
                    //IsPaid = item.IsPaid
                    //Saldo = (decimal)item.Saldo,
                };

                reportResult.Reports.Add(reportItem);
            }

            reportResult.AccountingUnitSummaries = reportResult.Reports
                        .GroupBy(report => new { report.AccountingUnitName, report.CurrencyCode })
                        .Select(report => new SummaryDCB()
                        {
                            AccountingUnitName = report.Key.AccountingUnitName,
                            CurrencyCode = report.Key.CurrencyCode,
                            SubTotal = report.Sum(sum => sum.Total),
                            SubTotalIDR = report.Sum(sum => sum.TotalIDR),
                        })
                        .OrderBy(order => order.AccountingUnitName).ToList();
            
            reportResult.CurrencySummaries = reportResult.Reports
                        .GroupBy(report => new { report.CurrencyCode })
                        .Select(report => new SummaryDCB()
                        {
                            CurrencyCode = report.Key.CurrencyCode,
                            SubTotal = report.Sum(sum => sum.Total),
                            SubTotalIDR = report.Sum(sum => sum.TotalIDR)
                        })
                        .OrderBy(order => order.CurrencyCode).ToList();

            reportResult.Reports = reportResult.Reports
                        .GroupBy(
                            key => new 
                            {
                                key.UPONo,
                                key.UPODate,
                                key.URNNo,
                                key.InvoiceNo,
                                key.SupplierName,
                                key.CategoryName,
                                key.AccountingUnitName,
                                key.DueDate,
                                key.CurrencyCode,
                                key.CategoryId,
                                key.DivisionName,
                                key.CategoryLayoutIndex
                            },
                            val => val,
                            (key, val) => new DetailCreditBalanceReport()
                            {
                                UPONo = key.UPONo,
                                UPODate = key.UPODate,
                                URNNo = key.URNNo,
                                InvoiceNo = key.InvoiceNo,
                                SupplierName = key.SupplierName,
                                CategoryName = key.CategoryName,
                                AccountingUnitName = key.AccountingUnitName,
                                DueDate = key.DueDate,
                                CurrencyCode = key.CurrencyCode,
                                Total = val.Sum(s => s.Total),
                                TotalIDR = val.Sum(s => s.TotalIDR),
                                CategoryId = key.CategoryId,
                                DivisionName = key.DivisionName,
                                CategoryLayoutIndex = key.CategoryLayoutIndex
                            })
                        .OrderBy(order => order.CategoryLayoutIndex).ToList();

            reportResult.GrandTotal = reportResult.Reports.Sum(sum => sum.Total);
            reportResult.AccountingUnitSummaryTotal = reportResult.AccountingUnitSummaries.Sum(summary => summary.SubTotal);

            return reportResult;
        }

        public Task<DetailCreditBalanceReportViewModel> GetReport(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency)
        {
            return GetReportData(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);
        }

        //public async Task<MemoryStream> GenerateExcel(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency)
        //{
        //    var result = await GetReport(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);
        //    var reportDataTable = new DataTable();
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal SPB", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No BP", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Jatuh Tempo", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Currency", DataType = typeof(string) });
        //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Saldo", DataType = typeof(decimal) });

        //    var accountingUnitDataTable = new DataTable();
        //    accountingUnitDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
        //    accountingUnitDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

        //    var currencyDataTable = new DataTable();
        //    currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
        //    currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });
        //    currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

        //    if (result.Reports.Count > 0)
        //    {
        //        foreach (var report in result.Reports)
        //        {
        //            reportDataTable.Rows.Add(report.UPODate.ToString("dd/MM/yyyy"), report.UPONo, report.URNNo, report.InvoiceNo, report.SupplierName, report.CategoryName, report.AccountingUnitName, report.DueDate.ToString("dd/MM/yyyy"), report.CurrencyCode, report.TotalSaldo);
        //        }
        //        foreach (var accountingUnitSummary in result.AccountingUnitSummaries)
        //            accountingUnitDataTable.Rows.Add(accountingUnitSummary.AccountingUnitName, accountingUnitSummary.SubTotal);

        //        foreach (var currencySummary in result.CurrencySummaries)
        //            currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal, currencySummary.SubTotal);
        //    }

        //    using (var package = new ExcelPackage())
        //    {
        //        var company = "PT DAN LIRIS";
        //        var sTitle = isImport ? "IMPOR" : isForeignCurrency ? "LOKAL VALAS" : "LOKAL";
        //        var title = $"LAPORAN SALDO HUTANG (DETAIL) {sTitle}";
        //        var period = $"Periode sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):dd/MM/yyyy}";

        //        var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
        //        worksheet.Cells["A1"].Value = company;
        //        worksheet.Cells["A2"].Value = title;
        //        worksheet.Cells["A3"].Value = period;
        //        worksheet.Cells["A4"].LoadFromDataTable(reportDataTable, true);
        //        worksheet.Cells[$"A{4 + 3 + result.Reports.Count}"].LoadFromDataTable(accountingUnitDataTable, true);
        //        worksheet.Cells[$"A{4 + result.Reports.Count + 3 + result.AccountingUnitSummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

        //        var stream = new MemoryStream();
        //        package.SaveAs(stream);

        //        return stream;
        //    }
        //}

        public async Task<MemoryStream> GenerateExcel(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency)
        {
            var dueDateString = $"{dateTo:dd-MMM-yyyy}";
            if (dateTo == DateTimeOffset.MaxValue)
                dueDateString = "-";

            var result = await GetReport(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);

            var unitName = "SEMUA UNIT";
            var divisionName = "SEMUA DIVISI";
            var separator = " - ";

            if (accountingUnitId > 0 && divisionId == 0)
            {
                var summary = result.Reports.FirstOrDefault();
                if (summary != null)
                {
                    unitName = $"UNIT {summary.AccountingUnitName}";
                    separator = "";
                    divisionName = "";
                }
                else
                {
                    unitName = "";
                    separator = "";
                    divisionName = "";
                }
            }
            else if (divisionId > 0 && accountingUnitId == 0)
            {
                var summary = result.Reports.FirstOrDefault();
                if (summary != null)
                {
                    divisionName = $"DIVISI {summary.DivisionName}";
                    separator = "";
                    unitName = "";
                }
                else
                {
                    divisionName = "";
                    separator = "";
                    unitName = "";
                }
            }
            else if (accountingUnitId > 0 && divisionId > 0)
            {
                var summary = result.Reports.FirstOrDefault();
                if (summary != null)
                {
                    unitName = $"UNIT {summary.AccountingUnitName}";
                    separator = " - ";
                    divisionName = $"DIVISI {summary.DivisionName}";
                }
                else
                {
                    divisionName = "";
                    separator = "";
                    unitName = "";
                }
            }

            var reportDataTable = GetFormatReportExcel();

            var unitDataTable = new DataTable();
            if (isForeignCurrency || isImport)
            {
                unitDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
                unitDataTable.Columns.Add(new DataColumn() { ColumnName = "Currency", DataType = typeof(string) });
                unitDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });
            }
            else
            {
                unitDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
                unitDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });
            }

            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            int space = 0;
            if (result.Reports.Count > 0)
            {
                var data = result.Reports.GroupBy(x => x.CategoryName);
                int i = 1;
                foreach (var reports in data)
                {
                    var totalCurrencies = new Dictionary<string, decimal>();
                    foreach (var v in reports)
                    {
                        reportDataTable.Rows.Add(v.UPODate.GetValueOrDefault().ToString("dd/MM/yyyy"), v.UPONo, v.URNNo, v.InvoiceNo, v.SupplierName, v.CategoryName, v.AccountingUnitName, v.DueDate.GetValueOrDefault().ToString("dd/MM/yyyy"), v.CurrencyCode, string.Format("{0:n}", v.Total));
                        i++;

                        // Currency summary
                        if (totalCurrencies.ContainsKey(v.CurrencyCode))
                        {
                            totalCurrencies[v.CurrencyCode] += v.Total;
                        }
                        else
                        {
                            totalCurrencies.Add(v.CurrencyCode, v.Total);
                        }
                    }

                    foreach (var totalCurrency in totalCurrencies)
                    {
                        reportDataTable.Rows.Add("", "", "", "", "", "", "", "Jumlah", totalCurrency.Key, string.Format("{0:n}", totalCurrency.Value));
                        space++;
                    }

                }

                List<SummaryDCB> summaries = new List<SummaryDCB>();

                foreach (var unitSummary in result.AccountingUnitSummaries)
                {
                    if (summaries.Any(x => x.AccountingUnitName == unitSummary.AccountingUnitName))
                        summaries.Add(new SummaryDCB
                        {
                            AccountingUnitName = "",
                            CurrencyCode = unitSummary.CurrencyCode,
                            SubTotal = unitSummary.SubTotal,
                            SubTotalIDR = unitSummary.SubTotalIDR,
                            AccountingLayoutIndex = unitSummary.AccountingLayoutIndex
                        });
                    else
                        summaries.Add(unitSummary);
                }

                foreach (var unitSummary in summaries)
                {
                    if (isForeignCurrency || isImport)
                        unitDataTable.Rows.Add(unitSummary.AccountingUnitName, unitSummary.CurrencyCode, unitSummary.SubTotal);
                    else
                        unitDataTable.Rows.Add(unitSummary.AccountingUnitName, unitSummary.SubTotalIDR);
                }

                foreach (var currencySummary in result.CurrencySummaries)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal);
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var title = "LAPORAN SALDO HUTANG (DETAIL) LOKAL";
                if (isForeignCurrency)
                    title = "LAPORAN SALDO HUTANG (DETAIL) LOKAL VALAS";
                else if (isImport)
                    title = "LAPORAN SALDO HUTANG (DETAIL) IMPOR";
                //var period = $"Periode sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):dd/MM/yyyy}";
                var period = $"PERIODE S.D. {dueDateString}";

                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].Value = company;
                worksheet.Cells["A2"].Value = title;
                worksheet.Cells["A3"].Value = unitName + separator + divisionName;
                worksheet.Cells["A4"].Value = period;
                worksheet.Cells["A5"].LoadFromDataTable(reportDataTable, true);
                worksheet.Cells[$"A{5 + 3 + result.Reports.Count + space}"].LoadFromDataTable(unitDataTable, true);
                worksheet.Cells[$"A{5 + result.Reports.Count + space + 3 + result.AccountingUnitSummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }

        private DataTable GetFormatReportExcel()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "Tgl SPB", DataType = typeof(string) });
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

    }
}