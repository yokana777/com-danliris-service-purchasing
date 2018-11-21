using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.PurchasingDispositionFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchasingDispositionViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchasingDispositionDataUtils;
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

namespace Com.DanLiris.Service.Purchasing.Test.Facades.PurchasingDispositionTests
{
    public class BasicTests
    {
        private const string ENTITY = "PurchasingDisposition";

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

        private PurchasingDispositionDataUtil _dataUtil(PurchasingDispositionFacade facade, string testName)
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

            
            return new PurchasingDispositionDataUtil(facade, externalPurchaseOrderDataUtil);
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            PurchasingDispositionFacade facade = new PurchasingDispositionFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            PurchasingDispositionFacade facade = new PurchasingDispositionFacade(ServiceProvider,_dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadModelById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            PurchasingDispositionFacade facade = new PurchasingDispositionFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var modelLocalSupplier = _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseLocalSupplier = await facade.Create(modelLocalSupplier, USERNAME,7);
            Assert.NotEqual(ResponseLocalSupplier, 0);

            var modelImportSupplier = _dataUtil(facade, GetCurrentMethod()).GetNewData();
            var ResponseImportSupplier = await facade.Create(modelImportSupplier, USERNAME, 7);
            Assert.NotEqual(ResponseImportSupplier, 0);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            PurchasingDispositionFacade facade = new PurchasingDispositionFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();

            var modelItem = _dataUtil(facade, GetCurrentMethod()).GetNewData().Items.First();
            var modelDetail = modelItem.Details.First();
            //model.Items.Clear();
            modelItem.EPONo = "test";
            var ResponseAdd1 = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAdd1, 0);

            var dispoItem =
                    new PurchasingDispositionItem
                    {
                        EPOId = modelItem.EPOId,
                        EPONo = modelItem.EPONo,
                        IncomeTaxId = "1",
                        IncomeTaxName = "tax",
                        IncomeTaxRate = 1,
                        UseIncomeTax = true,
                        UseVat = true,
                        Details = new List<PurchasingDispositionDetail>
                       {
                            new PurchasingDispositionDetail
                            {
                                EPODetailId=modelDetail.EPODetailId,
                                CategoryCode="test",
                                CategoryId="1",
                                CategoryName="test",
                                DealQuantity=10,
                                PaidQuantity=1000,
                                DealUomId="1",
                                DealUomUnit="test",
                                PaidPrice=1000,
                                PricePerDealUnit=100,
                                PriceTotal=10000,
                                PRId="1",
                                PRNo="test",
                                ProductCode="test",
                                ProductName="test",
                                ProductId="1",
                                UnitName = "test",
                                UnitCode = "test",
                                UnitId = "1",

                            }
                       }
                    };
            var dispoDetail = new PurchasingDispositionDetail
            {
                EPODetailId = modelDetail.EPODetailId,
                CategoryCode = "test",
                CategoryId = "1",
                CategoryName = "test",
                DealQuantity = 10,
                PaidQuantity = 1000,
                DealUomId = "1",
                DealUomUnit = "test",
                PaidPrice = 1000,
                PricePerDealUnit = 100,
                PriceTotal = 10000,
                PRId = "1",
                PRNo = "test",
                ProductCode = "test",
                ProductName = "test",
                ProductId = "1",

            };

            model.Items.First().Details.Add(dispoDetail);
            var ResponseAddDetail = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAddDetail, 0);

            var ResponseAddDetail2 = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAddDetail2, 0);

            model.Items.First().Details.Remove(modelDetail);
            var ResponseAddDetail1 = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAddDetail1, 0);

            model.Items.Add(dispoItem);
            var ResponseAdd = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAdd, 0);


            model.Items.Remove(modelItem);
            var ResponseAdd2 = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAdd2, 0);

        }

        [Fact]
        public async void Should_Error_Update_Data()
        {
            PurchasingDispositionFacade facade = new PurchasingDispositionFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();

            Exception errorInvalidId = await Assert.ThrowsAsync<Exception>(async () => await facade.Update(0, model, USERNAME));
            Assert.NotNull(errorInvalidId.Message);

            model.Items = null;
            Exception errorNullItems = await Assert.ThrowsAsync<Exception>(async () => await facade.Update((int)model.Id, model, USERNAME));
            Assert.NotNull(errorNullItems.Message);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            PurchasingDispositionFacade facade = new PurchasingDispositionFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var Data = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            int Deleted =  facade.Delete((int)Data.Id, USERNAME);
            Assert.True(Deleted > 0);
        }

        [Fact]
        public async void Should_Error_Delete_Data()
        {
            PurchasingDispositionFacade facade = new PurchasingDispositionFacade(ServiceProvider, _dbContext(GetCurrentMethod()));

            Exception e = await Assert.ThrowsAsync<Exception>(async () => facade.Delete(0, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public void Should_Success_Validate_Data()
        {
            PurchasingDispositionViewModel nullViewModel = new PurchasingDispositionViewModel();
            nullViewModel.Items = new List<PurchasingDispositionItemViewModel>();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            PurchasingDispositionViewModel viewModel = new PurchasingDispositionViewModel()
            {
                Currency = null,
                Supplier = null,
                Items = new List<PurchasingDispositionItemViewModel>
                {
                    new PurchasingDispositionItemViewModel(),
                    new PurchasingDispositionItemViewModel()
                    {
                        EPONo="testEpo",
                        Details=new List<PurchasingDispositionDetailViewModel>()
                    },
                    new PurchasingDispositionItemViewModel()
                    {
                        EPONo="testEpo",
                        Details=new List<PurchasingDispositionDetailViewModel>
                        {
                            new PurchasingDispositionDetailViewModel()
                            {
                                PaidPrice=0,
                                PaidQuantity=0
                            }
                        }
                    },
                    new PurchasingDispositionItemViewModel()
                    {
                        EPONo="testEpo1",
                        Details=new List<PurchasingDispositionDetailViewModel>()
                    }
                }
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_DispositonNo()
        {
            var facade = new PurchasingDispositionFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var model = await _dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadByDisposition(model.DispositionNo);
            Assert.NotNull(Response);
        }
    }
}
