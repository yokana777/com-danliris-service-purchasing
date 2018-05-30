using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.PurchaseRequestControllerTests
{
    [Collection("TestServerFixture Collection")]
    public class PurchaseRequestControllerTest
    {
        private const string MediaType = "application/json";
        private readonly string URI = "v1/purchase-requests";

        private TestServerFixture TestFixture { get; set; }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        public PurchaseRequestControllerTest(TestServerFixture fixture)
        {
            TestFixture = fixture;
        }

        [Fact]
        public async Task Should_Success_Get_All_Data()
        {
            var response = await this.Client.GetAsync(URI);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            var response = await this.Client.GetAsync($"{URI}/1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(new PurchaseRequest()).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
