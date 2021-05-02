using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentReportTests
{
    public class BudgetMasterSampleDisplayFacadeTest
    {
        private const string ENTITY = "BudgetMasterSampleDisplay";

        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private Mock<IServiceProvider> GetMockServiceProvider()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService { Username = "Username" });

            return mockServiceProvider;
        }

        private PurchasingDbContext GetDbContext(string testName)
        {
            DbContextOptionsBuilder<PurchasingDbContext> optionsBuilder = new DbContextOptionsBuilder<PurchasingDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging();

            PurchasingDbContext dbContext = new PurchasingDbContext(optionsBuilder.Options);

            return dbContext;
        }

        private GarmentPurchaseRequestDataUtil GetDataUtil(GarmentPurchaseRequestFacade facade)
        {
            return new GarmentPurchaseRequestDataUtil(facade);
        }

        [Fact]
        public async void Should_Success_Get_Monitoring()
        {
            var mockServiceProvider = GetMockServiceProvider();

            var dbContext = GetDbContext(GetCurrentMethod());

            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(mockServiceProvider.Object, dbContext);
            var dataUtil = GetDataUtil(garmentPurchaseRequestFacade);
            var dataGarmentPurchaseRequest = await dataUtil.GetTestData();

            var facade = new BudgetMasterSampleDisplayFacade(mockServiceProvider.Object, dbContext);

            var Response = facade.GetMonitoring(dataGarmentPurchaseRequest.Id, "{}");
            Assert.NotEmpty(Response.Item1);
        }

        [Fact]
        public async void Should_Success_Get_Excel()
        {
            var mockServiceProvider = GetMockServiceProvider();

            var dbContext = GetDbContext(GetCurrentMethod());

            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(mockServiceProvider.Object, dbContext);
            var dataUtil = GetDataUtil(garmentPurchaseRequestFacade);
            var dataGarmentPurchaseRequest = await dataUtil.GetTestData();

            var facade = new BudgetMasterSampleDisplayFacade(mockServiceProvider.Object, dbContext);

            var Response = facade.GenerateExcel(dataGarmentPurchaseRequest.Id);
            Assert.NotNull(Response);
        }
    }
}
