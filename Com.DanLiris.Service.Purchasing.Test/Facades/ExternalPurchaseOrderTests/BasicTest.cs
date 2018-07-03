using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ExternalPurchaseOrderTests
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

        private ExternalPurchaseOrderDataUtil DataUtil
        {
            get { return (ExternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
        }

        private ExternalPurchaseOrderFacade Facade
        {
            get { return (ExternalPurchaseOrderFacade)ServiceProvider.GetService(typeof(ExternalPurchaseOrderFacade)); }
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            await DataUtil.GetTestData("Unit test");
            Tuple<List<ExternalPurchaseOrder>, int, Dictionary<string, string>> Response = Facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_Unused()
        {
            ExternalPurchaseOrder externalPurchaseOrder = await DataUtil.GetTestDataUnused("Unit test");
            List<ExternalPurchaseOrder> Response = Facade.ReadUnused(Keyword:externalPurchaseOrder.EPONo);
            Assert.NotEqual(Response.Count, 0);
        }

        //[Fact]
        //public async void Should_Success_Get_Data_Posted()
        //{
        //    await DataUtil.GetTestDataPosted("Unit test");
        //    Tuple<List<PurchaseRequest>, int, Dictionary<string, string>> Response = Facade.ReadModelPosted();
        //    Assert.NotEqual(Response.Item1.Count, 0);
        //}

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
            ExternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.ReadModelById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            ExternalPurchaseOrder model = await DataUtil.GetNewData("Unit Test");
            var Response = await Facade.Create(model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = await Facade.Update((int)model.Id, model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.Delete((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        //[Fact]
        //public async void Should_Error_Update_Data_Invalid_Id()
        //{
        //    Exception exception = await Assert.ThrowsAsync<Exception>(() => Facade.Update(0, new ExternalPurchaseOrder(), "Unit Test"));
        //    Assert.Equal(exception.Message, "Invalid Id");
        //}
    }
}
