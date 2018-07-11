using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IBankExpenditureNoteFacade
    {
        ReadResponse Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        ReadResponse GetAllByPosition(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        Task<int> Update(int id, BankExpenditureNoteModel model, string username);
        Task<BankExpenditureNoteModel> ReadById(int Id);
        Task<int> Create(BankExpenditureNoteModel model, string username);
        Task<int> Delete(int Id, string username);
        ReadResponse GetReport(int Size, int Page, string DocumentNo, string UnitPaymentOrderNo, string InvoiceNo, string SupplierCode, DateTimeOffset? DateFrom, DateTimeOffset? DateTo, int Offset);
    }
}
