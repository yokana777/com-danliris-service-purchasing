using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtil.cs
{
    public class ExternalPurchaseOrderDetailDataUtil
    {
        private ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil;
        private InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil;
        private PurchaseRequestDataUtil purchaseRequestDataUtil;

        public ExternalPurchaseOrderDetailDataUtil(ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil, InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil, PurchaseRequestDataUtil purchaseRequestDataUtil)
        {
            this.externalPurchaseOrderDetailDataUtil = externalPurchaseOrderDetailDataUtil;
            this.internalPurchaseOrderDataUtil = internalPurchaseOrderDataUtil;
            this.purchaseRequestDataUtil = purchaseRequestDataUtil;
        }

        public ExternalPurchaseOrderDetail GetNewData()
        {
            Task<InternalPurchaseOrder> internalPurchaseOrder = Task.Run(() => internalPurchaseOrderDataUtil.GetNewData());
            internalPurchaseOrder.Wait();
            Task<PurchaseRequest> purchaseRequest = Task.Run(() => purchaseRequestDataUtil.GetNewData());
            purchaseRequest.Wait();

            List<InternalPurchaseOrderItem> purchaseOrderItems = new List<InternalPurchaseOrderItem>();
            foreach (var item in internalPurchaseOrder.Result.Items)
            {
                purchaseOrderItems.Add(item);
            }

            List<PurchaseRequestItem> purchaseRequestItems = new List<PurchaseRequestItem>();
            foreach (var item in purchaseRequest.Result.Items)
            {
                purchaseRequestItems.Add(item);
            }

            return new ExternalPurchaseOrderDetail
            {
                ProductId = "ProductId",
                ProductCode = "ProductCode",
                ProductName = "ProductName",
                DefaultQuantity = 10,
                DealUomId = "UomId",
                DealUomUnit = "Uom",
                ProductRemark = "Remark",
                POItemId= purchaseOrderItems[0].Id,
                PRItemId=purchaseRequestItems[0].Id,
                PriceBeforeTax=1000,
                PricePerDealUnit=200,
                DealQuantity= purchaseOrderItems[0].Quantity
            };
        }
    }
}
