using MongoDB.Bson;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class PurchaseRequestItemMongo : MongoBaseModel
    {
        public ObjectId productId { get; set; }
        public ProductMongo product { get; set; }
        public double quantity { get; set; }
        public List<object> deliveryOrderNos { get; set; }
        public List<object> purchaseOrderIds { get; set; }
        public string remark { get; set; }
    }
}