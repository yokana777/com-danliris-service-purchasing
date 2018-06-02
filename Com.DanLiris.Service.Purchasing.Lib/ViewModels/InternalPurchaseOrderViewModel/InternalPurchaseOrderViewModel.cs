using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel
{
    public class InternalPurchaseOrderViewModel : BaseViewModel, IValidatableObject
    {
        public string poNo { get; set; }
        public string isoNo { get; set; }
        public string prId { get; set; }
        public string prNo { get; set; }
        public DateTimeOffset prDate { get; set; }
        public DateTimeOffset expectedDeliveryDate { get; set; }
        public BudgetViewModel budget { get; set; }
        public DivisionViewModel division { get; set; }
        public UnitViewModel unit { get; set; }
        public CategoryViewModel category { get; set; }
        public string remark { get; set; }
        public bool isPosted { get; set; }
        public bool isClosed { get; set; }
        public string status { get; set; }
        public List<InternalPurchaseOrderItemViewModel> items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.prNo == null)
            {
                yield return new ValidationResult("No. PR is required", new List<string> { "prNo" });
            }
            if (this.items.Count.Equals(0))
            {
                yield return new ValidationResult("Items is required", new List<string> { "itemscount" });
            }
        }
    }
}