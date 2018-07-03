using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote
{
    public class UnitReceiptNoteViewModel : BaseViewModel, IValidatableObject
    {
        public bool _deleted { get; set; }
        public SupplierViewModel supplier { get; set; }
        public string no { get; set; }
        public DateTimeOffset date { get; set; }
        public UnitViewModel unit { get; set; }
        public string pibNo { get; set; }
        public string incomeTaxNo { get; set; }
        public string doNo { get; set; }
        public List<UnitReceiptNoteItemViewModel> items { get; set; }

        public bool isStorage { get; set; }
        public string remark { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.date.Equals(DateTimeOffset.MinValue) || this.date == null)
            {
                yield return new ValidationResult("Date is required", new List<string> { "date" });
            }
            
            else
            {
                if (this.no == "" || this.no ==null)
                {
                    if (this.supplier.import == true)
                    {
                        this.no = "BPI";
                    }
                    else
                    {
                        this.no = "BPL";
                    }
                }
               
            }
            if (this.unit == null)
            {
                yield return new ValidationResult("Unit is required", new List<string> { "unitId" });
            }
            if (this.supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "supplier" });
            }

            int itemErrorCount = 0;

            if (this.items.Count.Equals(0))
            {
                yield return new ValidationResult("Items is required", new List<string> { "itemscount" });
            }
            else
            {
                string itemError = "[";

                foreach (UnitReceiptNoteItemViewModel item in items)
                {
                    itemError += "{";

                    if (item.product == null || string.IsNullOrWhiteSpace(item.product._id))
                    {
                        itemErrorCount++;
                        itemError += "product: 'Product is required', ";
                    }
                    else
                    {
                        var itemsExist = items.Where(i => i.product != null && item.product != null && i.product._id.Equals(item.product._id)).Count();
                        if (itemsExist > 1)
                        {
                            itemErrorCount++;
                            itemError += "product: 'Product is duplicate', ";
                        }
                    }

                    if (item.deliveredQuantity <= 0)
                    {
                        itemErrorCount++;
                        itemError += "quantity: 'Quantity should be more than 0'";
                    }

                    itemError += "}, ";
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "items" });
            }
        }
    }
}