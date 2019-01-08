using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationIntegrationServices
{
    public interface IPurchaseOrderExternalItemIntegrationMigrationService
    {
        Task<int> SetPrAndPoInternalId();
    }
}
