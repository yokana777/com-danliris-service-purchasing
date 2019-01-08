using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationServices
{
    public interface IPurchaseOrderInternalMigrationService
    {
        Task<int> RunAsync(int startingNumber, int numberOfBatch);
    }
}
