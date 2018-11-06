using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel
{
    public class PurchasingDisposition : BaseModel
    {
        public long SupplierId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string Bank { get; set; }
        public string ConfirmationOrderNo { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentMethod { get; set; }
        public DateTimeOffset PaymentDueDate { get; set; }
        public string Calculation { get; set; }
        public string Remark { get; set; }
        public string ProformaNo { get; set; }
        public string Investation { get; set; }
        public double Amount { get; set; }
        public virtual ICollection<PurchasingDispositionItem> Items { get; set; }
    }
}
