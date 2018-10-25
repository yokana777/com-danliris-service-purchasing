using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentInvoiceTests
{
    //[Collection("ServiceProviderFixture Collection")]
    //public class BasicTest
    //{
    //    private const string ENTITY = "GarmentInvoice";

    //    private const string USERNAME = "Unit Test";
    //    private IServiceProvider ServiceProvider { get; set; }

    //    private PurchasingDbContext _dbContext(string testName)
    //    {
    //        DbContextOptionsBuilder<PurchasingDbContext> optionsBuilder = new DbContextOptionsBuilder<PurchasingDbContext>();
    //        optionsBuilder
    //            .UseInMemoryDatabase(testName)
    //            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

    //        PurchasingDbContext dbContext = new PurchasingDbContext(optionsBuilder.Options);

    //        return dbContext;
    //    }

    //    private GarmentInvoiceDataUtil dataUtil(GarmentInvoiceFacade facade, string testName)
    //    {
    //        var garmentInvoiceDetailFacade= new GarmentDeliveryOrderDetailFacade(_dbContext(testName));
    //        var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(_dbContext(testName));
    //        var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade);
    //        return new GarmentInvoiceDataUtil(facade, garmentDeliveryOrderDataUtil);
    //    }


    //    [Fact]
    //    public async void Should_Success_Get_Data_By_Id()
    //    {
    //        GarmentInvoice model = await dataUtil.GetTestData(USERNAME);
    //        var Response = Facade.ReadById((int)model.Id);
    //        Assert.NotNull(Response);
    //    }
    //}

    [Collection("ServiceProviderFixture Collection")]
    public class BasicTest
    {
        private const string USERNAME = "Unit Test";
        private IServiceProvider ServiceProvider { get; set; }

        public BasicTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = USERNAME;
        }

        private GarmentInvoiceDataUtil DataUtil
        {
            get { return (GarmentInvoiceDataUtil)ServiceProvider.GetService(typeof(GarmentInvoiceDataUtil)); }
        }

        private GarmentInvoiceFacade Facade
        {
            get { return (GarmentInvoiceFacade)ServiceProvider.GetService(typeof(GarmentInvoiceFacade)); }
        }

        //[Fact]
        //public async void Should_Success_Create_Data()
        //{
        //    GarmentInvoice model = await DataUtil.GetNewData(USERNAME);
        //    var Response = await Facade.Create(model, USERNAME);
        //    Assert.NotEqual(Response, 0);
        //}

        //[Fact]
        //public async void Should_Success_Get_Data_By_Id()
        //{
        //    GarmentInvoice model = await DataUtil.GetTestData(USERNAME);
        //    var Response = Facade.ReadById((int)model.Id);
        //    Assert.NotNull(Response);
        //}
    }
}
