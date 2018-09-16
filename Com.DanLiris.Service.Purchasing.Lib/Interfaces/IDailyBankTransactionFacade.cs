using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.DailyBankTransaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IDailyBankTransactionFacade
    {
        ReadResponse Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        Task<int> Create(DailyBankTransactionModel model, string username);
        Task<DailyBankTransactionModel> ReadById(int Id);
        ReadResponse GetReport(string bankId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo);
    }
}
