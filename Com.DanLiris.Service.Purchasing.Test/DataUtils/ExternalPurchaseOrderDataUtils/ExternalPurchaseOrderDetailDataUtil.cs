using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils
{
    public class ExternalPurchaseOrderDetailDataUtil
    {

        public ExternalPurchaseOrderDetail GetNewData(List<InternalPurchaseOrderItem> internalPurchaseOrderItem) => new ExternalPurchaseOrderDetail
        {

            POItemId = internalPurchaseOrderItem[0].Id,
            PRItemId = Convert.ToInt64(internalPurchaseOrderItem[0].PRItemId),
            ProductId = "ProductId",
            ProductCode = "ProductCode",
            ProductName = "ProductName",
            DefaultQuantity = 10,
            DealUomId = "UomId",
            DealUomUnit = "Uom",
            ProductRemark = "Remark",
            PriceBeforeTax = 1000,
            PricePerDealUnit = 200,
            DealQuantity = internalPurchaseOrderItem[0].Quantity
        };

    }
}
