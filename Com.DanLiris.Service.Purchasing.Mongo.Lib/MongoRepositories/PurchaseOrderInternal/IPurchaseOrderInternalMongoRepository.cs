using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories.PurchaseOrderInternal
{
    public interface IPurchaseOrderInternalMongoRepository
    {
        Task<IEnumerable<PurchaseOrderMongo>> GetByBatch(int startingNumber, int numberOfBatch);
    }
}
