using System;
using System.Collections.Generic;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchasingDispositionViewModel.EPODispositionLoader
{
    public class EPOViewModel : BaseViewModel
    {
        public string no { get; set; }
        public DivisionViewModel division { get; set; }
        public UnitViewModel unit { get; set; }
        public SupplierViewModel supplier { get; set; }
        public DateTimeOffset orderDate { get; set; }
        public DateTimeOffset deliveryDate { get; set; }
        public string freightCostBy { get; set; }
        public CurrencyViewModel currency { get; set; }
        public string paymentMethod { get; set; }
        public string paymentDueDays { get; set; }
        public bool useVat { get; set; }
        public IncomeTaxViewModel incomeTax { get; set; }
        public bool useIncomeTax { get; set; }
        public bool isPosted { get; set; }
        public bool isClosed { get; set; }
        public bool isCanceled { get; set; }
        public string remark { get; set; }
        public List<EPOItemViewModel> items { get; set; }
    }
}
