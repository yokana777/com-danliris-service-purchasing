using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel
{
    class InternalPurchaseOrderItem : BaseModel
    {
        [MaxLength(255)]
        public string POId { get; set; }
        [MaxLength(255)]
        public string PRDetailId { get; set; }

        /*Product*/
        [MaxLength(255)]
        public string ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(4000)]
        public string ProductName { get; set; }

        public long Quantity { get; set; }
        [MaxLength(255)]
        public string UomId { get; set; }
        [MaxLength(255)]
        public string UomUnit { get; set; }
        [MaxLength(1000)]
        public string ProductRemark { get; set; }
        [MaxLength(255)]
        public string Stauts { get; set; }

        public virtual long InternalPurchaseOrderId { get; set; }
        [ForeignKey("InternalPurchaseOrderId")]
        public virtual InternalPurchaseOrder InternalPurchaseOrder { get; set;}
    }
}
