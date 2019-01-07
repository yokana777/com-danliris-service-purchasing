using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories
{
    public interface IPurchaseRequestMongoRepository
    {
        Task<IEnumerable<PurchaseRequestMongo>> GetByBatch(int startingNumber, int numberOfBatch);
    }
}
