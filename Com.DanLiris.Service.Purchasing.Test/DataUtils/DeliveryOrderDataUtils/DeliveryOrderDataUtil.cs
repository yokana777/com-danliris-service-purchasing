using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils
{
    public class DeliveryOrderDataUtil
    {
        private DeliveryOrderItemDataUtil deliveryOrderItemDataUtil;
        private ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil;
        private readonly DeliveryOrderFacade facade;

        public DeliveryOrderDataUtil(DeliveryOrderItemDataUtil deliveryOrderItemDataUtil, ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil, DeliveryOrderFacade facade)
        {
            this.deliveryOrderItemDataUtil = deliveryOrderItemDataUtil;
            this.externalPurchaseOrderDataUtil = externalPurchaseOrderDataUtil;
            this.facade = facade;
        }

        public async Task<DeliveryOrder> GetNewData(string user)
        {
            var externalPurchaseOrder = await externalPurchaseOrderDataUtil.GetTestDataUnused(user);
            return new DeliveryOrder
            {
                DONo = DateTime.UtcNow.Ticks.ToString(),
                DODate = DateTimeOffset.Now,
                ArrivalDate = DateTimeOffset.Now,
                SupplierId = externalPurchaseOrder.SupplierId,
                SupplierCode = externalPurchaseOrder.SupplierCode,
                SupplierName = externalPurchaseOrder.SupplierName,
                Remark = "Ini Keterangan",
                Items = new List<DeliveryOrderItem> { deliveryOrderItemDataUtil.GetNewData(externalPurchaseOrder) }
            };
        }

        public async Task<DeliveryOrderViewModel> GetNewDataViewModel(string user)
        {
            var externalPurchaseOrder = await externalPurchaseOrderDataUtil.GetTestDataUnused(user);

            return new DeliveryOrderViewModel
            {
                no = DateTime.UtcNow.Ticks.ToString(),
                date = DateTimeOffset.Now,
                supplierDoDate = DateTimeOffset.Now,
                supplier = new SupplierViewModel
                {
                    _id = externalPurchaseOrder.SupplierId,
                    code = externalPurchaseOrder.SupplierCode,
                    name = externalPurchaseOrder.SupplierName,
                },
                remark = "Ini Ketereangan",
                items = new List<DeliveryOrderItemViewModel> { deliveryOrderItemDataUtil.GetNewDataViewModel(externalPurchaseOrder) }
            };
        }

        public async Task<DeliveryOrder> GetTestData(string user)
        {
            DeliveryOrder model = await GetNewData(user);

            await facade.Create(model, user);

            return model;
        }
    }
}
