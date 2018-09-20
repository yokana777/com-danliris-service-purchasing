using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentPurchaseRequestViewModel
{
    public class GarmentPurchaseRequestViewModel : BaseViewModel, IValidatableObject
    {
        public string PRNo { get; set; }
        public string RONo { get; set; }

        public BuyerViewModel Buyer { get; set; }

        public string Article { get; set; }

        public DateTimeOffset? Date { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public DateTimeOffset? ShipmentDate { get; set; }

        public UnitViewModel Unit { get; set; }

        public bool IsPosted { get; set; }
        public bool IsUsed { get; set; }
        public string Remark { get; set; }

        public List<GarmentPurchaseRequestItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(String.IsNullOrWhiteSpace(RONo))
            {
                yield return new ValidationResult("RONo is required", new List<string> { "RONo" });
            }
            else
            {
                PurchasingDbContext dbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
                var duplicateRONo = dbContext.GarmentPurchaseRequests.Where(m => m.RONo.Equals(RONo) && m.Id != Id).Count();
                if (duplicateRONo > 0)
                {
                    yield return new ValidationResult("RONo is already exist", new List<string> { "RONo" });
                }
            }

            if (Buyer == null) {
                yield return new ValidationResult("Buyer is required", new List<string> { "Buyer" });
            }
            else if (String.IsNullOrWhiteSpace(Buyer.Id) || Buyer.Id.Equals("0") || String.IsNullOrWhiteSpace(Buyer.Code) || String.IsNullOrWhiteSpace(Buyer.Name))
            {
                yield return new ValidationResult("Buyer format is incorrect", new List<string> { "Buyer" });
            }

            if (String.IsNullOrWhiteSpace(Article))
            {
                yield return new ValidationResult("Article is required", new List<string> { "Article" });
            }

            if (Date.Equals(DateTimeOffset.MinValue) || Date == null)
            {
                yield return new ValidationResult("Date is required", new List<string> { "Date" });
            }
            if (ShipmentDate.Equals(DateTimeOffset.MinValue) || ShipmentDate == null)
            {
                yield return new ValidationResult("ShipmentDate is required", new List<string> { "ShipmentDate" });
            }

            if (Unit == null)
            {
                yield return new ValidationResult("Unit is required", new List<string> { "Unit" });
            }
            else if (String.IsNullOrWhiteSpace(Unit.Id) || Unit.Id.Equals("0") || String.IsNullOrWhiteSpace(Unit.Code) || String.IsNullOrWhiteSpace(Unit.Name))
            {
                yield return new ValidationResult("Unit format is incorrect", new List<string> { "Unit" });
            }

            if(Items == null || Items.Count < 1)
            {
                yield return new ValidationResult("Items is required", new List<string> { "ItemsCount" });
            }
            else
            {
                string itemError = "[";
                int itemErrorCount = 0;

                foreach (var item in Items)
                {
                    itemError += "{";

                    if(String.IsNullOrWhiteSpace(item.PO_SerialNumber))
                    {
                        itemErrorCount++;
                        itemError += "PO_SerialNumber: 'PO_SerialNumber is required', ";
                    }

                    if (item.Product == null)
                    {
                        itemErrorCount++;
                        itemError += "Product: 'Product is required', ";
                    }
                    else if (String.IsNullOrWhiteSpace(item.Product.Id) || item.Product.Id.Equals("0") || String.IsNullOrWhiteSpace(item.Product.Code) || String.IsNullOrWhiteSpace(item.Product.Name))
                    {
                        itemErrorCount++;
                        itemError += "Product: 'Product is incorrect format', ";
                    }

                    if(item.Quantity < 1)
                    {
                        itemErrorCount++;
                        itemError += "Quantity: 'Quantity should more than 0', ";
                    }

                    if (item.BudgetPrice < 1)
                    {
                        itemErrorCount++;
                        itemError += "BudgetPrice: 'BudgetPrice should more than 0', ";
                    }

                    if (item.Uom == null)
                    {
                        itemErrorCount++;
                        itemError += "UOM: 'UOM is required', ";
                    }
                    else if (String.IsNullOrWhiteSpace(item.Uom.Id) || item.Uom.Id.Equals("0") || String.IsNullOrWhiteSpace(item.Uom.Unit))
                    {
                        itemErrorCount++;
                        itemError += "UOM: 'UOM is incorrect format', ";
                    }

                    if (item.Category == null)
                    {
                        itemErrorCount++;
                        itemError += "Category: 'Category is required', ";
                    }
                    else if (String.IsNullOrWhiteSpace(item.Category.Id) || item.Category.Id.Equals("0") || String.IsNullOrWhiteSpace(item.Category.Name))
                    {
                        itemErrorCount++;
                        itemError += "Category: 'Category is incorrect format', ";
                    }

                    itemError += "}, ";
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "Items" });
            }
        }
    }
}
