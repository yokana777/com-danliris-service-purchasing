using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel
{
    public class GarmentDeliveryOrderItemViewModel : BaseViewModel
    {
        public PurchaseOrderExternal purchaseOrderExternal { get; set; }
        public List<GarmentDeliveryOrderFulfillmentViewModel> fulfillments { get; set; }
    }

    public class PurchaseOrderExternal
    {
        public long _id { get; set; }
        public string no { get; set; }
    }
}
