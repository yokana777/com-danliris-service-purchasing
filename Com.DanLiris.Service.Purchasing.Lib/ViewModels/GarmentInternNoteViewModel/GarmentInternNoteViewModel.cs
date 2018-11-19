using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
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
        public List<GarmentInternNoteItemViewModel> items { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            PurchasingDbContext dbContext = validationContext == null ? null : (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));

            if (currency == null)
            {
                yield return new ValidationResult("currency is required", new List<string> { "currency" });
            }
            if (supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "supplier" });
            }

            int itemErrorCount = 0;
            int detailErrorCount = 0;

            if (this.items == null || items.Count <= 0)
            {
                yield return new ValidationResult("Item is required", new List<string> { "itemscount" });
            }
            else
            {
                string itemError = "[";
                bool? useincometax= null;
                bool? usevat = null;
                string paymentMethod = "";

                foreach (var item in items)
                {
                    itemError += "{";

                    if (item.garmentInvoice == null || item.garmentInvoice.Id == 0)
                    {
                        itemErrorCount++;
                        itemError += "garmentInvoice: 'No Garment Invoice selected', ";
                    }
                    var invoice = dbContext.GarmentInvoices.Single(m => m.Id == item.garmentInvoice.Id);

                    if (useincometax != null && useincometax != invoice.UseIncomeTax)
                    {
                        itemErrorCount++;
                        itemError += "useincometax: 'UseIncomeTax harus sama', ";
                    }
                    useincometax = invoice.UseIncomeTax;
                    if (usevat != null && usevat != invoice.UseVat)
                    {
                        itemErrorCount++;
                        itemError += "usevat: 'UseVat harus sama', ";
                    }
                    usevat = invoice.UseVat;
                    if (item.details == null || item.details.Count.Equals(0))
                    {
                        itemErrorCount++;
                        itemError += "detailscount: 'Details is required', ";
                    }
                    else
                    {
                        string detailError = "[";

                        foreach (var detail in item.details)
                        {
                            detailError += "{";
                            var deliveryOrder = dbContext.GarmentDeliveryOrders.Single(d => d.Id == detail.deliveryOrder.Id);
                            if (paymentMethod != "" && paymentMethod != deliveryOrder.PaymentMethod)
                            {
                                detailErrorCount++;
                                detailError += "paymentMethod: 'TermOfPayment Harus Sama', ";
                            }
                            paymentMethod = deliveryOrder.PaymentMethod;

                            detailError += "}, ";
                        }

                        detailError += "]";

                        if (detailErrorCount > 0)
                        {
                            itemErrorCount++;
                            itemError += $"details: {detailError}, ";
                        }
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
