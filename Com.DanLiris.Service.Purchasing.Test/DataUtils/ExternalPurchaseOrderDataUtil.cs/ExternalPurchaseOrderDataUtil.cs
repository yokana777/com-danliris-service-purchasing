using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtil.cs
{
    public class ExternalPurchaseOrderDataUtil
    {
        private ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil;
        private ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil;
        private readonly ExternalPurchaseOrderFacade facade;

        public ExternalPurchaseOrderDataUtil(ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil, ExternalPurchaseOrderFacade facade)
        {
            this.externalPurchaseOrderItemDataUtil = externalPurchaseOrderItemDataUtil;
            this.facade = facade;
        }

        public ExternalPurchaseOrder GetNewData()
        {
            return new ExternalPurchaseOrder
            {
                CurrencyCode = "CurrencyCode",
                CurrencyId = "CurrencyId",
                CurrencyRate = "CurrencyRate",
                UnitId = "UnitId",
                UnitCode = "UnitCode",
                UnitName = "UnitName",
                DivisionId = "DivisionId",
                DivisionCode = "DivisionCode",
                DivisionName = "DivisionName",
                FreightCostBy ="test",
                DeliveryDate = DateTime.Now,
                OrderDate = DateTime.Now,
                SupplierCode ="sup",
                SupplierId ="supId",
                SupplierName ="Supplier",
                PaymentMethod = "test",
                Remark = "Remark",
                Items = new List<ExternalPurchaseOrderItem> { externalPurchaseOrderItemDataUtil.GetNewData() }
            };
        }

        public async Task<ExternalPurchaseOrder> GetTestData(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = GetNewData();

            await facade.Create(externalPurchaseOrder, user);

            return externalPurchaseOrder;
        }
    }
}
