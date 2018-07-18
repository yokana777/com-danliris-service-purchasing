using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IUnitPaymentOrderFacade
    {
        Tuple<List<UnitPaymentOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        UnitPaymentOrder ReadById(int id);
        Task<int> Create(UnitPaymentOrder model, string username, bool isImport, int clientTimeZoneOffset = 7);
        Task<int> Update(int id, UnitPaymentOrder model, string user);
        Task<int> Delete(int id, string username);
        Tuple<List<UnitPaymentOrder>, int, Dictionary<string, string>> ReadSpb(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
    }
}
