using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentCorrectionNoteDataUtils;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.InternalPurchaseOrderTests
{
    //[Collection("ServiceProviderFixture Collection")]
    public class POMonitoringAllTest
    {
        //private IServiceProvider ServiceProvider { get; set; }

        //public POMonitoringAllTest(ServiceProviderFixture fixture)
        //{
        //    ServiceProvider = fixture.ServiceProvider;

        //    IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
        //    identityService.Username = "Unit Test";
        //}

        //private InternalPurchaseOrderDataUtil DataUtil
        //{
        //    get { return (InternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(InternalPurchaseOrderDataUtil)); }
        //}
        //private PurchaseRequestDataUtil DataUtilPR
        //{
        //    get { return (PurchaseRequestDataUtil)ServiceProvider.GetService(typeof(PurchaseRequestDataUtil)); }
        //}
        //private ExternalPurchaseOrderDataUtil DataUtilEPO
        //{
        //    get { return (ExternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
        //}
        //private DeliveryOrderDataUtil DataUtilDO
        //{
        //    get { return (DeliveryOrderDataUtil)ServiceProvider.GetService(typeof(DeliveryOrderDataUtil)); }
        //}
        //private UnitReceiptNoteDataUtil DataUtilURN
        //{
        //    get { return (UnitReceiptNoteDataUtil)ServiceProvider.GetService(typeof(UnitReceiptNoteDataUtil)); }
        //}
        //private UnitPaymentOrderDataUtil DataUtilUPO
        //{
        //    get { return (UnitPaymentOrderDataUtil)ServiceProvider.GetService(typeof(UnitPaymentOrderDataUtil)); }
        //}
        //private UnitPaymentCorrectionNoteDataUtil DataUtilCorr
        //{
        //    get { return (UnitPaymentCorrectionNoteDataUtil)ServiceProvider.GetService(typeof(UnitPaymentCorrectionNoteDataUtil)); }
        //}

        private const string ENTITY = "UnitPaymentPriceCorrectionNote";

        private const string USERNAME = "Unit Test";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private UnitPaymentOrderDataUtil _dataUtil2(UnitPaymentOrderFacade facade, string testName)
        {

            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(GetServiceProvider().Object, _dbContext(testName));
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(_dbContext(testName), GetServiceProvider().Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);

            UnitReceiptNoteFacade unitReceiptNoteFacade = new UnitReceiptNoteFacade(GetServiceProvider().Object, _dbContext(testName));
            UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil = new UnitReceiptNoteItemDataUtil();
            UnitReceiptNoteDataUtil unitReceiptNoteDataUtil = new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, unitReceiptNoteFacade, deliveryOrderDataUtil);

            return new UnitPaymentOrderDataUtil(unitReceiptNoteDataUtil, facade);
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

            serviceProvider
                .Setup(x => x.GetService(typeof(InternalPurchaseOrderFacade)))
                .Returns(new InternalPurchaseOrderFacade(serviceProvider.Object, _dbContext(GetCurrentMethod())));
            return serviceProvider;
        }

        private UnitPaymentPriceCorrectionNoteDataUtils _dataUtil(UnitPaymentPriceCorrectionNoteFacade facade, string testName)
        {

            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(GetServiceProvider().Object, _dbContext(testName));
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(_dbContext(testName), GetServiceProvider().Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);

            UnitReceiptNoteFacade unitReceiptNoteFacade = new UnitReceiptNoteFacade(GetServiceProvider().Object, _dbContext(testName));
            UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil = new UnitReceiptNoteItemDataUtil();
            UnitReceiptNoteDataUtil unitReceiptNoteDataUtil = new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, unitReceiptNoteFacade, deliveryOrderDataUtil);

            UnitPaymentOrderFacade unitPaymentOrderFacade = new UnitPaymentOrderFacade(_dbContext(testName));
            UnitPaymentOrderDataUtil unitPaymentOrderDataUtil = new UnitPaymentOrderDataUtil(unitReceiptNoteDataUtil, unitPaymentOrderFacade);

            return new UnitPaymentPriceCorrectionNoteDataUtils(unitPaymentOrderDataUtil, facade);
        }

        private DeliveryOrderDataUtil _dataUtilDO(DeliveryOrderFacade facade, string testName)
        {

            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(GetServiceProvider().Object, _dbContext(testName));
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            
            return new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, facade);
        }

        private InternalPurchaseOrderDataUtil _dataUtilIPO(InternalPurchaseOrderFacade facade, string testName)
        {

            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(GetServiceProvider().Object, _dbContext(testName));
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);


            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();

            return new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, facade, purchaseRequestDataUtil);
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

        //private PurchaseOrderMonitoringAllFacade Facade
        //{
        //    get { return (PurchaseOrderMonitoringAllFacade)ServiceProvider.GetService(typeof(PurchaseOrderMonitoringAllFacade)); }
        //}

        [Fact]
        public async Task Should_Success_Get_Report_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrderFacade ipoFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrder model = await _dataUtilIPO(ipoFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GetReport(model.PRNo, null, model.DivisionCode, model.UnitId, model.CategoryId, null, null, model.CreatedBy, null, null, null, 1, 25, "{}", 7, "");
            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data2()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade monitoringFacade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var modelLocalSupplier = await _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, false, USERNAME, 7);
            var prNo = "";
            foreach (var item in modelLocalSupplier.Items)
            {
                prNo = item.PRNo;
            }

            var Response = monitoringFacade.GetReport(prNo, null, null, null, null, null, null, modelLocalSupplier.CreatedBy, null, null, null, 1, 25, "{}", 7, "");
            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_All_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade monitoringFacade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);

            UnitPaymentPriceCorrectionNoteFacade facade = new UnitPaymentPriceCorrectionNoteFacade(GetServiceProvider().Object, dbContext);
            var modelLocalSupplier = await _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, false, USERNAME, 7);
            var today = DateTime.Now;
            var tomorrow = today.AddDays(1);
            var Response = monitoringFacade.GetReport(null, null, null, null, null, null, null, null, null, null, tomorrow.ToShortDateString(), 1, 25, "{}", 7, "");

            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrderFacade ipoFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrder model = await _dataUtilIPO(ipoFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GetReport(null, null, null, null, null, null, null, null, null, null, null, 1, 25, "{}", 7, "");
            Assert.NotEqual(Response.Item2, -1);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrderFacade ipoFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrder model = await _dataUtilIPO(ipoFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GenerateExcel(model.PRNo, null, model.DivisionCode, model.UnitId, model.CategoryId, null, null, model.CreatedBy, null, null, null, 7, "");
            Assert.IsType<System.IO.MemoryStream>(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrderFacade ipoFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, dbContext);
            InternalPurchaseOrder model = await _dataUtilIPO(ipoFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GenerateExcel("", "0", null, null, null, null, null, null, null, null, null, 7, "");
            Assert.IsType<System.IO.MemoryStream>(Response);
        }


        #region Staff Sarmut 
        [Fact]
        public async void Should_Success_Get_Report_Data_Staffs()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            DeliveryOrderFacade doFacade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtilDO(doFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GetReportStaff(DateTime.MinValue, DateTime.MaxValue, null, 7);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Staffs_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            DeliveryOrderFacade doFacade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtilDO(doFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GetReportStaff(null, null, null, 0);
            Assert.NotNull(Response);
        }


        [Fact]
        public async void Should_Success_Get_Report_Data_SubStaffs()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            DeliveryOrderFacade doFacade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtilDO(doFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GetReportsubStaff(DateTime.MinValue, DateTime.MaxValue, null, model.CreatedBy, 7);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_SubStaffs_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            DeliveryOrderFacade doFacade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtilDO(doFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GetReportsubStaff(null, null, null, null, 0);
            Assert.NotNull(Response);
        }


        [Fact]
        public async void Should_Success_Get_Report_Data_Excel_subStaffs()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            DeliveryOrderFacade doFacade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtilDO(doFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GenerateExcelSarmut(DateTime.MinValue, DateTime.MaxValue, null, model.CreatedBy, 7);
            Assert.IsType<System.IO.MemoryStream>(Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel_subStaffs_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            PurchaseOrderMonitoringAllFacade facade = new PurchaseOrderMonitoringAllFacade(GetServiceProvider().Object, dbContext);
            DeliveryOrderFacade doFacade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtilDO(doFacade, GetCurrentMethod()).GetTestData("Unit test");
            var Response = facade.GenerateExcelSarmut(null, null, null, null, 0);
            Assert.IsType<System.IO.MemoryStream>(Response);
        }
        #endregion
    }
}
