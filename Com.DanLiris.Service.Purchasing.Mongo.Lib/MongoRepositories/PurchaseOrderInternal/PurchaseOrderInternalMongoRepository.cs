using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using MongoDB.Driver;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories.PurchaseOrderInternal
{
    public class PurchaseOrderInternalMongoRepository : IPurchaseOrderInternalMongoRepository
    {
        private readonly IMongoDbContext _context;

        public PurchaseOrderInternalMongoRepository(IMongoDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<PurchaseOrderMongo>> GetByBatch(int startingNumber, int numberOfBatch)
        {
            return await _context
                            .PurchaseOrderInternals
                            .Find(_ => _.purchaseRequest._createdDate >= new DateTime(2019, 1, 1))
                            .Skip(startingNumber)
                            .Limit(numberOfBatch)
                            .ToListAsync();
        }
    }
}
