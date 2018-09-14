using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.DailyBankTransaction;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.DailyBankTransactionControllerTest
{
    public class DailyBankTransactionControllerTest
    {
        private DailyBankTransactionViewModel ViewModel
        {
            get
            {
                return new DailyBankTransactionViewModel()
                {
                    Bank = new AccountBankViewModel() { currency = new CurrencyViewModel() },
                    Supplier = new SupplierViewModel(),
                    Buyer = new BuyerViewModel()
                };
            }
        }

        private DailyBankTransactionModel Model
        {
            get
            {
                return new DailyBankTransactionModel();
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

        private DailyBankTransactionControllers GetController(Mock<IDailyBankTransactionFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            var servicePMock = GetServiceProvider();
            servicePMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateM.Object);

            DailyBankTransactionControllers controller = new DailyBankTransactionControllers(servicePMock.Object, facadeM.Object, mapper.Object)
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

            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Should_Success_Get_All_Data()
        {
            var mockFacade = new Mock<IDailyBankTransactionFacade>();
            mockFacade.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(new ReadResponse(new List<object>(), 1, new Dictionary<string, string>()));
            var mockMapper = new Mock<IMapper>();

            DailyBankTransactionControllers controller = new DailyBankTransactionControllers(GetServiceProvider().Object, mockFacade.Object, mockMapper.Object);
            var response = controller.Get(1, 25, "{}", null, "{}");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Not_Found_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IDailyBankTransactionFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .ReturnsAsync((DailyBankTransactionModel)null);

            var mockMapper = new Mock<IMapper>();

            DailyBankTransactionControllers controller = new DailyBankTransactionControllers(GetServiceProvider().Object, mockFacade.Object, mockMapper.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "test";

            var response = controller.GetById(It.IsAny<int>()).Result;
            Assert.Equal((int)HttpStatusCode.NotFound, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IDailyBankTransactionFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
               .Throws(new Exception());

            var mockMapper = new Mock<IMapper>();

            DailyBankTransactionControllers controller = new DailyBankTransactionControllers(GetServiceProvider().Object, mockFacade.Object, mockMapper.Object);
            var response = controller.GetById(It.IsAny<int>()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Return_Bad_Request_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<DailyBankTransactionViewModel>())).Throws(GetServiceValidationExeption());

            var mockFacade = new Mock<IDailyBankTransactionFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<DailyBankTransactionModel>(), "unittestusername"))
               .ReturnsAsync(1);

            var mockMapper = new Mock<IMapper>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IDailyBankTransactionFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .ReturnsAsync(Model);

            var mockMapper = new Mock<IMapper>();

            DailyBankTransactionControllers controller = new DailyBankTransactionControllers(GetServiceProvider().Object, mockFacade.Object, mockMapper.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "test";

            var response = controller.GetById(It.IsAny<int>()).Result;
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<DailyBankTransactionViewModel>())).Verifiable();

            var mockFacade = new Mock<IDailyBankTransactionFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<DailyBankTransactionModel>(), "unittestusername"))
               .ThrowsAsync(new Exception());

            var mockMapper = new Mock<IMapper>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
    }
}
