using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel
{
    public class PurchasingDispositionItem : BaseModel
    {
        [MaxLength(255)]
        public string EPONo { get; set; }
        public long EPOId { get; set; }
        public string UnitName { get; set; }
        public string UnitCode { get; set; }
        public long UnitId { get; set; }
        public bool UseVat { get; set; }
        public bool UseIncomeTax { get; set; }
        public long IncomeTaxId { get; set; }
        public string IncomeTaxName { get; set; }
        public double IncomeTaxRate { get; set; }
    

        public virtual long PurchasingDispositionId { get; set; }
        [ForeignKey("PurchasingDispositionId")]
        public virtual PurchasingDisposition PurchasingDisposition { get; set; }

        public virtual ICollection<PurchasingDispositionDetail> Details { get; set; }
    }
}
