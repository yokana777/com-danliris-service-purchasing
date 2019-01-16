using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel
{
    public class GarmentUnitExpenditureNoteViewModel : BaseViewModel, IValidatableObject
    {
        public string UENNo { get; set; }
        public DateTimeOffset ExpenditureDate { get; set; }
        public string ExpenditureType { get; set; }
        public string ExpenditureTo { get; set; }
        public long UnitDOId { get; set; }
        public string UnitDONo { get; set; }

        //public GarmentUnitDeliveryOrderViewModel GarmentUnitDO { get; set; }

        public UnitViewModel UnitRequest { get; set; }

        public UnitViewModel UnitSender { get; set; }
        public IntegrationViewModel.StorageViewModel Storage { get; set; }
        public IntegrationViewModel.StorageViewModel StorageRequest { get; set; }

        public List<GarmentUnitExpenditureNoteItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IGarmentUnitDeliveryOrder unitDeliveryOrderFacade = validationContext == null ? null : (IGarmentUnitDeliveryOrder)validationContext.GetService(typeof(IGarmentUnitDeliveryOrder));

            if (ExpenditureDate.Equals(DateTimeOffset.MinValue) || ExpenditureDate == null)
            {
                yield return new ValidationResult("Tanggal Pengeluaran Diperlukan", new List<string> { "ExpenditureDate" });
            }
            if (UnitDONo == null)
            {
                yield return new ValidationResult("Nomor Delivery Order Diperlukan", new List<string> { "UnitDONo" });
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
                    var unitDO = unitDeliveryOrderFacade.ReadById((int)UnitDOId);
                    if (unitDO!=null)
                    {
                        var unitDOItem = unitDO.Items.Where(s => s.Id == item.UnitDOItemId).FirstOrDefault();

                        if (item.Quantity > unitDOItem.Quantity)
                        {
                            itemErrorCount++;
                            itemError += "Quantity: 'Jumlah tidak boleh lebih dari yang ditampilkan', ";
                        }
                    }
                    if (item.Quantity <= 0)
                    {
                        itemErrorCount++;
                        itemError += "Quantity: 'Jumlah harus lebih dari 0', ";
                    }
                    //else if (item.Quantity > item.OldQuantity)
                    //{
                    //    itemErrorCount++;
                    //    itemError += "Quantity: 'Jumlah tidak boleh lebih dari yang ditampilkan', ";
                    //}
                    itemError += "}, ";
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "Items" });
            }
        }
    }
}
