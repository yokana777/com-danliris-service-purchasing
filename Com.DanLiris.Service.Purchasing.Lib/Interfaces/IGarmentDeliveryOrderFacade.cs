using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
		IQueryable<GarmentDeliveryOrder> DOForCustoms(string Keyword = null, string Filter = "{}");
		int IsReceived(List<int> Id);
        ReadResponse ReadForUnitReceiptNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}");

        ReadResponse ReadForCorrectionNoteQuantity(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}");

        IQueryable<AccuracyOfArrivalReportViewModel> GetReportQuery(string category, DateTime? dateFrom, DateTime? dateTo, int offset, string Filter = "{}");
        Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReport(string category, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset, string Filter = "{}");
        MemoryStream GenerateExcel(string category, DateTime? dateFrom, DateTime? dateTo, int offset, string Filter = "{}");
    }
}
