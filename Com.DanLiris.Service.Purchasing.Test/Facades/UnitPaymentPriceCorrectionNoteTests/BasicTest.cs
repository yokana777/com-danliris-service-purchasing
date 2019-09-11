using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentCorrectionNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.UnitPaymentPriceCorrectionNoteTests
{
    public class BasicTest
    {
        private const string ENTITY = "UnitPaymentPriceCorrectionNote";

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

        private UnitPaymentPriceCorrectionNoteDataUtils _dataUtil(UnitPaymentPriceCorrectionNoteFacade facade, string testName)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProviders = services.BuildServiceProvider();
            var memoryCache = serviceProviders.GetService<IMemoryCache>();

            serviceProvider
                .Setup(x => x.GetService(typeof(IMemoryCacheManager)))
                .Returns(new MemoryCacheManager(memoryCache));

            var mockCurrencyProvider = new Mock<ICurrencyProvider>();
            mockCurrencyProvider
                .Setup(x => x.GetCurrencyByCurrencyCode(It.IsAny<string>()))
                .ReturnsAsync((Currency)null);
            serviceProvider
                .Setup(x => x.GetService(typeof(ICurrencyProvider)))
                .Returns(mockCurrencyProvider.Object);

            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(serviceProvider.Object, _dbContext(testName));
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(serviceProvider.Object, _dbContext(testName));
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(serviceProvider.Object, _dbContext(testName));
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(_dbContext(testName), serviceProvider.Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);

            UnitReceiptNoteFacade unitReceiptNoteFacade = new UnitReceiptNoteFacade(serviceProvider.Object, _dbContext(testName));
            UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil = new UnitReceiptNoteItemDataUtil();
            UnitReceiptNoteDataUtil unitReceiptNoteDataUtil = new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, unitReceiptNoteFacade, deliveryOrderDataUtil);

            UnitPaymentOrderFacade unitPaymentOrderFacade = new UnitPaymentOrderFacade(_dbContext(testName));
            UnitPaymentOrderDataUtil unitPaymentOrderDataUtil = new UnitPaymentOrderDataUtil(unitReceiptNoteDataUtil, unitPaymentOrderFacade);

            return new UnitPaymentPriceCorrectionNoteDataUtils(unitPaymentOrderDataUtil, facade);
        }

        [Fact]
        public async Task Should_Success_Get_Data()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var modelLocalSupplier = await _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, false, USERNAME, 7);
            Assert.NotEqual(ResponseLocalSupplier, 0);

            var modelImportSupplier = await _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseImportSupplier = await facade.Create(modelImportSupplier,true, USERNAME, 7);
            Assert.NotEqual(ResponseImportSupplier, 0);
        }

        [Fact]
        public async Task Should_Error_Create_Data_Null_Parameter()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            
            Exception exception = await Assert.ThrowsAsync<Exception>(() => facade.Create(null, true, USERNAME, 7));
            Assert.Equal(exception.Message, "Object reference not set to an instance of an object.");
        }

        [Fact]
        public async Task Should_Success_Create_Data_garment()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var modelLocalSupplier = await _dataUtil(facade, GetCurrentMethod()).GetNewData();
            modelLocalSupplier.DivisionName = "GARMENT";
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, false, USERNAME, 7);
            Assert.NotEqual(ResponseLocalSupplier, 0);
        }

        [Fact]
        public async Task Should_Error_Get_Data_Supplier()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var modelLocalSupplier = await _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, false, USERNAME, 7);
            var Response = facade.GetSupplier(modelLocalSupplier.SupplierId);
            Assert.Null(Response);
        }

        [Fact]
        public async Task Should_Error_Get_Data_URN()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var modelLocalSupplier = await _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, false, USERNAME, 7);
            //var id = 0;
            var items=modelLocalSupplier.Items.ToList();
            var Response = facade.GetUrn(items[0].URNNo);
            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.GetReport(null, null, 1, 25, "{}", 7);
            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.GenerateExcel(null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();

            var Response = facade.GenerateDataExcel(null, null, 7);

            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel_Not_Found()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();

            var Response = facade.GenerateDataExcel(DateTime.MinValue, DateTime.MinValue, 7);

            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }
    }
}
