using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.ExternalPurchaseOrderTests
{
    [Collection("TestServerFixture Collection")]
    public class ExternalPurchaseOrderControllerTest
    {
        private const string MediaType = "application/json";
        private readonly string URI = "v1/external-purchase-orders";

        private TestServerFixture TestFixture { get; set; }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        protected ExternalPurchaseOrderDataUtil DataUtil
        {
            get { return (ExternalPurchaseOrderDataUtil)this.TestFixture.Service.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
        }

        public ExternalPurchaseOrderControllerTest(TestServerFixture fixture)
        {
            TestFixture = fixture;
        }
        [Fact]
        public async Task Should_Success_EPOPost()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");
            ExternalPurchaseOrderViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            viewModel._id = model.Id;
            List<ExternalPurchaseOrderViewModel> viewModelList = new List<ExternalPurchaseOrderViewModel> { viewModel };
            var response = await this.Client.PostAsync($"{URI}/post", new StringContent(JsonConvert.SerializeObject(viewModelList).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_EPOUnpost()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");
            var response = await this.Client.PutAsync($"{URI}/unpost/{model.Id}", new StringContent("", Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_EPOCancel()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");
            var response = await this.Client.PutAsync($"{URI}/cancel/{model.Id}", new StringContent("", Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_EPOClose()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");
            var response = await this.Client.PutAsync($"{URI}/close/{model.Id}", new StringContent("", Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        
    }
}
