using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.BankExpenditureNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PPHBankExpenditureNoteDataUtil;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.UnitPaymentOrderPaidStatusTests
{
    public class ReportTest
    {
        private const string ENTITY = "UnitPaymentOrderPaidStatus";
        private PurchasingDocumentAcceptanceDataUtil pdaDataUtil;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private BankExpenditureNoteDataUtil _dataUtilBEN(BankExpenditureNoteFacade facade, string testName)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());


            PurchasingDocumentExpeditionFacade pdeFacade = new PurchasingDocumentExpeditionFacade(serviceProvider.Object, _dbContext(testName));
            SendToVerificationDataUtil stvDataUtil = new SendToVerificationDataUtil(pdeFacade);
            pdaDataUtil = new PurchasingDocumentAcceptanceDataUtil(pdeFacade, stvDataUtil);

            return new BankExpenditureNoteDataUtil(facade, pdaDataUtil);
        }

        private PPHBankExpenditureNoteDataUtil _dataUtilBENPPH(PPHBankExpenditureNoteFacade facade, string testName)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());


            PurchasingDocumentExpeditionFacade pdeFacade = new PurchasingDocumentExpeditionFacade(serviceProvider.Object, _dbContext(testName));
            SendToVerificationDataUtil stvDataUtil = new SendToVerificationDataUtil(pdeFacade);
            pdaDataUtil = new PurchasingDocumentAcceptanceDataUtil(pdeFacade, stvDataUtil);

            return new PPHBankExpenditureNoteDataUtil(facade, pdaDataUtil);
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

        private Mock<IServiceProvider> GetServiceProviderMock()
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

        [Fact]
        public async void Should_Success_Get_Data()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            numberGeneratorMock.Setup(p => p.GenerateDocumentNumber(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("TEST");

            BankExpenditureNoteFacade facadeBEN = new BankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object, GetServiceProviderMock().Object);
            await _dataUtilBEN(facadeBEN, GetCurrentMethod()).GetTestData();

            PPHBankExpenditureNoteFacade facadeBENPPH = new PPHBankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object);
            await _dataUtilBENPPH(facadeBENPPH, GetCurrentMethod()).GetTestData();

            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", null, null, null, null, null, null, 0);

            Assert.NotEqual(null, response);
        }

        [Fact]
        public void Should_Success_Get_Data_With_Params()
        {
            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", "", "", "", null, null, null, 0);

            Assert.NotEqual(null, response);
        }

        [Fact]
        public void Should_Success_Get_Data_With_Date()
        {
            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", null, null, null, null, new DateTimeOffset(), new DateTimeOffset(), 0);

            Assert.NotEqual(null, response);
        }

        [Fact]
        public void Should_Success_Get_Data_With_Date_And_Params()
        {
            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", "", "", "", null, new DateTimeOffset(), new DateTimeOffset(), 0);

            Assert.NotEqual(null, response);
        }

        [Fact]
        public void Should_Success_Get_Data_LUNAS()
        {
            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", "", "", "", "LUNAS", new DateTimeOffset(), new DateTimeOffset(), 0);

            Assert.NotEqual(null, response);
        }

        [Fact]
        public void Should_Success_Get_Data_SUDAH_BAYAR_DPP_PPN()
        {
            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", "", "", "", "SUDAH BAYAR DPP+PPN", new DateTimeOffset(), new DateTimeOffset(), 0);

            Assert.NotEqual(null, response);
        }

        [Fact]
        public void Should_Success_Get_Data_SUDAH_BAYAR_PPH()
        {
            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", "", "", "", "SUDAH BAYAR PPH", new DateTimeOffset(), new DateTimeOffset(), 0);

            Assert.NotEqual(null, response);
        }

        [Fact]
        public void Should_Success_Get_Data_BELUM_BAYAR()
        {
            UnitPaymentOrderPaidStatusReportFacade facade = new UnitPaymentOrderPaidStatusReportFacade(_dbContext(GetCurrentMethod()));
            ReadResponse<object> response = facade.GetReport(25, 1, "{}", "", "", "", "BELUM BAYAR", new DateTimeOffset(), new DateTimeOffset(), 0);

            Assert.NotEqual(null, response);
        }
    }
}
