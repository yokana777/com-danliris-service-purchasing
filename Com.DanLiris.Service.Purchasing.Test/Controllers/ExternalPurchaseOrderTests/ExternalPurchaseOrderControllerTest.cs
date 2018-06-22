using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");
            var response = await this.Client.GetAsync($"{URI}/{model.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_All_Data()
        {
            var response = await this.Client.GetAsync(URI);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //var responseError = await this.Client.GetAsync(URI );
            //Assert.Equal(HttpStatusCode.InternalServerError, responseError.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_All_Data_With_Filter()
        {
            var response = await this.Client.GetAsync(URI + "?filter={'IsPosted':false}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Delete_Data_By_Id()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");
            var response = await this.Client.DeleteAsync($"{URI}/{model.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Delete_Data_Invalid_Id()
        {
            var response = await this.Client.DeleteAsync($"{URI}/0");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Get_Invalid_Id()
        {
            var response = await this.Client.GetAsync($"{URI}/null");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Invalid_Data()
        {
            ExternalPurchaseOrderViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            viewModel.orderDate = DateTimeOffset.MinValue;
            viewModel.currency = null;
            viewModel.unit = null;
            viewModel.supplier = null;
            viewModel.items = new List<ExternalPurchaseOrderItemViewModel> { };
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Invalid_Data_Item()
        {
            ExternalPurchaseOrderViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            foreach (ExternalPurchaseOrderItemViewModel item in viewModel.items)
            {
                item.prNo = "";
                item.poNo = "";
            }
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Data_date_more_than_expectedDeliveryDate()
        {
            ExternalPurchaseOrderViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            viewModel.orderDate = DateTimeOffset.MaxValue;
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Update_Data()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");

            var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
            var json = responseGetById.Content.ReadAsStringAsync().Result;

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
            Assert.True(result.ContainsKey("apiVersion"));
            Assert.True(result.ContainsKey("message"));
            Assert.True(result.ContainsKey("data"));
            Assert.True(result["data"].GetType().Name.Equals("JObject"));

            ExternalPurchaseOrderViewModel viewModel = JsonConvert.DeserializeObject<ExternalPurchaseOrderViewModel>(result.GetValueOrDefault("data").ToString());

            var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Update_Data_Id()
        {
            var response = await this.Client.PutAsync($"{URI}/0", new StringContent(JsonConvert.SerializeObject(new ExternalPurchaseOrderViewModel()).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Update_Invalid_Data()
        {
            ExternalPurchaseOrder model = await DataUtil.GetTestData("dev2");

            var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
            var json = responseGetById.Content.ReadAsStringAsync().Result;

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
            Assert.True(result.ContainsKey("apiVersion"));
            Assert.True(result.ContainsKey("message"));
            Assert.True(result.ContainsKey("data"));
            Assert.True(result["data"].GetType().Name.Equals("JObject"));

            ExternalPurchaseOrderViewModel viewModel = JsonConvert.DeserializeObject<ExternalPurchaseOrderViewModel>(result.GetValueOrDefault("data").ToString());
            viewModel.orderDate = DateTimeOffset.MinValue;
            viewModel.supplier = null;
            viewModel.unit = null;
            viewModel.currency = null;
            viewModel.items = new List<ExternalPurchaseOrderItemViewModel> { };

            var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
