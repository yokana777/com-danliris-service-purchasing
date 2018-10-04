using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentInternalPurchaseOrderFacade
    {
        Tuple<List<GarmentInternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentInternalPurchaseOrder ReadById(int id);
        Task<int> CreateMultiple(List<GarmentInternalPurchaseOrder> ListModel, string user, int clientTimeZoneOffset = 7);
    }
}
