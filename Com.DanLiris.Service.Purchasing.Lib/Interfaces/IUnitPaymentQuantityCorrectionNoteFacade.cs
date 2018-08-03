using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IUnitPaymentQuantityCorrectionNoteFacade
    {
        Tuple<List<UnitPaymentCorrectionNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        UnitPaymentCorrectionNote ReadById(int id);
        Task<int> Create(UnitPaymentCorrectionNote model, string username, int clientTimeZoneOffset = 7);
        //Task<int> Update(int id, UnitPaymentCorrectionNote model, string user);
        //Task<int> Delete(int id, string username);
    }
}
