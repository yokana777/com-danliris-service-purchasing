namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class CategoryMongo : MongoBaseModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string codeRequirement { get; set; }
    }
}