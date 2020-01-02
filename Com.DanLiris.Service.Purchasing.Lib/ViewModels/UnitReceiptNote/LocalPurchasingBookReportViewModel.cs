using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote
{
    public class LocalPurchasingBookReportViewModel
    {
        public LocalPurchasingBookReportViewModel()
        {
            Reports = new List<PurchasingReport>();
            CategorySummaries = new List<Summary>();
            CurrencySummaries = new List<Summary>();
        }
        public List<PurchasingReport> Reports { get; set; }
        public List<Summary> CategorySummaries { get; set; }
        public List<Summary> CurrencySummaries { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal CategorySummaryTotal { get; set; }
    }

    public class Summary
    {
        public string Category { get; set; }
        public string CurrencyCode { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class PurchasingReport
    {
        public DateTimeOffset ReceiptDate { get; set; }
        public string URNNo { get; set; }
        public string ProductName { get; set; }
        public string InvoiceNo { get; set; }
        public string CategoryName { get; set; }
        public string UnitName { get; set; }
        public decimal DPP { get; set; }
        public decimal DPPCurrency { get; set; }
        public decimal VAT { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal Total { get; set; }
        public bool IsUseVat { get; set; }
        public string SupplierName { get; set; }
        public string IPONo { get; set; }
        public string DONo { get; set; }
        public string UPONo { get; set; }
        public string CurrencyCode { get; set; }
        public string CategoryCode { get; set; }
        public string VATNo { get; set; }
        public double Quantity { get; set; }
        public string Uom { get; set; }
        public string PIBNo { get; set; }
        public decimal PIBBM { get; set; }
    }
}