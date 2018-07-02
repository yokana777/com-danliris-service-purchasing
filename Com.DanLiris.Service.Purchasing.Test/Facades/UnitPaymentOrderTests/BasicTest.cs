using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.UnitPaymentOrderTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class BasicTest
    {
        private const string USERNAME = "Unit Test";
        private IServiceProvider ServiceProvider { get; set; }

        public BasicTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = USERNAME;
        }

        private UnitPaymentOrderDataUtil DataUtil
        {
            get { return (UnitPaymentOrderDataUtil)ServiceProvider.GetService(typeof(UnitPaymentOrderDataUtil)); }
        }

        private UnitPaymentOrderFacade Facade
        {
            get { return (UnitPaymentOrderFacade)ServiceProvider.GetService(typeof(UnitPaymentOrderFacade)); }
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var model = await DataUtil.GetTestData(USERNAME);
            var Response = Facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

    }
}
