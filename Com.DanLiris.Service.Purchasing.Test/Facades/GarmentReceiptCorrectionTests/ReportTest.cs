using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentCorrectionNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReceiptCorrectionFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReceiptCorrectionViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentCorrectionNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentReceiptCorrectionDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.NewIntegrationDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentReceiptCorrectionTests
{
    public class ReportTest
    {

        private const string ENTITY = "GarmentReceiptCorrectionReport";
        private const string USERNAME = "unit-test";
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

        private IServiceProvider GetServiceProvider()
        {
            var httpClientService = new Mock<IHttpClientService>();
            httpClientService
                .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("master/garment-suppliers"))))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new SupplierDataUtil().GetResultFormatterOkString()) });
            httpClientService
                .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("master/garment-currencies"))))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new CurrencyDataUtil().GetMultipleResultFormatterOkString()) });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService { Username = "Username", TimezoneOffset = 7 });
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(httpClientService.Object);

            return serviceProviderMock.Object;
        }

       

        private GarmentReceiptCorrectionDataUtil dataUtil(GarmentReceiptCorrectionFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(ServiceProvider, _dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(GetServiceProvider(), _dbContext(testName));
            var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

            var garmentUnitReceiptNoteFacade = new GarmentUnitReceiptNoteFacade(GetServiceProvider(), _dbContext(testName));
            var garmentUnitReceiptNoteDataUtil = new GarmentUnitReceiptNoteDataUtil(garmentUnitReceiptNoteFacade, garmentDeliveryOrderDataUtil);

          

            return new GarmentReceiptCorrectionDataUtil(facade, garmentUnitReceiptNoteDataUtil);
        }

        private GarmentDeliveryOrderDataUtil _dataUtilDO(GarmentDeliveryOrderFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(ServiceProvider, _dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            return new GarmentDeliveryOrderDataUtil(facade, garmentExternalPurchaseOrderDataUtil);
        }

        private GarmentCorrectionNoteDataUtil _dataUtilCorr(GarmentCorrectionNotePriceFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(ServiceProvider, _dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(GetServiceProvider(), _dbContext(testName));
            var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

            return new GarmentCorrectionNoteDataUtil(facade, garmentDeliveryOrderDataUtil);

        }


       

        [Fact]
        public async Task Should_Success_Get_All_Data()
        {
            


            var serviceProvider = GetServiceProvider();
            var dbContext = _dbContext(GetCurrentMethod());
            var Facade = new GarmentReceiptCorrectionFacade(dbContext, serviceProvider);

            GarmentDeliveryOrderFacade facadeDO = new GarmentDeliveryOrderFacade(serviceProvider, dbContext);
            var dataUtilDO = _dataUtilDO(facadeDO, GetCurrentMethod());

            var FacadeCorrection = new GarmentCorrectionNotePriceFacade(serviceProvider, dbContext);
            var dataUtilCorrection = new GarmentCorrectionNoteDataUtil(FacadeCorrection, dataUtilDO);

            var FacadeUnitReceipt = new GarmentUnitReceiptNoteFacade(serviceProvider, dbContext);
            var dataUtilUnitReceipt = new GarmentUnitReceiptNoteDataUtil(FacadeUnitReceipt, dataUtilDO);

            var dataUtilReceiptCorr = new GarmentReceiptCorrectionDataUtil(Facade, dataUtilUnitReceipt);

            var dataDO = await dataUtilDO.GetNewData();
            await facadeDO.Create(dataDO, USERNAME);

            var dataCorr = await dataUtilCorrection.GetTestData2(dataDO);
            long nowTicks = DateTimeOffset.Now.Ticks;

            var dataUnit = await dataUtilUnitReceipt.GetNewData(nowTicks, dataDO);
            await FacadeUnitReceipt.Create(dataUnit);
            var dataReceipt = await dataUtilReceiptCorr.GetNewData(dataUnit);
            await Facade.Create(dataReceipt.GarmentReceiptCorrection, "unit-test", 7);

            var dateFrom = DateTimeOffset.MinValue;
            var dateTo = DateTimeOffset.UtcNow;
            var facade1 = new GarmentReceiptCorrectionReportFacade(dbContext, serviceProvider);

            var Response = facade1.GetReport(null, null, dateFrom, dateTo, "{}", 1, 25);

            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Excel()
        {


            var serviceProvider = GetServiceProvider();
            var dbContext = _dbContext(GetCurrentMethod());
            var Facade = new GarmentReceiptCorrectionFacade(_dbContext(GetCurrentMethod()), serviceProvider);



            GarmentDeliveryOrderFacade facadeDO = new GarmentDeliveryOrderFacade(serviceProvider, dbContext);
            var dataUtilDO = _dataUtilDO(facadeDO, GetCurrentMethod());

            var FacadeCorrection = new GarmentCorrectionNotePriceFacade(serviceProvider, dbContext);
            var dataUtilCorrection = new GarmentCorrectionNoteDataUtil(FacadeCorrection, dataUtilDO);

            var FacadeUnitReceipt = new GarmentUnitReceiptNoteFacade(serviceProvider, dbContext);
            var dataUtilUnitReceipt = new GarmentUnitReceiptNoteDataUtil(FacadeUnitReceipt, dataUtilDO);

            var dataUtilReceiptCorr = new GarmentReceiptCorrectionDataUtil(Facade, dataUtilUnitReceipt);

            var dataDO = await dataUtilDO.GetNewData();
            await facadeDO.Create(dataDO, USERNAME);

            var dataCorr = await dataUtilCorrection.GetTestData2(dataDO);
            long nowTicks = DateTimeOffset.Now.Ticks;

            var dataUnit = await dataUtilUnitReceipt.GetNewData(nowTicks, dataDO);
            await FacadeUnitReceipt.Create(dataUnit);
            var dataReceipt = await dataUtilReceiptCorr.GetNewData(dataUnit);
            await Facade.Create(dataReceipt.GarmentReceiptCorrection, "unit-test");

            var dateFrom = DateTimeOffset.MinValue;
            var dateTo = DateTimeOffset.UtcNow;
            var facade1 = new GarmentReceiptCorrectionReportFacade(_dbContext(GetCurrentMethod()), serviceProvider);

            var Response = facade1.GenerateExcel(null, null, dateFrom, dateTo, "{}");
            //var garmentReceiptCorrectionFacade = new GarmentReceiptCorrectionFacade(_dbContext(GetCurrentMethod()),GetServiceProvider() );
            // var dataUtilReceiptNote = await dataUtil(Facade, GetCurrentMethod()).GetTestData();

            Assert.NotNull(Response);
        }



    }
}
