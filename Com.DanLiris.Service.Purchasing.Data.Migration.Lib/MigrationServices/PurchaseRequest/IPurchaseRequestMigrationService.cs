using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationServices
{
    public interface IPurchaseRequestMigrationService
    {
        Task<int> RunAsync(int startingNumber, int numberOfBatch);
    }
}
