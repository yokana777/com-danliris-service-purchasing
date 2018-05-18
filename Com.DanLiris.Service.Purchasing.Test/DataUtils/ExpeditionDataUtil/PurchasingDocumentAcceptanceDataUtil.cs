using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition.PurchasingDocumentAcceptanceViewModel;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil
{
    public class PurchasingDocumentAcceptanceDataUtil
    {
        private readonly PurchasingDocumentExpeditionFacade Facade;
        private readonly SendToVerificationDataUtil sendToVerificationDataUtil;
        private PurchasingDocumentExpedition purchasingDocumentExpedition;

        public PurchasingDocumentAcceptanceDataUtil(PurchasingDocumentExpeditionFacade Facade, SendToVerificationDataUtil sendToVerificationDataUtil)
        {
            this.Facade = Facade;
            this.sendToVerificationDataUtil = sendToVerificationDataUtil;
        }

        public PurchasingDocumentAcceptanceViewModel GetNewData()
        {
            purchasingDocumentExpedition = Task.Run(() => this.sendToVerificationDataUtil.GetTestData()).Result;

            PurchasingDocumentAcceptanceItem item = new PurchasingDocumentAcceptanceItem()
            {
                Id = purchasingDocumentExpedition.Id,
                UnitPaymentOrderNo = purchasingDocumentExpedition.UnitPaymentOrderNo
            };

            PurchasingDocumentAcceptanceViewModel TestData = new PurchasingDocumentAcceptanceViewModel()
            {
                ReceiptDate = DateTimeOffset.UtcNow,
                PurchasingDocumentExpedition = new List<PurchasingDocumentAcceptanceItem>() { item }
            };

            return TestData;
        }

        public PurchasingDocumentAcceptanceViewModel GetVerificationNewData()
        {
            PurchasingDocumentAcceptanceViewModel TestData = GetNewData();
            TestData.Role = "VERIFICATION";
            return TestData;
        }

        public PurchasingDocumentAcceptanceViewModel GetCashierNewData()
        {
            PurchasingDocumentAcceptanceViewModel TestData = GetNewData();
            TestData.Role = "CASHIER";
            return TestData;
        }

        public PurchasingDocumentAcceptanceViewModel GetFinanceNewData()
        {
            PurchasingDocumentAcceptanceViewModel TestData = GetNewData();
            TestData.Role = "FINANCE";
            return TestData;
        }

        public async Task<PurchasingDocumentExpedition> GetVerificationTestData()
        {
            PurchasingDocumentAcceptanceViewModel vModel = GetVerificationNewData();
            await Task.Run(() => Facade.PurchasingDocumentAcceptance(vModel, "Unit Test"));
            return await Facade.ReadModelById(purchasingDocumentExpedition.Id);
        }

        public async Task<PurchasingDocumentExpedition> GetCashierTestData()
        {
            PurchasingDocumentAcceptanceViewModel vModel = GetCashierNewData();
            await Task.Run(() => Facade.PurchasingDocumentAcceptance(vModel, "Unit Test"));
            return await Facade.ReadModelById(purchasingDocumentExpedition.Id);
        }

        public async Task<PurchasingDocumentExpedition> GetFinanceTestData()
        {
            PurchasingDocumentAcceptanceViewModel vModel = GetFinanceNewData();
            await Task.Run(() => Facade.PurchasingDocumentAcceptance(vModel, "Unit Test"));
            return await Facade.ReadModelById(purchasingDocumentExpedition.Id);
        }
    }
}
