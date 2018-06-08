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
    public class ExternalPurchaseOrderItemDataUtil
    {
        private ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil;
        private InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil;
        private PurchaseRequestDataUtil purchaseRequestDataUtil;

        public ExternalPurchaseOrderItemDataUtil(ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil, InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil, PurchaseRequestDataUtil purchaseRequestDataUtil)
        {
            this.externalPurchaseOrderDetailDataUtil = externalPurchaseOrderDetailDataUtil;
            this.internalPurchaseOrderDataUtil = internalPurchaseOrderDataUtil;
            this.purchaseRequestDataUtil = purchaseRequestDataUtil;
        }

        public ExternalPurchaseOrderItem GetNewData()
        {
            Task<InternalPurchaseOrder> internalPurchaseOrder = Task.Run(() => internalPurchaseOrderDataUtil.GetNewData());
            internalPurchaseOrder.Wait();
            Task<PurchaseRequest> purchaseRequest = Task.Run(() => purchaseRequestDataUtil.GetNewData());
            purchaseRequest.Wait();

           
            return new ExternalPurchaseOrderItem
            {
                POId = internalPurchaseOrder.Result.Id,
                PRId = purchaseRequest.Result.Id,
                PONo = internalPurchaseOrder.Result.PONo,
                PRNo = purchaseRequest.Result.No,
                UnitCode = "unitcode",
                UnitName = "unit",
                UnitId = "unitId",
                Details = new List<ExternalPurchaseOrderDetail> { externalPurchaseOrderDetailDataUtil.GetNewData() }


            };
        }
    }
}
