using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentInternNoteControllers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.GarmentInternNoteTests
{
    public class GarmentInternNoteControllerTest
    {
        private GarmentInternNoteViewModel ViewModel
        {
            get
            {
                return new GarmentInternNoteViewModel
                {
                    supplier = new SupplierViewModel(),
                    currency = new CurrencyViewModel(),
                    items = new List<GarmentInternNoteItemViewModel>
                    {
                        new GarmentInternNoteItemViewModel()
                        {
                            garmentInvoice = new GarmentInvoiceViewModel(),
                            details = new List<GarmentInternNoteDetailViewModel>
                            {
                                new GarmentInternNoteDetailViewModel()
                                {
                                    unit = new UnitViewModel(),
                                    product = new ProductViewModel(),
                                    uomUnit = new UomViewModel(),
                                    deliveryOrder = new Lib.ViewModels.GarmentDeliveryOrderViewModel.GarmentDeliveryOrderViewModel(),

                                }
                            }
                        }
                    }
                };
            }
        }
        private GarmentInternNote Model
        {
            get
            {
                return new GarmentInternNote { };
            }
        }

        private GarmentDeliveryOrder DeliveryOrderModel
        {
            get
            {
                return new GarmentDeliveryOrder { };
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

        private GarmentInternNoteController GetController(Mock<IGarmentInternNoteFacade> facadeM, Mock<IGarmentDeliveryOrderFacade> facadeDO , Mock<IValidateService> validateM, Mock<IMapper> mapper,Mock<IGarmentInvoice> facadeINV)
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

            GarmentInternNoteController controller = new GarmentInternNoteController(servicePMock.Object, mapper.Object, facadeM.Object,facadeDO.Object, facadeINV.Object)
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
        public void Should_Success_Get_All_Data_By_User()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            var INVFacade = new Mock<IGarmentInvoice>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentInternNote>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentInternNoteViewModel>>(It.IsAny<List<GarmentInternNote>>()))
                .Returns(new List<GarmentInternNoteViewModel> { ViewModel });
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderViewModel>(It.IsAny<GarmentDeliveryOrder>()))
                .Returns(new GarmentDeliveryOrderViewModel());

            GarmentInternNoteController controller = GetController(mockFacade,IPOmockFacade, validateMock, mockMapper,INVFacade);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }
        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            var mockMapper = new Mock<IMapper>();
            var INVFacade = new Mock<IGarmentInvoice>();
            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            GarmentInternNoteController controller = new GarmentInternNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object, INVFacade.Object);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
        [Fact]
        public void Should_Success_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentInternNote>(It.IsAny<GarmentInternNoteViewModel>()))
                .Returns(Model);
            var INVFacade = new Mock<IGarmentInvoice>();

            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentInternNote>(), false, "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var controller = GetController(mockFacade,IPOmockFacade, validateMock, mockMapper, INVFacade);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentInternNoteFacade>();

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = GetController(mockFacade,IPOmockFacade, validateMock, mockMapper, INVFacade);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data_Empty()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentInternNote>(It.IsAny<GarmentInternNote>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentInternNote>(),false, "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = GetController(mockFacade,IPOmockFacade, validateMock, mockMapper, INVFacade);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Sucscess_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentInternNoteViewModel>(It.IsAny<GarmentInternNote>()))
                .Returns(ViewModel);
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderViewModel>(It.IsAny<GarmentDeliveryOrder>()))
                .Returns(new GarmentDeliveryOrderViewModel());

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            IPOmockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                 .Returns(DeliveryOrderModel);

            var INVFacade = new Mock<IGarmentInvoice>();

            GarmentInternNoteController controller = GetController(mockFacade, IPOmockFacade , null, mockMapper, INVFacade);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data()
        {
            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentInternNote>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentInternNoteViewModel>>(It.IsAny<List<GarmentInternNote>>()))
                .Returns(new List<GarmentInternNoteViewModel> { ViewModel });
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderViewModel>(It.IsAny<GarmentDeliveryOrder>()))
                .Returns(new GarmentDeliveryOrderViewModel());

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            GarmentInternNoteController controller = GetController(mockFacade,IPOmockFacade, null, mockMapper, INVFacade);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentInternNote>(It.IsAny<GarmentInternNoteViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentInternNote>(), false, "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            GarmentInternNoteController controller = new GarmentInternNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object, INVFacade.Object);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentInternNote>(It.IsAny<GarmentInternNoteViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentInternNote>(),false, "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = new GarmentInternNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object, INVFacade.Object);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentInternNoteViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Delete_Data()
        {
            var validateMock = new Mock<IValidateService>();
            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.Delete(It.IsAny<int>(), "unittestusername"))
                .Returns(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = GetController(mockFacade, IPOmockFacade, validateMock, mockMapper, INVFacade);

            var response = controller.Delete(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentInternNote>(It.IsAny<GarmentInternNoteViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<GarmentInternNote>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = GetController(mockFacade, IPOmockFacade, validateMock, mockMapper, INVFacade);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentInternNoteViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentInternNoteViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentInternNoteFacade>();

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var INVFacade = new Mock<IGarmentInvoice>();

            var controller = GetController(mockFacade, IPOmockFacade, validateMock, mockMapper, INVFacade);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentInternNoteViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_PDF_By_Id()
        {
            var mockFacade = new Mock<IGarmentInternNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentInternNoteViewModel>(It.IsAny<GarmentInternNote>()))
                .Returns(ViewModel);

            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderViewModel>(It.IsAny<GarmentDeliveryOrder>()))
                .Returns(new GarmentDeliveryOrderViewModel());

            mockMapper.Setup(x => x.Map<GarmentInvoiceViewModel>(It.IsAny<GarmentInvoice>()))
                .Returns(new GarmentInvoiceViewModel());

            var IPOmockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            IPOmockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                 .Returns(new GarmentDeliveryOrder());

            var INVmockFacade = new Mock<IGarmentInvoice>();
            INVmockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                 .Returns(new GarmentInvoice());

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentInternNoteController controller = GetController(mockFacade, IPOmockFacade, null, mockMapper, INVmockFacade);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.GetInternNotePDF(It.IsAny<int>());
            Assert.NotEqual(null, response.GetType().GetProperty("FileStream"));
        }
    }
}