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
    public class GarmentExternalPurchaseOrderControllerTest
    {
        private GarmentExternalPurchaseOrderViewModel ViewModel
        {
            get
            {
                return new GarmentExternalPurchaseOrderViewModel
                {
                    Items = new List<GarmentExternalPurchaseOrderItemViewModel>
                    {
                        new GarmentExternalPurchaseOrderItemViewModel()
                    }
                };
            }
        }

        private GarmentExternalPurchaseOrderViewModel ViewModelFabric
        {
            get
            {
                return new GarmentExternalPurchaseOrderViewModel
                {
                    Category = "FABRIC",
                    IncomeTax= new IncomeTaxViewModel
                    {
                        Id= It.IsAny<int>(),
                        Name="tax",
                        Rate=1
                    },
                    Supplier = new SupplierViewModel
                    {
                        Import = true,
                        Id = It.IsAny<int>(),
                        PIC = "importTest",
                        Contact = "0987654",
                        Code = "SupplierImport",
                        Name = "SupplierImport"
                    },
                    Currency= new CurrencyViewModel
                    {
                        Id= It.IsAny<int>(),
                        Code ="TEST",
                        Rate=1,
                        Symbol="tst",
                        Description="CurrencyTest"
                    },
                    Items = new List<GarmentExternalPurchaseOrderItemViewModel>
                    {
                        new GarmentExternalPurchaseOrderItemViewModel
                        {
                            Product = new GarmentProductViewModel
                            {
                                Name = "product",
                                Code = "codeProd",
                                Id = It.IsAny<int>(),
                                Remark = "kjsh",
                                Composition = "aa",
                                Const = "aa",
                                ProductType = "FABRIC",
                                Width = "aak",
                                Yarn = "asn",
                                Tags="test",
                                UOM=new UomViewModel
                                {
                                    Id=It.IsAny<string>(),
                                    Unit="TEST",
                                }
                            },
                            ShipmentDate = It.IsAny<DateTimeOffset>(),
                            DealUom=new UomViewModel
                            {
                                Id=It.IsAny<string>(),
                                Unit="TEST",
                            }
                            
                        }
                    }
                };
            }
        }

        private GarmentExternalPurchaseOrderViewModel ViewModelAcc
        {
            get
            {
                return new GarmentExternalPurchaseOrderViewModel
                {
                    Category = "Accessories",
                    Supplier = new SupplierViewModel
                    {
                        Import = true,
                        Id = 1,
                        PIC = "importTest",
                        Contact = "0987654",
                        Code = "SupplierImport",
                        Name = "SupplierImport"
                    },
                    Currency = new CurrencyViewModel
                    {
                        Code = "TEST",
                        Rate = 1,
                        Symbol = "tst"
                    },
                    Items = new List<GarmentExternalPurchaseOrderItemViewModel>
                    {
                        new GarmentExternalPurchaseOrderItemViewModel
                        {
                            Product = new GarmentProductViewModel
                            {
                                Name = "product",
                                Code = "codeProd",
                                Id = 1,
                                Remark = "kjsh",
                                Composition = "aa",
                                Const = "aa",
                                ProductType = "FABRIC",
                                Width = "aak",
                                Yarn = "asn"
                            },
                            ShipmentDate = It.IsAny<DateTimeOffset>(),
                            DealUom=new UomViewModel
                            {
                                Unit="TEST",
                            }

                        }
                    }
                };
            }
        }

        private GarmentExternalPurchaseOrder Model
        {
            get
            {
                return new GarmentExternalPurchaseOrder { };
            }
        }

        private ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();

            ViewModel.EPONo = ViewModel.EPONo;
            ViewModel.OrderDate = ViewModel.OrderDate;
            ViewModel.DeliveryDate = ViewModel.DeliveryDate;
            foreach (var item in ViewModel.Items)
            {
                item.BudgetPrice = item.BudgetPrice;
                item.PRNo = item.PRNo;
            }
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

        private GarmentExternalPurchaseOrderController GetController(Mock<IGarmentExternalPurchaseOrderFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper, Mock<IGarmentInternalPurchaseOrderFacade> facadeIPO)
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

            var controller = new GarmentExternalPurchaseOrderController(servicePMock.Object, mapper.Object, facadeM.Object, facadeIPO.Object)
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
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOvalidateMock = new Mock<IValidateService>();
            IPOvalidateMock.Setup(s => s.Validate(It.IsAny<GarmentInternalPurchaseOrderViewModel>())).Verifiable();

            var IPOmockMapper = new Mock<IMapper>();
            IPOmockMapper.Setup(x => x.Map<List<GarmentInternalPurchaseOrder>>(It.IsAny<List<GarmentInternalPurchaseOrderViewModel>>()))
                .Returns(new List<GarmentInternalPurchaseOrder>());

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();
            IPOmockFacade.Setup(x => x.CreateMultiple(It.IsAny<List<GarmentInternalPurchaseOrder>>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data_Empty()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data_Fabric()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Throws(GetServiceValidationExeption());

            var Model = this.Model;
            Model.Category = "FABRIC";

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            GarmentExternalPurchaseOrderController controller = new GarmentExternalPurchaseOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data_By_User()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentExternalPurchaseOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentExternalPurchaseOrderViewModel>>(It.IsAny<List<GarmentExternalPurchaseOrder>>()))
                .Returns(new List<GarmentExternalPurchaseOrderViewModel> { ViewModel });

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data_By_User_With_Filter()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentExternalPurchaseOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentExternalPurchaseOrderViewModel>>(It.IsAny<List<GarmentExternalPurchaseOrder>>()))
                .Returns(new List<GarmentExternalPurchaseOrderViewModel> { ViewModel });


            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            var response = controller.GetByUser(filter: "{ 'IsPosted': false }");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentExternalPurchaseOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentExternalPurchaseOrderViewModel>>(It.IsAny<List<GarmentExternalPurchaseOrder>>()))
                .Returns(new List<GarmentExternalPurchaseOrderViewModel> { ViewModel });


            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            GarmentExternalPurchaseOrderController controller = new GarmentExternalPurchaseOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data_By_User()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentExternalPurchaseOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentExternalPurchaseOrderViewModel>>(It.IsAny<List<GarmentExternalPurchaseOrder>>()))
                .Returns(new List<GarmentExternalPurchaseOrderViewModel> { ViewModel });


            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            GarmentExternalPurchaseOrderController controller = new GarmentExternalPurchaseOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentExternalPurchaseOrder());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrderViewModel>(It.IsAny<GarmentExternalPurchaseOrder>()))
                .Returns(ViewModel);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentExternalPurchaseOrder());

            var mockMapper = new Mock<IMapper>();

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentExternalPurchaseOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentExternalPurchaseOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var controller = new GarmentExternalPurchaseOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentExternalPurchaseOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_PDF_Garment_EPO_Import_Fabric()
        {

            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentExternalPurchaseOrder());

            

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrderViewModel>(It.IsAny<GarmentExternalPurchaseOrder>()))
                .Returns(ViewModelFabric);

            

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.Get(It.IsAny<int>());
            Assert.NotEqual(null, response.GetType().GetProperty("FileStream"));
        }

        [Fact]
        public void Should_Success_Get_PDF_Garment_EPO_Local_Fabric()
        {

            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentExternalPurchaseOrder());

            var ViewModelFabric = this.ViewModelFabric;
            ViewModelFabric.Supplier.Import = false;

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrderViewModel>(It.IsAny<GarmentExternalPurchaseOrder>()))
                .Returns(ViewModelFabric);



            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.Get(It.IsAny<int>());
            Assert.NotEqual(null, response.GetType().GetProperty("FileStream"));
        }

        [Fact]
        public void Should_Success_Get_PDF_Garment_EPO_Import_Acc()
        {

            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentExternalPurchaseOrder());



            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrderViewModel>(It.IsAny<GarmentExternalPurchaseOrder>()))
                .Returns(ViewModelAcc);



            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.Get(It.IsAny<int>());
            Assert.NotEqual(null, response.GetType().GetProperty("FileStream"));
        }

        [Fact]
        public void Should_Success_Get_PDF_Garment_EPO_Local_Acc()
        {

            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentExternalPurchaseOrder());

            var ViewModelAcc = this.ViewModelAcc;
            ViewModelAcc.Supplier.Import = false;
            ViewModelAcc.IsUseVat = true;

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrderViewModel>(It.IsAny<GarmentExternalPurchaseOrder>()))
                .Returns(ViewModelAcc);



            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentExternalPurchaseOrderController controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.Get(It.IsAny<int>());
            Assert.NotEqual(null, response.GetType().GetProperty("FileStream"));
        }

        [Fact]
        public void Should_Error_Get_PDF_Garment_EPO_By_Id()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentExternalPurchaseOrderController controller = new GarmentExternalPurchaseOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };

            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/pdf";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";

            var response = controller.Get(It.IsAny<int>());
            Assert.Equal(null, response.GetType().GetProperty("FileStream"));
        }

        [Fact]
        public void Should_Success_Delete_Data()
        {
            var validateMock = new Mock<IValidateService>();
            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Delete(It.IsAny<int>(), "unittestusername"))
                .Returns(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();
            

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.Delete(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Delete_Data()
        {
            var validateMock = new Mock<IValidateService>();
            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Delete(It.IsAny<int>(), "unittestusername"))
                .Returns(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();
            var controller = new GarmentExternalPurchaseOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object, IPOmockFacade.Object);

            var response = controller.Delete(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_POST_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOvalidateMock = new Mock<IValidateService>();
            IPOvalidateMock.Setup(s => s.Validate(It.IsAny<GarmentInternalPurchaseOrderViewModel>())).Verifiable();

            var IPOmockMapper = new Mock<IMapper>();
            IPOmockMapper.Setup(x => x.Map<List<GarmentInternalPurchaseOrder>>(It.IsAny<List<GarmentInternalPurchaseOrderViewModel>>()))
                .Returns(new List<GarmentInternalPurchaseOrder>());

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();
            IPOmockFacade.Setup(x => x.CreateMultiple(It.IsAny<List<GarmentInternalPurchaseOrder>>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.EPOPost(new List<GarmentExternalPurchaseOrderViewModel> { ViewModel });
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_UNPOST_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentExternalPurchaseOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentExternalPurchaseOrder>(It.IsAny<GarmentExternalPurchaseOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentExternalPurchaseOrderFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<GarmentExternalPurchaseOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var IPOmockFacade = new Mock<IGarmentInternalPurchaseOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper, IPOmockFacade);

            var response = controller.EPOUnpost(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }
    }
}
