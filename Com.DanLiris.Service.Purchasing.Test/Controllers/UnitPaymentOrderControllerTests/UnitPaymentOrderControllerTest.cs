using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.UnitPaymentOrderControllerTests
{
    [Collection("TestServerFixture Collection")]
    public class UnitPaymentOrderControllerTest
    {
        private const string MediaType = "application/json";
        private readonly string URI = "v1/unit-payment-orders";
        private readonly string USERNAME = "dev2";

        private TestServerFixture TestFixture { get; set; }

        public UnitPaymentOrderControllerTest(TestServerFixture testFixture)
        {
            TestFixture = testFixture;
        }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        protected UnitPaymentOrderDataUtil DataUtil
        {
            get { return (UnitPaymentOrderDataUtil)this.TestFixture.Service.GetService(typeof(UnitPaymentOrderDataUtil)); }
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            UnitPaymentOrder model = await DataUtil.GetTestData(USERNAME);
            var response = await this.Client.GetAsync($"{URI}/{model.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
