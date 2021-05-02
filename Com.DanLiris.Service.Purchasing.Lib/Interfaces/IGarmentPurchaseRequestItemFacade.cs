using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentPurchaseRequestItemFacade
    {
        ReadResponse<dynamic> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}", string Select = null, string Search = "[]");
        Task<int> Patch(string id, JsonPatchDocument<GarmentPurchaseRequestItem> jsonPatch);
    }
}
