using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel
{
    public class GarmentUnitDeliveryOrderViewModel : BaseViewModel, IValidatableObject
    {
        public string UnitDOType { get; set; }
        public string UnitDONo { get; set; }
        public DateTimeOffset UnitDODate { get; set; }

        public UnitViewModel UnitRequest { get; set; }

        public UnitViewModel UnitSender { get; set; }
        
        public IntegrationViewModel.StorageViewModel Storage { get; set; }
        public IntegrationViewModel.StorageViewModel StorageRequest { get; set; }

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
                yield return new ValidationResult("Tgl. Delivery Order harus diisi", new List<string> { "UnitDODate" });
            }

            if (UnitRequest == null || string.IsNullOrWhiteSpace(UnitRequest.Id))
            {
                yield return new ValidationResult("Unit yang meminta haris diisi", new List<string> { "UnitRequest" });
            }

            if (UnitDOType == "TRANSFER" && (StorageRequest == null || string.IsNullOrWhiteSpace(StorageRequest._id)))
            {
                yield return new ValidationResult("Gudang yang meminta harus diisi", new List<string> { "StorageRequest" });
            }

            if (UnitDOType == "TRANSFER" && (UnitSender == null || string.IsNullOrWhiteSpace(UnitSender.Id)))
            {
                yield return new ValidationResult("Unit yang mengirim harus diisi", new List<string> { "UnitSender" });
            }

            if (UnitDOType == "TRANSFER" && UnitSender.Id == UnitRequest.Id)
            {
                yield return new ValidationResult("Unit yang meminta dan Unit yang mengirim tidak boleh sama", new List<string> { "UnitSender" });
            }

            if (Storage == null)
            {
                yield return new ValidationResult("Gudang yang mengirim harus diisi", new List<string> { "Storage" });
            }

            if (string.IsNullOrWhiteSpace(RONo))
            {
                yield return new ValidationResult("No RO harus diisi", new List<string> { "RONo" });
            }
            else
            {
                int itemErrorCount = 0;

                if (Items == null || Items.Count(i => i.IsSave) <= 0)
                {
                    yield return new ValidationResult("Item is required", new List<string> { "ItemsCount" });
                }
                else
                {
                    string itemError = "[";

                    foreach (var item in Items)
                    {
                        itemError += "{";

                        if (item.IsSave)
                        {
                            if (item.Quantity == 0)
                            {
                                itemErrorCount++;
                                itemError += "Quantity: 'Jumlah tidak boleh 0', ";
                            }
                            else
                            {
                                PurchasingDbContext dbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
                                var URNItem = dbContext.GarmentUnitReceiptNoteItems.AsNoTracking().FirstOrDefault(x => x.Id == item.URNItemId);
                                if (URNItem != null)
                                {
                                    var UDOItem = dbContext.GarmentUnitDeliveryOrderItems.AsNoTracking().FirstOrDefault(x => x.Id == item.Id);
                                    var quantity = URNItem.SmallQuantity - URNItem.OrderQuantity + (decimal)(UDOItem != null ? UDOItem.Quantity : 0);
                                    if ((decimal)item.Quantity > quantity)
                                    {
                                        itemErrorCount++;
                                        itemError += $"Quantity: 'Jumlah tidak boleh lebih dari {quantity}', ";
                                    }
                                }
                            }
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
}
