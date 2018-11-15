using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentBeacukaiViewModel
{
	public class GarmentBeacukaiItemViewModel :BaseModel
	{
		public long garmentDOId { get; set; }
		public string garmentDONo { get; set; }
		public DateTimeOffset arrivalDate { get; set; }
		public DateTimeOffset dODate { get; set; }
		public double totalQty { get; set; }
		public decimal totalAmount { get; set; }
	}
}
