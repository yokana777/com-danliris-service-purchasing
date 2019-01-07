using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using Com.Moonlay.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel
{
    public class PurchaseRequestItem : StandardEntity<long>
    {
        public PurchaseRequestItem()
        {
        }

        public PurchaseRequestItem(PurchaseRequestItemMongo mongoPurchaseRequestItem)
        {
            Active = mongoPurchaseRequestItem._active;
            CreatedAgent = mongoPurchaseRequestItem._createAgent;
            CreatedBy = mongoPurchaseRequestItem._createdBy;
            CreatedUtc = mongoPurchaseRequestItem._createdDate;
            DeletedAgent = mongoPurchaseRequestItem._deleted ? mongoPurchaseRequestItem._updateAgent : "";
            DeletedBy = mongoPurchaseRequestItem._deleted ? mongoPurchaseRequestItem._updatedBy : "";
            DeletedUtc = mongoPurchaseRequestItem._deleted ? mongoPurchaseRequestItem._updatedDate : DateTime.MinValue;
            IsDeleted = mongoPurchaseRequestItem._deleted;
            LastModifiedAgent = mongoPurchaseRequestItem._updateAgent;
            LastModifiedBy = mongoPurchaseRequestItem._updatedBy;
            LastModifiedUtc = mongoPurchaseRequestItem._updatedDate;
            ProductCode = mongoPurchaseRequestItem.product.code;
            ProductName = mongoPurchaseRequestItem.product.name;
            Quantity = mongoPurchaseRequestItem.quantity;
            Remark = mongoPurchaseRequestItem.remark;
            Uom = mongoPurchaseRequestItem.product.uom.unit;
        }

        /* Product */
        [MaxLength(255)]
        public string ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(4000)]
        public string ProductName { get; set; }
        [MaxLength(255)]
        public string Uom { get; set; }
        public string UomId { get; set; }
        public string Status { get; set; }

        public double Quantity { get; set; }
        public string Remark { get; set; }

        public virtual long PurchaseRequestId { get; set; }
        [ForeignKey("PurchaseRequestId")]
        public virtual PurchaseRequest PurchaseRequest { get; set; }
    }
}
