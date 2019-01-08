using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationIntegrationServices
{
    public interface IPurchaseOrderInternalIntegrationMigrationService
    {
        Task<int> SetPRId();
    }
}
