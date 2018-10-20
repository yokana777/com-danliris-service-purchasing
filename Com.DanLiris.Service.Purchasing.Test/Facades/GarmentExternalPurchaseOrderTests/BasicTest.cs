using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentExternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentExternalPurchaseOrderTests
{
    public class BasicTest
    {
        private const string ENTITY = "GarmentExternalPurchaseOrder";

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

        private GarmentExternalPurchaseOrderDataUtil dataUtil(GarmentExternalPurchaseOrderFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            return new GarmentExternalPurchaseOrderDataUtil(facade, garmentInternalPurchaseOrderDataUtil);
        }

        [Fact]
        public async void Should_Success_Create_Data_Fabric()
        {
            var facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider,_dbContext(GetCurrentMethod()));
            var data = dataUtil(facade, GetCurrentMethod()).GetNewDataFabric();
            var Response = await facade.Create(data, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Create_Data_Acc()
        {
            var facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var data = dataUtil(facade, GetCurrentMethod()).GetNewDataACC();
            var Response = await facade.Create(data, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data()
        {
            GarmentExternalPurchaseOrderFacade facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider,_dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewDataACC();
            model.Items = null;
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(model, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public void Should_Success_Get_All_Data()
        {
            var facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider,_dbContext(GetCurrentMethod()));
            var data = dataUtil(facade, GetCurrentMethod()).GetTestDataAcc();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var data = dataUtil(facade, GetCurrentMethod()).GetNewDataACC();
            var Responses = await facade.Create(data, USERNAME);
            var Response = facade.ReadById((int)data.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Error_Update_Data()
        {
            GarmentExternalPurchaseOrderFacade facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider,_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestDataAcc();

            Exception errorInvalidId = await Assert.ThrowsAsync<Exception>(async () => await facade.Update(0, model, USERNAME));
            Assert.NotNull(errorInvalidId.Message);

            model.Items = null;
            Exception errorNullItems = await Assert.ThrowsAsync<Exception>(async () => await facade.Update((int)model.Id, model, USERNAME));
            Assert.NotNull(errorNullItems.Message);
        }

        [Fact]
        public async void Should_Success_EPOPost()
        {
            GarmentExternalPurchaseOrderFacade facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            List<GarmentExternalPurchaseOrder> modelList = new List<GarmentExternalPurchaseOrder>();
            GarmentExternalPurchaseOrder model = await dataUtil(facade, GetCurrentMethod()).GetTestDataAcc();
            modelList.Add(model);
            var Response = facade.EPOPost(modelList, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_EPOUnpost()
        {
            GarmentExternalPurchaseOrderFacade facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            GarmentExternalPurchaseOrder model = await dataUtil(facade, GetCurrentMethod()).GetTestDataAcc();
            var Response = facade.EPOUnpost((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        //[Fact]
        //public async void Should_Error_Get_Data_Supplier()
        //{
        //    GarmentExternalPurchaseOrderFacade facade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
        //    GarmentExternalPurchaseOrder model = await dataUtil(facade, GetCurrentMethod()).GetTestDataAcc();
        //    var Response = facade.GetSupplier(model.SupplierId);
        //    Assert.Null(Response);
        //}
    }
}
