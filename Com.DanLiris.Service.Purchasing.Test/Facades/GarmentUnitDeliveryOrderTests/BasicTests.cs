using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Migrations;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentUnitDeliveryOrderTests
{
    public class BasicTests
    {
        private const string ENTITY = "GarmentUnitDeliveryOrder";

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
        private Mock<IServiceProvider> GetServiceProvider()
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            message.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");
            var HttpClientService = new Mock<IHttpClientService>();
            HttpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(message);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(HttpClientService.Object);

            return serviceProvider;
        }
        private IServiceProvider GetServiceProviderUnitReceiptNote()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");

            var httpClientService = new Mock<IHttpClientService>();
            httpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(httpResponseMessage);

            var mapper = new Mock<IMapper>();
            mapper
                .Setup(x => x.Map<GarmentUnitReceiptNoteViewModel>(It.IsAny<GarmentUnitReceiptNote>()))
                .Returns(new GarmentUnitReceiptNoteViewModel
                {
                    Id = 1,
                    Items = new List<GarmentUnitReceiptNoteItemViewModel>
                    {
                        new GarmentUnitReceiptNoteItemViewModel()
                    }
                });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService { Username = "Username" });
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(httpClientService.Object);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IMapper)))
                .Returns(mapper.Object);

            return serviceProviderMock.Object;
        }

        private GarmentUnitDeliveryOrderDataUtil dataUtil(GarmentUnitDeliveryOrderFacade facade, string testName)
        {

            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(testName));
            var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

            var garmentUnitDeliveryOrderFacade = new GarmentUnitReceiptNoteFacade(GetServiceProviderUnitReceiptNote(), _dbContext(testName));
            var garmentInvoieDataUtil = new GarmentUnitReceiptNoteDataUtil(garmentUnitDeliveryOrderFacade, garmentDeliveryOrderDataUtil);

            return new GarmentUnitDeliveryOrderDataUtil(facade, garmentInvoieDataUtil);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            GarmentUnitDeliveryOrderFacade facade = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.Create(model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data()
        {
            GarmentUnitDeliveryOrderFacade facade = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(null, USERNAME));
            Assert.NotNull(e.Message);
        }

        //[Fact]
        //public async void Should_Success_Update_Data()
        //{
        //    var facade = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), ServiceProvider);
        //    var facadeDO = new GarmentDeliveryOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
        //    GarmentUnitDeliveryOrder data = dataUtil(facade, GetCurrentMethod()).GetNewData();

        //    var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
        //    Assert.NotEqual(ResponseUpdate, 0);

        //    List<GarmentUnitDeliveryOrderItem> Newitems = new List<GarmentUnitDeliveryOrderItem>(data.Items);
        //    Newitems.Add(item);
        //    data.Items = Newitems;

        //    var ResponseUpdate1 = await facade.Update((int)data.Id, data, USERNAME);
        //    Assert.NotEqual(ResponseUpdate1, 0);
        //}

        //[Fact]
        //public async void Should_Success_Update_Data2()
        //{
        //    var dbContext = _dbContext(GetCurrentMethod());
        //    var facade = new GarmentUnitDeliveryOrderFacade(dbContext, ServiceProvider);
        //    var facadeDO = new GarmentDeliveryOrderFacade(ServiceProvider, dbContext);
        //    GarmentUnitDeliveryOrder data = dataUtil(facade, GetCurrentMethod()).GetNewData();
        //    GarmentUnitDeliveryOrderItem item = await dataUtil(facade, GetCurrentMethod()).GetNewDataItem(USERNAME);

        //    var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
        //    Assert.NotEqual(ResponseUpdate, 0);

        //    List<GarmentUnitDeliveryOrderItem> Newitems = new List<GarmentUnitDeliveryOrderItem>(data.Items);
        //    Newitems.Add(item);
        //    data.Items = Newitems;

        //    var ResponseUpdate1 = await facade.Update((int)data.Id, data, USERNAME);
        //    Assert.NotEqual(ResponseUpdate, 0);

        //    dbContext.Entry(data).State = EntityState.Detached;
        //    foreach (var items in data.Items)
        //    {
        //        dbContext.Entry(items).State = EntityState.Detached;
        //        foreach (var detail in items.Details)
        //        {
        //            dbContext.Entry(detail).State = EntityState.Detached;
        //        }
        //    }

        //    var newData = dbContext.GarmentUnitDeliveryOrders.AsNoTracking()
        //        .Include(m => m.Items)
        //            .ThenInclude(i => i.Details)
        //        .FirstOrDefault(m => m.Id == data.Id);

        //    newData.Items = newData.Items.Take(1).ToList();

        //    var ResponseUpdate2 = await facade.Update((int)newData.Id, newData, USERNAME);
        //    Assert.NotEqual(ResponseUpdate2, 0);
        //}
        //[Fact]
        //public async void Should_Error_Update_Data()
        //{
        //    var facade = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), ServiceProvider);
        //    GarmentUnitDeliveryOrder data = dataUtil(facade, GetCurrentMethod()).GetNewData();
        //    List<GarmentUnitDeliveryOrderItem> item = new List<GarmentUnitDeliveryOrderItem>(data.Items);

        //    data.Items.Add(new GarmentUnitDeliveryOrderItem
        //    {
        //        URNId = It.IsAny<int>(),
        //        URNItemId = It.IsAny<int>(),
        //        URNNo = "urnno",
        //    });

        //    var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
        //    Assert.NotEqual(ResponseUpdate, 0);
        //    var newItem = new GarmentUnitDeliveryOrderItem
        //    {
        //        URNId = It.IsAny<int>(),
        //        URNItemId = It.IsAny<int>(),
        //        URNNo = "urnno",
        //    };
        //    List<GarmentUnitDeliveryOrderItem> Newitems = new List<GarmentUnitDeliveryOrderItem>(data.Items);
        //    Newitems.Add(newItem);
        //    data.Items = Newitems;

        //    Exception errorNullItems = await Assert.ThrowsAsync<Exception>(async () => await facade.Update((int)data.Id, data, USERNAME));
        //    Assert.NotNull(errorNullItems.Message);
        //}

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            GarmentUnitDeliveryOrderFacade facade = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Delete_Data()
        {
            var facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));

            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Delete(0, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Get_All_Data()
        {
            GarmentUnitDeliveryOrderFacade facade = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            GarmentUnitDeliveryOrderFacade facade = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }
    }
}
