using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentPurchaseRequestItemFacade
    {
        Task<int> Patch(string id, JsonPatchDocument<GarmentPurchaseRequestItem> jsonPatch);
    }
}
