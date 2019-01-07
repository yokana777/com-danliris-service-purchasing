using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationServices
{
    public class PurchaseRequestMigrationService : IPurchaseRequestMigrationService
    {
        private readonly IPurchaseRequestMongoRepository _mongoRepository;
        private readonly PurchasingDbContext _dbContext;
        private readonly DbSet<PurchaseRequest> _purchaseRequestDbSet;
        private readonly DbSet<PurchaseRequestItem> _purchaseRequestItemDbSet;

        public PurchaseRequestMigrationService(IPurchaseRequestMongoRepository mongoRepository, PurchasingDbContext dbContext)
        {
            _mongoRepository = mongoRepository;
            _dbContext = dbContext;
            _purchaseRequestDbSet = dbContext.Set<PurchaseRequest>();
            _purchaseRequestItemDbSet = dbContext.Set<PurchaseRequestItem>();
        }

        public int TotalInsertedData { get; private set; }

        public async Task<int> RunAsync(int startingNumber, int numberOfBatch)
        {
            var extractedData = await _mongoRepository.GetByBatch(startingNumber, numberOfBatch);

            if (extractedData.Count() > 0)
            {
                var transformedData = Transform(extractedData);
                startingNumber += transformedData.Count;

                //Insert into SQL
                TotalInsertedData += Load(transformedData);

                await RunAsync(startingNumber, numberOfBatch);
            }

            return TotalInsertedData;
        }

        private List<PurchaseRequest> Transform(IEnumerable<PurchaseRequestMongo> extractedData)
        {
            return extractedData.Select(mongoPurchaseRequest => new PurchaseRequest(mongoPurchaseRequest)).ToList();
        }

        private int Load(List<PurchaseRequest> transformedData)
        {
            throw new NotImplementedException();
        }        
    }
}
