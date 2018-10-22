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
        public string InvoiceNo { get; set; }
        public SupplierViewModel Suppliers { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public string VatNo { get; set; }
        public string IncomeTaxNo { get; set; }
        public bool UseVat { get; set; }
        public bool UseIncomeTax { get; set; }
        public bool IsPayTax { get; set; }
        public DateTimeOffset IncomeTaxDate { get; set; }
        public DateTimeOffset VatDate { get; set; }
        public List<GarmentInvoiceItemViewModel> items { get; set; }
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
