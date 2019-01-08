using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories.PurchaseOrderExternal
{
    public interface IPurchaseOrderExternalMongoRepository
    {
        Task<IEnumerable<PurchaseOrderExternalMongo>> GetByBatch(int startingNumber, int numberOfBatch);
    }
}
