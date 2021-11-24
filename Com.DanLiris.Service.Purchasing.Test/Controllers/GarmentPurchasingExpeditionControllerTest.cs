using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingExpedition;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers
{
    public class GarmentPurchasingExpeditionControllerTest
    {
        Mock<IServiceProvider> GetServiceProvider()
        {
            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
              .Setup(s => s.GetService(typeof(IdentityService)))
              .Returns(new IdentityService() { TimezoneOffset = 1, Token = "token", Username = "username" });

            return serviceProviderMock;
        }

        protected GarmentPurchasingExpeditionController GetController(IServiceProvider serviceProvider, Mock<IMapper> mapper)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentPurchasingExpeditionController controller = new GarmentPurchasingExpeditionController(serviceProvider, mapper.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");

            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Should_Success_GetGarmentInternalNotes()
        {
            Mock<IGarmentPurchasingExpeditionService> serviceMock = new Mock<IGarmentPurchasingExpeditionService>();
            var mockMapper = new Mock<IMapper>();
            var mockService = GetServiceProvider();
            mockService
                .Setup(s => s.GetService(typeof(IGarmentPurchasingExpeditionService)))
                .Returns(serviceMock.Object);

            var controller = GetController(mockService.Object, mockMapper);
            GarmentInternalNoteFilterDto filterDto = new GarmentInternalNoteFilterDto();
            var response = controller.GetGarmentInternalNotes("", filterDto);
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_GetGarmentInternalNotes()
        {
            var mockMapper = new Mock<IMapper>();

            var controller = GetController(GetServiceProvider().Object, mockMapper);
            GarmentInternalNoteFilterDto filterDto = new GarmentInternalNoteFilterDto();
            var response = controller.GetGarmentInternalNotes("", filterDto);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_GetGarmentDispositionNotes()
        {
            Mock<IGarmentPurchasingExpeditionService> serviceMock = new Mock<IGarmentPurchasingExpeditionService>();
            var mockMapper = new Mock<IMapper>();
            var mockService = GetServiceProvider();
            mockService
                .Setup(s => s.GetService(typeof(IGarmentPurchasingExpeditionService)))
                .Returns(serviceMock.Object);

            var controller = GetController(mockService.Object, mockMapper);
            var response = controller.GetGarmentDispositionNotes("", Lib.Enums.PurchasingGarmentExpeditionPosition.Purchasing);
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_GetGarmentDispositionNotes()
        {
            var mockMapper = new Mock<IMapper>();

            var controller = GetController(GetServiceProvider().Object, mockMapper);
            var response = controller.GetGarmentDispositionNotes("", Lib.Enums.PurchasingGarmentExpeditionPosition.Purchasing);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }


        [Fact]
        public void Should_Success_GetGarmentDispositionNotesVerification()
        {
            Mock<IGarmentPurchasingExpeditionService> serviceMock = new Mock<IGarmentPurchasingExpeditionService>();
            var mockMapper = new Mock<IMapper>();
            var mockService = GetServiceProvider();
            mockService
                .Setup(s => s.GetService(typeof(IGarmentPurchasingExpeditionService)))
                .Returns(serviceMock.Object);

            var controller = GetController(mockService.Object, mockMapper);
            var response = controller.GetGarmentDispositionNotesVerification("", Lib.Enums.PurchasingGarmentExpeditionPosition.Purchasing);
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_GetGarmentDispositionNotesVerification()
        {
            var mockMapper = new Mock<IMapper>();

            var controller = GetController(GetServiceProvider().Object, mockMapper);
            var response = controller.GetGarmentDispositionNotesVerification("", Lib.Enums.PurchasingGarmentExpeditionPosition.Purchasing);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

    }
}
