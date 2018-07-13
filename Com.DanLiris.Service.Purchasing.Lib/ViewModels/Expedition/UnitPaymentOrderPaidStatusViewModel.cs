using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition
{
    public class UnitPaymentOrderPaidStatusViewModel
    {
        public string UnitPaymentOrderNo { get; set; }
        public DateTimeOffset UPODate { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierName { get; set; }
        public string DivisionName { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public double DPP { get; set; }
        public double IncomeTax { get; set; }
        public double Vat { get; set; }
        public double TotalPaid { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset? BankExpenditureNotePPHDate { get; set; }
        public string PPHBank { get; set; }
        public DateTimeOffset? BankExpenditureNoteDate { get; set; }
        public string Bank { get; set; }
    }
}
