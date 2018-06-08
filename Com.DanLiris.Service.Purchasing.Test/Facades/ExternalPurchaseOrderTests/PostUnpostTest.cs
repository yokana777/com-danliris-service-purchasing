using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtil.cs;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ExternalPurchaseOrderTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class PostUnpostTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public PostUnpostTest(ServiceProviderFixture fixture)
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
        public async void Should_Success_EPOPost()
        {
            List<ExternalPurchaseOrder> modelList = new List<ExternalPurchaseOrder>();
            ExternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            modelList.Add(model);
            var Response = Facade.EPOPost(modelList, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_EPOUnpost()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.EPOUnpost((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_EPOClose()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.EPOClose((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_EPOCancel()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.EPOCancel((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }
    }
}
