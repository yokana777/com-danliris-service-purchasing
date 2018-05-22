using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Master;
using System;
using System.Collections.Generic;
using System.Text;

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