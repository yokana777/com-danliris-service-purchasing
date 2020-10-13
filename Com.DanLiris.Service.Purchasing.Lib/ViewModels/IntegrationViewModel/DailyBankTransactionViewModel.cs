using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel
{
    public class DailyBankTransactionViewModel : BaseViewModel
    {
        public NewIntegrationViewModel.AccountBankViewModel Bank { get; set; }
        public string Code { get; set; }
        public NewIntegrationViewModel.BuyerViewModel Buyer { get; set; }
        public DateTimeOffset? Date { get; set; }
        public double? Nominal { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceType { get; set; }
        public string Remark { get; set; }
        public string SourceType { get; set; }
        public string Status { get; set; }
        public NewIntegrationViewModel.NewSupplierViewModel Supplier { get; set; }
        public double? AfterNominal { get; set; }
        public double? BeforeNominal { get; set; }
        public bool IsPosted { get; set; }
    }
}
