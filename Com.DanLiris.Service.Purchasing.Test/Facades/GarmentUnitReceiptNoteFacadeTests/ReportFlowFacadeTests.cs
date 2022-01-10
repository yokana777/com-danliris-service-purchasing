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
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.NewIntegrationDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentUnitReceiptNoteFacadeTests
{
	public class ReportFlowFacadeTests
	{ 
        private const string ENTITY = "GarmentUnitReceiptNoteReport";

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
			message.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"codeRequirement\":\"FABRIC\",\"code\":\"BB\",\"rate\":13700.0,\"name\":\"FABRIC\",\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");
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

		private GarmentUnitReceiptNoteDataUtil garmentUnitReceiptNoteDataUtil(GarmentUnitReceiptNoteFacade garmentUnitReceiptNoteFacade, string testName)
		{
			var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(GetServiceProvider().Object, _dbContext(testName));
			var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

			var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
			var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

			var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(GetServiceProvider().Object, _dbContext(testName));
			var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

			var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(testName));
			var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

			return new GarmentUnitReceiptNoteDataUtil(garmentUnitReceiptNoteFacade, garmentDeliveryOrderDataUtil, null);
		}

		private GarmentUnitDeliveryOrderDataUtil UnitDOdataUtil(GarmentUnitDeliveryOrderFacade garmentUnitDeliveryOrderFacade, string testName)
		{
			var garmentUnitReceiptNoteFacade = new GarmentUnitReceiptNoteFacade(GetServiceProvider().Object, _dbContext(testName));
			var garmentUnitReceiptNoteDataUtil = this.garmentUnitReceiptNoteDataUtil(garmentUnitReceiptNoteFacade, testName);

			return new GarmentUnitDeliveryOrderDataUtil(garmentUnitDeliveryOrderFacade, garmentUnitReceiptNoteDataUtil);
		}
		[Fact]
		public async Task Should_Success_GENERATEEXCELSMP1()
		{
			var facade1 = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
			var data1 = await UnitDOdataUtil(facade1, GetCurrentMethod()).GetTestDataMultipleItemForURNProcess();

			 

			HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			httpResponseMessage.Content = new StringContent("\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"BB\",\"codeRequirement\":\"FABRIC\",\"name\":\"FABRIC\",\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}");

			var httpClientService = new Mock<IHttpClientService>();

			 
			httpClientService
				.Setup(x => x.GetAsync(It.IsAny<string>()))
				.ReturnsAsync(httpResponseMessage);

			httpClientService
			   .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("master/garment-categories"))))
			   .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new GarmentCategoryDataUtil().GetMultipleResultFormatterOkString()) });

			httpClientService
			   .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("delivery-returns"))))
			   .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new GarmentDeliveryReturnDataUtil().GetResultFormatterOkString(data1)) });

			httpClientService
			   .Setup(x => x.PutAsync(It.Is<string>(s => s.Contains("delivery-returns")), It.IsAny<HttpContent>()))
			   .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new GarmentDeliveryReturnDataUtil().GetResultFormatterOkString()) });

			var mapper = new Mock<IMapper>();
			mapper
				.Setup(x => x.Map<GarmentUnitReceiptNoteViewModel>(It.IsAny<GarmentUnitReceiptNote>()))
				.Returns(new GarmentUnitReceiptNoteViewModel
				{
					Id = 1,
					DOId = 1,
					DOCurrency = new CurrencyViewModel(),
					Supplier = new SupplierViewModel(),
					Unit = new UnitViewModel(),
					Items = new List<GarmentUnitReceiptNoteItemViewModel>
					{
						new GarmentUnitReceiptNoteItemViewModel {
							Product = new GarmentProductViewModel(),
							Uom = new UomViewModel()
						}
					}
				});

			var mockGarmentDeliveryOrderFacade = new Mock<IGarmentDeliveryOrderFacade>();
			mockGarmentDeliveryOrderFacade
				.Setup(x => x.ReadById(It.IsAny<int>()))
				.Returns(new GarmentDeliveryOrder());

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
			serviceProviderMock
				.Setup(x => x.GetService(typeof(IGarmentDeliveryOrderFacade)))
				.Returns(mockGarmentDeliveryOrderFacade.Object);

			serviceProviderMock
				.Setup(x => x.GetService(typeof(IdentityService)))
				.Returns(new IdentityService() { Token = "Token", Username = "Test" });



			var facade = new GarmentUnitReceiptNoteFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
			var data = await garmentUnitReceiptNoteDataUtil(facade, GetCurrentMethod()).GetNewDataMonitoring();

			var result =  facade.GenerateExcelFlowForUnit(DateTime.Now.AddDays(-7),DateTime.Now.AddDays(7),"SMP1","BB","fabric",7,"SAMPLE");
			Assert.NotNull(result);

		}
		[Fact]
		public async Task Should_Success_GENERATEEXCENotSample()
		{
			var facade1 = new GarmentUnitDeliveryOrderFacade(_dbContext(GetCurrentMethod()), GetServiceProvider().Object);
			var data1 = await UnitDOdataUtil(facade1, GetCurrentMethod()).GetTestDataMultipleItemForURNProcess();



			HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			httpResponseMessage.Content = new StringContent("\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"BB\",\"codeRequirement\":\"FABRIC\",\"name\":\"FABRIC\",\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}");

			var httpClientService = new Mock<IHttpClientService>();


			httpClientService
				.Setup(x => x.GetAsync(It.IsAny<string>()))
				.ReturnsAsync(httpResponseMessage);

			httpClientService
			   .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("master/garment-categories"))))
			   .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new GarmentCategoryDataUtil().GetMultipleResultFormatterOkString()) });

			httpClientService
			   .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("delivery-returns"))))
			   .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new GarmentDeliveryReturnDataUtil().GetResultFormatterOkString(data1)) });

			httpClientService
			   .Setup(x => x.PutAsync(It.Is<string>(s => s.Contains("delivery-returns")), It.IsAny<HttpContent>()))
			   .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new GarmentDeliveryReturnDataUtil().GetResultFormatterOkString()) });

			var mapper = new Mock<IMapper>();
			mapper
				.Setup(x => x.Map<GarmentUnitReceiptNoteViewModel>(It.IsAny<GarmentUnitReceiptNote>()))
				.Returns(new GarmentUnitReceiptNoteViewModel
				{
					Id = 1,
					DOId = 1,
					DOCurrency = new CurrencyViewModel(),
					Supplier = new SupplierViewModel(),
					Unit = new UnitViewModel(),
					Items = new List<GarmentUnitReceiptNoteItemViewModel>
					{
						new GarmentUnitReceiptNoteItemViewModel {
							Product = new GarmentProductViewModel(),
							Uom = new UomViewModel()
						}
					}
				});

			var mockGarmentDeliveryOrderFacade = new Mock<IGarmentDeliveryOrderFacade>();
			mockGarmentDeliveryOrderFacade
				.Setup(x => x.ReadById(It.IsAny<int>()))
				.Returns(new GarmentDeliveryOrder());

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
			serviceProviderMock
				.Setup(x => x.GetService(typeof(IGarmentDeliveryOrderFacade)))
				.Returns(mockGarmentDeliveryOrderFacade.Object);

			serviceProviderMock
				.Setup(x => x.GetService(typeof(IdentityService)))
				.Returns(new IdentityService() { Token = "Token", Username = "Test" });



			var facade = new GarmentUnitReceiptNoteFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
			var data = await garmentUnitReceiptNoteDataUtil(facade, GetCurrentMethod()).GetNewDataMonitoring();

			var result = facade.GenerateExcelFlowForUnit(DateTime.Now.AddDays(-7), DateTime.Now.AddDays(7), "", "BB", "fabric", 7, "");
			Assert.NotNull(result);

		}
	}
}
