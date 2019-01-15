using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentUnitExpenditureNoteControllers;
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

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.GarmentUnitExpenditureNoteControllerTests
{
    public class GarmentUnitExpenditureNoteControllerTest
    {
        private GarmentUnitExpenditureNoteViewModel ViewModel
        {
            get
            {
                return new GarmentUnitExpenditureNoteViewModel
                {
                    Items = new List<GarmentUnitExpenditureNoteItemViewModel>
                    {
                        new GarmentUnitExpenditureNoteItemViewModel()
                    }
                };
            }
        }

        private GarmentUnitExpenditureNote Model
        {
            get
            {
                return new GarmentUnitExpenditureNote
                {
                    Items = new List<GarmentUnitExpenditureNoteItem>
                    {
                        new GarmentUnitExpenditureNoteItem()
                    }
                };
            }
        }

        private ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(ViewModel, serviceProvider.Object, null);
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

        private GarmentUnitExpenditureNoteController GetController(Mock<IGarmentUnitExpenditureNoteFacade> facadeM, Mock<IGarmentUnitDeliveryOrder> facadeUnitDO, Mock<IValidateService> validateM, Mock<IMapper> mapper)
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

            var controller = new GarmentUnitExpenditureNoteController(servicePMock.Object, mapper.Object, facadeM.Object, facadeUnitDO.Object)
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
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(new ReadResponse<object>(new List<object>(), 0, new Dictionary<string, string>()));
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentUnitExpenditureNoteViewModel>>(It.IsAny<List<GarmentUnitExpenditureNote>>()))
                .Returns(new List<GarmentUnitExpenditureNoteViewModel> { ViewModel });

            GarmentUnitExpenditureNoteController controller = GetController(mockFacade, mockFacadeUnitDO, null, mockMapper);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            var mockMapper = new Mock<IMapper>();
            GarmentUnitExpenditureNoteController controller = new GarmentUnitExpenditureNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeUnitDO.Object);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(ViewModel);

            var mockMapper = new Mock<IMapper>();

            GarmentUnitExpenditureNoteController controller = GetController(mockFacade, mockFacadeUnitDO, null, mockMapper);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns((GarmentUnitExpenditureNoteViewModel)null);
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();

            var mockMapper = new Mock<IMapper>();

            GarmentUnitExpenditureNoteController controller = GetController(mockFacade, mockFacadeUnitDO, null, mockMapper);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Create_Data()
        {
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentUnitExpenditureNote>>(It.IsAny<List<GarmentUnitExpenditureNoteViewModel>>()))
                .Returns(new List<GarmentUnitExpenditureNote>());

            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitExpenditureNoteViewModel>()))
                .Verifiable();

            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentUnitExpenditureNote>()))
               .ReturnsAsync(1);

            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();

            var controller = GetController(mockFacade, mockFacadeUnitDO, validateMock, mockMapper);

            var response = controller.Post(ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitExpenditureNoteViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();

            var controller = GetController(mockFacade, mockFacadeUnitDO, validateMock, mockMapper);

            var response = controller.Post(ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var mockMapper = new Mock<IMapper>();
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();

            var controller = new GarmentUnitExpenditureNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeUnitDO.Object);

            var response = controller.Post(new GarmentUnitExpenditureNoteViewModel()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Update_Data()
        {
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentUnitExpenditureNote>>(It.IsAny<List<GarmentUnitExpenditureNoteViewModel>>()))
                .Returns(new List<GarmentUnitExpenditureNote>());

            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitExpenditureNoteViewModel>()))
                .Verifiable();

            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<GarmentUnitExpenditureNote>()))
               .ReturnsAsync(1);

            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();

            var controller = GetController(mockFacade, mockFacadeUnitDO, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentUnitExpenditureNoteViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();

            var controller = GetController(mockFacade, mockFacadeUnitDO, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Update_Data()
        {
            var mockMapper = new Mock<IMapper>();
            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();

            var controller = new GarmentUnitExpenditureNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeUnitDO.Object);

            var response = controller.Put(It.IsAny<int>(), new GarmentUnitExpenditureNoteViewModel()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Delete_Data()
        {
            var mockMapper = new Mock<IMapper>();
            var validateMock = new Mock<IValidateService>();

            var mockFacade = new Mock<IGarmentUnitExpenditureNoteFacade>();
            mockFacade.Setup(x => x.Delete(It.IsAny<int>()))
               .ReturnsAsync(1);
            var mockFacadeUnitDO = new Mock<IGarmentUnitDeliveryOrder>();

            var controller = GetController(mockFacade, mockFacadeUnitDO, validateMock, mockMapper);

            var response = controller.Delete(It.IsAny<int>()).Result;
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Delete_Data()
        {
            var controller = new GarmentUnitExpenditureNoteController(GetServiceProvider().Object, null, null, null);

            var response = controller.Delete(It.IsAny<int>()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
    }
}
