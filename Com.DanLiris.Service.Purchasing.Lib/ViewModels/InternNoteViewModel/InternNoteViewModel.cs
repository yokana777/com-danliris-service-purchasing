using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternNoteViewModel
{
    public class InternNoteViewModel : BaseViewModel, IValidatableObject
    {
        public string inNo { get; set; }
        public string remark { get; set; }
        public CurrencyViewModel currency { get; set; }
        public SupplierViewModel supplier { get; set; }
        public List<InternNoteItemViewModel> items { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
