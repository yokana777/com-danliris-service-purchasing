using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel
{
    public class UnitPaymentCorrectionNote : BaseModel
    {
        public string UPCNo { get; set; }
        public DateTimeOffset CorrectionDate { get; set; }
        public string CorrectionType { get; set; }
        public long UPOId { get; set; }
        public string UPONo { get; set; }
        public string SupplierId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceCorrectionNo { get; set; }
        public DateTimeOffset InvoiceCorrectionDate { get; set; }
        public bool useVat { get; set; }
        public string VatTaxCorrectionNo { get; set; }
        public DateTimeOffset VatTaxCorrectionDate { get; set; }
        public bool useIncomeTax { get; set; }
        public string IncomeTaxCorrectionNo { get; set; }
        public string IncomeTaxCorrectionName { get; set; }
        public string ReleaseOrderNoteNo { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string Remark { get; set; }
        public string ReturNoteNo { get; set; }
        public virtual ICollection<UnitPaymentCorrectionNoteItem> Items { get; set; }
    }
}
