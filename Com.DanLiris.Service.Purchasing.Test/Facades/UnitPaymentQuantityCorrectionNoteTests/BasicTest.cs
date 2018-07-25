using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentCorrectionNoteDataUtils;
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

namespace Com.DanLiris.Service.Purchasing.Test.Facades.UnitPaymentQuantityCorrectionNoteTests
{
    public class BasicTest
    {
        private const string ENTITY = "UnitPaymentCorrectionNote";

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

        private UnitPaymentCorrectionNoteDataUtil _dataUtil(UnitPaymentQuantityCorrectionNoteFacade facade, string testName)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            PurchaseRequestFacade purchaseRequestFacade = new PurchaseRequestFacade(serviceProvider.Object, _dbContext(testName));
            PurchaseRequestItemDataUtil purchaseRequestItemDataUtil = new PurchaseRequestItemDataUtil();
            PurchaseRequestDataUtil purchaseRequestDataUtil = new PurchaseRequestDataUtil(purchaseRequestItemDataUtil, purchaseRequestFacade);

            InternalPurchaseOrderFacade internalPurchaseOrderFacade = new InternalPurchaseOrderFacade(serviceProvider.Object, _dbContext(testName));
            InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil = new InternalPurchaseOrderItemDataUtil();
            InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil = new InternalPurchaseOrderDataUtil(internalPurchaseOrderItemDataUtil, internalPurchaseOrderFacade, purchaseRequestDataUtil);

            ExternalPurchaseOrderFacade externalPurchaseOrderFacade = new ExternalPurchaseOrderFacade(serviceProvider.Object, _dbContext(testName));
            ExternalPurchaseOrderDetailDataUtil externalPurchaseOrderDetailDataUtil = new ExternalPurchaseOrderDetailDataUtil();
            ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil = new ExternalPurchaseOrderItemDataUtil(externalPurchaseOrderDetailDataUtil);
            ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil = new ExternalPurchaseOrderDataUtil(externalPurchaseOrderFacade, internalPurchaseOrderDataUtil, externalPurchaseOrderItemDataUtil);

            DeliveryOrderFacade deliveryOrderFacade = new DeliveryOrderFacade(_dbContext(testName), serviceProvider.Object);
            DeliveryOrderDetailDataUtil deliveryOrderDetailDataUtil = new DeliveryOrderDetailDataUtil();
            DeliveryOrderItemDataUtil deliveryOrderItemDataUtil = new DeliveryOrderItemDataUtil(deliveryOrderDetailDataUtil);
            DeliveryOrderDataUtil deliveryOrderDataUtil = new DeliveryOrderDataUtil(deliveryOrderItemDataUtil, externalPurchaseOrderDataUtil, deliveryOrderFacade);

            UnitReceiptNoteFacade unitReceiptNoteFacade = new UnitReceiptNoteFacade(serviceProvider.Object, _dbContext(testName));
            UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil = new UnitReceiptNoteItemDataUtil();
            UnitReceiptNoteDataUtil unitReceiptNoteDataUtil = new UnitReceiptNoteDataUtil(unitReceiptNoteItemDataUtil, unitReceiptNoteFacade, deliveryOrderDataUtil);

            UnitPaymentOrderFacade unitPaymentOrderFacade = new UnitPaymentOrderFacade(_dbContext(testName));
            UnitPaymentOrderDataUtil unitPaymentOrderDataUtil = new UnitPaymentOrderDataUtil(unitReceiptNoteDataUtil, unitPaymentOrderFacade);

            return new UnitPaymentCorrectionNoteDataUtil(unitPaymentOrderDataUtil, facade);
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentQuantityCorrectionNoteFacade facade = new UnitPaymentQuantityCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentQuantityCorrectionNoteFacade facade = new UnitPaymentQuantityCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            UnitPaymentQuantityCorrectionNoteFacade facade = new UnitPaymentQuantityCorrectionNoteFacade(serviceProvider.Object, _dbContext(GetCurrentMethod()));
            var modelLocalSupplier = _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, USERNAME, 7);
            Assert.NotEqual(ResponseLocalSupplier, 0);

            var modelImportSupplier = _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseImportSupplier = await facade.Create(modelImportSupplier, USERNAME, 7);
            Assert.NotEqual(ResponseImportSupplier, 0);
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

        [Fact]
        public void Should_Success_Validate_Data()
        {
            UnitPaymentCorrectionNoteViewModel nullViewModel = new UnitPaymentCorrectionNoteViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            UnitPaymentCorrectionNoteViewModel viewModel = new UnitPaymentCorrectionNoteViewModel()
            {
                useIncomeTax = true,
                useVat = true,
                items = new List<UnitPaymentCorrectionNoteItemViewModel>
                {
                    new UnitPaymentCorrectionNoteItemViewModel()
                }
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }
    }
}
