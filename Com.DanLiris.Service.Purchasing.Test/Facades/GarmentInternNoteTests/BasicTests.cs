using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
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
            message.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018 - 10 - 20T00: 00:00 + 00:00\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");
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

            var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

            var garmentInvoiceFacade = new GarmentInvoiceFacade(_dbContext(testName), ServiceProvider);
            var garmentInvoiceDetailDataUtil = new GarmentInvoiceDetailDataUtil();
            var garmentInvoiceItemDataUtil = new GarmentInvoiceItemDataUtil(garmentInvoiceDetailDataUtil);
            var garmentInvoieDataUtil = new GarmentInvoiceDataUtil(garmentInvoiceItemDataUtil,garmentInvoiceDetailDataUtil, garmentDeliveryOrderDataUtil, garmentInvoiceFacade);

            return new GarmentInternNoteDataUtil(garmentInvoieDataUtil, facade );
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {

            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.Create(model, false, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(null,false, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            var Response = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Update_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            Exception errorInvalidId = await Assert.ThrowsAsync<Exception>(async () => await facade.Update(0, model, USERNAME));
            Assert.NotNull(errorInvalidId.Message);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Delete_Data()
        {
            var facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));

            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Delete(0, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Get_All_Data()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            GarmentInternNoteFacades facade = new GarmentInternNoteFacades(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }
        [Fact]
        public void Should_Success_Validate_Data()
        {
            GarmentInternNoteViewModel nullViewModel = new GarmentInternNoteViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentInternNoteViewModel viewModel = new GarmentInternNoteViewModel()
            {
                currency = new CurrencyViewModel
                {
                    Id = 1
                },
                supplier = new SupplierViewModel
                {
                    Id = 1
                },
                items = new List<GarmentInternNoteItemViewModel>
                {
                    new GarmentInternNoteItemViewModel(),
                    new GarmentInternNoteItemViewModel()
                    {
                        garmentInvoice = new GarmentInvoiceViewModel
                        {
                            _id = 1
                        },
                        details = new List<GarmentInternNoteDetailViewModel>
                        {
                            new GarmentInternNoteDetailViewModel(),
                            new GarmentInternNoteDetailViewModel()
                            {
                                deliveryOrder = new Lib.ViewModels.GarmentDeliveryOrderViewModel.GarmentDeliveryOrderViewModel
                                {
                                    Id = 1
                                }
                            },
                        },

                    }
                }
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }
    }
}
