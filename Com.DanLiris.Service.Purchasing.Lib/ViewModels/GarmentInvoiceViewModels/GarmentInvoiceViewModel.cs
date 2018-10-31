using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels
{
    public class GarmentInvoiceViewModel : BaseViewModel, IValidatableObject
    {
        public string invoiceNo { get; set; }
        public SupplierViewModel supplier { get; set; }
        public DateTimeOffset invoiceDate { get; set; }
        public CurrencyViewModel currency { get; set; }
        public string vatNo { get; set; }
        public string incomeTaxNo { get; set; }
        public bool useVat { get; set; }
        public bool useIncomeTax { get; set; }
        public bool isPayTax { get; set; }
        public DateTimeOffset incomeTaxDate { get; set; }
		public bool hasInternNote { get; set; }
        public DateTimeOffset vatDate { get; set; }
        public List<GarmentInvoiceItemViewModel> items { get; set; }
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (string.IsNullOrWhiteSpace(invoiceNo))
			{
				yield return new ValidationResult("No is required", new List<string> { "invoiceNo" });
			}

			if (invoiceDate.Equals(DateTimeOffset.MinValue) || invoiceDate == null)
			{
				yield return new ValidationResult("Date is required", new List<string> { "invoiceDate" });
			}
			if ( currency == null)
			{
				yield return new ValidationResult("Currency is required", new List<string> { "currency" });
			}
			if (supplier == null)
			{
				yield return new ValidationResult("Supplier is required", new List<string> { "supplier" });
			}
			if (useVat == true)
			{
				if (string.IsNullOrWhiteSpace(vatNo) || vatNo == null)
				{
					yield return new ValidationResult("No is required", new List<string> { "vatNo" });
				}
				if (vatDate.Equals(DateTimeOffset.MinValue) || vatDate == null)
				{
					yield return new ValidationResult("Date is required", new List<string> { "vatDate" });
				}
			}
			if (useIncomeTax == true)
			{
				if (string.IsNullOrWhiteSpace(incomeTaxNo) || incomeTaxNo == null)
				{
					yield return new ValidationResult("No is required", new List<string> { "incomeTaxNo" });
				}
				if (incomeTaxDate.Equals(DateTimeOffset.MinValue) || incomeTaxDate == null)
				{
					yield return new ValidationResult("Date is required", new List<string> { "incomeTaxDate" });
				}
			}
			int itemErrorCount = 0;
			int detailErrorCount = 0;

			if (this.items.Count.Equals(0))
			{
				yield return new ValidationResult("DeliveryOrder is required", new List<string> { "itemscount" });
			}
			else
			{
				string itemError = "[";
				var pphError = 0;
				var ppnError=0;
				foreach (var item in items)
				{
					itemError += "{";
					 if(currency !=null)
					{
						if(currency.Code != item.currency)
						{
							itemErrorCount++;
							itemError += "deliveryOrder: 'DO Currency "+ item.currency +" different from selected currency', ";
							 
						}

					}
					if (item.deliveryOrder == null)
					{
						itemErrorCount++;
						itemError += "deliveryOrder: 'No deliveryOrder selected', ";
					}
					else if (items.Count(i => i.deliveryOrder._id == item.deliveryOrder._id) > 1 && _id == 0)
					{
						itemErrorCount++;
						itemError += "deliveryOrder: 'Data sudah ada', ";
					}
					else if (item.details == null || item.details.Count.Equals(0))
					{
						itemErrorCount++;
						itemError += "detailscount: 'Details is required', ";
					}
					else
					{
						string detailError = "[";

						foreach (var detail in item.details)
						{
							detailError += "{";
						    
							if (detail.doQuantity == 0)
							{
								detailErrorCount++;
								detailError += "doQuantity: 'DOQuantity can not 0', ";
							}
							if (detail.useIncomeTax == true)
							{
								if (useIncomeTax != detail.useIncomeTax)
								{
									pphError += 1;
								}
							}
							 if (detail.useVat == true)
							{
								if (useVat != detail.useVat)
								{
									ppnError += 1;
								}
							}
							detailError += "}, ";
						}

						detailError += "]";

						if (detailErrorCount > 0)
						{
							itemErrorCount++;
							itemError += $"details: {detailError}, ";
						}
					}

					itemError += "}, ";
				}

				itemError += "]";
				if(pphError >0)
					yield return new ValidationResult("Using PPh is different with purchase order external", new List<string> { "useIncomeTax" });
				if (ppnError > 0)
					yield return new ValidationResult("Using PPn is different with purchase order external", new List<string> { "useIncomeTax" });

				if (itemErrorCount > 0)
					yield return new ValidationResult(itemError, new List<string> { "items" });
			}
		}
	}
}
