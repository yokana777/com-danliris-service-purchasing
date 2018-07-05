using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils
{
    public class UnitPaymentOrderDataUtil
    {
        private UnitPaymentOrderItemDataUtil unitPurchaseOrderItemDataUtil;
        private readonly UnitPaymentOrderFacade facade;

        public UnitPaymentOrderDataUtil(UnitPaymentOrderItemDataUtil unitPurchaseOrderItemDataUtil, UnitPaymentOrderFacade facade)
        {
            this.unitPurchaseOrderItemDataUtil = unitPurchaseOrderItemDataUtil;
            this.facade = facade;
        }

        public UnitPaymentOrder GetNewData()
        {
            UnitPaymentOrder unitPaymentOrder = new UnitPaymentOrder
            {
                Id = 0,
                UPONo = null,
                DivisionId = null,
                DivisionCode = null,
                DivisionName = null,
                SupplierId = null,
                SupplierCode = null,
                SupplierName = null,
                Date = new DateTimeOffset(),
                CategoryId = null,
                CategoryCode = null,
                CategoryName = null,
                CurrencyId = null,
                CurrencyCode = null,
                CurrencyRate = 0,
                PaymentMethod = null,
                InvoiceNo = null,
                InvoiceDate = new DateTimeOffset(),
                PibNo = null,
                UseIncomeTax = false,
                IncomeTaxId = null,
                IncomeTaxName = null,
                IncomeTaxRate = 0,
                IncomeTaxNo = null,
                IncomeTaxDate = new DateTimeOffset(),
                UseVat = false,
                VatNo = null,
                VatDate = new DateTimeOffset(),
                Remark = null,
                DueDate = new DateTimeOffset(),
                IsCorrection = null,
                IsPaid = null,
                Items = new List<UnitPaymentOrderItem> { unitPurchaseOrderItemDataUtil.GetNewData() }
            };
            return unitPaymentOrder;
        }

        public async Task<UnitPaymentOrder> GetTestData(string user)
        {
            var data = GetNewData();
            await facade.Create(data, user);
            return data;
        }
    }
}
