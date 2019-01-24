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

        public UnitViewModel UnitRequest { get; set; }

        public UnitViewModel UnitSender { get; set; }
        public IntegrationViewModel.StorageViewModel Storage { get; set; }
        public IntegrationViewModel.StorageViewModel StorageRequest { get; set; }

        public List<GarmentUnitExpenditureNoteItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IGarmentUnitDeliveryOrderFacade unitDeliveryOrderFacade = validationContext == null ? null : (IGarmentUnitDeliveryOrderFacade)validationContext.GetService(typeof(IGarmentUnitDeliveryOrderFacade));
            IGarmentUnitExpenditureNoteFacade unitExpenditureNoteFacade = validationContext == null ? null : (IGarmentUnitExpenditureNoteFacade)validationContext.GetService(typeof(IGarmentUnitExpenditureNoteFacade));

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

                    if (unitDO != null)
                    {
                        var unitDOItem = unitDO.Items.Where(s => s.Id == item.UnitDOItemId).FirstOrDefault();
                        if (unitDOItem != null)
                        {
                            if (item.Quantity > unitDOItem.Quantity)
                            {
                                itemErrorCount++;
                                itemError += "Quantity: 'Jumlah tidak boleh lebih dari yang ditampilkan', ";
                            }
                        }
                    }

                    var expenditureNote = unitExpenditureNoteFacade.ReadById((int)Id);

                    if (expenditureNote != null)
                    {
                        var expenditureNoteItem = expenditureNote.Items.FirstOrDefault(f => f.Id == item.Id);
                        if (expenditureNoteItem != null)
                        {
                            if (item.Quantity > expenditureNoteItem.Quantity)
                            {
                                itemErrorCount++;
                                itemError += "Quantity: 'Jumlah tidak boleh lebih dari yang ditampilkan', ";
                            }
                        }
                    }

                    if (item.Quantity <= 0)
                    {
                        itemErrorCount++;
                        itemError += "Quantity: 'Jumlah harus lebih dari 0', ";
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
