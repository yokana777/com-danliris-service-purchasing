using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentInternNoteTests
{
    public class BasicTests
    {
        private const string ENTITY = "GarmentInternNote";

        private const string USERNAME = "Unit Test";
        private IServiceProvider ServiceProvider { get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }
        private PurchasingDbContext _dbContext(string testName)
        {
            DbContextOptionsBuilder<PurchasingDbContext> optionsBuilder = new DbContextOptionsBuilder<PurchasingDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            PurchasingDbContext dbContext = new PurchasingDbContext(optionsBuilder.Options);

            return dbContext;
        }
        private Mock<IServiceProvider> GetServiceProvider()
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            message.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");
            var HttpClientService = new Mock<IHttpClientService>();
            HttpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(message);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(HttpClientService.Object);

            return serviceProvider;
        }

        private GarmentInternNoteDataUtil dataUtil(GarmentInternNoteFacades facade, string testName)
        {

            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

            var garmentInvoiceFacade = new GarmentInvoiceFacade(_dbContext(testName), ServiceProvider);
            var garmentInvoiceDetailDataUtil = new GarmentInvoiceDetailDataUtil();
            var garmentInvoiceItemDataUtil = new GarmentInvoiceItemDataUtil(garmentInvoiceDetailDataUtil);
            var garmentInvoieDataUtil = new GarmentInvoiceDataUtil(garmentInvoiceItemDataUtil,garmentInvoiceDetailDataUtil, garmentDeliveryOrderDataUtil, garmentInvoiceFacade);

            return new GarmentInternNoteDataUtil(garmentInvoieDataUtil, facade );
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.Create(model, false, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async Task Should_Error_Create_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(null,false, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async Task Should_Success_Update_Data()
        {
            var facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), ServiceProvider);
            var facadeDO = new GarmentDeliveryOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            GarmentInternNote data = dataUtil(facade, GetCurrentMethod()).GetNewData();
            GarmentInternNoteItem item = await dataUtil(facade, GetCurrentMethod()).GetNewDataItem(USERNAME);

            var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
            Assert.NotEqual(ResponseUpdate, 0);

            List<GarmentInternNoteItem> Newitems = new List<GarmentInternNoteItem>(data.Items);
            Newitems.Add(item);
            data.Items = Newitems;

            var ResponseUpdate1 = await facade.Update((int)data.Id, data, USERNAME);
            Assert.NotEqual(ResponseUpdate1, 0);
        }

        [Fact]
        public async Task Should_Success_Update_Data2()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            var facade = new GarmentInternNoteFacades(dbContext, ServiceProvider);
            var facadeDO = new GarmentDeliveryOrderFacade(ServiceProvider, dbContext);
            GarmentInternNote data = dataUtil(facade, GetCurrentMethod()).GetNewData();
            GarmentInternNoteItem item = await dataUtil(facade, GetCurrentMethod()).GetNewDataItem(USERNAME);

            var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
            Assert.NotEqual(ResponseUpdate, 0);

            List<GarmentInternNoteItem> Newitems = new List<GarmentInternNoteItem>(data.Items);
            Newitems.Add(item);
            data.Items = Newitems;

            var ResponseUpdate1 = await facade.Update((int)data.Id, data, USERNAME);
            Assert.NotEqual(ResponseUpdate, 0);

            dbContext.Entry(data).State = EntityState.Detached;
            foreach (var items in data.Items)
            {
                dbContext.Entry(items).State = EntityState.Detached;
                foreach (var detail in items.Details)
                {
                    dbContext.Entry(detail).State = EntityState.Detached;
                }
            }

            var newData = dbContext.GarmentInternNotes.AsNoTracking()
                .Include(m => m.Items)
                    .ThenInclude(i => i.Details)
                .FirstOrDefault(m => m.Id == data.Id);

            newData.Items = newData.Items.Take(1).ToList();

            var ResponseUpdate2 = await facade.Update((int)newData.Id, newData, USERNAME);
            Assert.NotEqual(ResponseUpdate2, 0);
        }
        [Fact]
        public async Task Should_Error_Update_Data()
        {
            var facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), ServiceProvider);
            GarmentInternNote data = dataUtil(facade, GetCurrentMethod()).GetNewData();
            List<GarmentInternNoteItem> item = new List<GarmentInternNoteItem>(data.Items);

            data.Items.Add(new GarmentInternNoteItem
            {
                InvoiceId = It.IsAny<int>(),
                InvoiceDate = DateTimeOffset.Now,
                InvoiceNo = "donos",
                TotalAmount = 2000,
                Details = null
            });

            var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
            Assert.NotEqual(ResponseUpdate, 0);
            var newItem = new GarmentInternNoteItem
            {
                InvoiceId = It.IsAny<int>(),
                InvoiceDate = DateTimeOffset.Now,
                InvoiceNo = "dono",
                TotalAmount = 2000,
                Details = null
            };
            List<GarmentInternNoteItem> Newitems = new List<GarmentInternNoteItem>(data.Items);
            Newitems.Add(newItem);
            data.Items = Newitems;

            Exception errorNullItems = await Assert.ThrowsAsync<Exception>(async () => await facade.Update((int)data.Id, data, USERNAME));
            Assert.NotNull(errorNullItems.Message);
        }

        [Fact]
        public async Task Should_Success_Delete_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async Task Should_Error_Delete_Data()
        {
            var facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));

            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Delete(0, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async Task Should_Success_Get_All_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }
        [Fact]
        public void Should_Success_Validate_Data()
        {
            var viewModelNullItems = new GarmentInternNoteViewModel
            {
                items = null,
            };
            Assert.True(viewModelNullItems.Validate(null).Count() > 0);

            Mock<IGarmentInvoice> garmentInvoiceFacadeMock = new Mock<IGarmentInvoice>();
            garmentInvoiceFacadeMock.Setup(s => s.ReadById(1))
                .Returns(new Lib.Models.GarmentInvoiceModel.GarmentInvoice { UseIncomeTax = false, UseVat = false,IncomeTaxId = 1, Items = new List<GarmentInvoiceItem>{
                    new GarmentInvoiceItem
                    {
                        InvoiceId = 1,
                        PaymentMethod = "PaymentMethod1"
                    }
                }
                });
            garmentInvoiceFacadeMock.Setup(s => s.ReadById(2))
                .Returns(new Lib.Models.GarmentInvoiceModel.GarmentInvoice
                {
                    UseIncomeTax = true,
                    UseVat = true,
                    IncomeTaxId = 2,
                    Items = new List<GarmentInvoiceItem>{
                    new GarmentInvoiceItem
                    {
                        InvoiceId = 2,
                        PaymentMethod = "PaymentMethod2"
                    }
                }
                });

            Mock<IGarmentDeliveryOrderFacade> garmentDeliveryOrderFacadeMock = new Mock<IGarmentDeliveryOrderFacade>();
            garmentDeliveryOrderFacadeMock.Setup(s => s.ReadById(It.IsAny<int>()))
                .Returns(new Lib.Models.GarmentDeliveryOrderModel.GarmentDeliveryOrder ());

            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.
                Setup(x => x.GetService(typeof(IGarmentInvoice)))
                .Returns(garmentInvoiceFacadeMock.Object);
            serviceProvider.
               Setup(x => x.GetService(typeof(IGarmentDeliveryOrderFacade)))
               .Returns(garmentDeliveryOrderFacadeMock.Object);

            var sameUseVat = new GarmentInternNoteViewModel
            {
                items = new List<GarmentInternNoteItemViewModel>
                {
                    new GarmentInternNoteItemViewModel
                    {
                        garmentInvoice  = new GarmentInvoiceViewModel
                        {
                            Id = 1
                        },
                        details = new List<GarmentInternNoteDetailViewModel>
                        {
                            new GarmentInternNoteDetailViewModel
                            {
                                deliveryOrder = new Lib.ViewModels.GarmentDeliveryOrderViewModel.GarmentDeliveryOrderViewModel
                                {
                                    Id = 1
                                }
                            }
                        }
                    },                
                    new GarmentInternNoteItemViewModel
                    {
                        garmentInvoice  = new GarmentInvoiceViewModel
                        {
                            Id = 2
                        },
                        details = new List<GarmentInternNoteDetailViewModel>
                        {
                            new GarmentInternNoteDetailViewModel
                            {
                                 deliveryOrder = new Lib.ViewModels.GarmentDeliveryOrderViewModel.GarmentDeliveryOrderViewModel
                                {
                                    Id = 2
                                }
                            }
                        }
                    },
                    
                }
            };

            ValidationContext Usevats = new ValidationContext(sameUseVat, serviceProvider.Object, null);
            Assert.True(sameUseVat.Validate(Usevats).Count() > 0);

            var viewModelNullDetail = new GarmentInternNoteViewModel
            {
                items = new List<GarmentInternNoteItemViewModel>
                {
                    new GarmentInternNoteItemViewModel
                    {
                        garmentInvoice = null,
                        details = null
                    }
                }
            };
            Assert.True(viewModelNullDetail.Validate(null).Count() > 0);
        }
    }
}
