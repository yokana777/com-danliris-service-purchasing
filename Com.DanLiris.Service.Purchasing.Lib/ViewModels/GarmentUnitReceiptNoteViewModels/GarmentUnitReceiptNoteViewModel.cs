using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using IntegrationViewModel = Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using NewIntegrationViewModel = Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels
{
    public class GarmentUnitReceiptNoteViewModel : BaseViewModel, IValidatableObject
    {
        public string UId { get; set; }
        public string URNNo { get; set; }
        public string URNType { get; set; }

        public NewIntegrationViewModel.UnitViewModel Unit { get; set; }

        public NewIntegrationViewModel.SupplierViewModel Supplier { get; set; }

        public long? DOId { get; set; }
        public string DONo { get; set; }
        public bool IsInvoice { get; set; }
        public string DRId { get; set; }
        public string DRNo { get; set; }
        public long UENId { get; set; }
        public string UENNo { get; set; }
        public DateTimeOffset? ReceiptDate { get; set; }

        public bool IsStorage { get; set; }
        public IntegrationViewModel.StorageViewModel Storage { get; set; }

        public string Remark { get; set; }

        public bool IsCorrection { get; set; }

        public bool IsUnitDO { get; set; }

        public string DeletedReason { get; set; }

        public CurrencyViewModel DOCurrency { get; set; }

        public List<GarmentUnitReceiptNoteItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReceiptDate == null || ReceiptDate == DateTimeOffset.MinValue)
            {
                yield return new ValidationResult("ReceiptDate tidak boleh kosong.", new List<string> { "ReceiptDate" });
            }

            bool checkDO = true;
            if (Unit == null || string.IsNullOrWhiteSpace(Unit.Id))
            {
                yield return new ValidationResult("Unit tidak boleh kosong.", new List<string> { "Unit" });
                checkDO = false;
            }
            if ((Supplier == null || Supplier.Id == 0) && URNType=="PEMBELIAN")
            {
                yield return new ValidationResult("Supplier tidak boleh kosong.", new List<string> { "Supplier" });
                checkDO = false;
            }
            else if(URNType == "PROSES")
            {
                checkDO = false;
            }
            if (Storage == null || string.IsNullOrWhiteSpace(Storage._id))
            {
                yield return new ValidationResult("Storage tidak boleh kosong.", new List<string> { "Storage" });
            }

            if (DOId == null || DOId == 0)
            {
                if (checkDO)
                {
                    yield return new ValidationResult("Surat Jalan tidak boleh kosong.", new List<string> { "DeliveryOrder" });
                }
            }
            else if (Items == null || Items.Count <= 0)
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

                    if (item.ReceiptQuantity <= 0)
                    {
                        itemErrorCount++;
                        itemError += "ReceiptQuantity: 'Jumlah harus lebih dari 0', ";
                    }

                    if (item.Conversion <= 0)
                    {
                        itemErrorCount++;
                        itemError += "Conversion: 'Konversi harus lebih dari 0', ";
                    }
                    else if(item.Uom.Id == item.SmallUom.Id && item.Conversion != 1)
                    {
                        itemErrorCount++;
                        itemError += "Conversion: 'Satuan sama, Konversi harus 1', ";
                    }

                    if (string.IsNullOrWhiteSpace(item.DesignColor))
                    {
                        itemErrorCount++;
                        itemError += "DesignColor: 'Design/Color tidak boleh kosong', ";
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
