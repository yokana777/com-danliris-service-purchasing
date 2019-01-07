using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel
{
    public class GarmentUnitDeliveryOrderItem : StandardEntity<long>
    {
        public long URNId { get; set; }
        public string URNNo { get; set; }
        public long URNItemId { get; set; }
        public long DODetailId { get; set; }
        public long EPOItemId { get; set; }
        public long POItemId { get; set; }
        public long PRItemId { get; set; }
        public string POSerialNumber { get; set; }

        /*Product*/
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }

        public string RONo { get; set; }
        public double Quantity { get; set; }

        /*UOM*/
        public long UomId { get; set; }
        public string UomUnit { get; set; }

        public double PricePerDealUnit { get; set; }
        public string FabricType { get; set; }

        public string DesignColor { get; set; }

        public virtual long UnitDOId { get; set; }
        [ForeignKey("UnitDOId")]
        public virtual GarmentUnitDeliveryOrder GarmentUnitDeliveryOrder { get; set; }
    }
}


