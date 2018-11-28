using Com.DanLiris.Service.Purchasing.Lib.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchasingDispositionViewModel
{
    public class PurchasingDispositionUpdatePositionPostedViewModel : IValidatableObject
    {
        public List<string> PurchasingDispositionNoes { get; set; }

        public ExpeditionPosition Position { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(PurchasingDispositionNoes.Count == 0)
            {
                yield return new ValidationResult("Purchasing Disposition No tidak boleh kosong", new List<string> { "PurchasingDispositionNoes" });
            }
        }
    }
}
