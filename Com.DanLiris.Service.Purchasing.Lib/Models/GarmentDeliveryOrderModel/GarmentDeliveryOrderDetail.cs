using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel
{
    public class GarmentDeliveryOrderDetail : StandardEntity<long>
    {
        public long EPODetailId { get; set; }
        public long POItemId { get; set; }
        public long PRId { get; set; }
        [MaxLength(255)]
        public string PRNo { get; set; }
        public long PRItemId { get; set; }
        public string POSerialNumber { get; set; }

        /* Unit */
        [MaxLength(255)]
        public string UnitId { get; set; }
        [MaxLength(255)]
        public string UnitCode { get; set; }

        /* Product */
        [MaxLength(255)]
        public string ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(1000)]
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }

        public double DOQuantity { get; set; }
        public double DealQuantity { get; set; }

        public double Conversion { get; set; }

        /* UOM */
        [MaxLength(255)]
        public string UomId { get; set; }
        [MaxLength(1000)]
        public string UomUnit { get; set; }

        public double SmallQuantity { get; set; }
        public string SmallUomId { get; set; }
        public string SmallUomUnit { get; set; }

        public double PricePerDealUnit { get; set; }
        public double PriceTotal { get; set; }

        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }

        //public double ReceiptQuantity { get; set; }
        public bool IsClosed { get; set; }

        public virtual long GarmentDOItemId { get; set; }
        [ForeignKey("DOItemId")]
        public virtual GarmentDeliveryOrderItem GarmentDeliveryOrderItem { get; set; }
    }
}
