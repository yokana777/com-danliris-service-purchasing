using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote
{
    public class DetailCreditBalanceReportViewModel
    {
        public DetailCreditBalanceReportViewModel()
        {
            Reports = new List<DetailCreditBalanceReport>();
            AccountingUnitSummaries = new List<SummaryDCB>();
            //CurrencySummaries = new List<SummaryDCB>();
        }
        public List<DetailCreditBalanceReport> Reports { get; set; }
        public List<SummaryDCB> AccountingUnitSummaries { get; set; }
        public List<SummaryDCB> CurrencySummaries { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal AccountingUnitSummaryTotal { get; set; }
    }

    public class SummaryDCB
    {
        public string AccountingUnitName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SubTotalIDR { get; set; }
        public int AccountingLayoutIndex { get; set; }
    }

    public class DetailCreditBalanceReport
    {
        public DateTimeOffset? UPODate { get; set; }
        public string UPONo { get; set; }
        public string URNNo { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierName { get; set; }
        public string CategoryName { get; set; }
        public string AccountingUnitName { get; internal set; }
        public DateTimeOffset? DueDate { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Total { get; set; }
        public decimal TotalIDR { get; set; }
        public string CategoryId { get; set; }
    }
}