using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentUnitReceiptNoteFacade
    {
        ReadResponse Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentUnitReceiptNoteViewModel ReadById(int id);
        MemoryStream GeneratePdf(GarmentUnitReceiptNoteViewModel garmentUnitReceiptNote);
        Task<int> Create(GarmentUnitReceiptNote garmentUnitReceiptNote);
        Task<int> Update(int id, GarmentUnitReceiptNote garmentUnitReceiptNote);
        Task<int> Delete(int id);
        ReadResponse ReadForUnitDO(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}");

    }
}
