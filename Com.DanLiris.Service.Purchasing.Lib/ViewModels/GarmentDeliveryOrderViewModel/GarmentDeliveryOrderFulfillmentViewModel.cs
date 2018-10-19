using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel
{
    public class GarmentDeliveryOrderFulfillmentViewModel : BaseViewModel
    {
        public long ePODetailId { get; set; }
        public long pOItemId { get; set; }
        public PurchaseOrder purchaseOrder { get; set; }
        public long pRItemId { get; set; }
        public string poSerialNumber { get; set; }

        public ProductViewModel product { get; set; }
        public double doQuantity { get; set; }
        public double dealQuantity { get; set; }
        public double conversion { get; set; }
        public UomViewModel purchaseOrderUom { get; set; } // UOM

        //public double receiptQuantity { get; set; }

        public double smallQuantity { get; set; }
        public UomViewModel smallUom { get; set; }

        public bool isClosed { get; set; }
        
        public double PricePerDealUnit { get; set; }
        public double PriceTotal { get; set; }

        public CurrencyViewModel currency { get; set; }
        public string remark { get; set; }
    }

    public class PurchaseOrder
    {
        public PurchaseRequest purchaseRequest { get; set; }
    }

    public class PurchaseRequest
    {
        public long _id { get; set; }
        public string no { get; set; }
        public UnitViewModel unit { get; set; }
    }
}
