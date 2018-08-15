using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.PurchaseRequestTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class MonitoringTests
    {
        private IServiceProvider ServiceProvider { get; set; }

        public MonitoringTests(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = "Unit Test";
        }

        private PurchaseRequestDataUtil DataUtil
        {
            get { return (PurchaseRequestDataUtil)ServiceProvider.GetService(typeof(PurchaseRequestDataUtil)); }
        }
		private InternalPurchaseOrderDataUtil IPODataUtil
		{
			get { return (InternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(InternalPurchaseOrderDataUtil)); }
		}
		private PurchaseRequestFacade Facade
        {
            get { return (PurchaseRequestFacade)ServiceProvider.GetService(typeof(PurchaseRequestFacade)); }
        }

        

        public async void Should_Success_Get_Report_Data()
        {
            PurchaseRequest model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport(model.No,model.UnitId,model.CategoryId,model.BudgetId,"","",null,null, 1,25,"{}",7, "Unit Test");
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Null_Parameter()
        {
            PurchaseRequest model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport("", null,null, null, "", "", null, null, 1, 25, "{}", 7, "Unit Test");
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel()
        {
            PurchaseRequest model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel(model.No, model.UnitId, model.CategoryId, model.BudgetId, "", "", null, null, 7, "Unit Test");
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            PurchaseRequest model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel("", "", "", "", "", "", null, null, 7, "Unit Test");
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }
		//Duration PR
		public async void Should_Success_Get_Report_PRDuration_Data()
		{
			var model = await IPODataUtil.GetTestData2("Unit test");
			var Response = Facade.GetPRDurationReport( model.UnitId, "8-14 hari", null, null, 1, 25, "{}", 7);
			Assert.NotEqual(Response.Item2, 0);
		}

		[Fact]
		public async void Should_Success_Get_Report_PRDuration_Null_Parameter()
		{
			var model = await IPODataUtil.GetTestData2("Unit test");
			var Response = Facade.GetPRDurationReport("", "8-14 hari", null, null, 1, 25, "{}", 7);
			Assert.NotEqual(Response.Item2, 0);
		}

		[Fact]
		public async void Should_Success_Get_Report_PRDuration_Excel()
		{
			var model = await IPODataUtil.GetTestData2("Unit test");
			var Response = Facade.GenerateExcelPRDuration(model.UnitId, "8-14 hari", null, null, 7);
			Assert.IsType(typeof(System.IO.MemoryStream), Response);
		}

		[Fact]
		public async void Should_Success_Get_Report_PRDuration_Excel_Null_Parameter()
		{
			var model = await IPODataUtil.GetTestData3("Unit test");
			var Response = Facade.GenerateExcelPRDuration("", "15-30 hari", null, null, 7);
			Assert.IsType(typeof(System.IO.MemoryStream), Response);
		}

        //Duration PR-PO Ex
        public async void Should_Success_Get_Report_PRPOExDuration_Data()
        {
            var model = await IPODataUtil.GetTestData2("Unit test");
            var Response = Facade.GetPREPODurationReport(model.UnitId, "8-14 hari", null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, 0);
        }

        public async void Should_Success_Get_Report_PRPOExDuration_Data_2()
        {
            var model = await IPODataUtil.GetTestData3("Unit test");
            var Response = Facade.GetPREPODurationReport(model.UnitId, "15-30 hari", null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_PRPOEDuration_Excel()
        {
            var model = await IPODataUtil.GetTestData2("Unit test");
            var Response = Facade.GenerateExcelPREPODuration("7", "8-14 hari", null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_PRPOEDuration_Excel_Null_Parameter()
        {
            var model = await IPODataUtil.GetTestData3("Unit test");
            var Response = Facade.GenerateExcelPRDuration("", "15-30 hari", null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }
    }
}
