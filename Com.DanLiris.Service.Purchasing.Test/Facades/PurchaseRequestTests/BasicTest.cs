using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using System;
using System.Collections.Generic;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.PurchaseRequestTests
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

        private PurchaseRequestDataUtil DataUtil
        {
            get { return (PurchaseRequestDataUtil)ServiceProvider.GetService(typeof(PurchaseRequestDataUtil)); }
        }

        private PurchaseRequestFacade Facade
        {
            get { return (PurchaseRequestFacade)ServiceProvider.GetService(typeof(PurchaseRequestFacade)); }
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            await DataUtil.GetTestData("Unit test");
            Tuple<List<object>, int, Dictionary<string, string>> Response = Facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_With_Arguments()
        {
            string order = "{\"UnitCode\":\"desc\"}";
            string filter = "{\"CreatedBy\":\"Unit Test\"}";
            string keyword = "Unit";

            await DataUtil.GetTestData("Unit Test");
            var Response = this.Facade.Read(1, 25, order, keyword, filter);
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            PurchaseRequest model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            PurchaseRequest model = DataUtil.GetNewData();
            var Response = await Facade.Create(model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            PurchaseRequest model = await DataUtil.GetTestData("Unit test");
            var Response = await Facade.Update((int)model.Id, model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            PurchaseRequest model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.Delete((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }
    }
}
