using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.UnitPaymentCorrectionNoteController;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.UnitPaymentCorrectionNoteControllerTests
{
    public class UnitPaymentQuantityCorrectionNoteControllerTest
    {
        private UnitPaymentCorrectionNoteViewModel ViewModel
        {
            get
            {
                return new UnitPaymentCorrectionNoteViewModel
                {
                    supplier = new SupplierViewModel
                    {
                        import = false
                    },
                    items = new List<UnitPaymentCorrectionNoteItemViewModel>()
                };
            }
        }

        private UnitPaymentCorrectionNote Model
        {
            get
            {
                return new UnitPaymentCorrectionNote
                {
                    DivisionId = "DivisionId",
                    DivisionCode = "DivisionCode",
                    DivisionName = "DivisionName",

                    SupplierId = "SupplierId",
                    SupplierCode = "SupplierCode",
                    SupplierName = "SupplierName",

                    UPCNo = "18-06-G-NKI-001",
                    UPOId = 1,

                    UPONo = "18-06-G-NKI-001",

                    CorrectionDate = new DateTimeOffset(),

                    CorrectionType = null,

                    InvoiceCorrectionDate = new DateTimeOffset(),
                    InvoiceCorrectionNo  = "123456",

                    useVat = true,
                    VatTaxCorrectionDate = new DateTimeOffset(),
                    VatTaxCorrectionNo = null,

                    useIncomeTax = true,
                    IncomeTaxCorrectionName = null,
                    IncomeTaxCorrectionNo = null,

                    ReleaseOrderNoteNo = "123456",
                    ReturNoteNo = "",

                    CategoryId = "CategoryId ",
                    CategoryCode = "CategoryCode",
                    CategoryName = "CategoryName",

                    Remark = null,

                    DueDate = new DateTimeOffset(), // ???

                    Items = new List<UnitPaymentCorrectionNoteItem> { }
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

        private UnitPaymentQuantityCorrectionNoteController GetController(Mock<IUnitPaymentQuantityCorrectionNoteFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper, Mock<IUnitPaymentOrderFacade> facadeSpb)
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

            UnitPaymentQuantityCorrectionNoteController controller = new UnitPaymentQuantityCorrectionNoteController(servicePMock.Object, mapper.Object, facadeM.Object, facadeSpb.Object)
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
            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentCorrectionNote>(), 0, new Dictionary<string, string>()));

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<UnitPaymentCorrectionNoteViewModel>>(It.IsAny<List<UnitPaymentCorrectionNote>>()))
                .Returns(new List<UnitPaymentCorrectionNoteViewModel> { ViewModel });

            UnitPaymentQuantityCorrectionNoteController controller = new UnitPaymentQuantityCorrectionNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeSpb.Object);
            var response = controller.Get(1, 25, "{}", null, "{}");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentCorrectionNote>(), 0, new Dictionary<string, string>()));

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();

            UnitPaymentQuantityCorrectionNoteController controller = new UnitPaymentQuantityCorrectionNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeSpb.Object);
            var response = controller.Get(1, 25, "{}", null, "{}");
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();

            UnitPaymentQuantityCorrectionNoteController controller = new UnitPaymentQuantityCorrectionNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeSpb.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<UnitPaymentCorrectionNoteViewModel>(It.IsAny<UnitPaymentCorrectionNote>()))
                .Throws(new Exception("Error Mapping"));

            UnitPaymentQuantityCorrectionNoteController controller = new UnitPaymentQuantityCorrectionNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeSpb.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentCorrectionNoteViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<UnitPaymentCorrectionNote>(It.IsAny<UnitPaymentCorrectionNoteViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<UnitPaymentCorrectionNote>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var controller = GetController(mockFacade, validateMock, mockMapper, mockFacadeSpb);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentCorrectionNoteViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<UnitPaymentCorrectionNote>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var controller = GetController(mockFacade, validateMock, mockMapper, mockFacadeSpb);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentCorrectionNoteViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<UnitPaymentCorrectionNote>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var controller = GetController(mockFacade, validateMock, mockMapper, mockFacadeSpb);

            var response = controller.Post(null).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_PDF_Nota_Koreksi_By_Id()
        {
            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();

            UnitPaymentQuantityCorrectionNoteController controller = new UnitPaymentQuantityCorrectionNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeSpb.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.GetPDF(It.IsAny<int>());
            Assert.NotEqual(null, response.GetType().GetProperty("FileStream"));
        }

        [Fact]
        public void Should_Success_Get_PDF_Nota_Retur_By_Id()
        {
            var mockFacade = new Mock<IUnitPaymentQuantityCorrectionNoteFacade>();
            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(Model);

            var mockFacadeSpb = new Mock<IUnitPaymentOrderFacade>();
            mockFacadeSpb.Setup(x => x.Read(1, 25, "{}", null, "{}"))
                .Returns(Tuple.Create(new List<UnitPaymentOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();

            UnitPaymentQuantityCorrectionNoteController controller = new UnitPaymentQuantityCorrectionNoteController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, mockFacadeSpb.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.GetPDFNotaRetur(It.IsAny<int>());
            Assert.NotEqual(null, response.GetType().GetProperty("FileStream"));
        }

        //[Fact]
        //public void Should_Success_Update_Data()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentOrderViewModel>())).Verifiable();

        //    var mockFacade = new Mock<IUnitPaymentOrderFacade>();
        //    mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<UnitPaymentOrder>(), "unittestusername"))
        //       .ReturnsAsync(1);

        //    var mockMapper = new Mock<IMapper>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Put(1, this.ViewModel).Result;
        //    Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        //}

        //[Fact]
        //public void Should_Validate_Update_Data()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentOrderViewModel>())).Throws(GetServiceValidationExeption());

        //    var mockFacade = new Mock<IUnitPaymentOrderFacade>();
        //    mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<UnitPaymentOrder>(), "unittestusername"))
        //       .ReturnsAsync(1);

        //    var mockMapper = new Mock<IMapper>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Put(1, this.ViewModel).Result;
        //    Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        //}

        //[Fact]
        //public void Should_Error_Update_Data()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentOrderViewModel>())).Verifiable();

        //    var mockFacade = new Mock<IUnitPaymentOrderFacade>();
        //    mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<UnitPaymentOrder>(), "unittestusername"))
        //       .ThrowsAsync(new Exception("Invalid Id"));

        //    var mockMapper = new Mock<IMapper>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Put(0, this.ViewModel).Result;
        //    Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        //}

        //[Fact]
        //public void Should_Success_Delete_Data()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentOrderViewModel>())).Verifiable();

        //    var mockFacade = new Mock<IUnitPaymentOrderFacade>();
        //    mockFacade.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<string>()))
        //       .ReturnsAsync(1);

        //    var mockMapper = new Mock<IMapper>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Delete(1).Result;
        //    Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        //}

        //[Fact]
        //public void Should_Error_Delete_Data()
        //{
        //    var validateMock = new Mock<IValidateService>();
        //    validateMock.Setup(s => s.Validate(It.IsAny<UnitPaymentOrderViewModel>())).Verifiable();

        //    var mockFacade = new Mock<IUnitPaymentOrderFacade>();
        //    mockFacade.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<string>()))
        //       .ThrowsAsync(new Exception());

        //    var mockMapper = new Mock<IMapper>();

        //    var controller = GetController(mockFacade, validateMock, mockMapper);

        //    var response = controller.Delete(1).Result;
        //    Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        //}
    }
}
