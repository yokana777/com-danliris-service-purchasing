using MongoDB.Bson;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class UnitMongo : MongoBaseModel
    {
        public string code { get; set; }
        public ObjectId divisionId { get; set; }
        public DivisionMongo division { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}