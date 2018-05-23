using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.PurchasingDocumentExpeditionTest
{
    public class UnitPaymentOrderNotVerifiedReportTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public UnitPaymentOrderNotVerifiedReportTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = "Unit Test";
        }

        private UnitPaymentOrderNotVerifiedDataUtil DataUtil
        {
            get { return (UnitPaymentOrderNotVerifiedDataUtil)ServiceProvider.GetService(typeof(UnitPaymentOrderNotVerifiedDataUtil)); }
        }

        private UnitPaymentOrderNotVerifiedReportFacade Facade
        {
            get { return (UnitPaymentOrderNotVerifiedReportFacade)ServiceProvider.GetService(typeof(UnitPaymentOrderNotVerifiedReportFacade)); }
        }

        [Fact]
        public async void Should_Success_Get_Report_Data()
        {
            PurchasingDocumentExpedition model = await DataUtil.GetTestData();
            //List<string> unitPaymentOrders = new List<string>() { model.UnitPaymentOrderNo };
            var Response = this.Facade.GetReport(model.UnitPaymentOrderNo, model.SupplierCode, model.DivisionCode, model._CreatedUtc, model._CreatedUtc, 1,25, model.UnitPaymentOrderNo, 7);
            Assert.NotEqual(Response.Item2, 0);
        }
    }
}
