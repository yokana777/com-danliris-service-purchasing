using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.InternalPurchaseOrderTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class BasicTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public BasicTest(ServiceProviderFixture fixture)
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

        [Fact]
        public async void Should_Success_Get_Data()
        {
            await DataUtil.GetTestData("Unit test");
            Tuple<List<InternalPurchaseOrder>, int, Dictionary<string, string>> Response = Facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            InternalPurchaseOrder model = await DataUtil.GetNewData("Unit test");
            var Response = await Facade.Create(model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = await Facade.Update((int)model.Id, model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.Delete((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_CountPRNo()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.ReadByPRNo((model.PRNo);
            Assert.NotEqual(Response, 0);
        }
    }
}
