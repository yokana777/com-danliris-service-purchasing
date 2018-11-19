using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentCorrectionNoteViewModel
{
    public class GarmentCorrectionNoteViewModel : BaseViewModel, IValidatableObject
    {
        public string CorrectionNo { get; set; }
        public DateTimeOffset CorrectionDate { get; set; }
        public string CorrectionType { get; set; }

        public long DOId { get; set; }
        public string DONo { get; set; }

        public SupplierViewModel Supplier { get; set; }

        public CurrencyViewModel Currency { get; set; }

        public bool UseVat { get; set; }
        public bool UseIncomeTax { get; set; }

        public IncomeTaxViewModel IncomeTax { get; set; }

        public string Remark { get; set; }

        public decimal TotalCorrection { get; set; }

        public List<GarmentCorrectionNoteItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(CorrectionType) || (CorrectionType.ToUpper() != "HARGA SATUAN" && CorrectionType.ToUpper() != "HARGA TOTAL"))
            {
                yield return new ValidationResult("Jenis Koreksi harus berupa 'Harga Satuan' atau 'Harga Total'", new List<string> { "CorrectionType" });
            }

            if (string.IsNullOrWhiteSpace(DONo))
            {
                yield return new ValidationResult("Nomor Surat Jalan tidak boleh kosong", new List<string> { "DONo" });
            }
            else if ((CorrectionType ?? "").ToUpper() == "HARGA SATUAN" || (CorrectionType ?? "").ToUpper() == "HARGA TOTAL")
            {
                if (Items == null || Items.Count < 1)
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

                        if (CorrectionType.ToUpper() == "HARGA SATUAN")
                        {
                            if (item.PricePerDealUnitAfter < 0)
                            {
                                itemErrorCount++;
                                itemError += "PricePerDealUnit: 'Harga Satuan tidak boleh kurang dari 0', ";
                            }
                            else if (item.PricePerDealUnitAfter == item.PricePerDealUnitBefore)
                            {
                                itemErrorCount++;
                                itemError += "PricePerDealUnit: 'Harga Satuan tidak berubah', ";
                            }
                        }
                        else if (CorrectionType.ToUpper() == "HARGA TOTAL")
                        {
                            if (item.PriceTotalAfter < 0)
                            {
                                itemErrorCount++;
                                itemError += "PriceTotal: 'Harga Total tidak boleh kurang dari 0', ";
                            }
                            else if (item.PriceTotalAfter == item.PriceTotalBefore)
                            {
                                itemErrorCount++;
                                itemError += "PriceTotal: 'Harga Total tidak berubah', ";
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
