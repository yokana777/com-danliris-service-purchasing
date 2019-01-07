using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using MongoDB.Driver;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib
{
    public interface IMongoDbContext
    {
        IMongoCollection<PurchaseRequestMongo> PurchaseRequests { get; }
    }
}
