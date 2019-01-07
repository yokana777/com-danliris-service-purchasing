using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class PurchaseRequestMongo : MongoBaseModel
    {
        public string no { get; set; }
        public DateTimeOffset date { get; set; }
        public DateTimeOffset expectedDeliveryDate { get; set; }
        public ObjectId budgetId { get; set; }
        public BudgetMongo budget { get; set; }
        public ObjectId unitId { get; set; }
        public UnitMongo unit { get; set; }
        public ObjectId categoryId { get; set; }
        public CategoryMongo category { get; set; }
        public bool isPosted { get; set; }
        public bool isUsed { get; set; }
        public string remark { get; set; }
        public List<PurchaseRequestItemMongo> items { get; set; }
        public bool @internal { get; set; }
        public Status status { get; set; }
        public List<object> purchaseOrderIds { get; set; }
    }

    public class Status
    {
        public string name { get; set; }
        public double value { get; set; }
        public string label { get; set; }
    }
}
