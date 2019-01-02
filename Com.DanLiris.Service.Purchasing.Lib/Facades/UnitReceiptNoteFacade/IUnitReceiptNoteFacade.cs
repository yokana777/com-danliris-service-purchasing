using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade
{
    public interface IUnitReceiptNoteFacade
    {
        ReadResponse<UnitReceiptNote> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        UnitReceiptNote ReadById(int id);
        Task<int> Create(UnitReceiptNote m, string user);
        Task<int> Update(int id, UnitReceiptNote unitReceiptNote, string user);
        int Delete(int id, string user);
        ReadResponse<UnitReceiptNote> ReadBySupplierUnit(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        ReadResponse<UnitReceiptNoteReportViewModel> GetReport(string urnNo, string prNo, string unitId, string categoryId, string supplierId, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset);
        MemoryStream GenerateExcel(string urnNo, string prNo, string unitId, string categoryId, string supplierId, DateTime? dateFrom, DateTime? dateTo, int offset);
    }
}
