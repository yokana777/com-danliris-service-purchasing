using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentExternalPurchaseOrderFacade
    {
        Tuple<List<GarmentExternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentExternalPurchaseOrder ReadById(int id);
        Task<int> Create(GarmentExternalPurchaseOrder m, string user, int clientTimeZoneOffset = 7);
        Task<int> Update(int id, GarmentExternalPurchaseOrder m, string user, int clientTimeZoneOffset = 7);
        int Delete(int id, string user);
        int EPOPost(List<GarmentExternalPurchaseOrder> ListEPO, string user);
        int EPOUnpost(int id, string user);
        int EPOClose(int id, string user);
        int EPOCancel(int id, string user);
        SupplierViewModel GetSupplier(long supplierId);
        GarmentProductViewModel GetProduct(long productId);
        int EPOApprove(List<GarmentExternalPurchaseOrder> ListEPO, string user);
    }
}
