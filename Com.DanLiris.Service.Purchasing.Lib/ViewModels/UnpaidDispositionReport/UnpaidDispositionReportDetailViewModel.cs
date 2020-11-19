using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnpaidDispositionReport
{
    public class UnpaidDispositionReportDetailViewModel
    {
        public UnpaidDispositionReportDetailViewModel()
        {
            Reports = new List<DispositionReport>();
            UnitSummaries = new List<UnitSummary>();
        }
        public List<DispositionReport> Reports { get; set; }
        public List<UnitSummary> UnitSummaries { get; set; }
        public double GrandTotal { get; set; }
        public double UnitSummaryTotal { get; set; }
    }

    public class UnitSummary
    {
        public string Unit { get; set; }
        public string UnitCode { get; set; }
        public double SubTotal { get; set; }
        public double SubTotalCurrency { get; set; }
        public int AccountingLayoutIndex { get; set; }
    }

    public class DispositionReport
    {
        public string URNNo { get; set; }
        public string InvoiceNo { get; set; }
        public double DPP { get; set; }
        public double DPPCurrency { get; set; }
        public double VAT { get; set; }
        public double Total { get; set; }
        public double TotalCurrency { get; set; }
        public string SupplierName { get; set; }
        public string UPONo { get; set; }
        public double IncomeTax { get; set; }
        public string DispositionNo { get; set; }
        public DateTimeOffset? DispositionDate { get; set; }
        public DateTimeOffset? PaymentDueDate { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public double CurrencyRate { get; set; }
        public string CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string UnitId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string AccountingCategoryName { get; internal set; }
        public string AccountingCategoryCode { get; internal set; }
        public string AccountingUnitName { get; internal set; }
        public string AccountingUnitCode { get; internal set; }
        public int AccountingLayoutIndex { get; set; }
        public string IncomeTaxBy { get; set; }
    }
}
