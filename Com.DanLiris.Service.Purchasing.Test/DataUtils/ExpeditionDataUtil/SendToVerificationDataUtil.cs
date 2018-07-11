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
            List<PurchasingDocumentExpeditionItem> Items = new List<PurchasingDocumentExpeditionItem>()
            {
                new PurchasingDocumentExpeditionItem()
                {
                    ProductId = "ProductId",
                    ProductCode = "ProductCode",
                    ProductName = "ProductName",
                    Price = 10000,
                    Quantity = 5,
                    Uom = "MTR",
                    UnitId = "UnitId",
                    UnitCode = "UnitCode",
                    UnitName = "UnitName"
                }
            };

            PurchasingDocumentExpedition TestData = new PurchasingDocumentExpedition()
            {
                SendToVerificationDivisionDate = DateTimeOffset.UtcNow,
                UnitPaymentOrderNo = Guid.NewGuid().ToString(),
                UPODate = DateTimeOffset.UtcNow,
                DueDate = DateTimeOffset.UtcNow,
                InvoiceNo = "Invoice",
                PaymentMethod = "CASH",
                SupplierCode = "Supplier",
                SupplierName = "Supplier",
                DivisionCode = "Division",
                DivisionName = "Division",
                IncomeTax = 20000,
                Vat = 100000,
                IncomeTaxId = "IncomeTaxId",
                IncomeTaxName = "IncomeTaxName",
                IncomeTaxRate = 2,
                TotalPaid = 1000000,
                Currency = "IDR",
                Items = Items,
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
