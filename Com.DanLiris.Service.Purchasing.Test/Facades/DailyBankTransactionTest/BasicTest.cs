using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DailyBankTransactionDataUtil;
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

namespace Com.DanLiris.Service.Purchasing.Test.Facades.DailyBankTransactionTest
{
    public class BasicTest
    {
        private const string ENTITY = "DailyBankTransaction";
        //private PurchasingDocumentAcceptanceDataUtil pdaDataUtil;

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

        private DailyBankTransactionDataUtil _dataUtil(DailyBankTransactionFacade facade)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return new DailyBankTransactionDataUtil(facade);
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            await _dataUtil(facade).GetTestDataIn();
            ReadResponse Response = facade.Read();
            Assert.NotEqual(Response.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var numberGeneratorMock = new Mock<IBankDocumentNumberGenerator>();
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            DailyBankTransactionModel modelIn = await _dataUtil(facade).GetTestDataIn();
            DailyBankTransactionModel modelOut = await _dataUtil(facade).GetTestDataOut();
            var Response = facade.ReadById(modelIn.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            DailyBankTransactionModel model = _dataUtil(facade).GetNewData();
            var Response = await facade.Create(model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public void Should_Success_Validate_All_Null_Data()
        {
            DailyBankTransactionViewModel vm = new DailyBankTransactionViewModel();

            Assert.True(vm.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Empty_Supplier_Operasional_Data()
        {
            DailyBankTransactionViewModel vm = new DailyBankTransactionViewModel()
            {
                Date = DateTimeOffset.Now.AddDays(1),
                Status = "OUT",
                SourceType = "Operasional",
            };

            Assert.True(vm.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Empty_Supplier_Non_Operasional_Data()
        {
            DailyBankTransactionViewModel vm = new DailyBankTransactionViewModel()
            {
                Date = DateTimeOffset.Now.AddDays(1),
                Status = "OUT",
                SourceType = "Investasi",
            };

            Assert.True(vm.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Empty_Buyer_Operasional_Data()
        {
            DailyBankTransactionViewModel vm = new DailyBankTransactionViewModel()
            {
                Date = DateTimeOffset.Now.AddDays(1),
                Status = "IN",
                SourceType = "Operasional",
            };

            Assert.True(vm.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Empty_Buyer_Non_Operasional_Data()
        {
            DailyBankTransactionViewModel vm = new DailyBankTransactionViewModel()
            {
                Date = DateTimeOffset.Now.AddDays(1),
                Status = "IN",
                SourceType = "Investasi",
            };

            Assert.True(vm.Validate(null).Count() > 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_All_Null()
        {
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            DailyBankTransactionModel model = _dataUtil(facade).GetNewData();
            var Response = await facade.Create(model, "Unit Test");

            ReadResponse Result = facade.GetReport(null, null, null, 7);
            Assert.NotEqual(Result.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Null_Date()
        {
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            DailyBankTransactionModel model = _dataUtil(facade).GetNewData();
            var Response = await facade.Create(model, "Unit Test");

            ReadResponse Result = facade.GetReport(model.AccountBankId, null, null, 7);
            Assert.NotEqual(Result.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Null_DateTo()
        {
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            DailyBankTransactionModel model = _dataUtil(facade).GetNewData();
            model.Date = model.Date.AddDays(-3);
            var Response = await facade.Create(model, "Unit Test");

            ReadResponse Result = facade.GetReport(model.AccountBankId, model.Date.AddDays(-10), null, 7);
            Assert.NotEqual(Result.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Null_DateFrom()
        {
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            DailyBankTransactionModel model = _dataUtil(facade).GetNewData();
            model.Date = model.Date.AddDays(-3);
            var Response = await facade.Create(model, "Unit Test");

            ReadResponse Result = facade.GetReport(model.AccountBankId, null, DateTimeOffset.Now, 7);
            Assert.NotEqual(Result.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_NotNull_Date_Param()
        {
            DailyBankTransactionFacade facade = new DailyBankTransactionFacade(_dbContext(GetCurrentMethod()));
            DailyBankTransactionModel model = _dataUtil(facade).GetNewData();
            model.Date = model.Date.AddDays(-3);
            var Response = await facade.Create(model, "Unit Test");

            ReadResponse Result = facade.GetReport(model.AccountBankId, model.Date.AddDays(-10), DateTimeOffset.Now, 7);
            Assert.NotEqual(Result.Data.Count, 0);
        }
    }
}
