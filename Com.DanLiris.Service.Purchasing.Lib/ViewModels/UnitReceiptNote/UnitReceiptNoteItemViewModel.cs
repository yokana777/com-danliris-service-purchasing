using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote
{
    public class UnitReceiptNoteItemViewModel
    {
        public double deliveredQuantity { get; set; }
        public double pricePerDealUnit { get; set; }
        public double currencyRate { get; set; }
        public ProductViewModel product { get; set; }
        public PurchaseOrderViewModel purchaseOrder { get; set; }
    }
}