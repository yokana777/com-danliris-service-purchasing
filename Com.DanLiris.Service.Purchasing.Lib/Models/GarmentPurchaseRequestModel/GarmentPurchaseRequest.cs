using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel
{
    public class GarmentPurchaseRequest : BaseModel
    {
        [MaxLength(255)]
        public string PRNo { get; set; }
        public string RONo { get; set; }

        [MaxLength(255)]
        public string BuyerId { get; set; }
        [MaxLength(255)]
        public string BuyerCode { get; set; }
        [MaxLength(1000)]
        public string BuyerName { get; set; }

        [MaxLength(255)]
        public string Article { get; set; }

        public DateTimeOffset Date { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public DateTimeOffset ShipmentDate { get; set; }

        [MaxLength(255)]
        public string UnitId { get; set; }
        [MaxLength(255)]
        public string UnitCode { get; set; }
        [MaxLength(1000)]
        public string UnitName { get; set; }

        public bool IsPosted { get; set; }
        public bool IsUsed { get; set; }
        public string Remark { get; set; }

        public virtual ICollection<GarmentPurchaseRequestItem> Items { get; set; }
    }
}
