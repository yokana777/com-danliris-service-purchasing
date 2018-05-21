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
    public class PurchasingToVerificationTest
    {
        private const string MediaType = "application/json";
        private readonly string URI = "v1/expedition/purchasing-to-verification";

        private TestServerFixture TestFixture { get; set; }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        protected SendToVerificationDataUtil DataUtil
        {
            get { return (SendToVerificationDataUtil)this.TestFixture.Service.GetService(typeof(SendToVerificationDataUtil)); }
        }

        public PurchasingToVerificationTest(TestServerFixture fixture)
        {
            TestFixture = fixture;
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            PurchasingToVerificationViewModel viewModel = new PurchasingToVerificationViewModel()
            {
                UnitPaymentOrders = new List<UnitPaymentOrderViewModel>()
                {
                    new UnitPaymentOrderViewModel(){
                        No = "UPONo",
                        Currency = "IDR",
                        DivisionCode = "Division",
                        DivisionName = "Division",
                        SupplierCode = "Supplier",
                        SupplierName = "Supplier",
                        DueDate = DateTimeOffset.UtcNow,
                        InvoiceNo = "Invoice",
                        TotalPaid = 1000000,
                        UPODate = DateTimeOffset.UtcNow,
                    }
                }
            };

            var response = await Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
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
    }
}
