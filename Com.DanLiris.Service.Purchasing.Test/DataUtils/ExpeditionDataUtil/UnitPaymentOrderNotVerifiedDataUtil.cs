using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil
{
    class UnitPaymentOrderNotVerifiedDataUtil
    {
        private readonly PurchasingDocumentExpeditionFacade Facade;

        public UnitPaymentOrderNotVerifiedDataUtil(PurchasingDocumentExpeditionFacade Facade)
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
                SupplierCode = "Supplier",
                SupplierName = "Supplier",
                DivisionCode = "Division",
                DivisionName = "Division",
                TotalPaid = 1000000,
                Currency = "IDR",
                Position = (ExpeditionPosition) 6,
                VerifyDate= DateTimeOffset.UtcNow
            };

            return TestData;
        }

        public async Task<PurchasingDocumentExpedition> GetTestData()
        {
            PurchasingDocumentExpedition model = GetNewData();
            await Facade.SendToVerification(new List<PurchasingDocumentExpedition>() { model }, "Unit Test");
            return model;
        }
    }
}

