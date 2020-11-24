using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Report
{
    public class DetailCreditBalanceReportFacade : IDetailCreditBalanceReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitReceiptNote> dbSet;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IdentityService _identityService;

        public DetailCreditBalanceReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitReceiptNote>();
            _currencyProvider = (ICurrencyProvider)serviceProvider.GetService(typeof(ICurrencyProvider));
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        public async Task<DetailCreditBalanceReportViewModel> GetReportData(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency)
        {
            // var d1 = dateFrom.GetValueOrDefault().ToUniversalTime();
            var d2 = (dateTo.HasValue ? dateTo.Value : DateTime.MaxValue).ToUniversalTime();

            var query = from urnWithItem in dbContext.UnitReceiptNoteItems

                        join urn in dbContext.UnitReceiptNotes on urnWithItem.URNId equals urn.Id into joinUnitReceiptNotes
                        from urnItemUrn in joinUnitReceiptNotes.DefaultIfEmpty()

                        join upoItem in dbContext.UnitPaymentOrderItems on urnItemUrn.Id equals upoItem.URNId into joinUnitPaymentOrderItems
                        from urnUPOItem in joinUnitPaymentOrderItems.DefaultIfEmpty()

                        join upo in dbContext.UnitPaymentOrders on urnUPOItem.UPOId equals upo.Id into joinUnitPaymentOrders
                        from urnUPO in joinUnitPaymentOrders.DefaultIfEmpty()

                        join epo in dbContext.ExternalPurchaseOrders on urnWithItem.EPOId equals epo.Id into joinExternalPurchaseOrder
                        from urnEPO in joinExternalPurchaseOrder.DefaultIfEmpty()

                        join pr in dbContext.PurchaseRequests on urnWithItem.PRId equals pr.Id into joinPurchaseRequest
                        from urnPR in joinPurchaseRequest.DefaultIfEmpty()

                            // Additional
                        join epoDetail in dbContext.ExternalPurchaseOrderDetails on urnWithItem.EPODetailId equals epoDetail.Id into joinExternalPurchaseOrderDetails
                        from urnEPODetail in joinExternalPurchaseOrderDetails.DefaultIfEmpty()

                        where urnItemUrn != null && urnItemUrn.ReceiptDate != null  
                        && urnEPO != null && urnEPO.PaymentDueDays != null
                        && urnItemUrn.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays)) <= d2
                        && urnUPO != null && urnUPO.IsPaid == false && urnEPO != null && urnEPO.SupplierIsImport == isImport
                        //where urnItemUrn != null && urnItemUrn.ReceiptDate != null  
                        //&& urnEPO != null && urnEPO.PaymentDueDays != null
                        //&& urnItemUrn.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays)) <= d2 
                        //&& urnUPO != null && urnUPO.IsPaid == false && urnEPO != null && urnEPO.SupplierIsImport == isImport

                        

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
                            //DueDate = urnWithItem.UnitReceiptNote.ReceiptDate != null ? urnWithItem.UnitReceiptNote.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays)) : DateTimeOffset.Now,
                            DueDate = urnItemUrn != null && urnItemUrn.ReceiptDate != null && urnEPO != null ? urnItemUrn.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays)) : DateTimeOffset.Now,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyCode,
                            TotalSaldo = urnWithItem.PricePerDealUnit * urnWithItem.ReceiptQuantity,

                            urnPR.CategoryId,
                            urnWithItem.UnitReceiptNote.UnitId,
                            urnPR.DivisionId,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseVat,
                            urnWithItem.ReceiptQuantity,
                            EPOPricePerDealUnit = urnEPODetail.PricePerDealUnit,
                            urnWithItem.IncomeTaxBy,
                            urnEPO.UseIncomeTax,
                            urnEPO.IncomeTaxRate,

                            //urnPR.CategoryCode,
                            //urnPR.DivisionCode,
                            //urnPR.DivisionName,
                            //urnPR.Remark,

                            //URNId = urnWithItem.UnitReceiptNote.Id,
                            //urnWithItem.PRId,
                            //urnWithItem.UnitReceiptNote.DOId,
                            //urnWithItem.UnitReceiptNote.DONo,
                            //urnWithItem.ProductName,
                            //urnWithItem.UnitReceiptNote.SupplierCode,
                            //urnWithItem.UnitReceiptNote.UnitCode,
                            //urnWithItem.UnitReceiptNote.UnitName,
                            //urnWithItem.EPODetailId,
                            //urnWithItem.PricePerDealUnit,
                            //urnWithItem.Uom,

                            //urnEPODetail.ExternalPurchaseOrderItem.PONo,

                            //VatNo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.VatNo : "",
                            //PibDate = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.PibDate : new DateTimeOffset(),
                            //PibNo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.PibNo : "",
                            //ImportDuty = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.ImportDuty : 0,
                            //TotalIncomeTaxAmount = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.TotalIncomeTaxAmount : 0,
                            //TotalVatAmount = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.TotalVatAmount : 0,
                            //ImportInfo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.ImportInfo : "",

                            //IsPaid = urnUPO != null && urnUPO.IsPaid,
                            //IsImport = urnWithItem.UnitReceiptNote.SupplierIsImport,
                            //DateTo = urnWithItem.UnitReceiptNote.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays))
                            //Saldo = urnWithItem.PricePerDealUnit,
                        };

            //query = query.Where(entity => !entity.IsPaid && entity.IsImport == isImport && entity.DateTo <= dateTo);

            //query = query.GroupBy(x => x.UPONo).Select(y => y.First());

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

            if (!isForeignCurrency && !isImport)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() == "IDR");
            else if (isForeignCurrency)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() != "IDR");

            var queryResult = query.OrderByDescending(item => item.Date).ToList();
            
            var currencyTuples = queryResult.Select(item => new Tuple<string, DateTimeOffset>(item.CurrencyCode, item.Date));
            var currencies = await _currencyProvider.GetCurrencyByCurrencyCodeDateList(currencyTuples);

            var unitIds = queryResult.Select(item =>
            {
                int.TryParse(item.UnitId, out var unitId);
                return unitId;
            }).Distinct().ToList();
            var units = await _currencyProvider.GetUnitsByUnitIds(unitIds);
            var accountingUnits = await _currencyProvider.GetAccountingUnitsByUnitIds(unitIds);

            /*
            var categoryIds = queryResult.Select(item =>
            {
                int.TryParse(item.CategoryId, out var categoryId);
                return categoryId;
            }).Distinct().ToList();
            var categories = await _currencyProvider.GetCategoriesByCategoryIds(categoryIds);
            var accountingCategories = await _currencyProvider.GetAccountingCategoriesByCategoryIds(categoryIds);
            */

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

                /*
                int.TryParse(item.CategoryId, out var categoryId);
                var category = categories.FirstOrDefault(element => element.Id == categoryId);
                var accountingCategory = new AccountingCategory();
                if (category != null)
                {
                    accountingCategory = accountingCategories.FirstOrDefault(element => element.Id == category.AccountingCategoryId);
                }
                */

                decimal dpp = 0;
                decimal dppCurrency = 0;
                decimal ppn = 0;
                decimal ppnCurrency = 0;

                double currencyRate = 1;
                var currencyCode = "IDR";

                decimal totalDebtIDR = 0;
                decimal totalDebt = 0;
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
                    ReceiptDate = item.Date,
                    UPONo = item.UPONo,
                    URNNo = item.URNNo,
                    InvoiceNo = item.InvoiceNo,
                    SupplierName = item.SupplierName,
                    CategoryName = item.CategoryName,
                    AccountingUnitName = accountingUnit.Name,
                    DueDate = item.DueDate,
                    CurrencyCode = currencyCode,
                    TotalSaldo = totalDebt,
                    TotalSaldoIDR = totalDebtIDR,
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
                        .GroupBy(report => new { report.AccountingUnitName })
                        .Select(report => new SummaryDCB()
                        {
                            AccountingUnitName = report.Key.AccountingUnitName,
                            SubTotal = report.Sum(sum => sum.TotalSaldo),
                            SubTotalIDR = report.Sum(sum => sum.TotalSaldoIDR),
                        })
                        .OrderBy(order => order.AccountingUnitName).ToList();
            
            reportResult.CurrencySummaries = reportResult.Reports
                        .GroupBy(report => new { report.CurrencyCode })
                        .Select(report => new SummaryDCB()
                        {
                            CurrencyCode = report.Key.CurrencyCode,
                            SubTotal = report.Sum(sum => sum.TotalSaldo),
                            SubTotalIDR = report.Sum(sum => sum.TotalSaldoIDR)
                        })
                        .OrderBy(order => order.CurrencyCode).ToList();

            reportResult.Reports = reportResult.Reports
                        .GroupBy(
                            key => new 
                            {
                                key.UPONo,
                                key.ReceiptDate,
                                key.URNNo,
                                key.InvoiceNo,
                                key.SupplierName,
                                key.CategoryName,
                                key.AccountingUnitName,
                                key.DueDate,
                                key.CurrencyCode,
                            },
                            val => val,
                            (key, val) => new DetailCreditBalanceReport()
                            {
                                UPONo = key.UPONo,
                                ReceiptDate = key.ReceiptDate,
                                URNNo = key.URNNo,
                                InvoiceNo = key.InvoiceNo,
                                SupplierName = key.SupplierName,
                                CategoryName = key.CategoryName,
                                AccountingUnitName = key.AccountingUnitName,
                                DueDate = key.DueDate,
                                CurrencyCode = key.CurrencyCode,
                                TotalSaldo = val.Sum(s => s.TotalSaldo),
                                TotalSaldoIDR = val.Sum(s => s.TotalSaldoIDR)
                            })
                        .OrderByDescending(order => order.DueDate).ToList();

            //reportResult.Reports = reportResult.Reports
            //            .GroupBy(report => new
            //            {
            //                report.UPONo,
            //                report.ReceiptDate,
            //                report.URNNo,
            //                report.InvoiceNo,
            //                report.SupplierName,
            //                report.CategoryName,
            //                report.AccountingUnitName,
            //                report.DueDate,
            //                report.CurrencyCode,
            //                report.TotalSaldo,
            //                report.TotalSaldoIDR
            //            })
            //            //.GroupBy(group => group.UPONo)
            //            .Select(report => new DetailCreditBalanceReport() 
            //            {
            //                ReceiptDate = report.Key.ReceiptDate,
            //                URNNo = report.Key.URNNo,
            //                InvoiceNo = report.Key.InvoiceNo,
            //                SupplierName = report.Key.SupplierName,
            //                CategoryName = report.Key.CategoryName,
            //                AccountingUnitName = report.Key.AccountingUnitName,
            //                DueDate = report.Key.DueDate,
            //                CurrencyCode = report.Key.CurrencyCode,
            //                TotalSaldo = report.Sum(sum => sum.TotalSaldo),
            //                TotalSaldoIDR = report.Sum(sum => sum.TotalSaldoIDR)
            //            })
            //            .OrderBy(order => order.DueDate).ToList();

            //reportResult.Reports = reportResult.Reports;

            //reportResult.GrandTotal = reportResult.Reports.Sum(sum => sum.Total);
            //reportResult.AccountingUnitSummaryTotal = reportResult.AccountingUnitSummaries.Sum(summary => summary.SubTotal);

            return reportResult;
        }

        public Task<DetailCreditBalanceReportViewModel> GetReport(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency)
        {
            return GetReportData(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);
        }

        public async Task<MemoryStream> GenerateExcel(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency)
        {
            var result = await GetReport(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);
            var reportDataTable = new DataTable();
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal SPB", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No BP", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Jatuh Tempo", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Currency", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Saldo", DataType = typeof(decimal) });

            var accountingUnitDataTable = new DataTable();
            accountingUnitDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            accountingUnitDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

            if (result.Reports.Count > 0)
            {
                foreach (var report in result.Reports)
                {
                    reportDataTable.Rows.Add(report.ReceiptDate.ToString("dd/MM/yyyy"), report.UPONo, report.URNNo, report.InvoiceNo, report.SupplierName, report.CategoryName, report.AccountingUnitName, report.DueDate.ToString("dd/MM/yyyy"), report.CurrencyCode, report.TotalSaldo);
                }
                foreach (var accountingUnitSummary in result.AccountingUnitSummaries)
                    accountingUnitDataTable.Rows.Add(accountingUnitSummary.AccountingUnitName, accountingUnitSummary.SubTotal);

                foreach (var currencySummary in result.CurrencySummaries)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal, currencySummary.SubTotal);
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var sTitle = isImport ? "IMPOR" : isForeignCurrency ? "LOKAL VALAS" : "LOKAL";
                var title = $"LAPORAN SALDO HUTANG (DETAIL) {sTitle}";
                var period = $"Periode sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):dd/MM/yyyy}";

                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].Value = company;
                worksheet.Cells["A2"].Value = title;
                worksheet.Cells["A3"].Value = period;
                worksheet.Cells["A4"].LoadFromDataTable(reportDataTable, true);
                worksheet.Cells[$"A{4 + 3 + result.Reports.Count}"].LoadFromDataTable(accountingUnitDataTable, true);
                worksheet.Cells[$"A{4 + result.Reports.Count + 3 + result.AccountingUnitSummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }

        /*
        public Task<LocalPurchasingBookReportViewModel> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            return GetReportData(no, unit, category, dateFrom, dateTo);
        }   
        */

        /*
        public async Task<MemoryStream> GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            var result = await GetReport(no, unit, category, dateFrom, dateTo);
            var reportDataTable = new DataTable();
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PIB Tanggal", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PIB No", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PIB BM", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPH Impor", DataType = typeof(decimal) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPN Impor", DataType = typeof(decimal) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Ket. Nilai Import", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Rate", DataType = typeof(decimal) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

            var categoryDataTable = new DataTable();
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

            if (result.Reports.Count > 0)
            {
                foreach (var report in result.Reports)
                {
                    reportDataTable.Rows.Add(report.ReceiptDate.ToString("dd/MM/yyyy"), report.SupplierName, report.ProductName, report.IPONo, report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.AccountingCategoryName, report.AccountingUnitName, report.PIBDate.ToString("dd/MM/yyyy"), report.PIBNo, report.PIBBM, report.PIBIncomeTax, report.PIBVat, report.PIBImportInfo, report.CurrencyCode, report.DPP, report.CurrencyRate, report.Total);
                }
                foreach (var categorySummary in result.CategorySummaries)
                    categoryDataTable.Rows.Add(categorySummary.Category, categorySummary.SubTotal);

                foreach (var currencySummary in result.CurrencySummaries)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal, currencySummary.SubTotalCurrency);
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var title = "BUKU PEMBELIAN Import";
                var period = $"Dari {dateFrom.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):dd/MM/yyyy} Sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):dd/MM/yyyy}";

                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].Value = company;
                worksheet.Cells["A2"].Value = title;
                worksheet.Cells["A3"].Value = period;
                worksheet.Cells["A4"].LoadFromDataTable(reportDataTable, true);
                worksheet.Cells[$"A{4 + 3 + result.Reports.Count}"].LoadFromDataTable(categoryDataTable, true);
                worksheet.Cells[$"A{4 + result.Reports.Count + 3 + result.CategorySummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }
        */

        /*
        public async Task<LocalPurchasingBookReportViewModel> GetReportData(string no, string unitCode, string categoryCode, DateTime? dateFrom, DateTime? dateTo)
        {
            var d1 = dateFrom.GetValueOrDefault().ToUniversalTime();
            var d2 = (dateTo.HasValue ? dateTo.Value : DateTime.Now).ToUniversalTime();

            var query = from urnWithItem in dbContext.UnitReceiptNoteItems

                        join pr in dbContext.PurchaseRequests on urnWithItem.PRId equals pr.Id into joinPurchaseRequest
                        from urnPR in joinPurchaseRequest.DefaultIfEmpty()

                        join epoDetail in dbContext.ExternalPurchaseOrderDetails on urnWithItem.EPODetailId equals epoDetail.Id into joinExternalPurchaseOrder
                        from urnEPODetail in joinExternalPurchaseOrder.DefaultIfEmpty()

                        join upoItem in dbContext.UnitPaymentOrderItems on urnWithItem.URNId equals upoItem.URNId into joinUnitPaymentOrder
                        from urnUPOItem in joinUnitPaymentOrder.DefaultIfEmpty()

                        where urnWithItem.UnitReceiptNote.ReceiptDate >= d1 && urnWithItem.UnitReceiptNote.ReceiptDate <= d2 && urnWithItem.UnitReceiptNote.SupplierIsImport
                        select new
                        {
                            urnPR.CategoryCode,
                            urnPR.CategoryName,
                            urnPR.CategoryId,

                            urnWithItem.PRId,
                            urnWithItem.UnitReceiptNote.DOId,
                            urnWithItem.UnitReceiptNote.DONo,
                            urnWithItem.UnitReceiptNote.URNNo,
                            URNId = urnWithItem.UnitReceiptNote.Id,
                            urnWithItem.ProductName,
                            urnWithItem.UnitReceiptNote.ReceiptDate,
                            urnWithItem.UnitReceiptNote.SupplierName,
                            urnWithItem.UnitReceiptNote.SupplierCode,
                            urnWithItem.UnitReceiptNote.UnitCode,
                            urnWithItem.UnitReceiptNote.UnitName,
                            urnWithItem.UnitReceiptNote.UnitId,
                            urnWithItem.EPODetailId,
                            urnWithItem.PricePerDealUnit,
                            urnWithItem.ReceiptQuantity,
                            urnWithItem.Uom,

                            urnEPODetail.ExternalPurchaseOrderItem.PONo,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseVat,
                            EPOPricePerDealUnit = urnEPODetail.PricePerDealUnit,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyCode,

                            InvoiceNo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.InvoiceNo : "",
                            UPONo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.UPONo : "",
                            VatNo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.VatNo : "",
                            PibDate = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.PibDate : new DateTimeOffset(),
                            PibNo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.PibNo : "",
                            ImportDuty = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.ImportDuty : 0,
                            TotalIncomeTaxAmount = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.TotalIncomeTaxAmount : 0,
                            TotalVatAmount = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.TotalVatAmount : 0,
                            ImportInfo = urnUPOItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrder.ImportInfo : "",
                            urnPR.Remark
                        };

            query = query.Where(urn => urn.CurrencyCode != "IDR");

            if (!string.IsNullOrWhiteSpace(no))
                query = query.Where(urn => urn.URNNo == no);

            if (!string.IsNullOrWhiteSpace(unitCode))
                query = query.Where(urn => urn.UnitCode == unitCode);

            if (!string.IsNullOrWhiteSpace(categoryCode))
                query = query.Where(urn => urn.CategoryCode == categoryCode);

            var queryResult = query.OrderByDescending(item => item.ReceiptDate).ToList();
            var currencyTuples = queryResult.GroupBy(item => new { item.CurrencyCode, item.ReceiptDate }).Select(item => new Tuple<string, DateTimeOffset>(item.Key.CurrencyCode, item.Key.ReceiptDate));
            var currencies = await _currencyProvider.GetCurrencyByCurrencyCodeDateList(currencyTuples);

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

            var reportResult = new LocalPurchasingBookReportViewModel();
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

                int.TryParse(item.CategoryId, out var categoryId);
                var category = categories.FirstOrDefault(element => element.Id == categoryId);
                var accountingCategory = new AccountingCategory();
                if (category != null)
                {
                    accountingCategory = accountingCategories.FirstOrDefault(element => element.Id == category.AccountingCategoryId);
                }

                decimal dpp = 0;
                decimal dppCurrency = 0;
                decimal ppn = 0;
                decimal ppnCurrency = 0;

                double currencyRate = 1;
                var currencyCode = "IDR";

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

                var reportItem = new PurchasingReport()
                {
                    CategoryName = item.CategoryName,
                    CategoryCode = item.CategoryCode,
                    AccountingCategoryName = accountingCategory.Name,
                    AccountingCategoryCode = accountingCategory.Code,
                    AccountingLayoutIndex = accountingCategory.AccountingLayoutIndex,
                    CurrencyRate = (decimal)currencyRate,
                    DONo = item.DONo,
                    DPP = dpp,
                    DPPCurrency = dppCurrency,
                    InvoiceNo = item.InvoiceNo,
                    VATNo = item.VatNo,
                    IPONo = item.PONo,
                    VAT = ppn,
                    VATCurrency = ppnCurrency,
                    Total = dpp * (decimal)currencyRate,
                    ProductName = item.ProductName,
                    ReceiptDate = item.ReceiptDate,
                    SupplierCode = item.SupplierCode,
                    SupplierName = item.SupplierName,
                    UnitName = item.UnitName,
                    UnitCode = item.UnitCode,
                    AccountingUnitName = accountingUnit.Name,
                    AccountingUnitCode = accountingUnit.Code,
                    UPONo = item.UPONo,
                    URNNo = item.URNNo,
                    IsUseVat = item.UseVat,
                    CurrencyCode = currencyCode,
                    PIBDate = item.PibDate,
                    PIBNo = item.PibNo,
                    PIBBM = (decimal)item.ImportDuty,
                    PIBIncomeTax = (decimal)item.TotalIncomeTaxAmount,
                    PIBVat = (decimal)item.TotalVatAmount,
                    PIBImportInfo = item.ImportInfo,
                    Remark = item.Remark,
                    Quantity = item.ReceiptQuantity
                };

                reportResult.Reports.Add(reportItem);
            }

            reportResult.CategorySummaries = reportResult.Reports
                        .GroupBy(report => new { report.AccountingCategoryName })
                        .Select(report => new Summary()
                        {
                            Category = report.Key.AccountingCategoryName,
                            SubTotal = report.Sum(sum => sum.Total),
                            AccountingLayoutIndex = report.Select(item => item.AccountingLayoutIndex).FirstOrDefault()
                        }).OrderBy(order => order.AccountingLayoutIndex).ToList();
            reportResult.CurrencySummaries = reportResult.Reports
                .GroupBy(report => new { report.CurrencyCode })
                .Select(report => new Summary()
                {
                    CurrencyCode = report.Key.CurrencyCode,
                    SubTotal = report.Sum(sum => sum.DPP),
                    SubTotalCurrency = report.Sum(sum => sum.Total)
                }).OrderBy(order => order.CurrencyCode).ToList();
            reportResult.Reports = reportResult.Reports;
            reportResult.GrandTotal = reportResult.Reports.Sum(sum => sum.Total);
            reportResult.CategorySummaryTotal = reportResult.CategorySummaries.Sum(categorySummary => categorySummary.SubTotalCurrency);

            return reportResult;
        }
        */
    }

    public interface IDetailCreditBalanceReportFacade
    {
        Task<DetailCreditBalanceReportViewModel> GetReport(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency);
        Task<MemoryStream> GenerateExcel(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency);
        /*
        Task<LocalPurchasingBookReportViewModel> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo);
        Task<MemoryStream> GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo);
        */
    }
}