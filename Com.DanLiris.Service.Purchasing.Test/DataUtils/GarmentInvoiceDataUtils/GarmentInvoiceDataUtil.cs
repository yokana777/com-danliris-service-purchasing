using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
//using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils
{
    public class GarmentInvoiceDataUtil
    {
        private GarmentInvoiceItemDataUtil GarmentInvoiceItemDataUtil;
        //private GarmentDeliveryOrderDataUtil GarmentDeliveryOrderDataUtil;
        private readonly GarmentInvoiceFacade facade;
        private GarmentInvoiceDetailDataUtil GarmentInvoiceDetailDataUtil;

        public GarmentInvoiceDataUtil(GarmentInvoiceItemDataUtil GarmentInvoiceItemDataUtil, GarmentInvoiceDetailDataUtil GarmentInvoiceDetailDataUtil,  GarmentInvoiceFacade facade)
        {
            this.GarmentInvoiceItemDataUtil = GarmentInvoiceItemDataUtil;
            this.GarmentInvoiceDetailDataUtil = GarmentInvoiceDetailDataUtil;
            //this.GarmentDeliveryOrderDataUtil = GarmentDeliveryOrderDataUtil;
            this.facade = facade;
        }
        public async Task<GarmentInvoice> GetNewData(string user)
        {
            //var gDeliveryOrder = GarmentDeliveryOrderDataUtil.GetNewData();
            long nowTicks = DateTimeOffset.Now.Ticks;
            return new GarmentInvoice
            {
                InvoiceNo = "INV" + Guid.NewGuid(),
                InvoiceDate = DateTimeOffset.Now,
                SupplierId = nowTicks,
                SupplierCode ="SupplierCode",
                SupplierName = "SupplierName",
                
               // Items = new List<DeliveryOrderItem> { deliveryOrderItemDataUtil.GetNewData(externalPurchaseOrder) }
            };
        }

        public async Task<GarmentInvoice> GetTestData(string user)
        {
            GarmentInvoice model = await GetNewData(user);

            await facade.Create(model, user);

            return model;
        }
    }
}
