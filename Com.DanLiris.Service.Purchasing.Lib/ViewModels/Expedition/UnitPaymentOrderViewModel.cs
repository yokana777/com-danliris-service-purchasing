using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition
{
    public class UnitPaymentOrderViewModel
    {
        public string No { get; set; }
        public DateTimeOffset UPODate { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public double TotalPaid { get; set; }
        public string Currency { get; set; }
    }
}
