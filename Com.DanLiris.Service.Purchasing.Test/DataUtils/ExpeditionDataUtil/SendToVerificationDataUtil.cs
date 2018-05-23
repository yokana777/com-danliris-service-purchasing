using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil
{
    public class SendToVerificationDataUtil
    {
        private readonly PurchasingDocumentExpeditionFacade Facade;

        public SendToVerificationDataUtil(PurchasingDocumentExpeditionFacade Facade)
        {
            this.Facade = Facade;
        }
        public PurchasingDocumentExpedition GetNewData()
        {
            PurchasingDocumentExpedition TestData = new PurchasingDocumentExpedition()
            {
                SendToVerificationDivisionDate = DateTimeOffset.UtcNow,
                UnitPaymentOrderNo = Guid.NewGuid().ToString(),
                UPODate = DateTimeOffset.UtcNow,
                DueDate = DateTimeOffset.UtcNow,
                InvoiceNo = "Invoice",
                SupplierCode = "Supplier",
                SupplierName = "Supplier",
                DivisionCode = "Division",
                DivisionName = "Division",
                TotalPaid = 1000000,
                Currency = "IDR",
            };

            return TestData;
        }

        public async Task<PurchasingDocumentExpedition> GetTestData()
        {
            PurchasingDocumentExpedition model = GetNewData();
            await Facade.SendToVerification(new List<PurchasingDocumentExpedition>() { model }, "Unit Test");
            return model;
        }

        public async Task<PurchasingDocumentExpedition> GetTestDataNotVerified()
        {
            PurchasingDocumentExpedition model = Task.Run(() => GetTestData()).Result;
            model.Position = ExpeditionPosition.SEND_TO_PURCHASING_DIVISION;
            model.VerifyDate = DateTimeOffset.UtcNow;
            await Facade.UnitPaymentOrderVerification(model , "Unit Test");
            return model;
        }
    }
}
