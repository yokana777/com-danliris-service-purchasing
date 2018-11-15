using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentBeacukaiViewModel
{
	public class GarmentBeacukaiViewModel : BaseViewModel//, IValidatableObject
	{
		public long bCIdTemp { get; set; }
		public string beacukaiNo { get; set; }
		public DateTimeOffset beacukaiDate { get; set; }
		public SupplierViewModel supplier  { get; set; }
		public double packagingQty { get; set; }
		public string packaging { get; set; }
		public double bruto { get; set; }
		public double netto { get; set; }
		public CurrencyViewModel currency{ get; set; }
		public List<GarmentBeacukaiItemViewModel> items { get; set; }
	}
}
