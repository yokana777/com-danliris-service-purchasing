using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchasingDispositionViewModel;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.PurchasingDispositionControllers;
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

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.PurchasingDispositionControllerTests
{
    public class PurchasingDispositionControllerTest
    {
        private PurchasingDispositionViewModel ViewModel
        {
            get
            {
                List<PurchasingDispositionItemViewModel> items = new List<PurchasingDispositionItemViewModel>();

                List<PurchasingDispositionDetailViewModel> details = new List<PurchasingDispositionDetailViewModel>();

                items.Add(
                    new PurchasingDispositionItemViewModel
                    {
                      Details = details
                        
                    });

                details.Add(
                    new PurchasingDispositionDetailViewModel
                    {
                        PricePerDealUnit = 1000,
                        PriceTotal = 10000,
                        DealQuantity=10,

                    });

                return new PurchasingDispositionViewModel
                {
                    Supplier = new SupplierViewModel
                    {
                        Name="NameSupp",
                        Id= It.IsAny<int>()
                    },
                    Items = items
                };
            }
        }


        private PurchasingDisposition Model
        {
            get
            {
                return new PurchasingDisposition
                {
                    SupplierId = It.IsAny<int>(),
                    SupplierCode = "SupplierCode",
                    SupplierName = "SupplierName",
                    

                    PaymentMethod = "CASH",

                    InvoiceNo = "INV000111",
                    

                    Remark = null,

                    PaymentDueDate = new DateTimeOffset(), // ???

                    Items = new List<PurchasingDispositionItem> {
                        
                        new PurchasingDispositionItem
                        {
                            Details=new List<PurchasingDispositionDetail> { }
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

        private PurchasingDispositionController GetController(Mock<IPurchasingDispositionFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper)
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

            PurchasingDispositionController controller = new PurchasingDispositionController(servicePMock.Object, mapper.Object, facadeM.Object)
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
        public void Should_Success_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<PurchasingDisposition>(It.IsAny<PurchasingDispositionViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IPurchasingDispositionFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<PurchasingDisposition>(), "unittestusername", 7))
               .ReturnsAsync(1);

            

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IPurchasingDispositionFacade>();


            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<PurchasingDisposition>(It.IsAny<PurchasingDispositionViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IPurchasingDispositionFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<PurchasingDisposition>(), "unittestusername", 7))
               .ReturnsAsync(1);


            PurchasingDispositionController controller = new PurchasingDispositionController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data_By_User()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Verifiable();

            var mockFacade = new Mock<IPurchasingDispositionFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<PurchasingDisposition>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<PurchasingDispositionViewModel>>(It.IsAny<List<PurchasingDisposition>>()))
                .Returns(new List<PurchasingDispositionViewModel> { ViewModel });


            PurchasingDispositionController controller = GetController(mockFacade, validateMock, mockMapper);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var mockFacade = new Mock<IPurchasingDispositionFacade>();

            mockFacade.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<PurchasingDisposition>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();

            PurchasingDispositionController controller = new PurchasingDispositionController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get(1, 25, "{}", null, "{}");
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Verifiable();

            var mockFacade = new Mock<IPurchasingDispositionFacade>();

            mockFacade.Setup(x => x.ReadModelById(It.IsAny<int>()))
                .Returns(new PurchasingDisposition());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<PurchasingDispositionViewModel>(It.IsAny<PurchasingDisposition>()))
                .Returns(ViewModel);

            PurchasingDispositionController controller = GetController(mockFacade, validateMock, mockMapper);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IPurchasingDispositionFacade>();
            mockFacade.Setup(x => x.ReadModelById(It.IsAny<int>()))
                .Returns(Model);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<PurchasingDispositionViewModel>(It.IsAny<PurchasingDisposition>()))
                .Throws(new Exception("Error Mapping"));

            PurchasingDispositionController controller = new PurchasingDispositionController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Verifiable();

            var mockFacade = new Mock<IPurchasingDispositionFacade>();

            mockFacade.Setup(x => x.ReadModelById(It.IsAny<int>()))
                .Returns(new PurchasingDisposition());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<PurchasingDispositionViewModel>(It.IsAny<PurchasingDisposition>()))
                .Returns(ViewModel);

            PurchasingDispositionController controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<PurchasingDispositionViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IPurchasingDispositionFacade>();


            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<PurchasingDispositionViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<PurchasingDispositionViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<PurchasingDisposition>(It.IsAny<PurchasingDispositionViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IPurchasingDispositionFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<PurchasingDisposition>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = new PurchasingDispositionController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<PurchasingDispositionViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
    }
}
