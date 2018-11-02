using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchasingDispositionViewModel
{
    public class PurchasingDispositionViewModel : BaseViewModel
    {
        public SupplierViewModel Supplier { get; set; }
        public string Bank { get; set; }
        public string ConfirmationOrderNo { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentMethod { get; set; }
        public int PaymentDueDays { get; set; }
        public string Calculation { get; set; }
        public string Remark { get; set; }
        public string ProformaNo { get; set; }
        public string Investation { get; set; }
        public double Amount { get; set; }
        public virtual List<PurchasingDispositionItemViewModel> Items { get; set; }
    }
}
