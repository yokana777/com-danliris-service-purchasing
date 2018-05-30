using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;

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
    }
}
