using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager.CacheData;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.DeliveryOrderTests
{
    //[Collection("ServiceProviderFixture Collection")]
    public class BasicTest
    {
        private const string ENTITY = "DeliveryOrdersSJ";
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
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging();

            PurchasingDbContext dbContext = new PurchasingDbContext(optionsBuilder.Options);

            return dbContext;
        }


        private Mock<IServiceProvider> GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            serviceProvider
                .Setup(x => x.GetService(typeof(InternalPurchaseOrderFacade)))
                .Returns(new InternalPurchaseOrderFacade(serviceProvider.Object, _dbContext(GetCurrentMethod())));

            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProviders = services.BuildServiceProvider();
            var memoryCache = serviceProviders.GetService<IMemoryCache>();
            var mockMemoryCache = new Mock<IMemoryCacheManager>();
            mockMemoryCache.Setup(x => x.Get<List<CategoryCOAResult>>(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, List<CategoryCOAResult>>>()))
                .Returns(new List<CategoryCOAResult>() { new CategoryCOAResult() });
            mockMemoryCache.Setup(x => x.Get<List<IdCOAResult>>(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, List<IdCOAResult>>>()))
               .Returns(new List<IdCOAResult>() { new IdCOAResult() });
            serviceProvider
                .Setup(x => x.GetService(typeof(IMemoryCacheManager)))
                .Returns(mockMemoryCache.Object);

            var mockCurrencyProvider = new Mock<ICurrencyProvider>();
            mockCurrencyProvider
                .Setup(x => x.GetCurrencyByCurrencyCode(It.IsAny<string>()))
                .ReturnsAsync((Currency)null);
            serviceProvider
                .Setup(x => x.GetService(typeof(ICurrencyProvider)))
                .Returns(mockCurrencyProvider.Object);

            //var cache = new Mock<IMemoryCache>();
            //cache.Setup(x => x.GetOrCreate<List<IdCOAResult>>());

            return serviceProvider;
        }

        private DeliveryOrderDataUtil _dataUtil(DeliveryOrderFacade facade, PurchasingDbContext _DbContext)
        {
            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(GetServiceProvider().Object, _DbContext);
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(GetServiceProvider().Object, _DbContext);
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(GetServiceProvider().Object, _DbContext);
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(_DbContext, GetServiceProvider().Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);



            return new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, facade);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtil(facade, dbContext).GetNewData(USERNAME);
            
            var response = await facade.Create(model, USERNAME);

            Assert.NotEqual(response, 0);

            
        }

        [Fact]
        public async Task Should_Error_Create_Data_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(() => facade.Create(null, USERNAME));
            Assert.Equal(exception.Message, "Object reference not set to an instance of an object.");
        }

        [Fact]
        public async Task Should_Success_Get_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);
            Tuple<List<DeliveryOrder>, int, Dictionary<string, string>> Response = facade.Read(Keyword: model.DONo);
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Update_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);
            foreach (var item in model.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.DOQuantity -= 1;
                }
            }
            var Response = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(Response, 0);

            
        }

        [Fact]
        public async Task Should_Success_UpdateThenAdd()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);
            foreach (var item in model.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.DOQuantity -= 1;
                }
            }
            var Response = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(Response, 0);

            DeliveryOrderItem oldItem = model.Items.FirstOrDefault();
            DeliveryOrderDetail oldDetail = oldItem.Details.FirstOrDefault();
            DeliveryOrderItem newDuplicateItem = new DeliveryOrderItem
            {
                EPOId = oldItem.EPOId,
                EPONo = oldItem.EPONo,
                Details = new List<DeliveryOrderDetail>()
            };
            DeliveryOrderDetail oldDuplicateDetail = new DeliveryOrderDetail
            {
                EPODetailId = oldDetail.EPODetailId,
                POItemId = oldDetail.POItemId,
                PRId = oldDetail.PRId,
                PRNo = oldDetail.PRNo,
                PRItemId = oldDetail.PRItemId,
                ProductId = oldDetail.ProductId,
                ProductCode = oldDetail.ProductCode,
                ProductName = oldDetail.ProductName,
                ProductRemark = oldDetail.ProductRemark,
                DOQuantity = oldDetail.DOQuantity,
                DealQuantity = oldDetail.DealQuantity,
                UomId = oldDetail.UomId,
                UomUnit = oldDetail.UomUnit,
                ReceiptQuantity = oldDetail.ReceiptQuantity,
                IsClosed = oldDetail.IsClosed,
            };
            DeliveryOrderDetail newDuplicateDetail = new DeliveryOrderDetail
            {
                EPODetailId = oldDetail.EPODetailId,
                POItemId = oldDetail.POItemId,
                PRId = oldDetail.PRId,
                PRNo = oldDetail.PRNo,
                PRItemId = oldDetail.PRItemId,
                ProductId = "PrdId2",
                ProductCode = "PrdCode2",
                ProductName = "PrdName2",
                ProductRemark = oldDetail.ProductRemark,
                DOQuantity = oldDetail.DOQuantity,
                DealQuantity = oldDetail.DealQuantity,
                UomId = oldDetail.UomId,
                UomUnit = oldDetail.UomUnit,
                ReceiptQuantity = oldDetail.ReceiptQuantity,
                IsClosed = oldDetail.IsClosed,
            };
            newDuplicateItem.Details.Add(oldDuplicateDetail);
            newDuplicateItem.Details.Add(newDuplicateDetail);
            model.Items.Add(newDuplicateItem);
            var ResponseAddDuplicateItem = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAddDuplicateItem, 0);

            
        }

        [Fact]
        public async Task Should_Success_UpdateThenAddMany()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);
            foreach (var item in model.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.DOQuantity -= 1;
                }
            }
            var Response = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(Response, 0);

            var newModelForAddItem = await _dataUtil(facade, dbContext).GetNewData(USERNAME);
            DeliveryOrderItem newModelItem = newModelForAddItem.Items.FirstOrDefault();
            model.Items.Add(newModelItem);
            model.Items.Add(newModelItem);
            var ResponseAddItem = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAddItem, 0);


        }

        //[Fact]
        //public async Task Should_Success_UpdateThenRemoveDetails()
        //{
        //    var dbContext = _dbContext(GetCurrentMethod() + "Details");
        //    DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
        //    DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);

        //    model.Items.FirstOrDefault().Details.Clear();
        //    var ResponseRemoveItemDetail = await facade.Update((int)model.Id, model, USERNAME);
        //    Assert.NotEqual(ResponseRemoveItemDetail, 0);
        //}

        //[Fact]
        //public async Task Should_Success_Update_DataThenRemoveItems()
        //{
        //    var dbContext = _dbContext(GetCurrentMethod() + "Items");
        //    DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
        //    DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);


        //    model.Items = new List<DeliveryOrderItem>();
        //    var ResponseRemoveItemDetail = await facade.Update((int)model.Id, model, USERNAME);
        //    Assert.NotEqual(ResponseRemoveItemDetail, 0);
        //}


        [Fact]
        public async Task Should_Error_Update_Data_Invalid_Id()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            Exception exception = await Assert.ThrowsAsync<Exception>(() => facade.Update(0, new DeliveryOrder(), USERNAME));
            Assert.Equal(exception.Message, "Invalid Id");
        }

        [Fact]
        public async Task Should_Success_Delete_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);
            var Response = facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData("Unit test");
            var Response = facade.GetReport("", null, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, -1);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Null_Parameter_Using_Two_Test_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model_1 = await _dataUtil(facade, dbContext).GetTestDataDummy("Unit test");
            DeliveryOrder model_2 = await _dataUtil(facade, dbContext).GetTestDataDummy("Unit test");
            var Response = facade.GetReport("", null, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(Response.Item2, -1);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData("Unit test");
            var Response = facade.GenerateExcel(model.DONo, model.SupplierId, null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            DeliveryOrder model = await _dataUtil(facade, dbContext).GetTestData("Unit test");
            var Response = facade.GenerateExcel("", "", null, null, 7);
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async Task Should_Success_Get_ReadBySupplier()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            DeliveryOrderFacade facade = new DeliveryOrderFacade(dbContext, GetServiceProvider().Object);
            var model = await _dataUtil(facade, dbContext).GetTestData(USERNAME);
            var Response = facade.ReadBySupplier(null, model.Items.FirstOrDefault().Details.FirstOrDefault().UnitId, model.SupplierId);
            Assert.NotEqual(0, Response.Count);
        }
    }
}
