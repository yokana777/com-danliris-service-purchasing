using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel
{
    public class PurchaseRequestViewModel : BaseViewModel
    {
        public string no { get; set; }
        public DateTimeOffset date { get; set; }
        public DateTimeOffset expectedDeliveryDate { get; set; }
        public BudgetViewModel budget { get; set; }
        public UnitViewModel unit { get; set; }
        public CategoryViewModel category { get; set; }
        public bool isPosted { get; set; }
        public bool isUsed { get; set; }
        public string remark { get; set; }
        public bool @internal { get; set; }
        public List<PurchaseRequestItemViewModel> items { get; set; }
    }
}
