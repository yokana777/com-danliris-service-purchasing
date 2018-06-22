using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel
{
    public class ExternalPurchaseOrderViewModel : BaseViewModel, IValidatableObject
    {
        public string no { get; set; }
        public DivisionViewModel division { get; set; }
        public UnitViewModel unit { get; set; }
        public SupplierViewModel supplier { get; set; }
        public DateTimeOffset orderDate { get; set; }
        public DateTimeOffset deliveryDate { get; set; }
        public string freightCostBy { get; set; }
        public CurrencyViewModel currency { get; set; }
        public string paymentMethod { get; set; }
        public string paymentDueDays { get; set; }
        public bool useVat { get; set; }
        public IncomeTaxViewModel incomeTax { get; set; }
        public bool useIncomeTax { get; set; }
        public bool isPosted { get; set; }
        public bool isClosed { get; set; }
        public bool isCanceled { get; set; }
        public string remark { get; set; }
        public List<ExternalPurchaseOrderItemViewModel> items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.unit == null)
            {
                yield return new ValidationResult("Unit is required", new List<string> { "unit" });
            }
            if (this.supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "supplier" });
            }
            if (this.currency == null)
            {
                yield return new ValidationResult("Currency is required", new List<string> { "currency" });
            }
            if (this.orderDate.Equals(DateTimeOffset.MinValue) || this.orderDate == null)
            {
                yield return new ValidationResult("OrderDate is required", new List<string> { "orderDate" });
            }
            else if (this.deliveryDate != null && this.orderDate > this.deliveryDate)
            {
                yield return new ValidationResult("OrderDate is greater than delivery date", new List<string> { "orderDate" });
            }

            if (this.deliveryDate.Equals(DateTimeOffset.MinValue) || this.deliveryDate == null)
            {
                yield return new ValidationResult("Delivery Date is required", new List<string> { "deliveryDate" });
            }
            else if (this.deliveryDate != null && this.orderDate > this.deliveryDate)
            {
                yield return new ValidationResult("OrderDate is greater than delivery date", new List<string> { "deliveryDate" });
            }

            int itemErrorCount = 0;
            int detailErrorCount = 0;

            if (this.items.Count.Equals(0))
            {
                yield return new ValidationResult("Items is required", new List<string> { "itemscount" });
            }
            else
            {
                string externalPurchaseOrderItemError = "[";

                foreach (ExternalPurchaseOrderItemViewModel Item in items)
                {
                    externalPurchaseOrderItemError += "{ ";

                    if (string.IsNullOrWhiteSpace(Item.poNo))
                    {
                        itemErrorCount++;
                        externalPurchaseOrderItemError += "purchaseOrder: 'PurchaseRequest is required', ";
                    }

                    if (Item.details.Count.Equals(0))
                    {
                        yield return new ValidationResult("Details is required", new List<string> { "details" });
                    }
                    else
                    {

                        string externalPurchaseOrderDetailError = "[";

                        foreach (ExternalPurchaseOrderDetailViewModel Detail in Item.details)
                        {
                            externalPurchaseOrderDetailError += "{ ";

                            //if (Detail.DefaultUom.unit.Equals(Detail.DealUom.unit) && Detail.DefaultQuantity == Detail.DealQuantity && Detail.Convertion != 1)
                            if (Detail.defaultUom == null)
                            {
                                Detail.defaultUom = Detail.product.uom;
                            }
                            if (Detail.defaultUom.unit.Equals(Detail.dealUom.unit) && Detail.conversion != 1)
                            {
                                detailErrorCount++;
                                externalPurchaseOrderDetailError += "conversion: 'Conversion should be 1', ";
                            }

                            if (Detail.priceBeforeTax <= 0)
                            {
                                detailErrorCount++;
                                externalPurchaseOrderDetailError += "price: 'Price should be more than 0', ";
                            }

                            externalPurchaseOrderDetailError += " }, ";
                        }

                        externalPurchaseOrderDetailError += "]";

                        if (detailErrorCount > 0)
                        {
                            itemErrorCount++;
                            externalPurchaseOrderItemError += string.Concat("details: ", externalPurchaseOrderDetailError);
                        }

                    }
                    externalPurchaseOrderItemError += " }, ";
                }

                externalPurchaseOrderItemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(externalPurchaseOrderItemError, new List<string> { "items" });
            }
        
        }
    }
}
