using Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IPurchasingDispositionFacade
    {
        Tuple<List<PurchasingDisposition>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        PurchasingDisposition ReadModelById(int id);
        Task<int> Create(PurchasingDisposition m, string user, int clientTimeZoneOffset = 7);
        int Delete(int id, string user);
        Task<int> Update(int id, PurchasingDisposition purchasingDisposition, string user);
        IQueryable<PurchasingDisposition> ReadByDisposition(string Keyword = null, string Filter = "{}");
    }
}
