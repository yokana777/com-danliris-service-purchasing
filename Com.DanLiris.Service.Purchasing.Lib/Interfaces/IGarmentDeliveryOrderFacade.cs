using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentDeliveryOrderFacade
    {
        Tuple<List<GarmentDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentDeliveryOrder ReadById(int id);
        Task<int> Create(GarmentDeliveryOrder m, string user, int clientTimeZoneOffset = 7);
        Task<int> Update(int id, GarmentDeliveryOrderViewModel vm, GarmentDeliveryOrder m, string user, int clientTimeZoneOffset = 7);

        Task<int> Delete(int id, string username);
       IQueryable<GarmentDeliveryOrder>  ReadBySupplier( string Keyword = null, string Filter = "{}");
    }
}
