using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel
{
    public class PurchaseRequestItemViewModel : BaseViewModel
    {
        public ProductViewModel product { get; set; }
        public long quantity { get; set; }
        public string remark { get; set; }
    }
}
