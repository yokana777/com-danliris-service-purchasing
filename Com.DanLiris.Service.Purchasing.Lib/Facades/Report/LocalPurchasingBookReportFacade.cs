using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using Microsoft.EntityFrameworkCore;
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
    public class LocalPurchasingBookReportFacade : ILocalPurchasingBookReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitReceiptNote> dbSet;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly string IDRCurrencyCode = "IDR";

        public LocalPurchasingBookReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitReceiptNote>();
            _currencyProvider = (ICurrencyProvider)serviceProvider.GetService(typeof(ICurrencyProvider));
        }

        public async Task<LocalPurchasingBookReportViewModel> GetReportData(string no, string unit, string categoryCode, DateTime? dateFrom, DateTime? dateTo, bool isValas)
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

                        where urnWithItem.UnitReceiptNote.ReceiptDate >= d1 && urnWithItem.UnitReceiptNote.ReceiptDate <= d2 && !urnWithItem.UnitReceiptNote.SupplierIsImport
                        select new
                        {
                            // PR Info
                            urnPR.CategoryCode,
                            urnPR.CategoryName,

                            urnWithItem.PRId,
                            urnWithItem.UnitReceiptNote.DOId,
                            urnWithItem.UnitReceiptNote.DONo,
                            urnWithItem.UnitReceiptNote.URNNo,
                            URNId = urnWithItem.UnitReceiptNote.Id,
                            urnWithItem.ProductName,
                            urnWithItem.UnitReceiptNote.ReceiptDate,
                            urnWithItem.UnitReceiptNote.SupplierName,
                            urnWithItem.UnitReceiptNote.SupplierIsImport,
                            urnWithItem.UnitReceiptNote.UnitCode,
                            urnWithItem.UnitReceiptNote.UnitName,
                            urnWithItem.EPODetailId,
                            urnWithItem.PricePerDealUnit,
                            urnWithItem.ReceiptQuantity,
                            urnWithItem.Uom,

                            // EPO Info
                            urnEPODetail.ExternalPurchaseOrderItem.PONo,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseVat,
                            EPOPricePerDealUnit = urnEPODetail.PricePerDealUnit,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyCode,

                            // UPO Info
                            urnUPOItem.UnitPaymentOrder.InvoiceNo,
                            urnUPOItem.UnitPaymentOrder.UPONo,
                            urnUPOItem.UnitPaymentOrder.VatNo
                        };


            //var query = dbSet
            //    .Where(urn => urn.ReceiptDate >= d1.ToUniversalTime() && urn.ReceiptDate.ToUniversalTime() <= d2 && !urn.SupplierIsImport);

            if (isValas)
            {
                query = query.Where(urn => urn.CurrencyCode != IDRCurrencyCode);

            }
            else
            {
                query = query.Where(urn => urn.CurrencyCode == IDRCurrencyCode);
            }

            if (!string.IsNullOrWhiteSpace(no))
                query = query.Where(urn => urn.URNNo == no);

            if (!string.IsNullOrWhiteSpace(unit))
                query = query.Where(urn => urn.UnitCode == unit);

            //var prIds = query.SelectMany(urn => urn.Items.Select(s => s.PRId)).ToList();

            if (!string.IsNullOrWhiteSpace(categoryCode))
                query = query.Where(urn => urn.CategoryCode == categoryCode);

            var queryResult = query.OrderByDescending(item => item.ReceiptDate).ToList();
            //var currencyCodes = queryResult.Select(item => item.CurrencyCode).ToList();
            var currencyTuples = queryResult.Select(item => new Tuple<string, DateTimeOffset>(item.CurrencyCode, item.ReceiptDate));
            var currencies = await _currencyProvider.GetCurrencyByCurrencyCodeDateList(currencyTuples);

            var reportResult = new LocalPurchasingBookReportViewModel();
            foreach (var item in queryResult)
            {
                //var purchaseRequest = purchaseRequests.FirstOrDefault(f => f.Id.Equals(urnItem.PRId));
                //var unitPaymentOrder = unitPaymentOrders.FirstOrDefault(f => f.URNId.Equals(urnItem.URNId));
                //var epoItem = epoItems.FirstOrDefault(f => f.epoDetailIds.Contains(urnItem.EPODetailId));
                //var epoDetail = epoItem.Details.FirstOrDefault(f => f.Id.Equals(urnItem.EPODetailId));
                var currency = currencies.FirstOrDefault(f => f.Code.Equals(item.CurrencyCode));

                decimal dpp = 0;
                decimal dppCurrency = 0;
                decimal ppn = 0;

                //default IDR
                double currencyRate = 1;
                var currencyCode = "IDR";
                if (currency != null && !currency.Code.Equals("IDR"))
                {
                    dppCurrency = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);
                    currencyRate = currency.Rate.GetValueOrDefault();
                    currencyCode = currency.Code;
                }
                else
                    dpp = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);

                if (item.UseVat)
                    ppn = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity * 0.1);



                var reportItem = new PurchasingReport()
                {
                    CategoryName = item.CategoryName,
                    CategoryCode = item.CategoryCode,
                    CurrencyRate = (decimal)currencyRate,
                    DONo = item.DONo,
                    DPP = dpp,
                    DPPCurrency = dppCurrency,
                    InvoiceNo = item.InvoiceNo,
                    VATNo = item.VatNo,
                    IPONo = item.PONo,
                    VAT = ppn,
                    Total = (dpp + dppCurrency + ppn) * (decimal)currencyRate,
                    ProductName = item.ProductName,
                    ReceiptDate = item.ReceiptDate,
                    SupplierName = item.SupplierName,
                    UnitName = item.UnitName,
                    UPONo = item.UPONo,
                    URNNo = item.URNNo,
                    IsUseVat = item.UseVat,
                    CurrencyCode = currencyCode,
                    Quantity = item.ReceiptQuantity,
                    Uom = item.Uom
                };

                reportResult.Reports.Add(reportItem);
            }

            reportResult.CategorySummaries = reportResult.Reports
                        .GroupBy(report => new { report.CategoryCode })
                        .Select(report => new Summary()
                        {
                            Category = report.Key.CategoryCode,
                            SubTotal = report.Sum(sum => sum.Total)
                        }).OrderBy(order => order.Category).ToList();
            reportResult.CurrencySummaries = reportResult.Reports
                .GroupBy(report => new { report.CurrencyCode })
                .Select(report => new Summary()
                {
                    CurrencyCode = report.Key.CurrencyCode,
                    SubTotal = report.Sum(sum => sum.DPP + sum.DPPCurrency + sum.VAT)
                }).OrderBy(order => order.CurrencyCode).ToList();
            reportResult.Reports = reportResult.Reports;
            reportResult.GrandTotal = reportResult.Reports.Sum(sum => sum.Total);
            reportResult.CategorySummaryTotal = reportResult.CategorySummaries.Sum(categorySummary => categorySummary.SubTotal);

            #region Old Query
            //if (prIds.Count > 0)
            //{
            //    var purchaseRequestQuery = dbContext.PurchaseRequests.AsQueryable();


            //    if (purchaseRequestQuery.Count() > 0)
            //    {
            //        //var purchaseRequests = purchaseRequestQuery.Select(pr => new { pr.Id, pr.CategoryName, pr.CategoryCode }).ToList();
            //        //prIds = purchaseRequests.Select(pr => pr.Id).ToList();
            //        //var categories = purchaseRequests.Select(pr => pr.CategoryCode).Distinct().ToList();

            //        //var urnIds = query.Select(urn => urn.Id).ToList();
            //        //var urnItems = dbContext.UnitReceiptNoteItems
            //        //    .Include(urnItem => urnItem.UnitReceiptNote)
            //        //    .Where(urnItem => urnIds.Contains(urnItem.URNId) && prIds.Contains(urnItem.PRId))
            //        //    .Select(urnItem => new
            //        //    {
            //        //        urnItem.PRId,
            //        //        urnItem.UnitReceiptNote.DOId,
            //        //        urnItem.UnitReceiptNote.DONo,
            //        //        urnItem.UnitReceiptNote.URNNo,
            //        //        URNId = urnItem.UnitReceiptNote.Id,
            //        //        urnItem.ProductName,
            //        //        urnItem.UnitReceiptNote.ReceiptDate,
            //        //        urnItem.UnitReceiptNote.SupplierName,
            //        //        urnItem.UnitReceiptNote.UnitCode,
            //        //        urnItem.EPODetailId,
            //        //        urnItem.PricePerDealUnit,
            //        //        urnItem.ReceiptQuantity,
            //        //        urnItem.Uom
            //        //    })
            //        //    .ToList();

            //        //var epoDetailIds = urnItems.Select(urnItem => urnItem.EPODetailId).ToList();
            //        //var epoItemIds = dbContext.ExternalPurchaseOrderDetails
            //        //    .Include(epoDetail => epoDetail.ExternalPurchaseOrderItem)
            //        //    .Where(epoDetail => epoDetailIds.Contains(epoDetail.Id))
            //        //    .Select(epoDetail => epoDetail.ExternalPurchaseOrderItem.Id)
            //        //    .ToList();
            //        var epoItems = dbContext.ExternalPurchaseOrderItems
            //            .Include(epoItem => epoItem.ExternalPurchaseOrder)
            //            .Where(epoItem => epoItemIds.Contains(epoItem.Id))
            //            .Select(epoItem => new
            //            {
            //                epoItem.PONo,
            //                epoDetailIds = epoItem.Details.Select(epoDetail => epoDetail.Id).ToList(),
            //                epoItem.ExternalPurchaseOrder.CurrencyCode,
            //                epoItem.ExternalPurchaseOrder.UseVat,
            //                Details = epoItem.Details.Select(epoDetail => new { epoDetail.PricePerDealUnit, epoDetail.Id }).ToList()
            //            })
            //            .ToList();

            //        var unitPaymentOrders = dbContext.UnitPaymentOrderItems
            //            .Include(upoItem => upoItem.UnitPaymentOrder)
            //            .Where(upoItem => urnIds.Contains(upoItem.URNId))
            //            .Select(upoItem => new
            //            {
            //                upoItem.URNId,
            //                upoItem.UnitPaymentOrder.InvoiceNo,
            //                upoItem.UnitPaymentOrder.UPONo,
            //                upoItem.UnitPaymentOrder.VatNo
            //            });

            //        var currencyCodes = epoItems.Select(epoItem => epoItem.CurrencyCode).Distinct().ToList();
            //        var currencies = await _currencyProvider.GetCurrencyByCurrencyCodeList(currencyCodes);

            //        var reportResult = new LocalPurchasingBookReportViewModel();
            //        foreach (var urnItem in urnItems)
            //        {
            //            var purchaseRequest = purchaseRequests.FirstOrDefault(f => f.Id.Equals(urnItem.PRId));
            //            var unitPaymentOrder = unitPaymentOrders.FirstOrDefault(f => f.URNId.Equals(urnItem.URNId));
            //            var epoItem = epoItems.FirstOrDefault(f => f.epoDetailIds.Contains(urnItem.EPODetailId));
            //            var epoDetail = epoItem.Details.FirstOrDefault(f => f.Id.Equals(urnItem.EPODetailId));
            //            var currency = currencies.FirstOrDefault(f => f.Code.Equals(epoItem.CurrencyCode));

            //            decimal dpp = 0;
            //            decimal dppCurrency = 0;
            //            decimal ppn = 0;

            //            //default IDR
            //            double currencyRate = 1;
            //            var currencyCode = "IDR";
            //            if (currency != null && !currency.Code.Equals("IDR"))
            //            {
            //                dppCurrency = (decimal)(epoDetail.PricePerDealUnit * urnItem.ReceiptQuantity);
            //                currencyRate = currency.Rate.GetValueOrDefault();
            //                currencyCode = currency.Code;
            //            }
            //            else
            //                dpp = (decimal)(epoDetail.PricePerDealUnit * urnItem.ReceiptQuantity);

            //            if (epoItem.UseVat)
            //                ppn = (decimal)(epoDetail.PricePerDealUnit * urnItem.ReceiptQuantity * 0.1);



            //            var reportItem = new PurchasingReport()
            //            {
            //                CategoryName = purchaseRequest.CategoryName,
            //                CategoryCode = purchaseRequest.CategoryCode,
            //                CurrencyRate = (decimal)currencyRate,
            //                DONo = urnItem.DONo,
            //                DPP = dpp,
            //                DPPCurrency = dppCurrency,
            //                InvoiceNo = unitPaymentOrder?.InvoiceNo,
            //                VATNo = unitPaymentOrder?.VatNo,
            //                IPONo = epoItem.PONo,
            //                VAT = ppn,
            //                Total = (dpp + dppCurrency + ppn) * (decimal)currencyRate,
            //                ProductName = urnItem.ProductName,
            //                ReceiptDate = urnItem.ReceiptDate,
            //                SupplierName = urnItem.SupplierName,
            //                UnitName = urnItem.UnitCode,
            //                UPONo = unitPaymentOrder?.UPONo,
            //                URNNo = urnItem.URNNo,
            //                IsUseVat = epoItem.UseVat,
            //                CurrencyCode = currencyCode,
            //                Quantity = urnItem.ReceiptQuantity,
            //                Uom = urnItem.Uom
            //            };

            //            reportResult.Reports.Add(reportItem);
            //        }

            //        reportResult.CategorySummaries = reportResult.Reports
            //            .GroupBy(report => new { report.CategoryCode })
            //            .Select(report => new Summary()
            //            {
            //                Category = report.Key.CategoryCode,
            //                SubTotal = report.Sum(sum => sum.Total)
            //            }).OrderBy(order => order.Category).ToList();
            //        reportResult.CurrencySummaries = reportResult.Reports
            //            .GroupBy(report => new { report.CurrencyCode })
            //            .Select(report => new Summary()
            //            {
            //                CurrencyCode = report.Key.CurrencyCode,
            //                SubTotal = report.Sum(sum => sum.DPP + sum.DPPCurrency + sum.VAT)
            //            }).OrderBy(order => order.CurrencyCode).ToList();
            //        reportResult.Reports = reportResult.Reports.OrderByDescending(order => order.ReceiptDate).ToList();
            //        reportResult.GrandTotal = reportResult.Reports.Sum(sum => sum.Total);
            //        reportResult.CategorySummaryTotal = reportResult.CategorySummaries.Sum(categorySummary => categorySummary.SubTotal);

            //        return reportResult;
            //    }
            //}
            #endregion

            return reportResult;
        }

        public Task<LocalPurchasingBookReportViewModel> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo, bool isValas)
        {
            return GetReportData(no, unit, category, dateFrom, dateTo, isValas);
        }

        private DataTable GetFormatReportExcel(bool isValas)
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });

            if (isValas)
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "Kurs", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
            }
            else
            {

                dt.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
            }

            dt.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            return dt;
        }

        public async Task<MemoryStream> GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo, bool isValas)
        {
            var result = await GetReport(no, unit, category, dateFrom, dateTo, isValas);
            //var Data = reportResult.Reports;
            var reportDataTable = GetFormatReportExcel(isValas);
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            var categoryDataTable = new DataTable();
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            if (result.Reports.Count > 0)
            {
                foreach (var report in result.Reports) { 
                    if (isValas)
                    {
                        reportDataTable.Rows.Add(report.ReceiptDate.ToString("dd/MM/yyyy"), report.SupplierName, report.ProductName, report.IPONo, report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CategoryCode + " - " + report.CategoryName, report.UnitName, report.CurrencyRate, report.DPPCurrency, report.VAT, report.Total);

                    }
                    else
                    {
                        reportDataTable.Rows.Add(report.ReceiptDate.ToString("dd/MM/yyyy"), report.SupplierName, report.ProductName, report.IPONo, report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CategoryCode + " - " + report.CategoryName, report.UnitName, report.DPP, report.VAT, report.Total);

                    }
                }
                foreach (var categorySummary in result.CategorySummaries)
                    categoryDataTable.Rows.Add(categorySummary.Category, categorySummary.SubTotal);

                foreach (var currencySummary in result.CurrencySummaries)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal);
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].LoadFromDataTable(reportDataTable, true);
                worksheet.Cells[$"A{1 + 3 + result.Reports.Count}"].LoadFromDataTable(categoryDataTable, true);
                worksheet.Cells[$"A{1 + result.Reports.Count + 3 + result.CategorySummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }

    }

    public interface ILocalPurchasingBookReportFacade
    {
        Task<LocalPurchasingBookReportViewModel> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo, bool isValas);
        Task<MemoryStream> GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo, bool isValas);
    }
}