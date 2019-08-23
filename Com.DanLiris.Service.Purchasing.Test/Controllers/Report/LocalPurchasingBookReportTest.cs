using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.Report
{
    [Collection("TestServerFixture Collection")]
    public class LocalPurchasingBookReportTest
    {
        private const string MediaType = "application/json";
        private readonly string URI = "v1/report/local-purchasing-book-reports";
        //private readonly List<string> CreateValidationAttributes;

        private TestServerFixture TestFixture { get; set; }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        public LocalPurchasingBookReportTest(TestServerFixture fixture)
        {
            TestFixture = fixture;
        }

        //[Fact]
        //public async Task Should_Success_Get_Report()
        //{
        //    var response = await this.Client.GetAsync(URI);
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var json = await response.Content.ReadAsStringAsync();
        //    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());

        //    Assert.True(result.ContainsKey("apiVersion"));
        //    Assert.True(result.ContainsKey("message"));
        //    Assert.True(result.ContainsKey("data"));
        //    Assert.True(result["data"].GetType().Name.Equals("JArray"));
        //}

        [Fact]
        public async Task Should_Success_Get_Report_Excel()
        {
            var response = await this.Client.GetAsync(URI + "/download");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
