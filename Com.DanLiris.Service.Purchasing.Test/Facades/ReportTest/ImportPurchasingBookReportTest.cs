using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ReportTest
{
    //[Collection("ServiceProviderFixture Collection")]
    public class ImportPurchasingBookReportTest
    {
        //private IServiceProvider ServiceProvider { get; set; }

        private const string ENTITY = "UnitPaymentPriceCorrectionNote";

        private const string USERNAME = "Unit Test";

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
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return serviceProvider;
        }

        private Mock<IServiceProvider> _ServiceProvider => GetServiceProvider();

        private UnitReceiptNoteDataUtil _dataUtil(UnitReceiptNoteFacade facade, PurchasingDbContext dbContext)
        {
            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(_ServiceProvider.Object, dbContext);
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(_ServiceProvider.Object, dbContext);
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(_ServiceProvider.Object, dbContext);
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(dbContext, _ServiceProvider.Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);

            UnitReceiptNoteFacade unitReceiptNoteFacade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil = new UnitReceiptNoteItemDataUtil();
            UnitReceiptNoteDataUtil unitReceiptNoteDataUtil = new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, unitReceiptNoteFacade, deliveryOrderDataUtil);

            UnitPaymentOrderFacade unitPaymentOrderFacade = new UnitPaymentOrderFacade(dbContext);
            UnitPaymentOrderDataUtil unitPaymentOrderDataUtil = new UnitPaymentOrderDataUtil(unitReceiptNoteDataUtil, unitPaymentOrderFacade);

            return new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, facade, deliveryOrderDataUtil);
        }

        //      public ImportPurchasingBookReportTest(ServiceProviderFixture fixture)
        //      {
        //          ServiceProvider = fixture.ServiceProvider;
        //      }

        //      private UnitReceiptNoteDataUtil DataUtil
        //      {
        //          get { return (UnitReceiptNoteDataUtil)ServiceProvider.GetService(typeof(UnitReceiptNoteDataUtil)); }
        //      }
        //private ImportPurchasingBookReportFacade IPRFacade
        //{
        //	get { return (ImportPurchasingBookReportFacade)ServiceProvider.GetService(typeof(ImportPurchasingBookReportFacade)); }
        //}
        //private UnitReceiptNoteFacade Facade
        //      {
        //          get { return (UnitReceiptNoteFacade)ServiceProvider.GetService(typeof(UnitReceiptNoteFacade)); }
        //      }
        //private ExternalPurchaseOrderDataUtil EPODataUtil
        //{
        //	get { return (ExternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
        //}
        //private ExternalPurchaseOrderFacade EPOFacade
        //{
        //	get { return (ExternalPurchaseOrderFacade)ServiceProvider.GetService(typeof(ExternalPurchaseOrderFacade)); }
        //}
        //private DeliveryOrderDataUtil DODataUtil
        //{
        //	get { return (DeliveryOrderDataUtil)ServiceProvider.GetService(typeof(DeliveryOrderDataUtil)); }
        //}

        //private DeliveryOrderFacade DOFacade
        //{
        //	get { return (DeliveryOrderFacade)ServiceProvider.GetService(typeof(DeliveryOrderFacade)); }
        //}

        //private UnitPaymentOrderDataUtil UPODataUtil
        //{
        //    get { return (UnitPaymentOrderDataUtil)ServiceProvider.GetService(typeof(UnitPaymentOrderDataUtil)); }
        //}

        //private UnitPaymentOrderFacade UPOFacade
        //{
        //    get { return (UnitPaymentOrderFacade)ServiceProvider.GetService(typeof(UnitPaymentOrderFacade)); }
        //}

        [Fact]
        //public async Task Should_Success_Get_Report_Data()
        //{
        //	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
        //	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
        //	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
        //	await DOFacade.Create(deliveryOrder, "unit-test");
        //	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
        //	await Facade.Create(urn, "unit-test");
        //          //UnitPaymentOrder upo = await UPODataUtil.GetTestData();
        //          //await UPOFacade.Create(upo, "unit-test", false, 7);
        //          var DateFrom = DateTime.Now;
        //          DateFrom = DateFrom.Date;
        //          var DateTo = DateTime.Now;
        //          DateTo = DateTo.Date;
        //          var Response = IPRFacade.GetReport(null, null,null ,DateFrom,DateTo);
        //	Assert.NotEqual(Response.Item2, 0);
        //}
        public void Should_Success_Get_Report_Data()
        {
            //ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetTestData("Unit test");
            //DeliveryOrder deliveryOrder = await DODataUtil.GetTestData("unit-test");
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            //UnitReceiptNote urn = await _dataUtil.GetTestData2("unit-test");
            var DateFrom = DateTime.Now.AddDays(-1);
            DateFrom = DateFrom.Date;
            var DateTo = DateTime.Now.AddDays(1);
            DateTo = DateTo.Date;
            ImportPurchasingBookReportFacade response = new ImportPurchasingBookReportFacade(_ServiceProvider.Object, dbContext);
            var Response = response.GetReport(null, null, null, DateFrom, DateTo);
            Assert.Equal(Response.Item2, 0);
        }
        //[Fact]
        //public async Task Should_Success_Get_Report_Data_No_Parameter()
        //{
        //	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
        //	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
        //	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
        //	await DOFacade.Create(deliveryOrder, "unit-test");
        //	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
        //	await Facade.Create(urn, "unit-test");
        //	DateTime DateFrom = new DateTime(2018, 8, 27);
        //	DateTime DateTo = new DateTime(2018, 8, 27);
        //	var Response = IPRFacade.GetReport("18-08-BPI-001-unitcode-001", null, null, DateFrom, DateTo);
        //	Assert.NotEqual(Response.Item2, 0);
        //}
        //[Fact]
        //public async Task Should_Success_Get_Report_Data_Unit_Parameter()
        //{
        //	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
        //	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
        //	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
        //	await DOFacade.Create(deliveryOrder, "unit-test");
        //	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
        //	await Facade.Create(urn, "unit-test");
        //	DateTime DateFrom = new DateTime(2018, 8, 27);
        //	DateTime DateTo = new DateTime(2018, 8, 27);
        //	var Response = IPRFacade.GetReport(null, "UnitName", null, DateFrom, DateTo);
        //	Assert.NotEqual(Response.Item2, 0);
        //}
        //[Fact]
        //public async Task Should_Success_Get_Report_Data_Category_Parameter()
        //{
        //	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
        //	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
        //	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
        //	await DOFacade.Create(deliveryOrder, "unit-test");
        //	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
        //	await Facade.Create(urn, "unit-test");
        //	DateTime DateFrom = new DateTime(2018, 8, 27);
        //	DateTime DateTo = new DateTime(2018, 8, 27);
        //	var Response = IPRFacade.GetReport(null, null, "CategoryName", DateFrom, DateTo);
        //	Assert.NotEqual(Response.Item2, 0);
        //}
        //    if (externalPurchaseOrder != null && deliveryOrder != null && urn != null)
        //    {
        //        var Response = IPRFacade.GetReport(null, null, null, DateFrom, DateTo);
        //        Assert.NotEqual(Response.Item2, 0);
        //    }
        //}
        //[Fact]
        //public async Task Should_Success_Get_Report_Data_No_Parameter()
        //{
        //	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
        //	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
        //	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
        //	await DOFacade.Create(deliveryOrder, "unit-test");
        //	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
        //	await Facade.Create(urn, "unit-test");
        //	DateTime DateFrom = new DateTime(2018, 8, 27);
        //	DateTime DateTo = new DateTime(2018, 8, 27);
        //	var Response = IPRFacade.GetReport("18-08-BPI-001-unitcode-001", null, null, DateFrom, DateTo);
        //	Assert.NotEqual(Response.Item2, 0);
        //
        //public async Task Should_Success_Get_Report_Data_Unit_Parameter()
        //{
        //	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
        //	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
        //	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
        //	await DOFacade.Create(deliveryOrder, "unit-test");
        //	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
        //	await Facade.Create(urn, "unit-test");
        //	DateTime DateFrom = new DateTime(2018, 8, 27);
        //	DateTime DateTo = new DateTime(2018, 8, 27);
        //	var Response = IPRFacade.GetReport(null, "UnitName", null, DateFrom, DateTo);
        //	Assert.NotEqual(Response.Item2, 0);
        //}
        //[Fact]
        //public async Task Should_Success_Get_Report_Data_Category_Parameter()
        //{
        //	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
        //	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
        //	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
        //	await DOFacade.Create(deliveryOrder, "unit-test");
        //	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
        //	await Facade.Create(urn, "unit-test");
        //	DateTime DateFrom = new DateTime(2018, 8, 27);
        //	DateTime DateTo = new DateTime(2018, 8, 27);
        //	var Response = IPRFacade.GetReport(null, null, "CategoryName", DateFrom, DateTo);
        //	Assert.NotEqual(Response.Item2, 0);
        //}

        [Fact]
        public void Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            //UnitReceiptNote urn = await _dataUtil.GetTestData2("unit-test");
            var DateFrom = DateTime.Now;
            DateFrom = DateFrom.Date;
            var DateTo = DateTime.Now;
            DateTo = DateTo.Date;
            ImportPurchasingBookReportFacade iprFacade = new ImportPurchasingBookReportFacade(_ServiceProvider.Object, dbContext);
            //var Response = facade.GetReport(null, null, null, DateFrom, DateTo);
            var Response = iprFacade.GenerateExcel(null, null, null, DateFrom, DateTo);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }
        //[Fact]
        //public async Task Should_Success_Get_Report_Total_Purchase_By_Units_Null_Data_Excel()
        //{
        //          ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetTestData("Unit test");
        //          DeliveryOrder deliveryOrder = await DODataUtil.GetTestData("unit-test");
        //          UnitReceiptNote urn = await DataUtil.GetTestData2("unit-test");
        //          var DateFrom = DateTime.Now;
        //          DateFrom = DateFrom.Date;
        //          var DateTo = DateTime.Now;
        //          DateTo = DateTo.Date;
        //          var Response = IPRFacade.GenerateExcel(null, null, null, DateFrom, DateTo);
        //	Assert.IsType(typeof(System.IO.MemoryStream), Response);
        //}
    }
}
