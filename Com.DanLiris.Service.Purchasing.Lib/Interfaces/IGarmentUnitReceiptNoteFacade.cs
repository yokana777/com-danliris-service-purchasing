using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentUnitReceiptNoteFacade
    {
        Tuple<List<GarmentUnitReceiptNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentUnitReceiptNote ReadById(int id);
        Task<int> Create(GarmentUnitReceiptNote garmentUnitReceiptNote);
        Task<int> Update(int id, GarmentUnitReceiptNote garmentUnitReceiptNote);
        Task<int> Delete(int id);

    }
}
