using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories
{
    public class PurchaseRequestMongoRepository : IPurchaseRequestMongoRepository
    {
        private readonly IMongoDbContext _context;

        public PurchaseRequestMongoRepository(IMongoDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<PurchaseRequestMongo>> GetByBatch(int startingNumber, int numberOfBatch)
        {
            return await _context
                            .PurchaseRequests
                            .Find(_ => _._createdDate >= new DateTime(2019, 1, 1))
                            .Skip(startingNumber)
                            .Limit(numberOfBatch)
                            .ToListAsync();
        }
    }
}
