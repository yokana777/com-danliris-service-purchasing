using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentPOMasterDistributionViewModels
{
    public class GarmentPOMasterDistributionViewModel : BaseViewModel, IValidatableObject
    {
        public long DOId { get; set; }
        public string DONo { get; set; }

        public DateTimeOffset DODate { get; set; }

        public SupplierViewModel Supplier { get; set; }

        public List<GarmentPOMasterDistributionItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Supplier == null || Supplier.Id < 1)
            {
                yield return new ValidationResult("Supplier tidak boleh kosong", new List<string> { "Supplier" });
            }
            else if (DOId < 1 || string.IsNullOrWhiteSpace(DONo))
            {
                yield return new ValidationResult("Surat Jalan tidak boleh kosong", new List<string> { "DeliveryOrder" });
            }
            else if (Items == null || Items.Count < 1)
            {
                yield return new ValidationResult("Items tidak boleh kosong", new List<string> { "ItemsCount" });
            }
            else if (Items.All(i => i.Details == null || i.Details.Count < 1))
            {
                yield return new ValidationResult("Details tidak boleh kosong", new List<string> { "ItemsCount" });
            }
            else
            {
                int itemsErrorsCount = 0;
                string itemsErrors = "[";

                foreach (var item in Items)
                {
                    itemsErrors += "{";

                    int detailsErrorsCount = 0;
                    string detailsErrors = "\"Details\" : [";

                    if (item.Details != null && item.Details.Count > 0)
                    {
                        foreach (var detail in item.Details)
                        {
                            detailsErrors += "{";

                            if (detail.CostCalculationId < 1 || string.IsNullOrWhiteSpace(detail.RONo))
                            {
                                detailsErrorsCount++;
                                detailsErrors += "\"CostCalculation\": \"Harus diisi\", ";
                            }
                            else if (string.IsNullOrWhiteSpace(detail.POSerialNumber))
                            {
                                detailsErrorsCount++;
                                detailsErrors += "\"POSerialNumber\": \"Harus diisi\", ";
                            }
                            else
                            {
                                if (detail.Conversion <= 0)
                                {
                                    detailsErrorsCount++;
                                    detailsErrors += "\"Conversion\": \"Harus lebih dari 0\", ";
                                }

                                if (detail.Quantity <= 0)
                                {
                                    detailsErrorsCount++;
                                    detailsErrors += "\"Quantity\": \"Harus lebih dari 0\", ";
                                }
                                else
                                {
                                    //var defaultQuantity = decimal.Parse(((decimal)detail.Conversion * detail.QuantityCC).ToString("N2"));
                                    ////var defaultQuantity = Math.Round((decimal)detail.Conversion * detail.QuantityCC, 2, MidpointRounding.AwayFromZero);
                                    if (detail.Quantity > detail.QuantityCC)
                                    {
                                        detailsErrorsCount++;
                                        detailsErrors += $"\"Quantity\": \"Tidak boleh lebih dari {detail.QuantityCC}\", ";
                                    }
                                }
                            }

                            detailsErrors += "}, ";
                        }

                        detailsErrors += "], ";

                        if (detailsErrorsCount > 0)
                        {
                            itemsErrorsCount++;
                            itemsErrors += detailsErrors;
                        }

                        if (item.Details.Sum(d => d.Quantity * (decimal)d.Conversion) > (decimal)item.DOQuantity)
                        {
                            itemsErrorsCount++;
                            itemsErrors += $"\"TotalQuantity\": \"Tidak boleh lebih dari {item.DOQuantity}\", ";
                        }
                    }

                    itemsErrors += "}, ";
                }

                itemsErrors += "]";

                if (itemsErrorsCount > 0)
                {
                    yield return new ValidationResult(itemsErrors, new List<string> { "Items" });
                }
            }
        }
    }
}
