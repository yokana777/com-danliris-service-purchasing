using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.DeliveryOrderTests
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

        private DeliveryOrderDataUtil DataUtil
        {
            get { return (DeliveryOrderDataUtil)ServiceProvider.GetService(typeof(DeliveryOrderDataUtil)); }
        }

        private DeliveryOrderFacade Facade
        {
            get { return (DeliveryOrderFacade)ServiceProvider.GetService(typeof(DeliveryOrderFacade)); }
        }

        public async void Should_Success_Get_Report_Data()
        {
            DeliveryOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport( model.DONo, model.SupplierId, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Null_Parameter()
        {
            DeliveryOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport("", null, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Null_Parameter_Using_Two_Test_Data()
        {
            DeliveryOrder model_1 = await DataUtil.GetTestData("Unit test");
            DeliveryOrder model_2 = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport("", null, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel()
        {
            DeliveryOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel(model.DONo, model.SupplierId, null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            DeliveryOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel("", "", null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }
    }
}
