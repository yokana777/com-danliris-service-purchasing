using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoRepositories.PurchaseOrderExternal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationServices
{
    public class PurchaseOrderExternalMigrationService : IPurchaseOrderExternalMigrationService
    {
        private readonly IPurchaseOrderExternalMongoRepository _mongoRepository;
        private readonly PurchasingDbContext _dbContext;
        private readonly DbSet<ExternalPurchaseOrder> _purchaseOrderExternalDbSet;
        private readonly DbSet<ExternalPurchaseOrderItem> _purchaseOrderExternalItemDbSet;
        private readonly DbSet<ExternalPurchaseOrderDetail> _purchaseOrderExternalDetailDbSet;

        public PurchaseOrderExternalMigrationService(IPurchaseOrderExternalMongoRepository mongoRepository, PurchasingDbContext dbContext)
        {
            _mongoRepository = mongoRepository;
            _dbContext = dbContext;
            _purchaseOrderExternalDbSet = dbContext.Set<ExternalPurchaseOrder>();
            _purchaseOrderExternalItemDbSet = dbContext.Set<ExternalPurchaseOrderItem>();
            _purchaseOrderExternalDetailDbSet = dbContext.Set<ExternalPurchaseOrderDetail>();
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

        private List<ExternalPurchaseOrder> Transform(IEnumerable<PurchaseOrderExternalMongo> extractedData)
        {
            return extractedData.Select(mongoPurchaseOrderExternal => new ExternalPurchaseOrder(mongoPurchaseOrderExternal)).ToList();
        }

        private int Load(List<ExternalPurchaseOrder> transformedData)
        {
            var existingUids = _purchaseOrderExternalDbSet.Select(entity => entity.UId).ToList();
            transformedData = transformedData.Where(entity => !existingUids.Contains(entity.UId)).ToList();
            if (transformedData.Count > 0)
            {
                _purchaseOrderExternalDetailDbSet.AddRange(transformedData.SelectMany(x => x.Items.SelectMany(y => y.Details)));
                _purchaseOrderExternalItemDbSet.AddRange(transformedData.SelectMany(x => x.Items));
                _purchaseOrderExternalDbSet.AddRange(transformedData);
            }
            return _dbContext.SaveChanges();
        }
    }
}
