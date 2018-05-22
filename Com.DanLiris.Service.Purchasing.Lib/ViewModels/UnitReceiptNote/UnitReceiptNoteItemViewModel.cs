using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Master;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote
{
    public class UnitReceiptNoteItemViewModel
    {
        public double DeliveredQuantity { get; set; }
        public double PricePerDealUnit { get; set; }
        public double CurrencyRate { get; set; }
        public ProductViewModel Product { get; set; }
        public PurchaseOrderViewModel PurchaseOrder { get; set; }
    }
}
