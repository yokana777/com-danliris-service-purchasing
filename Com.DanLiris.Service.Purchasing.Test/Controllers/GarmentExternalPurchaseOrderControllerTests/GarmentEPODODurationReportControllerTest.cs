using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentExternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentExternalPurchaseOrderControllers;
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

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.GarmentExternalPurchaseOrderControllerTests
{
    public class GarmentEPODODurationReportControllerTest
    {
        private GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel ViewModel
        {
            get
            {
                return new GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel
                {
                    
                };
            }
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

        private GarmentExternalPurchaseOrderDeliveryOrderDurationReportController GetController(Mock<IGarmentExternalPurchaseOrderFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper, Mock<IGarmentInternalPurchaseOrderFacade> facadeIPO)
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

            var controller = new GarmentExternalPurchaseOrderDeliveryOrderDurationReportController(servicePMock.Object, mapper.Object, facadeM.Object)
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
        public void Should_Success_Get_Report_Data()
        {
            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.GetEPODODurationReport(null, null, It.IsAny<string>(), null, null, It.IsAny<int>(), It.IsAny<int>(), "{}", It.IsAny<int>()))
                .Returns(Tuple.Create(new List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> { ViewModel }, 25));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel>>(It.IsAny<List<GarmentExternalPurchaseOrder>>()))
                .Returns(new List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> { ViewModel });

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            GarmentExternalPurchaseOrderDeliveryOrderDurationReportController controller = new GarmentExternalPurchaseOrderDeliveryOrderDurationReportController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";
            var response = controller.GetReport(null, null, It.IsAny<string>(), null, null, 1, 25, "{}");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Report_Xls_Data()
        {
            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.GetEPODODurationReport(null, null, It.IsAny<string>(), null, null, It.IsAny<int>(), It.IsAny<int>(), "{}", It.IsAny<int>()))
                .Returns(Tuple.Create(new List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> { ViewModel }, 25));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel>>(It.IsAny<List<GarmentExternalPurchaseOrder>>()))
                .Returns(new List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> { ViewModel });

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            GarmentExternalPurchaseOrderDeliveryOrderDurationReportController controller = new GarmentExternalPurchaseOrderDeliveryOrderDurationReportController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";
            var response = controller.GetXls(null, null, It.IsAny<string>(), null, null);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Report_Xls_Data()
        {
            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.GetEPODODurationReport(null, null, It.IsAny<string>(), null, null, It.IsAny<int>(), It.IsAny<int>(), "{}", It.IsAny<int>()))
                .Returns(Tuple.Create(new List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> { ViewModel }, 25));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel>>(It.IsAny<List<GarmentInternalPurchaseOrder>>()))
                .Returns(new List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> { ViewModel });

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            GarmentExternalPurchaseOrderDeliveryOrderDurationReportController controller = new GarmentExternalPurchaseOrderDeliveryOrderDurationReportController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";
            var response = controller.GetXls(null, null, "0-30 hari", null, null);
            Assert.Null(response.GetType().GetProperty("FileStream"));
        }
    }
}
