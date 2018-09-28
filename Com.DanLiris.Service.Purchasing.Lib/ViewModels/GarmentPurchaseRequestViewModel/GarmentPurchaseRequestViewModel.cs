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
            PurchasingDbContext dbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));

            if (String.IsNullOrWhiteSpace(RONo))
            {
                yield return new ValidationResult("RONo tidak boleh kosong", new List<string> { "RONo" });
            }
            else
            {
                var duplicateRONo = dbContext.GarmentPurchaseRequests.Where(m => m.RONo.Equals(RONo) && m.Id != Id).Count();
                if (duplicateRONo > 0)
                {
                    yield return new ValidationResult("RONo sudah ada", new List<string> { "RONo" });
                }
            }

            if (Buyer == null) {
                yield return new ValidationResult("Buyer tidak boleh kosong", new List<string> { "Buyer" });
            }
            else if (String.IsNullOrWhiteSpace(Buyer.Id) || Buyer.Id.Equals("0") || String.IsNullOrWhiteSpace(Buyer.Code) || String.IsNullOrWhiteSpace(Buyer.Name))
            {
                yield return new ValidationResult("Data Buyer tidak benar", new List<string> { "Buyer" });
            }

            if (String.IsNullOrWhiteSpace(Article))
            {
                yield return new ValidationResult("Article tidak boleh kosong", new List<string> { "Article" });
            }

            if (Date.Equals(DateTimeOffset.MinValue) || Date == null)
            {
                yield return new ValidationResult("Date tidak boleh kosong", new List<string> { "Date" });
            }
            if (ShipmentDate.Equals(DateTimeOffset.MinValue) || ShipmentDate == null)
            {
                yield return new ValidationResult("ShipmentDate tidak boleh kosong", new List<string> { "ShipmentDate" });
            }

            if (Unit == null)
            {
                yield return new ValidationResult("Unit tidak boleh kosong", new List<string> { "Unit" });
            }
            else if (String.IsNullOrWhiteSpace(Unit.Id) || Unit.Id.Equals("0") || String.IsNullOrWhiteSpace(Unit.Code) || String.IsNullOrWhiteSpace(Unit.Name))
            {
                yield return new ValidationResult("Data Unit tidak benar", new List<string> { "Unit" });
            }

            if(Items == null || Items.Count < 1)
            {
                yield return new ValidationResult("Items tidak boleh kosong", new List<string> { "ItemsCount" });
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
                        itemError += "PO_SerialNumber: 'PO_SerialNumber tidak boleh kosong', ";
                    }
                    else if(Id != 0)
                    {
                        var duplicatePO_SerialNumber = dbContext.GarmentPurchaseRequests
                            .SingleOrDefault(m => m.Id == Id && m.Items.Any(i => i.PO_SerialNumber.Equals(item.PO_SerialNumber) && i.Id != item.Id));
                        if (duplicatePO_SerialNumber != null)
                        {
                            itemErrorCount++;
                            itemError += "PO_SerialNumber: 'PO_SerialNumber sudah ada', ";
                        }
                    }

                    if (item.Product == null)
                    {
                        itemErrorCount++;
                        itemError += "Product: 'Product tidak boleh kosong', ";
                    }
                    else if (String.IsNullOrWhiteSpace(item.Product.Id) || item.Product.Id.Equals("0") || String.IsNullOrWhiteSpace(item.Product.Code) || String.IsNullOrWhiteSpace(item.Product.Name))
                    {
                        itemErrorCount++;
                        itemError += "Product: 'Data Product tidak benar', ";
                    }

                    if(item.Quantity < 1)
                    {
                        itemErrorCount++;
                        itemError += "Quantity: 'Quantity harus lebih dari 0', ";
                    }

                    //if (item.BudgetPrice < 1)
                    //{
                    //    itemErrorCount++;
                    //    itemError += "BudgetPrice: 'BudgetPrice harus lebih dari 0', ";
                    //}

                    if (item.Uom == null)
                    {
                        itemErrorCount++;
                        itemError += "UOM: 'UOM tidak boleh kosong', ";
                    }
                    else if (String.IsNullOrWhiteSpace(item.Uom.Id) || item.Uom.Id.Equals("0") || String.IsNullOrWhiteSpace(item.Uom.Unit))
                    {
                        itemErrorCount++;
                        itemError += "UOM: 'Data UOM tidak benar', ";
                    }

                    if (item.Category == null)
                    {
                        itemErrorCount++;
                        itemError += "Category: 'Category tidak boleh kosong', ";
                    }
                    else if (String.IsNullOrWhiteSpace(item.Category.Id) || item.Category.Id.Equals("0") || String.IsNullOrWhiteSpace(item.Category.Name))
                    {
                        itemErrorCount++;
                        itemError += "Category: 'Data Category tidak benar', ";
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
