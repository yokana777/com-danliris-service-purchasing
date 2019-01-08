using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationIntegrationServices
{
    public class PurchaseOrderInternalIntegrationMigrationService : IPurchaseOrderInternalIntegrationMigrationService
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly DbSet<InternalPurchaseOrder> _purchaseOrderInternalDbSet;
        private readonly DbSet<PurchaseRequest> _purchaseRequestDbSet;

        public PurchaseOrderInternalIntegrationMigrationService(PurchasingDbContext dbContext)
        {
            _dbContext = dbContext;
            _purchaseOrderInternalDbSet = dbContext.Set<InternalPurchaseOrder>();
            _purchaseRequestDbSet = dbContext.Set<PurchaseRequest>();
        }

        public Task<int> SetPRId()
        {
            var listOfPR = _purchaseRequestDbSet.Select(pr => new { pr.Id, pr.No }).ToList();

            foreach (var purchaseOrderInternal in _purchaseOrderInternalDbSet.ToList())
            {
                var matchPr = listOfPR.FirstOrDefault(f => f.No.Equals(purchaseOrderInternal.PRNo));
                if (matchPr != null)
                {
                    purchaseOrderInternal.PRId = $"{matchPr.Id}";
                    _purchaseOrderInternalDbSet.Update(purchaseOrderInternal);
                }

            }
            return _dbContext.SaveChangesAsync();
        }
    }
}
