using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils
{
    public class InternalPurchaseOrderItemDataUtil
    {
        private InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil;
        public InternalPurchaseOrderItem GetNewData() => new InternalPurchaseOrderItem
        {
            ProductId = "ProductId",
            ProductCode = "ProductCode",
            ProductName = "ProductName",
            Quantity = 10,
            UomId = "UomId",
            UomUnit = "Uom",
            ProductRemark = "Remark",
            POId = internalPurchaseOrderDataUtil.GetNewData().Id
        };
        public InternalPurchaseOrderItemViewModel GetNewDataViewModel() => new InternalPurchaseOrderItemViewModel
        {
            product = new ProductViewModel
            {
                _id = "ProductId",
                code = "ProductCode",
                name = "ProductName",
                uom = new UomViewModel
                {
                    _id = "UomId",
                    unit = "Uom",
                }
            },
            quantity = 10,
            productRemark = "Remark",
            poId = internalPurchaseOrderDataUtil.GetNewData().Id
        };
    }
}
