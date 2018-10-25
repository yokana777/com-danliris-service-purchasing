using Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IInternNoteFacade
    {
        Tuple<List<GarmentInternNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentInternNote ReadById(int id);
        Task<int> Create(GarmentInternNote m, string user, int clientTimeZoneOffset = 7);
        Task<int> Update(int id, GarmentInternNote m, string user, int clientTimeZoneOffset = 7);

        Task<int> Delete(int id, string username);
    }
}
