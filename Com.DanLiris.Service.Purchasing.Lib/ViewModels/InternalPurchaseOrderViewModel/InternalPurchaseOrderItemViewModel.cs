using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel
{
    public class InternalPurchaseOrderItemViewModel : BaseViewModel
    {
        public string prItemId { get; set; }
        public ProductViewModel product { get; set; }
        public long quantity { get; set; }
        //public string UomId { get; set; }
        //public string UomUnit { get; set; }        
        public string productRemark { get; set; }
        public string status { get; set; }
        public long poId { get; set; }
    }
}
