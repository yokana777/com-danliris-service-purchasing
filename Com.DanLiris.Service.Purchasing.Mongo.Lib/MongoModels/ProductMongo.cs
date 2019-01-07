using MongoDB.Bson;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class ProductMongo : MongoBaseModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public CurrencyMongo currency { get; set; }
        public string description { get; set; }
        public ObjectId uomId { get; set; }
        public UomMongo uom { get; set; }
        public List<object> properties { get; set; }
        public ObjectId currencyId { get; set; }
    }
}