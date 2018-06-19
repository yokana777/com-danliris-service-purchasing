using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder
{
    public class PurchaseOrderViewModel
    {
        public CategoryViewModel category { get; set; }
        public bool useIncomeTax { get; set; }
    }
}