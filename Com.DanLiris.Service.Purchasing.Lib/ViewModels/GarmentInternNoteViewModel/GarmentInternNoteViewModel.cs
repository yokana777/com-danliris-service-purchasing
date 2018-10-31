using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel
{
    public class GarmentInternNoteViewModel : BaseViewModel, IValidatableObject
    {
        public string inNo { get; set; }
        public DateTimeOffset inDate { get; set; }
        public string remark { get; set; }
        public CurrencyViewModel currency { get; set; }
        public SupplierViewModel supplier { get; set; }
        public string invoiceNoteNo { get; set; }
        public string hasUnitReceiptNote { get; set; }
        public List<GarmentInternNoteItemViewModel> items { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (currency == null)
            {
                yield return new ValidationResult("currency is required", new List<string> { "currency" });
            }
            if (supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "supplier" });
            }

            int itemErrorCount = 0;

            if (this.items == null || items.Count <= 0)
            {
                yield return new ValidationResult("Garment Invoice is required", new List<string> { "itemscount" });
            }
            else
            {
                string itemError = "[";

                foreach (var item in items)
                {
                    itemError += "{";

                    if (item.garmentInvoice == null)
                    {
                        itemErrorCount++;
                        itemError += "garmentInvoice: 'No Garment Invoice selected', ";
                    }

                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "items" });
            }
        }
    }
}
