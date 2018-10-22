using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels
{
    public class GarmentInvoiceItemViewModel : BaseViewModel
    {
        public Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel.GarmentDeliveryOrderViewModel DeliveryOrders { get; set; }
        public GarmentInvoiceDetailViewModel Details { get; set; }
    }
}
