using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel
{
    public class GarmentInternNoteDetail : StandardEntity<long>
    {
        public long INDetailId { get; set; }
        public string DONo { get; set; }
        public long EPONo { get; set; }
        public string POSerialNumber { get; set; }
        [MaxLength(255)]
        public string RONo { get; set; }
        public string TermOfPayment { get; set; }
        public string PaymentType { get; set; }

        /*Product*/
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(255)]
        public string ProductId { get; set; }
        [MaxLength(255)]
        public string ProductName { get; set; }

        public long Quantity { get; set; }

        /* Unit */
        [MaxLength(255)]
        public string UnitId { get; set; }
        [MaxLength(255)]
        public string UnitCode { get; set; }
        [MaxLength(255)]
        public string UnitName { get; set; }
        
        public double PricePerDealUnit { get; set; }
        public double PriceTotal { get; set; }

        public virtual long GarmentDOId { get; set; }

        public virtual long GarmentINDetailId { get; set; }
        [ForeignKey("INItemId")]
        public virtual GarmentInternNoteItem InternNoteItem { get; set; }
    }
}
