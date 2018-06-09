using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils
{
    public class ExternalPurchaseOrderItemDataUtil
    {
        private ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil;
        public ExternalPurchaseOrderItem GetNewData(InternalPurchaseOrder internalPurchaseOrder)
        {
            List<InternalPurchaseOrderItem> detail = new List<InternalPurchaseOrderItem>();
            foreach (var POdetail in internalPurchaseOrder.Items)
            {
                detail.Add(POdetail);
            }
            return new ExternalPurchaseOrderItem
            {
                POId = internalPurchaseOrder.Id,
                PRId = Convert.ToInt64(internalPurchaseOrder.PRId),
                PONo = internalPurchaseOrder.PONo,
                PRNo = internalPurchaseOrder.PRNo,
                UnitCode = "unitcode",
                UnitName = "unit",
                UnitId = "unitId",
                Details = new List<ExternalPurchaseOrderDetail> { externalPurchaseOrderDetailDataUtil.GetNewData(detail) }


            };

        }
    }
}
