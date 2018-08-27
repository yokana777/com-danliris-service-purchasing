using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentCorrectionNoteDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.InternalPurchaseOrderTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class POMonitoringAllTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public POMonitoringAllTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = "Unit Test";
        }

        private InternalPurchaseOrderDataUtil DataUtil
        {
            get { return (InternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(InternalPurchaseOrderDataUtil)); }
        }

        private UnitPaymentPriceCorrectionNoteDataUtils DataCorrectionUtil
        {
            get { return (UnitPaymentPriceCorrectionNoteDataUtils)ServiceProvider.GetService(typeof(UnitPaymentPriceCorrectionNoteDataUtils)); }
        }

        private PurchaseOrderMonitoringAllFacade Facade
        {
            get { return (PurchaseOrderMonitoringAllFacade)ServiceProvider.GetService(typeof(PurchaseOrderMonitoringAllFacade)); }
        }

        [Fact]
        public async void Should_Success_Get_Report_Data()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport(model.PRNo, null, model.UnitId, model.CategoryId, null , null ,model.CreatedBy, null, null,null, 1, 25, "{}", 7,"");
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_All_Null_Parameter()
        {
            UnitPaymentCorrectionNote model = await DataCorrectionUtil.GetTestData();
            var Response = Facade.GetReport("", null, null, null, null, null, null, null, null, null, 1, 25, "{}", 7, "");

            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Null_Parameter()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GetReport("", null, null, null, null,null,null,null,null,null, 1, 25, "{}", 7,"");
            Assert.NotEqual(Response.Item2, 0);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel(model.PRNo, null, model.UnitId, model.CategoryId, null, null, model.CreatedBy, null, null, null, 7,"");
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Excel_Null_Parameter()
        {
            InternalPurchaseOrder model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.GenerateExcel("", "0", null, null, null, null, null, null, null, null, 7,"");
            Assert.IsType(typeof(System.IO.MemoryStream), Response);
        }
    }
}
