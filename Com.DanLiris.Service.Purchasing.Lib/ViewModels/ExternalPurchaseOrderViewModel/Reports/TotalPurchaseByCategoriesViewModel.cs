using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel.Reports
{
    public class TotalPurchaseByCategoriesViewModel
    {
		public string supplierName { get; set; }
		public string unitName { get; set; }
		public string categoryName { get; set; }
		public decimal amount { get; set; }
		public decimal total { get; set; }
	}
}
