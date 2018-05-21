using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.Expedition
{
    [Collection("TestServerFixture Collection")]
    public class PurchasingDocumentAcceptanceTest
    {
        private const string MediaType = "application/json";
        private readonly string URI = "v1/expedition/purchasing-document-acceptance";

        private TestServerFixture TestFixture { get; set; }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        protected PurchasingDocumentAcceptanceDataUtil DataUtil
        {
            get { return (PurchasingDocumentAcceptanceDataUtil)this.TestFixture.Service.GetService(typeof(PurchasingDocumentAcceptanceDataUtil)); }
        }

        public PurchasingDocumentAcceptanceTest(TestServerFixture fixture)
        {
            TestFixture = fixture;
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            PurchasingDocumentAcceptanceViewModel ViewModel = DataUtil.GetVerificationNewData();

            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(ViewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseLocationHeader = await this.Client.GetAsync(response.Headers.Location.OriginalString);
            Assert.Equal(HttpStatusCode.OK, responseLocationHeader.StatusCode);

            var json = responseLocationHeader.Content.ReadAsStringAsync().Result;
            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());

            Assert.True(result.ContainsKey("apiVersion"));
            Assert.True(result.ContainsKey("message"));
            Assert.True(result.ContainsKey("data"));
            Assert.True(result["data"].GetType().Name.Equals("JObject"));
        }

        [Fact]
        public async Task Should_Success_Delete_Data()
        {
            PurchasingDocumentExpedition Model = await DataUtil.GetVerificationTestData();

            var response = await this.Client.DeleteAsync(string.Concat(URI, "/", Model.Id));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
