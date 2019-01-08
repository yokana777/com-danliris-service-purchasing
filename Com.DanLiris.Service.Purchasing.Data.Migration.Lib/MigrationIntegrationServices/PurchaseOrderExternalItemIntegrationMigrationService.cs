using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationIntegrationServices
{
    public class PurchaseOrderExternalItemIntegrationMigrationService : IPurchaseOrderExternalItemIntegrationMigrationService
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly DbSet<ExternalPurchaseOrderItem> _purchaseOrderExternalItemDbSet;
        private readonly DbSet<InternalPurchaseOrder> _purchaseOrderInternalDbSet;
        private readonly DbSet<PurchaseRequest> _purchaseRequestDbSet;

        public PurchaseOrderExternalItemIntegrationMigrationService(PurchasingDbContext dbContext)
        {
            _dbContext = dbContext;
            _purchaseOrderExternalItemDbSet = dbContext.Set<ExternalPurchaseOrderItem>();
            _purchaseOrderInternalDbSet = dbContext.Set<InternalPurchaseOrder>();
            _purchaseRequestDbSet = dbContext.Set<PurchaseRequest>();
        }

        public Task<int> SetPrAndPoInternalId()
        {
            var listOfPR = _purchaseRequestDbSet.Select(pr => new { pr.Id, pr.No }).ToList();
            var listOfPOInternal = _purchaseOrderInternalDbSet.Select(po => new { po.Id, po.PONo }).ToList();

            foreach (var purchaseOrderExternalItem in _purchaseOrderExternalItemDbSet.ToList())
            {
                var matchPr = listOfPR.FirstOrDefault(f => f.No.Equals(purchaseOrderExternalItem.PRNo));
                var matchPO = listOfPOInternal.FirstOrDefault(f => f.PONo.Equals(purchaseOrderExternalItem.PONo));

                if (matchPr != null)
                    purchaseOrderExternalItem.PRId = matchPr.Id;

                if (matchPO != null)
                    purchaseOrderExternalItem.POId = matchPO.Id;

                _purchaseOrderExternalItemDbSet.Update(purchaseOrderExternalItem);
            }
            return _dbContext.SaveChangesAsync();
        }
    }
}
