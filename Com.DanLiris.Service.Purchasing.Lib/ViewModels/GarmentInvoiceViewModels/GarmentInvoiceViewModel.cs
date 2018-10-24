using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels
{
    public class GarmentInvoiceViewModel : BaseViewModel//, IValidatableObject
    {
        public string invoiceNo { get; set; }
        public SupplierViewModel supplier { get; set; }
        public DateTimeOffset invoiceDate { get; set; }

        public CurrencyViewModel currency { get; set; }
        public string vatNo { get; set; }
        public string incomeTaxNo { get; set; }
        public bool useVat { get; set; }
        public bool useIncomeTax { get; set; }
        public bool isPayTax { get; set; }
        public DateTimeOffset incomeTaxDate { get; set; }
        public DateTimeOffset vatDate { get; set; }
        public List<GarmentInvoiceItemViewModel> items { get; set; }
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
