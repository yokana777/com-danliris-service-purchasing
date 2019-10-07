using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentBeacukaiFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDailyPurchasingReportFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentBeacukaiDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentDeliveryOrderTests
{
    public class MonitoringTest
    {
        private const string ENTITY = "GarmentDeliveryOrderMonitoring";

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

        private Mock<IServiceProvider> GetServiceProviderError()
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            message.Content = null;
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

        private GarmentDeliveryOrderDataUtil dataUtil(GarmentDeliveryOrderFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(ServiceProvider, _dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            return new GarmentDeliveryOrderDataUtil(facade, garmentExternalPurchaseOrderDataUtil);
        }

        [Fact]
        public async Task Should_Success_Get_Report_AccuracyArrival()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data.DODate = DateTimeOffset.Now.AddDays(-35);
            foreach (var item in data.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "LBL";
                }
            }
            await facade.Create(data, USERNAME);

            var Facade = new GarmentDeliveryOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var Response = Facade.GetReportHeaderAccuracyofArrival(null, null, null, 7);
            Assert.NotNull(Response.Item1);

            var data2 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data2.DODate = DateTimeOffset.Now.AddDays(-35);
            foreach (var item in data2.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data2, USERNAME);

            var data3 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data3.DODate = DateTimeOffset.Now.AddDays(-34);
            foreach (var item in data3.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data3, USERNAME);

            var data4 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data4.DODate = DateTimeOffset.Now.AddDays(-33);
            foreach (var item in data4.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "LBL";
                }
            }
            await facade.Create(data4, USERNAME);

            var Response1 = Facade.GetReportHeaderAccuracyofArrival(null, null, null, 7);
            Assert.NotNull(Response1.Item1);

            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            AccuracyOfArrivalReportViewModel viewModelAccuracy = new AccuracyOfArrivalReportViewModel
            {
                supplier = new SupplierViewModel(),
                product = new GarmentProductViewModel(),
            };
            viewModelAccuracy.Id = 1;
            viewModelAccuracy.doNo = data.DONo;
            viewModelAccuracy.supplier.Id = data.SupplierId;
            viewModelAccuracy.supplier.Code = data.SupplierCode;
            viewModelAccuracy.supplier.Name = data.SupplierName;
            viewModelAccuracy.doDate = data.DODate;
            viewModelAccuracy.poSerialNumber = nowTicksA;
            viewModelAccuracy.product.Id = nowTicks;
            viewModelAccuracy.product.Code = nowTicksA;
            viewModelAccuracy.prDate = DateTimeOffset.Now;
            viewModelAccuracy.poDate = DateTimeOffset.Now;
            viewModelAccuracy.epoDate = DateTimeOffset.Now;
            viewModelAccuracy.article = nowTicksA;
            viewModelAccuracy.roNo = nowTicksA;
            viewModelAccuracy.shipmentDate = DateTimeOffset.Now;
            viewModelAccuracy.status = nowTicksA;
            viewModelAccuracy.staff = nowTicksA;
            viewModelAccuracy.category = nowTicksA;
            viewModelAccuracy.dateDiff = (int)nowTicks;
            viewModelAccuracy.ok_notOk = nowTicksA;
            viewModelAccuracy.percentOk_notOk = (int)nowTicks;
            viewModelAccuracy.jumlahOk = (int)nowTicks;
            viewModelAccuracy.jumlah = (int)nowTicks;

            var Response2 = Facade.GetReportDetailAccuracyofArrival($"BuyerCode{nowTicksA}", null, null, null, 7);
            Assert.NotNull(Response2.Item1);

            var Response3 = Facade.GetReportDetailAccuracyofArrival($"BuyerCode{nowTicksA}", "Bahan Pendukung", null, null, 7);
            Assert.NotNull(Response3.Item1);
        }

        [Fact]
        public async Task Should_Success_Get_Report_AccuracyArrival_Excel()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            await facade.Create(data, USERNAME);

            var data2 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data2.DODate = DateTimeOffset.Now.AddDays(-35);
            foreach (var item in data2.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data2, USERNAME);

            var data3 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data3.DODate = DateTimeOffset.Now.AddDays(-34);
            foreach (var item in data3.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data3, USERNAME);

            var data4 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data4.DODate = DateTimeOffset.Now.AddDays(-33);
            foreach (var item in data4.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "LBL";
                }
            }
            await facade.Create(data4, USERNAME);

            var Facade = new GarmentDeliveryOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var Response = Facade.GenerateExcelArrivalHeader(null, null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response);

            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            var Response1 = Facade.GenerateExcelArrivalDetail($"BuyerCode{nowTicksA}", null, null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response1);
        }


        [Fact]
        public async Task Should_Success_Get_Report_AccuracyDelivery()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data.DODate = DateTimeOffset.Now.AddDays(10);
            foreach (var item in data.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "LBL";
                }
            }
            await facade.Create(data, USERNAME);

            var Facade = new GarmentDeliveryOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var Response = Facade.GetReportHeaderAccuracyofDelivery(null, null, 7);
            Assert.NotNull(Response.Item1);

            var data2 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data2.DODate = DateTimeOffset.Now.AddDays(10);
            foreach (var item in data2.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data2, USERNAME);

            var data3 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data3.DODate = DateTimeOffset.Now.AddDays(10);
            foreach (var item in data3.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data3, USERNAME);

            var data4 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data4.DODate = DateTimeOffset.Now.AddDays(11);
            foreach (var item in data4.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "LBL";
                }
            }
            await facade.Create(data4, USERNAME);

            var Response1 = Facade.GetReportHeaderAccuracyofDelivery(null, null, 7);
            Assert.NotNull(Response1.Item1);

            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            var Response2 = Facade.GetReportDetailAccuracyofDelivery($"BuyerCode{nowTicksA}", null, null, 7);
            Assert.NotNull(Response2.Item1);
        }

        [Fact]
        public async Task Should_Success_Get_Report_AccuracyDelivery_Excel()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            await facade.Create(data, USERNAME);

            var data2 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data2.DODate = DateTimeOffset.Now.AddDays(10);
            foreach (var item in data2.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data2, USERNAME);

            var data3 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data3.DODate = DateTimeOffset.Now.AddDays(10);
            foreach (var item in data3.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "SUB";
                }
            }
            await facade.Create(data3, USERNAME);

            var data4 = await dataUtil(facade, GetCurrentMethod()).GetNewData3();
            data4.DODate = DateTimeOffset.Now.AddDays(10);
            foreach (var item in data4.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductCode = "LBL";
                }
            }
            await facade.Create(data4, USERNAME);

            var Facade = new GarmentDeliveryOrderFacade(ServiceProvider, _dbContext(GetCurrentMethod()));
            var Response = Facade.GenerateExcelDeliveryHeader(null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response);

            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            var Response1 = Facade.GenerateExcelDeliveryDetail($"BuyerCode{nowTicksA}", null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response1);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.GetReportDO(model.DONo, "", model.SupplierId, null, null, 1, 25, "{}", 7);
            Assert.NotEqual(-1, Response.Item2);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Null_Parameter()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.GetReportDO("", "", 0, null, null, 1, 25, "{}", 7);
            Assert.NotEmpty(Response.Item1);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.GenerateExcelDO(model.DONo, "", model.SupplierId, null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Report_Data_Excel_Null_parameter()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.GenerateExcelDO("", "", 0, null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response);
        }
        // Buku Harian Pembelian
        [Fact]
        public async Task Should_Success_Get_Buku_Sub_Beli_Data()
        {
            GarmentDeliveryOrderFacade facadeDO = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var datautilDO = dataUtil(facadeDO, GetCurrentMethod());
            var garmentDeliveryOrder = await Task.Run(() => datautilDO.GetNewData("User"));

            var garmentBeaCukaiFacade = new GarmentBeacukaiFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var datautilBC = new GarmentBeacukaiDataUtil(datautilDO, garmentBeaCukaiFacade);

            GarmentDailyPurchasingReportFacade DataSJ = new GarmentDailyPurchasingReportFacade(ServiceProvider, _dbContext(GetCurrentMethod()));

            var dataDO = await datautilDO.GetTestData();
            var dataBC = await datautilBC.GetTestData(USERNAME, dataDO);

            DateTime d1 = dataBC.BeacukaiDate.DateTime;
            DateTime d2 = dataBC.BeacukaiDate.DateTime;

            var Response = DataSJ.GetGDailyPurchasingReport(null, true, null, null, null, 7);
            Assert.NotNull(Response.Item1);
            Assert.NotEqual(-1, Response.Item2);
        }

        [Fact]
        public async Task Should_Success_Get_Buku_Sub_Beli_Null_Parameter()
        {
            GarmentDeliveryOrderFacade facadeDO = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var datautilDO = dataUtil(facadeDO, GetCurrentMethod());
            var garmentDeliveryOrder = await Task.Run(() => datautilDO.GetNewData("User"));

            var garmentBeaCukaiFacade = new GarmentBeacukaiFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var datautilBC = new GarmentBeacukaiDataUtil(datautilDO, garmentBeaCukaiFacade);

            GarmentDailyPurchasingReportFacade DataSJ = new GarmentDailyPurchasingReportFacade(ServiceProvider, _dbContext(GetCurrentMethod()));

            var dataDO = await datautilDO.GetTestData();
            var dataBC = await datautilBC.GetTestData(USERNAME, dataDO);

            DateTime d1 = dataBC.BeacukaiDate.DateTime.AddDays(30);
            DateTime d2 = dataBC.BeacukaiDate.DateTime.AddDays(30);

            var Response = DataSJ.GetGDailyPurchasingReport(null, true, null, null, null, 7);
            Assert.NotNull(Response.Item1);
            Assert.NotEqual(-1, Response.Item2);
        }

        [Fact]
        public async Task Should_Success_Get_Buku_Sub_Beli_Excel()
        {
            GarmentDeliveryOrderFacade facadeDO = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var datautilDO = dataUtil(facadeDO, GetCurrentMethod());
            var garmentDeliveryOrder = await Task.Run(() => datautilDO.GetNewData("User"));

            var garmentBeaCukaiFacade = new GarmentBeacukaiFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var datautilBC = new GarmentBeacukaiDataUtil(datautilDO, garmentBeaCukaiFacade);

            GarmentDailyPurchasingReportFacade DataSJ = new GarmentDailyPurchasingReportFacade(ServiceProvider, _dbContext(GetCurrentMethod()));

            var dataDO = await datautilDO.GetTestData();
            var dataBC = await datautilBC.GetTestData(USERNAME, dataDO);

            DateTime d1 = dataBC.BeacukaiDate.DateTime;
            DateTime d2 = dataBC.BeacukaiDate.DateTime;

            var Response = DataSJ.GenerateExcelGDailyPurchasingReport(null, true, null, null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response);
        }

        [Fact]
        public async Task Should_Success_Get_Buku_Sub_Beli_Excel_Null_Parameter()
        {
            GarmentDeliveryOrderFacade facadeDO = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var datautilDO = dataUtil(facadeDO, GetCurrentMethod());
            var garmentDeliveryOrder = await Task.Run(() => datautilDO.GetNewData("User"));

            var garmentBeaCukaiFacade = new GarmentBeacukaiFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
            var datautilBC = new GarmentBeacukaiDataUtil(datautilDO, garmentBeaCukaiFacade);

            GarmentDailyPurchasingReportFacade DataSJ = new GarmentDailyPurchasingReportFacade(ServiceProvider, _dbContext(GetCurrentMethod()));

            var dataDO = await datautilDO.GetTestData();
            var dataBC = await datautilBC.GetTestData(USERNAME, dataDO);

            DateTime d1 = dataBC.BeacukaiDate.DateTime.AddDays(30);
            DateTime d2 = dataBC.BeacukaiDate.DateTime.AddDays(30);

            var Response = DataSJ.GenerateExcelGDailyPurchasingReport(null, true, null, null, null, 7);
            Assert.IsType<System.IO.MemoryStream>(Response);
        }

    }
}
