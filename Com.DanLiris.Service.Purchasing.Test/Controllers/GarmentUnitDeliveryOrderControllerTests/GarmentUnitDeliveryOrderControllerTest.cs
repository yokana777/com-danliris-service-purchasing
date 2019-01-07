using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentUnitDeliveryOrderControllers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.GarmentUnitDeliveryOrderControllerTests
{
    public class GarmentUnitDeliveryOrderControllerTest
    {
        private GarmentUnitDeliveryOrderViewModel ViewModel
        {
            get
            {
                return new GarmentUnitDeliveryOrderViewModel
                {
                    Storage = new Lib.ViewModels.IntegrationViewModel.StorageViewModel(),
                    StorageRequest = new Lib.ViewModels.IntegrationViewModel.StorageViewModel(),
                    UnitRequest = new UnitViewModel(),
                    UnitSender = new UnitViewModel(),
                    Items = new List<GarmentUnitDeliveryOrderItemViewModel>
                    {
                        new GarmentUnitDeliveryOrderItemViewModel()
                        {
                            Id = It.IsAny<int>(),
                            ProductId = It.IsAny<int>(),
                            ProductCode = "ProductCode",
                            ProductName = "ProductName",
                            ProductRemark = "ProductRemark",
                            UomId = It.IsAny<int>(),
                            UomUnit = "UOMUnit",
                            URNId = It.IsAny<int>(),
                            URNItemId = It.IsAny<int>(),
                            DODetailId = It.IsAny<int>(),
                            EPOItemId = It.IsAny<int>(),
                            POItemId = It.IsAny<int>(),
                            PRItemId = It.IsAny<int>(),
                            PricePerDealUnit = It.IsAny<int>(),
                            POSerialNumber = "PO12345",
                            URNNo = "URNNO",
                            Quantity = It.IsAny<int>(),
                            FabricType = "SAMPLE"
                        }
                    }
                };
            }
        }
        private GarmentUnitDeliveryOrder Model
        {
            get
            {
                return new GarmentUnitDeliveryOrder { };
            }
        }
        private ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(ViewModel, serviceProvider.Object, null);
            return new ServiceValidationExeption(validationContext, validationResults);
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        private GarmentUnitDeliveryOrderControllers GetController(Mock<IGarmentUnitDeliveryOrder> facade, Mock<IValidateService> validateM, Mock<IMapper> mapper)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            var servicePMock = GetServiceProvider();
            if (validateM != null)
            {
                servicePMock
                    .Setup(x => x.GetService(typeof(IValidateService)))
                    .Returns(validateM.Object);
            }

            GarmentUnitDeliveryOrderControllers controller = new GarmentUnitDeliveryOrderControllers(servicePMock.Object, mapper.Object, facade.Object)
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

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockMapper = new Mock<IMapper>();
            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            var INVFacade = new Mock<IGarmentInvoice>();

            GarmentUnitDeliveryOrderControllers controller = new GarmentUnitDeliveryOrderControllers(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            var mockMapper = new Mock<IMapper>();
            var INVFacade = new Mock<IGarmentInvoice>();
            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            GarmentUnitDeliveryOrderControllers controller = new GarmentUnitDeliveryOrderControllers(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
        [Fact]
        public void Should_Success_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentUnitDeliveryOrder>(It.IsAny<GarmentUnitDeliveryOrderViewModel>()))
                .Returns(Model);
            var INVFacade = new Mock<IGarmentInvoice>();

            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentUnitDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        //[Fact]
        //public void Should_Validate_Create_Data()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Throws(GetServiceValidationExeption());

        //    var mockMapper = new Mock<IMapper>();

        //    var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();

        //    var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

        //    var INVFacade = new Mock<IGarmentInvoice>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Post(this.ViewModel).Result;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        //}

        //[Fact]
        //public void Should_Validate_Create_Data_Empty()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Throws(GetServiceValidationExeption());

        //    var mockMapper = new Mock<IMapper>();
        //    mockMapper.Setup(x => x.Map<GarmentUnitDeliveryOrder>(It.IsAny<GarmentUnitDeliveryOrder>()))
        //        .Returns(Model);

        //    var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
        //    mockFacade.Setup(x => x.Create(It.IsAny<GarmentUnitDeliveryOrder>(), "unittestusername", 7))
        //       .ReturnsAsync(1);

        //    var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

        //    var INVFacade = new Mock<IGarmentInvoice>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Post(this.ViewModel).Result;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        //}

        [Fact]
        public void Should_Sucscess_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentUnitDeliveryOrderViewModel>(It.IsAny<GarmentUnitDeliveryOrder>()))
                .Returns(ViewModel);

            GarmentUnitDeliveryOrderControllers controller = GetController(mockFacade, null, mockMapper);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }
        [Fact]
        public void Should_Success_Get_All_Data()
        {
            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentUnitDeliveryOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentUnitDeliveryOrderViewModel>>(It.IsAny<List<GarmentUnitDeliveryOrder>>()))
                .Returns(new List<GarmentUnitDeliveryOrderViewModel> { ViewModel });

            GarmentUnitDeliveryOrderControllers controller = GetController(mockFacade, null, mockMapper);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentUnitDeliveryOrder>(It.IsAny<GarmentUnitDeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentUnitDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            GarmentUnitDeliveryOrderControllers controller = new GarmentUnitDeliveryOrderControllers(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentUnitDeliveryOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentUnitDeliveryOrder>(It.IsAny<GarmentUnitDeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentUnitDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = new GarmentUnitDeliveryOrderControllers(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentUnitDeliveryOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Delete_Data()
        {
            var validateMock = new Mock<IValidateService>();
            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.Delete(It.IsAny<int>(), "unittestusername"))
                .Returns(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Delete(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentUnitDeliveryOrder>(It.IsAny<GarmentUnitDeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<GarmentUnitDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentUnitDeliveryOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        //[Fact]
        //public void Should_Validate_Update_Data()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitDeliveryOrderViewModel>())).Throws(GetServiceValidationExeption());

        //    var mockMapper = new Mock<IMapper>();

        //    var mockFacade = new Mock<IGarmentUnitDeliveryOrder>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentUnitDeliveryOrderViewModel>()).Result;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        //}
    }
}
