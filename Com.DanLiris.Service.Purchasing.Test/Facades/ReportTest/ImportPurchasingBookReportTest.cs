using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MongoDB.Bson;
using Moq;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ReportTest
{
    [Collection("ServiceProviderFixture Collection")]
    public class ImportPurchasingBookReportTest
    {
        private IServiceProvider ServiceProvider { get; set; }

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

        private UnitReceiptNoteDataUtil _dataUtil(UnitReceiptNoteFacade facade, string testName)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

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

            return new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, facade, deliveryOrderDataUtil);
        }

        public ImportPurchasingBookReportTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;
        }

        private UnitReceiptNoteDataUtil DataUtil
        {
            get { return (UnitReceiptNoteDataUtil)ServiceProvider.GetService(typeof(UnitReceiptNoteDataUtil)); }
        }
		private ImportPurchasingBookReportFacade IPRFacade
		{
			get { return (ImportPurchasingBookReportFacade)ServiceProvider.GetService(typeof(ImportPurchasingBookReportFacade)); }
		}
		private UnitReceiptNoteFacade Facade
        {
            get { return (UnitReceiptNoteFacade)ServiceProvider.GetService(typeof(UnitReceiptNoteFacade)); }
        }
		private ExternalPurchaseOrderDataUtil EPODataUtil
		{
			get { return (ExternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
		}
		private ExternalPurchaseOrderFacade EPOFacade
		{
			get { return (ExternalPurchaseOrderFacade)ServiceProvider.GetService(typeof(ExternalPurchaseOrderFacade)); }
		}
		private DeliveryOrderDataUtil DODataUtil
		{
			get { return (DeliveryOrderDataUtil)ServiceProvider.GetService(typeof(DeliveryOrderDataUtil)); }
		}

		private DeliveryOrderFacade DOFacade
		{
			get { return (DeliveryOrderFacade)ServiceProvider.GetService(typeof(DeliveryOrderFacade)); }
		}

        //private UnitPaymentOrderDataUtil UPODataUtil
        //{
        //    get { return (UnitPaymentOrderDataUtil)ServiceProvider.GetService(typeof(UnitPaymentOrderDataUtil)); }
        //}

        //private UnitPaymentOrderFacade UPOFacade
        //{
        //    get { return (UnitPaymentOrderFacade)ServiceProvider.GetService(typeof(UnitPaymentOrderFacade)); }
        //}

        [Fact]
		public async void Should_Success_Get_Report_Data()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
			await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
			DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
			await DOFacade.Create(deliveryOrder, "unit-test");
			UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
			await Facade.Create(urn, "unit-test");
            //UnitPaymentOrder upo = await UPODataUtil.GetTestData();
            //await UPOFacade.Create(upo, "unit-test", false, 7);
            var DateFrom = DateTime.Now;
            DateFrom = DateFrom.Date;
            var DateTo = DateTime.Now;
            DateTo = DateTo.Date;
            if (externalPurchaseOrder != null && deliveryOrder != null && urn != null)
            {
                var Response = IPRFacade.GetReport(null, null, null, DateFrom, DateTo);
                Assert.NotEqual(Response.Item2, 0);
            }
		}
		//[Fact]
		//public async void Should_Success_Get_Report_Data_No_Parameter()
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
		//public async void Should_Success_Get_Report_Data_Unit_Parameter()
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
		//public async void Should_Success_Get_Report_Data_Category_Parameter()
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
		public async void Should_Success_Get_Report_Data_Excel_Null_Parameter()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
			await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
			DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
			await DOFacade.Create(deliveryOrder, "unit-test");
			UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
			await Facade.Create(urn, "unit-test");
            var DateFrom = DateTime.Now;
            DateFrom = DateFrom.Date;
            var DateTo = DateTime.Now;
            DateTo = DateTo.Date;
            var Response = IPRFacade.GenerateExcel(null,null,null,DateFrom,DateTo);
			Assert.IsType(typeof(System.IO.MemoryStream), Response);
		}
		[Fact]
		public async void Should_Success_Get_Report_Total_Purchase_By_Units_Null_Data_Excel()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
			await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
			DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
			await DOFacade.Create(deliveryOrder, "unit-test");
			UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
			await Facade.Create(urn, "unit-test");
            var DateFrom = DateTime.Now;
            DateFrom = DateFrom.Date;
            var DateTo = DateTime.Now;
            DateTo = DateTo.Date;
            var Response = IPRFacade.GenerateExcel(null, null, null, DateFrom, DateTo);
			Assert.IsType(typeof(System.IO.MemoryStream), Response);
		}
	}
}
