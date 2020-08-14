using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels
{
	public class GarmentDOItemViewModel : BaseViewModel, IValidatableObject
	{
		public string DOItemNo { get; set; }
		public string UId { get; set; }
		public long UnitId { get; set; }
		public string UnitCode { get; set; }
		public string UnitName { get; set; }
		public long StorageId { get; set; }
		public string StorageCode { get; set; }
		public string StorageName { get; set; }
		public long POId { get; set; }
		public long POItemId { get; set; }
		public long PRItemId { get; set; }
		public long EPOItemId { get; set; }
		public string POSerialNumber { get; set; }
		public long ProductId { get; set; }
		public string ProductCode { get; set; }
		public string ProductName { get; set; }
		public string DesignColor { get; set; }
		public decimal SmallQuantity { get; set; }
		public decimal RemainingQuantity { get; set; }
		public long SmallUomId { get; set; }
		public string SmallUomUnit { get; set; }
		public double DOCurrencyRate { get; set; }
		public long DetailReferenceId { get; set; }
		public long URNItemId { get; set; }
		public string RO { get; set; }
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			throw new NotImplementedException();
		}
	}
}
