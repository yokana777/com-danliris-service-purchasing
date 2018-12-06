using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel
{
    public class GarmentUnitReceiptNoteItem : StandardEntity<long>
    {
        public long URNId { get; set; }
        [ForeignKey("URNId")]
        public virtual GarmentUnitReceiptNote GarmentUnitReceiptNote { get; set; }

        public long DODetailId { get; set; }

        public long EPOItemId { get; set; }

        public long PRId { get; set; }
        [MaxLength(255)]
        public string PRNo { get; set; }
        public long PRItemId { get; set; }

        public long POId { get; set; }
        public long POItemId { get; set; }
        [MaxLength(1000)]
        public string POSerialNumber { get; set; }

        public long ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(1000)]
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }

        [MaxLength(255)]
        public string RONo { get; set; }

        public decimal ReceiptQuantity { get; set; }

        public long UomId { get; set; }
        [MaxLength(1000)]
        public string UomUnit { get; set; }

        public decimal PricePerDealUnit { get; set; }

        [MaxLength(1000)]
        public string DesignColor { get; set; }

        public bool IsCorrection { get; set; }

        public decimal Conversion { get; set; }

        public decimal SmallQuantity { get; set; }

        public decimal ReceiptCorrection { get; set; }

        public decimal OrderQuantity { get; set; }

        public long SmallUomId { get; set; }
        [MaxLength(1000)]
        public string SmallUomUnit { get; set; }
    }
}
