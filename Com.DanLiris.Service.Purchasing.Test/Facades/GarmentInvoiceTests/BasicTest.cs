using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
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

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentInvoiceTests
{

	public class BasicTest
	{
		private const string ENTITY = "GarmentInvoice";

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

		private GarmentInvoiceDataUtil dataUtil(GarmentInvoiceFacade facade, string testName)
		{
			var garmentInvoiceFacade = new GarmentInvoiceFacade(_dbContext(testName), ServiceProvider);
			var garmentInvoiceDetailDataUtil = new GarmentInvoiceDetailDataUtil();
			var garmentInvoiceItemDataUtil = new GarmentInvoiceItemDataUtil(garmentInvoiceDetailDataUtil);
			var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(_dbContext(testName));
			var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
			var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

			var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
			var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

			var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
			var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

			var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);
			return new GarmentInvoiceDataUtil(garmentInvoiceItemDataUtil,garmentInvoiceDetailDataUtil,garmentDeliveryOrderDataUtil,facade );
		}

		[Fact]
		public async void Should_Success_Create_Data()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			GarmentInvoice data = await dataUtil(facade, GetCurrentMethod()).GetNewData(USERNAME);
			var Response = await facade.Create(data, USERNAME);
			Assert.NotEqual(Response, 0);
		}

		[Fact]
		public async void Should_Error_Create_Data()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			GarmentInvoice model =await  dataUtil(facade, GetCurrentMethod()).GetNewData(USERNAME);
			model.Items = null;
			Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(model, USERNAME));
			Assert.NotNull(e.Message);
		}


		[Fact]
		public async void Should_Success_Get_All_Data()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			GarmentInvoice data = await dataUtil(facade, GetCurrentMethod()).GetNewData(USERNAME);
			var Responses = await facade.Create(data, USERNAME);
			var Response = facade.Read();
			Assert.NotNull(Response);
		}

		[Fact]
		public async void Should_Success_Get_Data_By_Id()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			GarmentInvoice data = await dataUtil(facade, GetCurrentMethod()).GetNewData(USERNAME);
			var Responses = await facade.Create(data, USERNAME);
			var Response = facade.ReadById((int)data.Id);
			Assert.NotNull(Response);
		}
		[Fact]
		public async void Should_Success_Update_Data()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			GarmentInvoice data = await dataUtil(facade, GetCurrentMethod()).GetNewData(USERNAME);
			List<GarmentInvoiceItem> item = new List<GarmentInvoiceItem>(data.Items);

			data.Items.Add(new GarmentInvoiceItem
			{
				DeliveryOrderId = It.IsAny<int>(),
				DODate = DateTimeOffset.Now,
				DeliveryOrderNo = "donos",
				ArrivalDate = DateTimeOffset.Now,
				TotalAmount = 2000,
				CurrencyId = It.IsAny<int>(),
				Details = new List<GarmentInvoiceDetail>
							{
								new GarmentInvoiceDetail
								{
									EPOId=It.IsAny<int>(),
									EPONo="epono",
									IPOId=It.IsAny<int>(),
									PRItemId=It.IsAny<int>(),
									PRNo="prno",
									RONo="12343",
									ProductId= It.IsAny<int>(),
									ProductCode="code",
									ProductName="name",
									UomId=It.IsAny<int>(),
									UomUnit="ROLL",
									DOQuantity=40,
									PricePerDealUnit=5000,
									PaymentType="type",
									PaymentDueDays=2,
									PaymentMethod="method",
									POSerialNumber="PM132434"

								}
							}
			});

			var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
			Assert.NotEqual(ResponseUpdate, 0);
			var newItem = new GarmentInvoiceItem
			{
				DeliveryOrderId = It.IsAny<int>(),
				DODate = DateTimeOffset.Now,
				DeliveryOrderNo = "dono",
				ArrivalDate = DateTimeOffset.Now,
				TotalAmount = 2000,
				CurrencyId = It.IsAny<int>(),
				Details = new List<GarmentInvoiceDetail>
							{
								new GarmentInvoiceDetail
								{
									EPOId=It.IsAny<int>(),
									EPONo="epono",
									IPOId=It.IsAny<int>(),
									PRItemId=It.IsAny<int>(),
									PRNo="prno",
									RONo="12343",
									ProductId= It.IsAny<int>(),
									ProductCode="code",
									ProductName="name",
									UomId=It.IsAny<int>(),
									UomUnit="ROLL",
									DOQuantity=40,
									PricePerDealUnit=5000,
									PaymentType="type",
									PaymentDueDays=2,
									PaymentMethod="method",
									POSerialNumber="PM132434"

								}
							}
			};
			List<GarmentInvoiceItem> Newitems = new List<GarmentInvoiceItem>(data.Items);
			Newitems.Add(newItem);
			data.Items = Newitems;

			var ResponseUpdate1 = await facade.Update((int)data.Id, data, USERNAME);
			Assert.NotEqual(ResponseUpdate, 0);
		}
		[Fact]
		public async void Should_Error_Update_Data()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			GarmentInvoice data = await dataUtil(facade, GetCurrentMethod()).GetNewData(USERNAME);
			List<GarmentInvoiceItem> item = new List<GarmentInvoiceItem>(data.Items);

			data.Items.Add(new GarmentInvoiceItem
			{
				DeliveryOrderId = It.IsAny<int>(),
				DODate = DateTimeOffset.Now,
				DeliveryOrderNo = "donos",
				ArrivalDate = DateTimeOffset.Now,
				TotalAmount = 2000,
				CurrencyId = It.IsAny<int>(),
				Details = null
			});

			var ResponseUpdate = await facade.Update((int)data.Id, data, USERNAME);
			Assert.NotEqual(ResponseUpdate, 0);
			var newItem = new GarmentInvoiceItem
			{
				DeliveryOrderId = It.IsAny<int>(),
				DODate = DateTimeOffset.Now,
				DeliveryOrderNo = "dono",
				ArrivalDate = DateTimeOffset.Now,
				TotalAmount = 2000,
				CurrencyId = It.IsAny<int>(),
				Details =null
			};
			List<GarmentInvoiceItem> Newitems = new List<GarmentInvoiceItem>(data.Items);
			Newitems.Add(newItem);
			data.Items = Newitems;

			Exception errorNullItems = await Assert.ThrowsAsync<Exception>(async () => await facade.Update((int)data.Id, data, USERNAME));
			Assert.NotNull(errorNullItems.Message);
		}

		[Fact]
		public async void Should_Success_Delete_Data()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			GarmentInvoice data = await dataUtil(facade, GetCurrentMethod()).GetNewData(USERNAME);
			await facade.Create(data, USERNAME); 
			var Response = facade.Delete((int)data.Id, USERNAME);
			Assert.NotEqual(Response, 0);
		}

		[Fact]
		public async void Should_Error_Delete_Data()
		{
			var facade = new GarmentInvoiceFacade(_dbContext(USERNAME), ServiceProvider);
			Exception e = await Assert.ThrowsAsync<Exception>(async () => facade.Delete(0, USERNAME));
			Assert.NotNull(e.Message);
		}
		[Fact]
		public void Should_Success_Validate_Data()
		{
			GarmentInvoiceViewModel nullViewModel = new GarmentInvoiceViewModel();
			Assert.True(nullViewModel.Validate(null).Count() > 0);
			GarmentInvoiceViewModel viewModel = new GarmentInvoiceViewModel
			{
				invoiceNo = "",
				invoiceDate = DateTimeOffset.MinValue,
				supplier = { },
				incomeTaxId = It.IsAny<int>(),
				incomeTaxName = "name",
				incomeTaxNo = "",
				incomeTaxDate = DateTimeOffset.MinValue,
				incomeTaxRate = 2,
				vatNo = "",
				vatDate = DateTimeOffset.MinValue,
				useIncomeTax = true,
				useVat = true,
				isPayTax = true,
				hasInternNote = false,
				currency = { },
				items = new List<GarmentInvoiceItemViewModel>
					{
						new GarmentInvoiceItemViewModel
						{
							deliveryOrder =null,
							currency="IDR",
							details= new List<GarmentInvoiceDetailViewModel>
							{
								new GarmentInvoiceDetailViewModel
								{
									doQuantity=0
								}
							}
						}
					}
			};
			Assert.True(viewModel.Validate(null).Count() > 0);

			GarmentInvoiceViewModel viewModels = new GarmentInvoiceViewModel
			{
				invoiceNo = "",
				invoiceDate = DateTimeOffset.MinValue,
				supplier = { },
				incomeTaxId = It.IsAny<int>(),
				incomeTaxName = "name",
				incomeTaxNo = "",
				incomeTaxDate = DateTimeOffset.MinValue,
				incomeTaxRate = 2,
				vatNo = "",
				vatDate = DateTimeOffset.MinValue,
				useIncomeTax = true,
				useVat = true,
				isPayTax = true,
				hasInternNote = false,
				currency =new CurrencyViewModel{ Id = It.IsAny<int>(), Code = "USD", Symbol = "$", Rate = 13000, Description = "" },
				items = new List<GarmentInvoiceItemViewModel>
					{
						new GarmentInvoiceItemViewModel
						{
							deliveryOrder =null,
							currency="USD",
							details= null
						}
					}
			};
			Assert.True(viewModels.Validate(null).Count() > 0);
		}
	}
}