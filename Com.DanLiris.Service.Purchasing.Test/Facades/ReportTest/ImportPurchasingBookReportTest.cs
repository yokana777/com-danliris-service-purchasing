using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ReportTest
{
    //[Collection("ServiceProviderFixture Collection")]
    public class ImportPurchasingBookReportTest
    {
        private const string ENTITY = "ImportPurchasingReport";

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

        private UnitPaymentOrderDataUtil _dataUtil(UnitPaymentOrderFacade facade, PurchasingDbContext dbContext)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            serviceProvider
                .Setup(x => x.GetService(typeof(InternalPurchaseOrderFacade)))
                .Returns(new InternalPurchaseOrderFacade(serviceProvider.Object, _dbContext(GetCurrentMethod())));

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

            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(serviceProvider.Object, dbContext);
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(serviceProvider.Object, dbContext);
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(serviceProvider.Object, dbContext);
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(dbContext, serviceProvider.Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);

            UnitReceiptNoteFacade unitReceiptNoteFacade = new UnitReceiptNoteFacade(serviceProvider.Object, dbContext);
            UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil = new UnitReceiptNoteItemDataUtil();
            UnitReceiptNoteDataUtil unitReceiptNoteDataUtil = new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, unitReceiptNoteFacade, deliveryOrderDataUtil);

            return new UnitPaymentOrderDataUtil(unitReceiptNoteDataUtil, facade);
        }

        private Mock<IServiceProvider> _getServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            var mockCurrencyProvider = new Mock<ICurrencyProvider>();
            mockCurrencyProvider
                .Setup(x => x.GetCurrencyByCurrencyCodeList(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Currency>());
            serviceProvider
                .Setup(x => x.GetService(typeof(ICurrencyProvider)))
                .Returns(mockCurrencyProvider.Object);

            return serviceProvider;
        }

        [Fact]
        public async Task Should_Success_Get_Data()
        {
            var dbContext = _dbContext("testImport");
            var serviceProvider = _getServiceProvider().Object;

            var unitPaymentOrderFacade = new UnitPaymentOrderFacade(dbContext);
            var dataUtil = await _dataUtil(unitPaymentOrderFacade, dbContext).GetTestImportData();

            var urnId = dataUtil.Items.FirstOrDefault().URNId;
            var urn = dbContext.UnitReceiptNotes.FirstOrDefault(f => f.Id.Equals(urnId));
            var prId = urn.Items.FirstOrDefault(f => f.URNId.Equals(urn.Id)).PRId;
            var pr = dbContext.PurchaseRequests.FirstOrDefault(f => f.Id.Equals(prId));

            var facade = new ImportPurchasingBookReportFacade(serviceProvider, dbContext);

            var result = await facade.GetReport(urn.URNNo, urn.UnitCode, pr.CategoryCode, DateTime.Now.AddDays(-7), DateTime.Now.AddDays(7));
            Assert.NotEqual(result.Reports.Count, 0);
        }

        [Fact]
        public async Task Should_Success_Get_Data_Empty()
        {
            var dbContext = _dbContext("testImport");
            var serviceProvider = _getServiceProvider().Object;

            var unitPaymentOrderFacade = new UnitPaymentOrderFacade(dbContext);
            var dataUtil = await _dataUtil(unitPaymentOrderFacade, dbContext).GetTestImportData();

            var urnId = dataUtil.Items.FirstOrDefault().URNId;
            var urn = dbContext.UnitReceiptNotes.FirstOrDefault(f => f.Id.Equals(urnId));
            var prId = urn.Items.FirstOrDefault(f => f.URNId.Equals(urn.Id)).PRId;
            var pr = dbContext.PurchaseRequests.FirstOrDefault(f => f.Id.Equals(prId));

            var facade = new ImportPurchasingBookReportFacade(serviceProvider, dbContext);

            var result = await facade.GetReport("Invalid URNNo", urn.UnitCode, pr.CategoryCode, DateTime.Now.AddDays(-7), DateTime.Now.AddDays(7));
            Assert.Equal(result.Reports.Count, 0);
        }

        [Fact]
        public async Task Should_Success_GenerateExcel_Data_Empty()
        {
            var dbContext = _dbContext("testImport");
            var serviceProvider = _getServiceProvider().Object;

            var unitPaymentOrderFacade = new UnitPaymentOrderFacade(dbContext);
            var dataUtil = await _dataUtil(unitPaymentOrderFacade, dbContext).GetTestImportData();

            var urnId = dataUtil.Items.FirstOrDefault().URNId;
            var urn = dbContext.UnitReceiptNotes.FirstOrDefault(f => f.Id.Equals(urnId));
            var prId = urn.Items.FirstOrDefault(f => f.URNId.Equals(urn.Id)).PRId;
            var pr = dbContext.PurchaseRequests.FirstOrDefault(f => f.Id.Equals(prId));

            var facade = new ImportPurchasingBookReportFacade(serviceProvider, dbContext);

            var result = await facade.GenerateExcel(urn.URNNo, urn.UnitCode, pr.CategoryCode, DateTime.Now.AddDays(-7), DateTime.Now.AddDays(7));
            Assert.NotNull(result);
        }
    }
}
