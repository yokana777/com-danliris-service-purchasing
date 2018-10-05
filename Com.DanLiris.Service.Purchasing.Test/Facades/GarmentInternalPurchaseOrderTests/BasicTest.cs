using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentInternalPurchaseOrderTests
{
    public class BasicTest
    {
        private const string ENTITY = "GarmentInternalPurchaseOrder";

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

        private GarmentInternalPurchaseOrderDataUtil dataUtil(GarmentInternalPurchaseOrderFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            return new GarmentInternalPurchaseOrderDataUtil(facade, garmentPurchaseRequestDataUtil);
        }

        [Fact]
        public async void Should_Success_Create_Multiple_Data()
        {
            var facade = new GarmentInternalPurchaseOrderFacade(_dbContext(GetCurrentMethod()));
            var listData = dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.CreateMultiple(listData, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Multiple_Data()
        {
            var facade = new GarmentInternalPurchaseOrderFacade(_dbContext(GetCurrentMethod()));
            var listData = dataUtil(facade, GetCurrentMethod()).GetNewData();
            foreach (var data in listData)
            {
                data.Items = null;
            }
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.CreateMultiple(listData, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Get_All_Data()
        {
            var facade = new GarmentInternalPurchaseOrderFacade(_dbContext(GetCurrentMethod()));
            var listData = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var facade = new GarmentInternalPurchaseOrderFacade(_dbContext(GetCurrentMethod()));
            var listData = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int) listData.First().Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public void Should_Success_Validate_Data()
        {
            var viewModel = new GarmentInternalPurchaseOrderViewModel
            {
                Items = null
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }
    }
}
