namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class CurrencyMongo : MongoBaseModel
    {
        public string code { get; set; }
        public string symbol { get; set; }
        public double rate { get; set; }
        public string description { get; set; }
    }
}