using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel
{
    public class InternalPurchaseOrderItem : StandardEntity<long>
    {
        [MaxLength(255)]
        public string PRDetailId { get; set; }

        /*Product*/
        [MaxLength(255)]
        public string ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(4000)]
        public string ProductName { get; set; }
        [MaxLength(255)]
        public string UomId { get; set; }
        [MaxLength(255)]
        public string UomUnit { get; set; }


        public long Quantity { get; set; }
        [MaxLength(1000)]
        public string ProductRemark { get; set; }
        [MaxLength(255)]
        public string Status { get; set; }

        public virtual long POId { get; set; }
        [ForeignKey("POId")]
        public virtual InternalPurchaseOrder InternalPurchaseOrder { get; set;}
    }
}
