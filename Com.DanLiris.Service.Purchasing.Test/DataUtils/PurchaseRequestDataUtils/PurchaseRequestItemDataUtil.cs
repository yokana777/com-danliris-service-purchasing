using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils
{
    public class PurchaseRequestItemDataUtil
    {
        public PurchaseRequestItem GetNewData() => new PurchaseRequestItem
        {
            ProductId = "ProductId",
            ProductCode = "ProductCode",
            ProductName = "ProductName",
            Quantity = 10,
            UomId = "UomId",
            Uom = "Uom",
            Remark = "Remark"
        };
        public PurchaseRequestItemViewModel GetNewDataViewModel() => new PurchaseRequestItemViewModel
        {
            product = new ProductViewModel
            {
                _id = "ProductId",
                code  = "ProductCode",
                name  = "ProductName",
                uom = new UomViewModel
                {
                    _id = "UomId",
                    unit = "Uom",
                }
            },
            quantity = 10,
            remark = "Remark"
        };
    }
}
