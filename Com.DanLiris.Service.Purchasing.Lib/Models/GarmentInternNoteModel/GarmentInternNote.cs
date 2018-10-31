using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel
{
    public class GarmentInternNote : BaseModel
    {
        public string INNo { get; set; }
        public string Remark { get; set; }
        public DateTimeOffset INDate { get; set; }
        public string InvoiceNoteNo { get; set; }
        public string HasUnitReceiptNote { get; set; }

        /*Currency*/
        [MaxLength(255)]
        public string CurrencyId { get; set; }
        [MaxLength(255)]
        public string CurrencyCode { get; set; }
        [MaxLength(1000)]
        public double CurrencyRate { get; set; }

        /*Supplier*/
        [MaxLength(255)]
        public string SupplierId { get; set; }
        [MaxLength(255)]
        public string SupplierCode { get; set; }
        [MaxLength(1000)]
        public string SupplierName { get; set; }
        public virtual ICollection<GarmentInternNoteItem> Items { get; set; }

    }
}
