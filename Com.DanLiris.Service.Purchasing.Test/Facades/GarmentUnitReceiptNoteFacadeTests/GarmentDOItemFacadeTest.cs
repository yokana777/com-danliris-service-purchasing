using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentUnitReceiptNoteFacadeTests
{
	public class GarmentDOItemFacadeTest
	{

		[Fact]
		public async Task TestModel()
		{
			GarmentDOItems dOItems = new GarmentDOItems { UId = "aaa",DOCurrencyRate=1,DesignColor="aa",DetailReferenceId=1,DOItemNo="sdoitem",EPOItemId=2,POId=1,POItemId=2,POSerialNumber="pp",PRItemId=1,ProductCode="ss",ProductId=2,ProductName="name",RemainingQuantity=100,SmallQuantity=100,SmallUomId=2,SmallUomUnit="pcs",StorageCode="ss",StorageName="ss",StorageId=3,RO="@2",UnitCode="s",UnitId=4,UnitName="s",URNItemId=4 };
			Assert.NotNull(dOItems.UId);
		}
	}
}
