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
    [Collection("ServiceProviderFixture Collection")]
    public class UnitPaymentOrderNotVerifiedReportTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public UnitPaymentOrderNotVerifiedReportTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = "Unit Test";
        }

        private SendToVerificationDataUtil DataUtil
        {
            get { return (SendToVerificationDataUtil)ServiceProvider.GetService(typeof(SendToVerificationDataUtil)); }
        }

        private UnitPaymentOrderNotVerifiedReportFacade Facade
        {
            get { return (UnitPaymentOrderNotVerifiedReportFacade)ServiceProvider.GetService(typeof(UnitPaymentOrderNotVerifiedReportFacade)); }
        }

        [Fact]
        public async void Should_Success_Get_Report_Data()
        {
            PurchasingDocumentExpedition model = await DataUtil.GetTestDataNotVerified();
            var Response = this.Facade.GetReport(model.UnitPaymentOrderNo, model.SupplierCode, model.DivisionCode, null, null, 1,25, model.UnitPaymentOrderNo, 7);
            Assert.NotEqual(Response.Item1.Count, 0);
        }
    }
}
