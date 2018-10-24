using Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IInternNoteFacade
    {
        Tuple<List<InternNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        InternNote ReadById(int id);
        Task<int> Create(InternNote m, string user, int clientTimeZoneOffset = 7);
        Task<int> Update(int id, InternNote m, string user, int clientTimeZoneOffset = 7);

        Task<int> Delete(int id, string username);
    }
}
