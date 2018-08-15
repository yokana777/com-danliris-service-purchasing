using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade.Reports;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ExternalPurchaseOrderTests.Reports
{
	[Collection("ServiceProviderFixture Collection")]
	public class TotalPurchaseBySupplierTest
	{
		private IServiceProvider ServiceProvider { get; set; }
		public TotalPurchaseBySupplierTest(ServiceProviderFixture fixture)
		{
			ServiceProvider = fixture.ServiceProvider;

			IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
			identityService.Username = "Unit Test";
		}
		private ExternalPurchaseOrderDataUtil DataUtil
		{
			get { return (ExternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
		}
		private InternalPurchaseOrderDataUtil IPODataUtil
		{
			get { return (InternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(InternalPurchaseOrderDataUtil)); }
		}
		private TotalPurchaseFacade Facade
		{
			get { return (TotalPurchaseFacade)ServiceProvider.GetService(typeof(TotalPurchaseFacade)); }
		}
		[Fact]
		public async void Should_Success_Get_Report_Total_Purchase_By_Supplier_Data_Null_Parameter()
		{
			var model = await DataUtil.GetTestData("Unit test");
			var Response = Facade.GetTotalPurchaseBySupplierReport(null, null,null,null,"{}",7 );
			Assert.NotEqual(1, 0);
		}
		[Fact]
		public async void Should_Success_Get_Report_Total_Purchase_By_Supplier_Data_Excel_Null_Parameter()
		{
			var model = await DataUtil.GetTestData("Unit test");
			var Response = Facade.GenerateExcelTotalPurchaseBySupplier(null, null, null, null,  7);
			Assert.IsType(typeof(System.IO.MemoryStream), Response);
		}
		//[Fact]
		//public async void Should_Success_Get_Report_PRDuration_Null_Parameter()
		//{
		//	var model = await IPODataUtil.GetTestData2("Unit test");
		//	var Response = Facade.GetPRDurationReport("", "8-14 hari", null, null, 1, 25, "{}", 7);
		//	Assert.NotEqual(Response.Item2, 0);
		//}

		//[Fact]
		//public async void Should_Success_Get_Report_PRDuration_Excel()
		//{
		//	var model = await IPODataUtil.GetTestData2("Unit test");
		//	var Response = Facade.GenerateExcelPRDuration(model.UnitId, "8-14 hari", null, null, 7);
		//	Assert.IsType(typeof(System.IO.MemoryStream), Response);
		//}

		//[Fact]
		//public async void Should_Success_Get_Report_PRDuration_Excel_Null_Parameter()
		//{
		//	var model = await IPODataUtil.GetTestData3("Unit test");
		//	var Response = Facade.GenerateExcelPRDuration("", "15-30 hari", null, null, 7);
		//	Assert.IsType(typeof(System.IO.MemoryStream), Response);
		//}
	}
}
