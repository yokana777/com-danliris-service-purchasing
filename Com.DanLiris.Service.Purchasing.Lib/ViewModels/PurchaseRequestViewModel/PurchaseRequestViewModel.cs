using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel
{
    public class PurchaseRequestViewModel : BaseViewModel, IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.date.Equals(DateTimeOffset.MinValue))
            {
                yield return new ValidationResult("Date is required", new List<string> { "date" });
            }
            if (this.budget == null)
            {
                yield return new ValidationResult("Budget is required", new List<string> { "budget" });
            }
            if (this.unit == null)
            {
                yield return new ValidationResult("Unit is required", new List<string> { "unit" });
            }
            if (this.category == null)
            {
                yield return new ValidationResult("Category is required", new List<string> { "category" });
            }

            if (this.items.Count.Equals(0))
            {
                yield return new ValidationResult("Items is required", new List<string> { "itemscount" });
            }
        }
    }
}
