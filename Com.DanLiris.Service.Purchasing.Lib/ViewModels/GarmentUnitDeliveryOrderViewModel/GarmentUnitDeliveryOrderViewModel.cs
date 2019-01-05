using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel
{
    public class GarmentUnitDeliveryOrderViewModel : BaseViewModel, IValidatableObject
    {
        public string UnitDOType { get; set; }
        public DateTimeOffset UnitDODate { get; set; }
        public string UnitDONo { get; set; }

        public UnitViewModel UnitRequest { get; set; }

        public UnitViewModel UnitSender { get; set; }
        
        public IntegrationViewModel.StorageViewModel Storage { get; set; }
        public string RONo { get; set; }
        public string Article { get; set; }
        public bool IsUsed { get; set; }
        public long DOId { get; set; }

        public string DONo { get; set; }
        public long CorrectionId { get; set; }
        public string CorrectionNo { get; set; }

        public List<GarmentUnitDeliveryOrderItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UnitDODate.Equals(DateTimeOffset.MinValue) || UnitDODate == null)
            {
                yield return new ValidationResult("UnitDODate is required", new List<string> { "UnitDODate" });
            }
            if (UnitRequest == null)
            {
                yield return new ValidationResult("UnitRequest is required", new List<string> { "UnitRequest" });
            }
            if (UnitSender == null)
            {
                yield return new ValidationResult("UnitSender is required", new List<string> { "UnitSender" });
            }
            if (Storage == null)
            {
                yield return new ValidationResult("Storage is required", new List<string> { "Storage" });
            }
            if (RONo == null)
            {
                yield return new ValidationResult("RONo is required", new List<string> { "RONo" });
            }
            if (Article == null)
            {
                yield return new ValidationResult("Article is required", new List<string> { "Article" });
            }

            int itemErrorCount = 0;

            if (this.Items == null || Items.Count <= 0)
            {
                yield return new ValidationResult("Item is required", new List<string> { "ItemsCount" });
            }
            else
            {
                string itemError = "[";

                foreach (var item in Items)
                {
                    itemError += "{";

                    if (item.Quantity == 0)
                    {
                        itemErrorCount++;
                        itemError += "Quantity: 'Jumlah tidk boleh 0', ";
                    }

                    //if (item.Product == null)
                    //{
                    //    itemErrorCount++;
                    //    itemError += "Product: 'Product tidk boleh kosong', ";
                    //}
                    itemError += "}, ";
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "items" });
            }
        }
    }
}
