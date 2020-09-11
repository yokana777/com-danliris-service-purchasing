using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.DeliveryOrderControllers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.DeliveryOrderControllerTests
{
    //[Collection("TestServerFixture Collection")]
    public class DeliveryOrderControllerTest
    {
        //private const string MediaType = "application/json";
        //private readonly string URI = "v1/delivery-orders";
        //private readonly string USERNAME = "dev2";

        //private TestServerFixture TestFixture { get; set; }

        //public DeliveryOrderControllerTest(TestServerFixture fixture)
        //{
        //    TestFixture = fixture;
        //}

        //private HttpClient Client
        //{
        //    get { return this.TestFixture.Client; }
        //}

        //protected DeliveryOrderDataUtil DataUtil
        //{
        //    get { return (DeliveryOrderDataUtil)this.TestFixture.Service.GetService(typeof(DeliveryOrderDataUtil)); }
        //}

        //[Fact]
        //public async Task Should_Success_Get_All_Data()
        //{
        //    var response = await this.Client.GetAsync(URI);
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    // add error ^_^
        //    var responseError = await this.Client.GetAsync(URI + "?filter={'IsPosted':}");
        //    Assert.Equal(HttpStatusCode.InternalServerError, responseError.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Get_All_Data_By_User()
        //{
        //    string URI = $"{this.URI}/by-user";

        //    var response = await this.Client.GetAsync(URI);
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var responseWithFilter = await this.Client.GetAsync(URI + "?filter={'IsClosed ':false}");
        //    Assert.Equal(HttpStatusCode.OK, responseWithFilter.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Get_Data_By_Id()
        //{
        //    DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
        //    var response = await this.Client.GetAsync($"{URI}/{model.Id}");
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Get_Invalid_Id()
        //{
        //    var response = await this.Client.GetAsync($"{URI}/0");
        //    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Create_Data()
        //{
        //    DeliveryOrderViewModel viewModel = await DataUtil.GetNewDataViewModel(USERNAME);
        //    HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType);
        //    var response = await this.Client.PostAsync(URI, httpContent);
        //    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Create_Data()
        //{
        //    DeliveryOrderViewModel viewModel = await DataUtil.GetNewDataViewModel(USERNAME);
        //    viewModel.items = null;
        //    HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType);
        //    var response = await this.Client.PostAsync(URI, httpContent);
        //    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Create_Data_same_number()
        //{
        //    DeliveryOrderViewModel viewModel = await DataUtil.GetNewDataViewModel(USERNAME);
        //    HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType);
        //    var response = await this.Client.PostAsync(URI, httpContent);

        //    DeliveryOrderViewModel viewModel1 = viewModel;
        //    HttpContent httpContent1 = new StringContent(JsonConvert.SerializeObject(viewModel1).ToString(), Encoding.UTF8, MediaType);
        //    var response1 = await this.Client.PostAsync(URI, httpContent);
        //    Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Create_Invalid_Data()
        //{
        //    DeliveryOrderViewModel viewModel = await DataUtil.GetNewDataViewModel(USERNAME);
        //    var response = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
        //    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        //    viewModel.date = DateTimeOffset.MinValue;
        //    viewModel.supplierDoDate = DateTimeOffset.MinValue;
        //    viewModel.supplier = null;
        //    viewModel.items.FirstOrDefault().fulfillments.FirstOrDefault().deliveredQuantity = 0;
        //    viewModel.items.Add(new DeliveryOrderItemViewModel
        //    {
        //        purchaseOrderExternal = new PurchaseOrderExternal { }
        //    });
        //    viewModel.items.Add(new DeliveryOrderItemViewModel
        //    {
        //        purchaseOrderExternal = new PurchaseOrderExternal
        //        {
        //            _id = viewModel.items.FirstOrDefault().purchaseOrderExternal._id,
        //        }
        //    });
        //    viewModel.items.Add(new DeliveryOrderItemViewModel
        //    {
        //        purchaseOrderExternal = new PurchaseOrderExternal
        //        {
        //            _id = viewModel.items.FirstOrDefault().purchaseOrderExternal._id + 1,
        //        },
        //        fulfillments = null
        //    });
        //    viewModel.items.Add(new DeliveryOrderItemViewModel
        //    {
        //        purchaseOrderExternal = new PurchaseOrderExternal
        //        {
        //            _id = viewModel.items.FirstOrDefault().purchaseOrderExternal._id + 2,
        //        },
        //        fulfillments = new List<DeliveryOrderFulFillMentViewModel>
        //        {
        //            new DeliveryOrderFulFillMentViewModel
        //            {
        //                deliveredQuantity = 0
        //            }
        //        }
        //    });
        //    var responseInvalid = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
        //    Assert.Equal(HttpStatusCode.BadRequest, responseInvalid.StatusCode);

        //    viewModel.no = null;
        //    viewModel.items = new List<DeliveryOrderItemViewModel> { };
        //    var responseNoItem = await this.Client.PostAsync(URI, new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
        //    Assert.Equal(HttpStatusCode.BadRequest, responseNoItem.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Update_Data()
        //{
        //    DeliveryOrder model = await DataUtil.GetTestData(USERNAME);

        //    var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
        //    var json = await responseGetById.Content.ReadAsStringAsync();

        //    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
        //    Assert.True(result.ContainsKey("apiVersion"));
        //    Assert.True(result.ContainsKey("message"));
        //    Assert.True(result.ContainsKey("data"));
        //    Assert.True(result["data"].GetType().Name.Equals("JObject"));

        //    DeliveryOrderViewModel viewModel = JsonConvert.DeserializeObject<DeliveryOrderViewModel>(result.GetValueOrDefault("data").ToString());

        //    var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
        //    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Update_Data_Id()
        //{
        //    var response = await this.Client.PutAsync($"{URI}/0", new StringContent(JsonConvert.SerializeObject(new DeliveryOrderViewModel()).ToString(), Encoding.UTF8, MediaType));
        //    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Update_Invalid_Data()
        //{
        //    DeliveryOrder model = await DataUtil.GetTestData(USERNAME);

        //    var responseGetById = await this.Client.GetAsync($"{URI}/{model.Id}");
        //    var json = await responseGetById.Content.ReadAsStringAsync();

        //    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
        //    Assert.True(result.ContainsKey("apiVersion"));
        //    Assert.True(result.ContainsKey("message"));
        //    Assert.True(result.ContainsKey("data"));
        //    Assert.True(result["data"].GetType().Name.Equals("JObject"));

        //    DeliveryOrderViewModel viewModel = JsonConvert.DeserializeObject<DeliveryOrderViewModel>(result.GetValueOrDefault("data").ToString());
        //    viewModel.date = DateTimeOffset.MinValue;
        //    viewModel.supplier = null;
        //    viewModel.items = new List<DeliveryOrderItemViewModel> { };

        //    var response = await this.Client.PutAsync($"{URI}/{model.Id}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, MediaType));
        //    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Delete_Data_By_Id()
        //{
        //    DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
        //    var response = await this.Client.DeleteAsync($"{URI}/{model.Id}");
        //    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Delete_Data_Invalid_Id()
        //{
        //    var response = await this.Client.DeleteAsync($"{URI}/0");
        //    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Get_Data_By_Supplier()
        //{
        //    DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
        //    var response = await this.Client.GetAsync($"{URI}/by-supplier?unitId={model.Items.FirstOrDefault().Details.FirstOrDefault().UnitId}&supplierId={model.SupplierId}");
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Get_Report()
        //{
        //    var response = await this.Client.GetAsync(URI + "/monitoring" + "?page=1&size=25");
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var json = await response.Content.ReadAsStringAsync();
        //    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());

        //    Assert.True(result.ContainsKey("apiVersion"));
        //    Assert.True(result.ContainsKey("message"));
        //    Assert.True(result.ContainsKey("data"));
        //    Assert.True(result["data"].GetType().Name.Equals("JArray"));
        //}

        //[Fact]
        //public async Task Should_Success_Get_Report_Excel()
        //{
        //    var response = await this.Client.GetAsync(URI + "/monitoring/download");
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Error_Get_Report_Without_Page()
        //{
        //    var response = await this.Client.GetAsync(URI + "/monitoring");
        //    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        //}

        //[Fact]
        //public async Task Should_Success_Get_Report_Excel_Empty_Data()
        //{
        //    var response = await this.Client.GetAsync($"{URI}/monitoring/download?doNo=0");
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //}

        private DeliveryOrderViewModel ViewModel
        {
            get
            {
                List<DeliveryOrderItemViewModel> items = new List<DeliveryOrderItemViewModel>
                {
                    new DeliveryOrderItemViewModel()
                    {
                        fulfillments = new List<DeliveryOrderFulFillMentViewModel>()
                        {
                            new DeliveryOrderFulFillMentViewModel()
                            {

                            }
                        }                    }
                };

                return new DeliveryOrderViewModel
                {
                    UId = null,

                    items = items,

                };
            }
        }

        private DeliveryOrder Model
        {
            get
            {
                return new DeliveryOrder
                {
                    Items = new List<DeliveryOrderItem>() { new DeliveryOrderItem(){

                        Details = new List<DeliveryOrderDetail>()
                            {

                            }
                        }
                    }
                };
            }
        }

        private ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(this.ViewModel, serviceProvider.Object, null);
            return new ServiceValidationExeption(validationContext, validationResults);
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return serviceProvider;
        }


        private DeliveryOrderController GetController(Mock<IDeliveryOrderFacade> facadeMock, Mock<IServiceProvider> serviceProviderMock, Mock<IMapper> autoMapperMock)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            //var servicePMock = GetServiceProvider();
            //servicePMock
            //    .Setup(x => x.GetService(typeof(IValidateService)))
            //    .Returns(validateM.Object);

            DeliveryOrderController controller = new DeliveryOrderController(autoMapperMock.Object, facadeMock.Object, serviceProviderMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = user.Object
                    }
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "7";

            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Should_Success_Get_All_Data()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Tuple<List<DeliveryOrder>, int, Dictionary<string, string>>(new List<DeliveryOrder>() { Model }, 1, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<DeliveryOrderViewModel>>(It.IsAny<List<DeliveryOrder>>()))
                .Returns(new List<DeliveryOrderViewModel> { ViewModel });

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), "{}");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Fail_Get_All_Data()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("error"));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<DeliveryOrderViewModel>>(It.IsAny<List<DeliveryOrder>>()))
                .Returns(new List<DeliveryOrderViewModel> { ViewModel });

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), "{}");
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_ByUser()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Tuple<List<DeliveryOrder>, int, Dictionary<string, string>>(new List<DeliveryOrder>() { Model }, 1, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<DeliveryOrderViewModel>>(It.IsAny<List<DeliveryOrder>>()))
                .Returns(new List<DeliveryOrderViewModel> { ViewModel });

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.GetByUser(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), "{}");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_ById()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new Tuple<DeliveryOrder, List<long>>(Model, new List<long>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrderViewModel>(It.IsAny<DeliveryOrder>()))
                .Returns(ViewModel);

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Fail_Get_ById_when_InvalidId()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new Tuple<DeliveryOrder, List<long>>(Model, new List<long>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrderViewModel>(It.IsAny<DeliveryOrder>()))
                .Returns(()=>null);

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Fail_Get_ById()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Throws(new Exception("error"));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrderViewModel>(It.IsAny<DeliveryOrder>()))
                .Returns(ViewModel);

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }



        [Fact]
        public async Task Should_Success_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock
                .Setup(s => s.Validate(It.IsAny<DeliveryOrderViewModel>()))
                .Verifiable();

            var serviceProviderMock = GetServiceProvider();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateMock.Object);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrder>(It.IsAny<DeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<DeliveryOrder>(), It.IsAny<string>()))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, serviceProviderMock, mockMapper);

            var response = await controller.Post(ViewModel);
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Fail_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock
                .Setup(s => s.Validate(It.IsAny<DeliveryOrderViewModel>()))
                .Verifiable();

            var serviceProviderMock = GetServiceProvider();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateMock.Object);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrder>(It.IsAny<DeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<DeliveryOrder>(), It.IsAny<string>()))
               .ThrowsAsync(new Exception("error"));

            var controller = GetController(mockFacade, serviceProviderMock, mockMapper);

            var response = await controller.Post(ViewModel);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Fail_Validate_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock
                .Setup(s => s.Validate(It.IsAny<DeliveryOrderViewModel>()))
                .Throws(GetServiceValidationExeption());

            var serviceProviderMock = GetServiceProvider();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateMock.Object);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrder>(It.IsAny<DeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<DeliveryOrder>(), It.IsAny<string>()))
               .ThrowsAsync(new Exception("error"));

            var controller = GetController(mockFacade, serviceProviderMock, mockMapper);

            var response = await controller.Post(ViewModel);
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Success_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock
                .Setup(s => s.Validate(It.IsAny<DeliveryOrderViewModel>()))
                .Verifiable();

            var serviceProviderMock = GetServiceProvider();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateMock.Object);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrder>(It.IsAny<DeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<DeliveryOrder>(), It.IsAny<string>()))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, serviceProviderMock, mockMapper);

            var response = await controller.Put(It.IsAny<int>(), ViewModel);
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Fail_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock
                .Setup(s => s.Validate(It.IsAny<DeliveryOrderViewModel>()))
                .Verifiable();

            var serviceProviderMock = GetServiceProvider();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateMock.Object);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrder>(It.IsAny<DeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<DeliveryOrder>(), It.IsAny<string>()))
               .ThrowsAsync(new Exception("error"));

            var controller = GetController(mockFacade, serviceProviderMock, mockMapper);

            var response = await controller.Put(It.IsAny<int>(), ViewModel);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Fail_Validate_Data_Update()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock
                .Setup(s => s.Validate(It.IsAny<DeliveryOrderViewModel>()))
                .Throws(GetServiceValidationExeption());

            var serviceProviderMock = GetServiceProvider();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateMock.Object);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<DeliveryOrder>(It.IsAny<DeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<DeliveryOrder>(), It.IsAny<string>()))
               .ThrowsAsync(new Exception("error"));

            var controller = GetController(mockFacade, serviceProviderMock, mockMapper);

            var response = await controller.Put(It.IsAny<int>(), ViewModel);
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Delete_Data()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade
                .Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(1);

            var mockMapper = new Mock<IMapper>();

            var controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.Delete(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public void Should_Fail_Delete_Data()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();
            mockFacade
                .Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<string>()))
                .Throws(new Exception("error"));

            var mockMapper = new Mock<IMapper>();

            var controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.Delete(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_BySupplier()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.ReadBySupplier(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<DeliveryOrder>());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<DeliveryOrderViewModel>>(It.IsAny<List<DeliveryOrder>>()))
                .Returns(new List<DeliveryOrderViewModel> { ViewModel });

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.BySupplier(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_GetReport()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.GetReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new Tuple<List<DeliveryOrderReportViewModel>, int>(new List<DeliveryOrderReportViewModel>(), It.IsAny<int>()));

            var mockMapper = new Mock<IMapper>();

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.GetReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Fail_GetReport()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.GetReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new Exception("error"));

            var mockMapper = new Mock<IMapper>();

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.GetReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_GetXls()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.GenerateExcel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                 It.IsAny<int>()))
                .Returns(new MemoryStream());

            var mockMapper = new Mock<IMapper>();

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.GetXls(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>());
            Assert.NotNull(response);
        }

        [Fact]
        public void Should_Fail_GetXls()
        {
            var mockFacade = new Mock<IDeliveryOrderFacade>();

            mockFacade.Setup(x => x.GenerateExcel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                 It.IsAny<int>()))
                .Throws(new Exception("error"));

            var mockMapper = new Mock<IMapper>();

            DeliveryOrderController controller = GetController(mockFacade, GetServiceProvider(), mockMapper);
            var response = controller.GetXls(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
    }
}
