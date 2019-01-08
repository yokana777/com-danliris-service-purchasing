using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories.PurchaseOrderExternal
{
    public class PurchaseOrderExternalMongoRepository : IPurchaseOrderExternalMongoRepository
    {
        private readonly IMongoDbContext _context;

        public PurchaseOrderExternalMongoRepository(IMongoDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<PurchaseOrderExternalMongo>> GetByBatch(int startingNumber, int numberOfBatch)
        {
            var filter = new BsonDocument("items.purchaseRequest._createdDate", new BsonDocument("$gte", new DateTime(2019, 1, 1)));
            return await _context
                            .PurchaseOrderExternals
                            .Find(filter)
                            .Skip(startingNumber)
                            .Limit(numberOfBatch)
                            .ToListAsync();
        }
    }
}
