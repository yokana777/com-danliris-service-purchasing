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
        bool CheckDuplicate(GarmentInternalPurchaseOrder model);
        Task<int> CreateMultiple(List<GarmentInternalPurchaseOrder> listModel, string user, int clientTimeZoneOffset = 7);
        Task<int> Split(int id, GarmentInternalPurchaseOrder model, string user, int clientTimeZoneOffset = 7);
        Task<int> Delete(int id, string username);
    }
}
