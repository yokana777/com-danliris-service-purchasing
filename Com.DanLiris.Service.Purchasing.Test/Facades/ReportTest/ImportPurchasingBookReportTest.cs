using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ReportTest
{
    [Collection("ServiceProviderFixture Collection")]
    public class ImportPurchasingBookReportTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public ImportPurchasingBookReportTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;
        }

        private UnitReceiptNoteDataUtil DataUtil
        {
            get { return (UnitReceiptNoteDataUtil)ServiceProvider.GetService(typeof(UnitReceiptNoteDataUtil)); }
        }
		private ImportPurchasingBookReportFacade IPRFacade
		{
			get { return (ImportPurchasingBookReportFacade)ServiceProvider.GetService(typeof(ImportPurchasingBookReportFacade)); }
		}
		private UnitReceiptNoteFacade Facade
        {
            get { return (UnitReceiptNoteFacade)ServiceProvider.GetService(typeof(UnitReceiptNoteFacade)); }
        }
		private ExternalPurchaseOrderDataUtil EPODataUtil
		{
			get { return (ExternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
		}
		private ExternalPurchaseOrderFacade EPOFacade
		{
			get { return (ExternalPurchaseOrderFacade)ServiceProvider.GetService(typeof(ExternalPurchaseOrderFacade)); }
		}
		private DeliveryOrderDataUtil DODataUtil
		{
			get { return (DeliveryOrderDataUtil)ServiceProvider.GetService(typeof(DeliveryOrderDataUtil)); }
		}

		private DeliveryOrderFacade DOFacade
		{
			get { return (DeliveryOrderFacade)ServiceProvider.GetService(typeof(DeliveryOrderFacade)); }
		}
		[Fact]
		public async void Should_Success_Get_Report_Data()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
			await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
			DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
			await DOFacade.Create(deliveryOrder, "unit-test");
			UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
			await Facade.Create(urn, "unit-test");
			DateTime DateFrom = new DateTime(2018,8, 27);
			DateTime DateTo = new DateTime(2018, 8, 27);
			var Response = IPRFacade.GetReport(null, null,null ,DateFrom,DateTo);
			Assert.NotEqual(Response.Item2, 0);
		}
		//[Fact]
		//public async void Should_Success_Get_Report_Data_No_Parameter()
		//{
		//	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
		//	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
		//	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
		//	await DOFacade.Create(deliveryOrder, "unit-test");
		//	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
		//	await Facade.Create(urn, "unit-test");
		//	DateTime DateFrom = new DateTime(2018, 8, 27);
		//	DateTime DateTo = new DateTime(2018, 8, 27);
		//	var Response = IPRFacade.GetReport("18-08-BPI-001-unitcode-001", null, null, DateFrom, DateTo);
		//	Assert.NotEqual(Response.Item2, 0);
		//}
		//[Fact]
		//public async void Should_Success_Get_Report_Data_Unit_Parameter()
		//{
		//	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
		//	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
		//	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
		//	await DOFacade.Create(deliveryOrder, "unit-test");
		//	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
		//	await Facade.Create(urn, "unit-test");
		//	DateTime DateFrom = new DateTime(2018, 8, 27);
		//	DateTime DateTo = new DateTime(2018, 8, 27);
		//	var Response = IPRFacade.GetReport(null, "UnitName", null, DateFrom, DateTo);
		//	Assert.NotEqual(Response.Item2, 0);
		//}
		//[Fact]
		//public async void Should_Success_Get_Report_Data_Category_Parameter()
		//{
		//	ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
		//	await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
		//	DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
		//	await DOFacade.Create(deliveryOrder, "unit-test");
		//	UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
		//	await Facade.Create(urn, "unit-test");
		//	DateTime DateFrom = new DateTime(2018, 8, 27);
		//	DateTime DateTo = new DateTime(2018, 8, 27);
		//	var Response = IPRFacade.GetReport(null, null, "CategoryName", DateFrom, DateTo);
		//	Assert.NotEqual(Response.Item2, 0);
		//}

		[Fact]
		public async void Should_Success_Get_Report_Data_Excel_Null_Parameter()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
			await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
			DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
			await DOFacade.Create(deliveryOrder, "unit-test");
			UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
			await Facade.Create(urn, "unit-test");
			DateTime DateFrom = new DateTime(2018, 8, 27);
			DateTime DateTo = new DateTime(2018, 8, 27);
			var Response = IPRFacade.GenerateExcel(null,null,null,DateFrom,DateTo);
			Assert.IsType(typeof(System.IO.MemoryStream), Response);
		}
		[Fact]
		public async void Should_Success_Get_Report_Total_Purchase_By_Units_Null_Data_Excel()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await EPODataUtil.GetNewData("unit-test");
			await EPOFacade.Create(externalPurchaseOrder, "unit-test", 7);
			DeliveryOrder deliveryOrder = await DODataUtil.GetNewData("unit-test");
			await DOFacade.Create(deliveryOrder, "unit-test");
			UnitReceiptNote urn = await DataUtil.GetNewDatas("unit-test");
			await Facade.Create(urn, "unit-test");
			DateTime DateFrom = new DateTime(2018, 8, 28);
			DateTime DateTo = new DateTime(2018, 8, 28);
			var Response = IPRFacade.GenerateExcel(null, null, null, DateFrom, DateTo);
			Assert.IsType(typeof(System.IO.MemoryStream), Response);
		}
	}
}
