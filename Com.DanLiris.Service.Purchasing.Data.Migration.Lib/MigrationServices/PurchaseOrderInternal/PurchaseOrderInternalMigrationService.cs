using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories.PurchaseOrderInternal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationServices
{ 
    public class PurchaseOrderInternalMigrationService : IPurchaseOrderInternalMigrationService
    {
        private readonly IPurchaseOrderInternalMongoRepository _mongoRepository;
        private readonly PurchasingDbContext _dbContext;
        private readonly DbSet<InternalPurchaseOrder> _purchaseOrderInternalDbSet;
        private readonly DbSet<InternalPurchaseOrderItem> _purchaseOrderInternalItemDbSet;

        public PurchaseOrderInternalMigrationService(IPurchaseOrderInternalMongoRepository mongoRepository, PurchasingDbContext dbContext)
        {
            _mongoRepository = mongoRepository;
            _dbContext = dbContext;
            _purchaseOrderInternalDbSet = dbContext.Set<InternalPurchaseOrder>();
            _purchaseOrderInternalItemDbSet = dbContext.Set<InternalPurchaseOrderItem>();
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
                Load(transformedData);
                TotalInsertedData += transformedData.Count;

                await RunAsync(startingNumber, numberOfBatch);
            }

            return TotalInsertedData;
        }

        private List<InternalPurchaseOrder> Transform(IEnumerable<PurchaseOrderMongo> extractedData)
        {
            return extractedData.Select(mongoPurchaseOrderInternal => new InternalPurchaseOrder(mongoPurchaseOrderInternal)).ToList();
        }

        private int Load(List<InternalPurchaseOrder> transformedData)
        {
            var existingUids = _purchaseOrderInternalDbSet.Select(entity => entity.UId).ToList();
            transformedData = transformedData.Where(entity => !existingUids.Contains(entity.UId)).ToList();
            if (transformedData.Count > 0)
            {
                _purchaseOrderInternalItemDbSet.AddRange(transformedData.SelectMany(x => x.Items));
                _purchaseOrderInternalDbSet.AddRange(transformedData);
            }
            return _dbContext.SaveChanges();
        }
    }
}
