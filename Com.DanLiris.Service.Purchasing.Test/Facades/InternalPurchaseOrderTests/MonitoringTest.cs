using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.InternalPurchaseOrderTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class MonitoringTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public MonitoringTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = "Unit Test";
        }

        private InternalPurchaseOrderDataUtil DataUtil
        {
            get { return (InternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(InternalPurchaseOrderDataUtil)); }
        }

        private InternalPurchaseOrderFacade Facade
        {
            get { return (InternalPurchaseOrderFacade)ServiceProvider.GetService(typeof(InternalPurchaseOrderFacade)); }
        }

        public async void Should_Success_Get_Report_Data()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport( model.UnitId, model.CategoryId, model.CreatedBy, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Null_Parameter()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport("", null, null, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel(model.UnitId, model.CategoryId, model.CreatedBy, null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel("", "", "", null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }
    }
}
