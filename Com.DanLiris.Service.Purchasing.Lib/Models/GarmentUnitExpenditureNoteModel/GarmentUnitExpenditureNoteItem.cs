using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel
{
    public class GarmentUnitExpenditureNoteItem : StandardEntity<long>
    {
        public long UENId { get; set; }
        [ForeignKey("UENId")]
        public virtual GarmentUnitExpenditureNote GarmentUnitExpenditureNote { get; set; }

        public long UnitDOItemId { get; set; }
        public long URNItemId { get; set; }
        public long DODetailId { get; set; }
        public long EPOItemId { get; set; }
        public long POItemId { get; set; }
        public long PRItemId { get; set; }
        public string POSerialNumber { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }
        public string RONo { get; set; }
        public double Quantity { get; set; }
        public long UomId { get; set; }
        public string UomUnit { get; set; }
        public double PricePerDealUnit { get; set; }
        public string FabricType{ get; set; }
        public long BuyerId { get; set; }
        public string BuyerCode { get; set; }
        public double? DOCurrencyRate { get; set; }
        [Column(TypeName = "decimal(38, 20)")]
        public decimal Conversion { get; set; }
        [Column(TypeName = "decimal(38, 4)")]
        public decimal BasicPrice { get; set; }

        [NotMapped]
        public bool IsSave { get; set; }
    }
}
