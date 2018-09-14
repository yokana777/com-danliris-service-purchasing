using Com.DanLiris.Service.Purchasing.Lib.Facades.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Lib.Models.DailyBankTransaction;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.DailyBankTransactionDataUtil
{
    public class DailyBankTransactionDataUtil
    {
        private readonly DailyBankTransactionFacade Facade;

        public DailyBankTransactionDataUtil(DailyBankTransactionFacade facade)
        {
            Facade = facade;
        }

        public DailyBankTransactionModel GetNewData()
        {
            DailyBankTransactionModel TestData = new DailyBankTransactionModel()
            {
                AccountBankAccountName = "AccountName",
                AccountBankAccountNumber = "AccountNumber",
                AccountBankCode = "BankCode",
                AccountBankCurrencyCode = "CurrencyCode",
                AccountBankCurrencyId = "CurrencyId",
                AccountBankCurrencySymbol = "CurrencySymbol",
                AccountBankId = "BankId",
                AccountBankName = "BankName",
                AfterNominal = 0,
                BeforeNominal = 0,
                BuyerCode = "BuyerCode",
                BuyerId = "BuyerId",
                BuyerName = "BuyerName",
                Date = DateTimeOffset.UtcNow,
                Nominal = 1000,
                ReferenceNo = "ReferenceNo",
                ReferenceType = "ReferenceType",
                Remark = "Remark",
                SourceType = "Operasional",
                Status = "IN",
                SupplierCode = "SupplierCode",
                SupplierName = "SupplierName",
                SupplierId = "SupplierId"
            };

            return TestData;
        }

        public async Task<DailyBankTransactionModel> GetTestDataIn()
        {
            DailyBankTransactionModel model = GetNewData();
            await Facade.Create(model, "Unit Test");
            return await Facade.ReadById(model.Id);
        }

        public async Task<DailyBankTransactionModel> GetTestDataOut()
        {
            DailyBankTransactionModel model = GetNewData();
            model.Status = "OUT";
            await Facade.Create(model, "Unit Test");
            return await Facade.ReadById(model.Id);
        }
    }
}
