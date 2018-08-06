using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.DeliveryOrderControllerTests
{
    [Collection("TestServerFixture Collection")]
    public class DeliveryOrderControllerTest
    {
        private const string MediaType = "application/json";
        private readonly string URI = "v1/delivery-orders";
        private readonly string USERNAME = "dev2";

        private TestServerFixture TestFixture { get; set; }

        public DeliveryOrderControllerTest(TestServerFixture fixture)
        {
            TestFixture = fixture;
        }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        protected DeliveryOrderDataUtil DataUtil
        {
            get { return (DeliveryOrderDataUtil)this.TestFixture.Service.GetService(typeof(DeliveryOrderDataUtil)); }
        }

        [Fact]
        public async Task Should_Success_Get_All_Data()
        {
            var response = await this.Client.GetAsync(URI);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // add error ^_^
            var responseError = await this.Client.GetAsync(URI + "?filter={'IsPosted':}");
            Assert.Equal(HttpStatusCode.InternalServerError, responseError.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_All_Data_By_User()
        {
            string URI = $"{this.URI}/by-user";

            var response = await this.Client.GetAsync(URI);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseWithFilter = await this.Client.GetAsync(URI + "?filter={'IsClosed ':false}");
            Assert.Equal(HttpStatusCode.OK, responseWithFilter.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
            var response = await this.Client.GetAsync($"{URI}/{model.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Get_Invalid_Id()
        {
            var response = await this.Client.GetAsync($"{URI}/0");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            DeliveryOrderViewModel viewModel = await DataUtil.GetNewDataViewModel(USERNAME);
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType);
            var response = await this.Client.PostAsync(URI, httpContent);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Data()
        {
            DeliveryOrderViewModel viewModel = await DataUtil.GetNewDataViewModel(USERNAME);
            viewModel.items = null;
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType);
            var response = await this.Client.PostAsync(URI, httpContent);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Invalid_Data()
        {
            DeliveryOrderViewModel viewModel = await DataUtil.GetNewDataViewModel(USERNAME);
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            viewModel.date = DateTimeOffset.MinValue;
            viewModel.supplierDoDate = DateTimeOffset.MinValue;
            viewModel.supplier = null;
            viewModel.items.FirstOrDefault().fulfillments.FirstOrDefault().deliveredQuantity = 0;
            viewModel.items.Add(new DeliveryOrderItemViewModel
            {
                purchaseOrderExternal = new PurchaseOrderExternal { }
            });
            viewModel.items.Add(new DeliveryOrderItemViewModel
            {
                purchaseOrderExternal = new PurchaseOrderExternal
                {
                    _id = viewModel.items.FirstOrDefault().purchaseOrderExternal._id,
                }
            });
            viewModel.items.Add(new DeliveryOrderItemViewModel
            {
                purchaseOrderExternal = new PurchaseOrderExternal
                {
                    _id = viewModel.items.FirstOrDefault().purchaseOrderExternal._id + 1,
                },
                fulfillments = null
            });
            viewModel.items.Add(new DeliveryOrderItemViewModel
            {
                purchaseOrderExternal = new PurchaseOrderExternal
                {
                    _id = viewModel.items.FirstOrDefault().purchaseOrderExternal._id + 2,
                },
                fulfillments = new List<DeliveryOrderFulFillMentViewModel>
                {
                    new DeliveryOrderFulFillMentViewModel
                    {
                        deliveredQuantity = 0
                    }
                }
            });
            var responseInvalid = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, responseInvalid.StatusCode);

            viewModel.no = null;
            viewModel.items = new List<DeliveryOrderItemViewModel> { };
            var responseNoItem = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, responseNoItem.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Update_Data()
        {
            DeliveryOrder model = await DataUtil.GetTestData(USERNAME);

            var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
            var json = responseGetById.Content.ReadAsStringAsync().Result;

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
            Assert.True(result.ContainsKey("apiVersion"));
            Assert.True(result.ContainsKey("message"));
            Assert.True(result.ContainsKey("data"));
            Assert.True(result["data"].GetType().Name.Equals("JObject"));

            DeliveryOrderViewModel viewModel = JsonConvert.DeserializeObject<DeliveryOrderViewModel>(result.GetValueOrDefault("data").ToString());

            var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Update_Data_Id()
        {
            var response = await this.Client.PutAsync($"{URI}/0", new StringContent(JsonConvert.SerializeObject(new DeliveryOrderViewModel()).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Update_Invalid_Data()
        {
            DeliveryOrder model = await DataUtil.GetTestData(USERNAME);

            var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
            var json = responseGetById.Content.ReadAsStringAsync().Result;

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
            Assert.True(result.ContainsKey("apiVersion"));
            Assert.True(result.ContainsKey("message"));
            Assert.True(result.ContainsKey("data"));
            Assert.True(result["data"].GetType().Name.Equals("JObject"));

            DeliveryOrderViewModel viewModel = JsonConvert.DeserializeObject<DeliveryOrderViewModel>(result.GetValueOrDefault("data").ToString());
            viewModel.date = DateTimeOffset.MinValue;
            viewModel.supplier = null;
            viewModel.items = new List<DeliveryOrderItemViewModel> { };

            var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Delete_Data_By_Id()
        {
            DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
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
        public async Task Should_Success_Get_Data_By_Supplier()
        {
            DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
            var response = await this.Client.GetAsync($"{URI}/by-supplier?unitId={model.Items.FirstOrDefault().Details.FirstOrDefault().UnitId}&supplierId={model.SupplierId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_Report()
        {
            var response = await this.Client.GetAsync(URI + "/monitoring" + "?page=1&size=25");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = response.Content.ReadAsStringAsync().Result;
            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());

            Assert.True(result.ContainsKey("apiVersion"));
            Assert.True(result.ContainsKey("message"));
            Assert.True(result.ContainsKey("data"));
            Assert.True(result["data"].GetType().Name.Equals("JArray"));
        }

        [Fact]
        public async Task Should_Success_Get_Report_Excel()
        {
            var response = await this.Client.GetAsync(URI + "/monitoring/download");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Get_Report_Without_Page()
        {
            var response = await this.Client.GetAsync(URI + "/monitoring");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Excel_Empty_Data()
        {
            var response = await this.Client.GetAsync($"{URI}/monitoring/download?doNo=0");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
