using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentUnitExpenditureNoteFacade
    {
        ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentUnitExpenditureNoteViewModel ReadById(int id);
        Task<int> Create(GarmentUnitExpenditureNote garmentUnitExpenditureNote);
        Task<int> Update(int id, GarmentUnitExpenditureNote garmentUnitExpenditureNote);
        Task<int> Delete(int id);
        ReadResponse<object> ReadForGPreparing(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}");
        Task<int> UpdateIsPreparing(int id, GarmentUnitExpenditureNote garmentUnitExpenditureNote);
        Task<int> UpdateReturQuantity(int id, double quantity);
        GarmentUnitExpenditureNote ReadByUENId(int id);
    }
}
