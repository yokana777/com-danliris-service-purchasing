namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class DivisionMongo : MongoBaseModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}