using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.UnitReceiptNoteTests
{
    public class BasicTest
    {
        private const string ENTITY = "UnitReceiptNote";

        private const string USERNAME = "Unit Test";
        //private IServiceProvider ServiceProvider { get; set; }

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
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return serviceProvider;
        }

        private Mock<IServiceProvider> _ServiceProvider => GetServiceProvider();

        private UnitReceiptNoteDataUtil _dataUtil(UnitReceiptNoteFacade facade, PurchasingDbContext _DbContext)
        {
            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(_ServiceProvider.Object, _DbContext);
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(_ServiceProvider.Object, _DbContext);
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(_ServiceProvider.Object, _DbContext);
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(_DbContext, _ServiceProvider.Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, deliveryOrderDetailDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);

            UnitReceiptNoteFacade unitReceiptNoteFacade = new UnitReceiptNoteFacade(_ServiceProvider.Object, _DbContext);
            UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil = new UnitReceiptNoteItemDataUtil();
            UnitReceiptNoteDataUtil unitReceiptNoteDataUtil = new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, unitReceiptNoteFacade, deliveryOrderDataUtil);

            return new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, facade, deliveryOrderDataUtil);
        }

        [Fact]
        public void Should_Success_Get_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var Response = facade.Read();
            Assert.NotEqual(Response.Data.Count, 0);
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var Response = facade.ReadById((int)dataUtil.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public void Should_Success_Create_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var model = _dataUtil(facade, dbContext).GetNewData(USERNAME).Result;
            model.IsStorage = true;
            var response = facade.Create(model, USERNAME).Result;
            Assert.NotEqual(response, 0);
        }

        [Fact]
        public void Should_Success_Update_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var response = facade.Update((int)dataUtil.Id, dataUtil, dataUtil.CreatedBy).Result;
            Assert.NotEqual(response, 0);
        }

        [Fact]
        public async Task Should_Success_Delete_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var response = await facade.Delete((int)dataUtil.Id, dataUtil.CreatedBy);
            Assert.NotEqual(response, 0);
        }

        [Fact]
        public void Should_Success_Read_DataBySupplier()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var filter = JsonConvert.SerializeObject(new
            {
                DivisionId = "DivisionId",
                SupplierId = "supId",
                PaymentMethod = "test",
                CurrencyCode = "CurrencyCode",
                UseIncomeTax = true,
                UseVat = false,
                CategoryId = "CategoryId"
            });

            var response = facade.ReadBySupplierUnit(Filter: filter);

            Assert.NotEqual(response.Data.Count, 0);
        }

        [Fact]
        public void Should_Success_GetReport()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var response = facade.GetReport(dataUtil.URNNo, "", dataUtil.UnitId, "", dataUtil.SupplierId, null, null, 1, 25, "{}", 1);
            Assert.NotEqual(response.Data.Count, 0);
        }

        [Fact]
        public void Should_Success_GenerateExcel()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var response = facade.GenerateExcel(dataUtil.URNNo, "", dataUtil.UnitId, "", dataUtil.SupplierId, null, null, 1);
            Assert.NotNull(response);
        }

        [Fact]
        public void Should_Success_Get_By_No()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            UnitReceiptNoteFacade facade = new UnitReceiptNoteFacade(_ServiceProvider.Object, dbContext);
            var dataUtil = _dataUtil(facade, dbContext).GetTestData(USERNAME).Result;
            var response = facade.ReadByNoFiltered(1, 25, "{}", null, "{ no : ['" + dataUtil.URNNo + "']}");
            Assert.NotNull(response);
        }

        //[Fact]
        //public async void Should_Success_Update_Data()
        //{
        //    UnitPaymentOrderFacade facade = new UnitPaymentOrderFacade(_dbContext(GetCurrentMethod()));
        //    var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();

        //    var modelItem = _dataUtil(facade, GetCurrentMethod()).GetNewData().Items.First();
        //    //model.Items.Clear();
        //    model.Items.Add(modelItem);
        //    var ResponseAdd = await facade.Update((int)model.Id, model, USERNAME);
        //    Assert.NotEqual(ResponseAdd, 0);
        //}

        //[Fact]
        //public async void Should_Success_Delete_Data()
        //{
        //    UnitPaymentOrderFacade facade = new UnitPaymentOrderFacade(_dbContext(GetCurrentMethod()));
        //    var Data = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
        //    int Deleted = await facade.Delete((int)Data.Id, USERNAME);
        //    Assert.True(Deleted > 0);
        //}

        //[Fact]
        //public void Should_Success_Validate_Data()
        //{
        //    UnitPaymentOrderViewModel nullViewModel = new UnitPaymentOrderViewModel();
        //    Assert.True(nullViewModel.Validate(null).Count() > 0);

        //    UnitPaymentOrderViewModel viewModel = new UnitPaymentOrderViewModel()
        //    {
        //        useIncomeTax = true,
        //        useVat = true,
        //        items = new List<UnitPaymentOrderItemViewModel>
        //        {
        //            new UnitPaymentOrderItemViewModel(),
        //            new UnitPaymentOrderItemViewModel()
        //            {
        //                unitReceiptNote = new UnitReceiptNote
        //                {
        //                    _id = 1
        //                }
        //            },
        //            new UnitPaymentOrderItemViewModel()
        //            {
        //                unitReceiptNote = new UnitReceiptNote
        //                {
        //                    _id = 1
        //                }
        //            }
        //        }
        //    };
        //    Assert.True(viewModel.Validate(null).Count() > 0);
        //}

        //[Fact]
        //public async void Should_Success_Get_Data_Spb()
        //{
        //    UnitPaymentOrderFacade facade = new UnitPaymentOrderFacade(_dbContext(GetCurrentMethod()));
        //    await _dataUtil(facade, GetCurrentMethod()).GetTestData();
        //    var Response = facade.ReadSpb();
        //    Assert.NotEqual(Response.Item1.Count, 0);
        //}
    }
}
