using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.BankExpenditureNote;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.BankExpenditureNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.BankExpenditureNoteTest
{
    public class BasicTest
    {
        private const string ENTITY = "BankExpenditureNote";
        private PurchasingDocumentAcceptanceDataUtil pdaDataUtil;

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

        private BankExpenditureNoteDataUtil _dataUtil(BankExpenditureNoteFacade facade, string testName)
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

        [Fact]
        public async void Should_Success_Get_Data()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            BankExpenditureNoteFacade facade = new BankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object);
            await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            ReadResponse Response = facade.Read();
            Assert.NotEqual(Response.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Unit_Payment_Order()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            BankExpenditureNoteFacade facade = new BankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object);
            _dataUtil(facade, GetCurrentMethod());
            PurchasingDocumentExpedition model = await pdaDataUtil.GetCashierTestData();

            var Response = facade.GetAllByPosition(1, 25, "{}", null, "{}");
            Assert.NotEqual(Response.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            BankExpenditureNoteFacade facade = new BankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object);
            BankExpenditureNoteModel model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            numberGeneratorMock.Setup(s => s.GenerateDocumentNumber(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("test-code");
            BankExpenditureNoteFacade facade = new BankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object);
            BankExpenditureNoteModel model = _dataUtil(facade, GetCurrentMethod()).GetNewData();
            
            var Response = await facade.Create(model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            BankExpenditureNoteFacade facade = new BankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object);
            BankExpenditureNoteModel model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();

            BankExpenditureNoteDetailModel modelDetail = _dataUtil(facade, GetCurrentMethod()).GetNewDetailData();
            model.Details.Clear();
            model.Details.Add(modelDetail);
            var Response = await facade.Update((int)model.Id, model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            BankExpenditureNoteFacade facade = new BankExpenditureNoteFacade(_dbContext(GetCurrentMethod()), numberGeneratorMock.Object);
            BankExpenditureNoteModel Data = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            int AffectedRows = await facade.Delete((int)Data.Id, "Test");
            Assert.True(AffectedRows > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data()
        {
            BankExpenditureNoteViewModel vm = new BankExpenditureNoteViewModel()
            {
                Date = null,
                Bank = null,
                BGCheckNumber = null,
                Details = new List<BankExpenditureNoteDetailViewModel>()
            };

            Assert.True(vm.Validate(null).Count() > 0);
        }
    }
}
