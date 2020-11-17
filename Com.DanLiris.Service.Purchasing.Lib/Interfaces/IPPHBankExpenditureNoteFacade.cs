using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IPPHBankExpenditureNoteFacade
    {
        List<object> GetUnitPaymentOrder(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, string incomeTaxName, double incomeTaxRate, string currency);
        ReadResponse<object> Read(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}");
        Task<int> Update(int id, PPHBankExpenditureNote model, string username);
        Task<PPHBankExpenditureNote> ReadById(int id);
        Task<int> Create(PPHBankExpenditureNote model, string username);
        Task<int> Delete(int id, string username);
        Task<int> Posting(List<long> ids);
    }
}
