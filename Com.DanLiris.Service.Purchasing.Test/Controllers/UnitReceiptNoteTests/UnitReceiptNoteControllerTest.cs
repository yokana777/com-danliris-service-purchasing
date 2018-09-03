using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.UnitReceiptNoteTests
{
    [Collection("TestServerFixture Collection")]
    public class UnitReceiptNoteControllerTest
    {
        private const string MediaType = "application/json";
        private const string MediaTypePdf = "application/pdf";
        private readonly string URI = "v1/unit-receipt-notes/by-user";

        private TestServerFixture TestFixture { get; set; }

        private HttpClient Client
        {
            get { return this.TestFixture.Client; }
        }

        protected UnitReceiptNoteDataUtil DataUtil
        {
            get { return (UnitReceiptNoteDataUtil)this.TestFixture.Service.GetService(typeof(UnitReceiptNoteDataUtil)); }
        }

        public UnitReceiptNoteControllerTest(TestServerFixture fixture)
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
        public async Task Should_Success_Get_All_Data_With_Filter()
        {
            var response = await this.Client.GetAsync(URI + "?filter={'UnitName':'UnitName'}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            UnitReceiptNote model = await DataUtil.GetTestData("dev2");
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
        public async Task Should_Success_Get_Data_PDF_By_Id()
        {
            UnitReceiptNote model = await DataUtil.GetTestData("dev2");
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{Client.BaseAddress}{URI}/{model.Id}"),
                Method = HttpMethod.Get
            };
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypePdf));
            requestMessage.Headers.Add("x-timezone-offset", "0");
            var response = await this.Client.SendAsync(requestMessage);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            UnitReceiptNoteViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType);
            httpContent.Headers.Add("x-timezone-offset", "0");
            var response = await this.Client.PostAsync(URI, httpContent);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Invalid_Data()
        {
            UnitReceiptNoteViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            viewModel.date = DateTimeOffset.MinValue;
            viewModel.unit = null;
            viewModel.items = new List<UnitReceiptNoteItemViewModel> { };
            viewModel.isStorage = true;
            viewModel.storage = null;
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Invalid_Date_Data()
        {
            UnitReceiptNoteViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            viewModel.date = DateTimeOffset.Now.AddMonths(-1);
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Create_Invalid_Data_Item()
        {
            UnitReceiptNoteViewModel viewModel = await DataUtil.GetNewDataViewModel("dev2");
            foreach (UnitReceiptNoteItemViewModel item in viewModel.items)
            {
                item.product = null;
                item.deliveredQuantity = 0;
            }
            var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        //[Fact]
        //public async Task Should_Success_Update_Data()
        //{
        //    UnitReceiptNote model = await DataUtil.GetTestData("dev2");

        //    var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
        //    var json = responseGetById.Content.ReadAsStringAsync().Result;

        //    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
        //    Assert.True(result.ContainsKey("apiVersion"));
        //    Assert.True(result.ContainsKey("message"));
        //    Assert.True(result.ContainsKey("data"));
        //    Assert.True(result["data"].GetType().Name.Equals("JObject"));

        //    UnitReceiptNoteViewModel viewModel = JsonConvert.DeserializeObject<UnitReceiptNoteViewModel>(result.GetValueOrDefault("data").ToString());

        //    var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
        //    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //}

        [Fact]
        public async Task Should_Error_Update_Data_Id()
        {
            var response = await this.Client.PutAsync($"{URI}/0", new StringContent(JsonConvert.SerializeObject(new UnitReceiptNoteViewModel()).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Update_Invalid_Data()
        {
            UnitReceiptNote model = await DataUtil.GetTestData("dev2");

            var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
            var json = responseGetById.Content.ReadAsStringAsync().Result;

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
            Assert.True(result.ContainsKey("apiVersion"));
            Assert.True(result.ContainsKey("message"));
            Assert.True(result.ContainsKey("data"));
            Assert.True(result["data"].GetType().Name.Equals("JObject"));

            UnitReceiptNoteViewModel viewModel = JsonConvert.DeserializeObject<UnitReceiptNoteViewModel>(result.GetValueOrDefault("data").ToString());
            viewModel.date = DateTimeOffset.MinValue;
            viewModel.supplier = null;
            viewModel.unit = null;
            viewModel.items = new List<UnitReceiptNoteItemViewModel> { };

            var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Should_Success_Delete_Data_By_Id()
        {
            UnitReceiptNote model = await DataUtil.GetTestData("dev2");
            var response = await this.Client.DeleteAsync($"{URI}/{model.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Should_Error_Delete_Data_Invalid_Id()
        {
            var response = await this.Client.DeleteAsync($"{URI}/0");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
