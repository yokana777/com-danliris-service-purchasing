using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentClosingDateViewModels
{
    public class GarmentClosingDateViewModel : BaseViewModel, IValidatableObject
    {
        public DateTimeOffset ClosingDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ClosingDate.Equals(DateTimeOffset.MinValue) || ClosingDate == null)
            {
                yield return new ValidationResult("ClosingDate harus diisi", new List<string> { "ClosingDate" });
            }
        }
    }
}
