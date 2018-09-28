using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentPurchaseRequestFacade
    {
        Tuple<List<GarmentPurchaseRequest>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentPurchaseRequest ReadById(int id);
        GarmentPurchaseRequest ReadByRONo(string rono);
        Task<int> Create(GarmentPurchaseRequest m, string user, int clientTimeZoneOffset = 7);
        Task<int> Update(int id, GarmentPurchaseRequest m, string user, int clientTimeZoneOffset = 7);
    }
}
