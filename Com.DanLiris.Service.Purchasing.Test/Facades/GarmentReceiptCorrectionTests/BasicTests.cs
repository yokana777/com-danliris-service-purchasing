using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReceiptCorrectionFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReceiptCorrectionViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentReceiptCorrectionDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.NewIntegrationDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentReceiptCorrectionTests
{
    public class BasicTests
    {
        private const string ENTITY = "GarmentReceiptCorrection";
        private const string USERNAME = "unitTest";
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

        private IServiceProvider GetServiceProvider()
        {
            var httpClientService = new Mock<IHttpClientService>();
            httpClientService
                .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("master/garment-suppliers"))))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new SupplierDataUtil().GetResultFormatterOkString()) });
            httpClientService
                .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("master/garment-currencies"))))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new CurrencyDataUtil().GetMultipleResultFormatterOkString()) });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService { Username = "Username", TimezoneOffset = 7 });
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(httpClientService.Object);

            return serviceProviderMock.Object;
        }

        private GarmentReceiptCorrectionDataUtil dataUtil(GarmentReceiptCorrectionFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(GetServiceProvider(), _dbContext(testName));
            var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

            var garmentUnitReceiptNoteFacade = new GarmentUnitReceiptNoteFacade(GetServiceProvider(), _dbContext(testName));
            var garmentUnitReceiptNoteDataUtil = new GarmentUnitReceiptNoteDataUtil(garmentUnitReceiptNoteFacade, garmentDeliveryOrderDataUtil);

            return new GarmentReceiptCorrectionDataUtil(facade, garmentUnitReceiptNoteDataUtil);
        }

        [Fact]
        public async Task Should_Success_Get_All_Data()
        {
            var facade = new GarmentReceiptCorrectionFacade(_dbContext(GetCurrentMethod()),GetServiceProvider());
            var data = await dataUtil(facade, GetCurrentMethod()).GetTestDataKoreksiJumlahPlus();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            var facade = new GarmentReceiptCorrectionFacade(_dbContext(GetCurrentMethod()), GetServiceProvider());
            var data = await dataUtil(facade, GetCurrentMethod()).GetTestDataKoreksiJumlahMinus();
            var Response = facade.ReadById((int)data.Id);
            Assert.NotEqual(Response.Id, 0);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            var facade = new GarmentReceiptCorrectionFacade(_dbContext(GetCurrentMethod()), GetServiceProvider());
            var data = await dataUtil(facade, GetCurrentMethod()).GetNewDataKoreksiKonversi();
            var Response = await facade.Create(data, USERNAME);
            Assert.NotEqual(Response, 0);

            var data2 = await dataUtil(facade, GetCurrentMethod()).GetNewDataKoreksiKonversi();
            var dataItem = data.Items.First();
            long nowTicks = DateTimeOffset.Now.Ticks;
            data2.Items.First().ProductId = nowTicks;
            data2.Items.First().SmallUomId = dataItem.SmallUomId;
            data2.StorageId = data.StorageId;
            var Response2 = await facade.Create(data2, USERNAME);
            Assert.NotEqual(Response2, 0);

            var data3 = await dataUtil(facade, GetCurrentMethod()).GetNewDataKoreksiJumlahPlus();
            var dataItem1 = data.Items.First();
            data3.Items.First().ProductId = nowTicks;
            data3.Items.First().SmallUomId = dataItem.SmallUomId;
            data3.StorageId = nowTicks;
            var Response3 = await facade.Create(data3, USERNAME);
            Assert.NotEqual(Response2, 0);
        }

        //[Fact]
        //public async Task Should_Success_Create_Data_With_Tax()
        //{
        //    var facade = new GarmentReceiptCorrectionFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
        //    var data = await dataUtil(facade, GetCurrentMethod()).GetNewDataWithTax();
        //    var Response = await facade.Create(data, false, USERNAME);
        //    Assert.NotEqual(Response, 0);

        //    var data2nd = await dataUtil(facade, GetCurrentMethod()).GetNewDataWithTax();
        //    var Response2nd = await facade.Create(data2nd, false, USERNAME);
        //    Assert.NotEqual(Response2nd, 0);
        //}

        [Fact]
        public async Task Should_Error_Create_Data_Null_Items()
        {
            var facade = new GarmentReceiptCorrectionFacade(_dbContext(GetCurrentMethod()), GetServiceProvider());
            var data = await dataUtil(facade, GetCurrentMethod()).GetNewDataKoreksiJumlahMinus();
            data.Items = null;
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(data, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public void Should_Success_Validate_Data()
        {
            GarmentReceiptCorrectionViewModel AllNullViewModel = new GarmentReceiptCorrectionViewModel();
            Assert.True(AllNullViewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data_Null_Items()
        {
            GarmentReceiptCorrectionViewModel viewModel = new GarmentReceiptCorrectionViewModel
            {
                CorrectionType = "Harga Satuan",
                URNNo = "test",
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }


        [Fact]
        public void Should_Success_Validate_Data_Retur()
        {
            GarmentReceiptCorrectionViewModel viewModel = new GarmentReceiptCorrectionViewModel
            {
                CorrectionType = "Jumlah",
                URNNo = "test",
                CorrectionDate=DateTimeOffset.Now,
                CorrectionNo="test",
                Remark=It.IsAny<string>(),
                Unit= It.IsAny<UnitViewModel>(),
                Storage=It.IsAny<Lib.ViewModels.IntegrationViewModel.StorageViewModel>(),
                URNId= It.IsAny<long>(),
                Items = new List<GarmentReceiptCorrectionItemViewModel>
                {
                    new GarmentReceiptCorrectionItemViewModel
                    {
                        QuantityCheck=100,
                        Quantity = 0,
                        CorrectionQuantity=0,
                        Conversion=It.IsAny<double>(),
                        CorrectionConversion=It.IsAny<double>(),
                        DODetailId=It.IsAny<long>(),
                        EPOItemId=It.IsAny<long>(),
                        FabricType=It.IsAny<string>(),
                        IsSave=true,
                        PRItemId=It.IsAny<long>(),
                        RONo=It.IsAny<string>(),
                        POItemId=It.IsAny<long>(),
                        DesignColor=It.IsAny<string>(),
                        PricePerDealUnit=It.IsAny<double>(),
                        POSerialNumber=It.IsAny<string>(),
                        SmallQuantity=It.IsAny<double>(),
                        URNItemId=It.IsAny<long>(),
                        SmallUomId=It.IsAny<long>(),
                        UomId=It.IsAny<long>(),
                        UomUnit=It.IsAny<string>(),
                        SmallUomUnit=It.IsAny<string>(),
                        ProductRemark=It.IsAny<string>(),
                        Product=It.IsAny<ProductViewModel>(),
                    },
                }
            };
            Assert.True(viewModel.Validate(null).Count() > 0);

            GarmentReceiptCorrectionViewModel viewModel2 = new GarmentReceiptCorrectionViewModel
            {
                CorrectionType = "Konversi",
                URNNo = "test",
                Items = new List<GarmentReceiptCorrectionItemViewModel>
                {
                    new GarmentReceiptCorrectionItemViewModel
                    {
                        QuantityCheck=100,
                        Quantity = 500,
                        CorrectionQuantity=0,
                        CorrectionConversion=0,
                        IsSave=true,
                    },
                }
            };
            Assert.True(viewModel2.Validate(null).Count() > 0);

            GarmentReceiptCorrectionViewModel viewModel3 = new GarmentReceiptCorrectionViewModel
            {
                CorrectionType = "Jumlah",
                URNNo = "test",
                Items = new List<GarmentReceiptCorrectionItemViewModel>
                {
                    new GarmentReceiptCorrectionItemViewModel
                    {
                        QuantityCheck=100,
                        Quantity = 500,
                        CorrectionQuantity=-200,
                        IsSave=true,

                    },
                }
            };
            Assert.True(viewModel3.Validate(null).Count() > 0);
        }
    }
}
