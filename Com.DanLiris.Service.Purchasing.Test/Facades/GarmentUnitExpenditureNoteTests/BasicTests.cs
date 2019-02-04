using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitExpenditureNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Migrations;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitExpenditureDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentUnitExpenditureNoteTests
{
    public class BasicTests
    {
        private const string ENTITY = "GarmentUnitExpenditureNote";

        private const string USERNAME = "Unit Test";
        private IServiceProvider ServiceProvider { get; set; }

        private IServiceProvider GetServiceProvider()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");

            var httpClientService = new Mock<IHttpClientService>();
            httpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(httpResponseMessage);

            var mapper = new Mock<IMapper>();
            mapper
                .Setup(x => x.Map<GarmentUnitExpenditureNoteViewModel>(It.IsAny<GarmentUnitExpenditureNote>()))
                .Returns(new GarmentUnitExpenditureNoteViewModel
                {
                    Id = 1,
                    UnitDONo = "UnitDONO1234",
                    Storage = new Lib.ViewModels.IntegrationViewModel.StorageViewModel(),
                    StorageRequest = new Lib.ViewModels.IntegrationViewModel.StorageViewModel(),
                    UnitSender = new UnitViewModel(),
                    UnitRequest = new UnitViewModel(),
                    Items = new List<GarmentUnitExpenditureNoteItemViewModel>
                    {
                        new GarmentUnitExpenditureNoteItemViewModel {
                            ProductId = 1,
                            UomId = 1
                        }
                    }
                });

            var mockGarmentDeliveryOrderFacade = new Mock<IGarmentUnitDeliveryOrderFacade>();
            mockGarmentDeliveryOrderFacade
                .Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentUnitDeliveryOrder());

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


            return serviceProviderMock.Object;
        }
        private IServiceProvider GetServiceProviderUnitReceiptNote()
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");

            var httpClientService = new Mock<IHttpClientService>();
            httpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(httpResponseMessage);

            var mapper = new Mock<IMapper>();
            mapper
                .Setup(x => x.Map<GarmentUnitReceiptNoteViewModel>(It.IsAny<GarmentUnitReceiptNote>()))
                .Returns(new GarmentUnitReceiptNoteViewModel
                {
                    Items = new List<GarmentUnitReceiptNoteItemViewModel>
                    {
                        new GarmentUnitReceiptNoteItemViewModel()
                    }
                });

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

            return serviceProviderMock.Object;
        }

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

        private GarmentUnitExpenditureNoteDataUtil dataUtil(GarmentUnitExpenditureNoteFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider, _dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            var garmentDeliveryOrderFacade = new GarmentDeliveryOrderFacade(GetServiceProvider(), _dbContext(testName));
            var garmentDeliveryOrderDataUtil = new GarmentDeliveryOrderDataUtil(garmentDeliveryOrderFacade, garmentExternalPurchaseOrderDataUtil);

            var garmentUnitReceiptNoteFacade = new GarmentUnitReceiptNoteFacade(GetServiceProviderUnitReceiptNote(), _dbContext(testName));
            var garmentUnitReceiptNoteDatautil = new GarmentUnitReceiptNoteDataUtil(garmentUnitReceiptNoteFacade, garmentDeliveryOrderDataUtil);

            var garmentUnitDeliveryOrderFacade = new GarmentUnitDeliveryOrderFacade(_dbContext(testName), GetServiceProvider());
            var garmentUnitDeliveryOrderDatautil = new GarmentUnitDeliveryOrderDataUtil(garmentUnitDeliveryOrderFacade, garmentUnitReceiptNoteDatautil);


            return new GarmentUnitExpenditureNoteDataUtil(facade, garmentUnitDeliveryOrderDatautil);
        }

        [Fact]
        public async void Should_Success_Get_All_Data()
        {
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Data.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)data.Id);
            Assert.NotEqual(Response.Id, 0);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), _dbContext(GetCurrentMethod()));
            var data = dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.Create(data);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data_Null_Items()
        {
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), _dbContext(GetCurrentMethod()));
            var data = dataUtil(facade, GetCurrentMethod()).GetNewData();
            data.Items = null;
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(data));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), dbContext);
            var dataUtil = this.dataUtil(facade, GetCurrentMethod());
            var data = await dataUtil.GetTestData();

            dbContext.Entry(data).State = EntityState.Detached;
            foreach (var item in data.Items)
            {
                dbContext.Entry(item).State = EntityState.Detached;
            }

            var newItem = dbContext.GarmentUnitExpenditureNoteItems.AsNoTracking().Single(m => m.Id == data.Items.First().Id);
            newItem.Id = 0;
            newItem.IsSave = true;
            newItem.Quantity = 5;

            data.Items.Add(newItem);

            var ResponseUpdate = await facade.Update((int)data.Id, data);
            Assert.NotEqual(ResponseUpdate, 0);

            dbContext.Entry(data).State = EntityState.Detached;
            foreach (var item in data.Items)
            {
                dbContext.Entry(item).State = EntityState.Detached;
            }

            var newData = dbContext.GarmentUnitExpenditureNotes
                .AsNoTracking()
                .Include(x => x.Items)
                .Single(m => m.Id == data.Items.First().Id);

            newData.Items = newData.Items.Take(1).ToList();
            newData.Items.First().IsSave = true;

            var ResponseUpdateRemoveItem = await facade.Update((int)newData.Id, newData);
            Assert.NotEqual(ResponseUpdateRemoveItem, 0);
        }

        [Fact]
        public async void Should_Error_Update_Data_Null_Items()
        {
            var dbContext = _dbContext(GetCurrentMethod());
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), dbContext);

            var data = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            dbContext.Entry(data).State = EntityState.Detached;
            data.Items = null;

            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Update((int)data.Id, data));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            var Response = await facade.Delete((int)data.Id);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Delete_Data_Invalid_Id()
        {
            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), _dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Delete(0));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Validate_Data()
        {
            GarmentUnitExpenditureNoteViewModel viewModel = new GarmentUnitExpenditureNoteViewModel { };
            Assert.True(viewModel.Validate(null).Count() > 0);

            GarmentUnitExpenditureNoteViewModel viewModelCheckExpenditureDate = new GarmentUnitExpenditureNoteViewModel
            {
                ExpenditureDate = DateTimeOffset.Now
            };
            Assert.True(viewModelCheckExpenditureDate.Validate(null).Count() > 0);

            GarmentUnitExpenditureNoteViewModel viewModelCheckUnitDeliveryOrder = new GarmentUnitExpenditureNoteViewModel
            {
                ExpenditureDate = DateTimeOffset.Now,
                UnitDONo = "UnitDONO123",
                
            };
            Assert.True(viewModelCheckUnitDeliveryOrder.Validate(null).Count() > 0);
            
            GarmentUnitExpenditureNoteViewModel viewModelCheckItemsCount = new GarmentUnitExpenditureNoteViewModel { UnitDOId = 1 };
            Assert.True(viewModelCheckItemsCount.Validate(null).Count() > 0);

            Mock<IGarmentUnitDeliveryOrderFacade> garmentUnitDeliveryOrderFacadeMock = new Mock<IGarmentUnitDeliveryOrderFacade>();

            Mock<IGarmentUnitExpenditureNoteFacade> garmentUnitExpenditureNoteFacadeMock = new Mock<IGarmentUnitExpenditureNoteFacade>();
            garmentUnitDeliveryOrderFacadeMock.Setup(s => s.ReadById(It.IsAny<int>()))
                .Returns(new GarmentUnitDeliveryOrder {
                    Id = 1,
                    
                    Items = new List<GarmentUnitDeliveryOrderItem>
                    {
                        new GarmentUnitDeliveryOrderItem
                        {
                            Id = 1,
                            Quantity = 4
                        },
                    }
                });

            var facade = new GarmentUnitExpenditureNoteFacade(GetServiceProvider(), _dbContext(GetCurrentMethod()));
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.
                Setup(x => x.GetService(typeof(IGarmentUnitDeliveryOrderFacade)))
                .Returns(garmentUnitDeliveryOrderFacadeMock.Object);
            serviceProvider.Setup(x => x.GetService(typeof(PurchasingDbContext)))
                .Returns(_dbContext(GetCurrentMethod()));
            var data = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var item = data.Items.First();
            var garmentUnitExpenditureNote = new GarmentUnitExpenditureNoteViewModel
            {
                UnitDOId = 1,
                Items = new List<GarmentUnitExpenditureNoteItemViewModel>
                {
                    new GarmentUnitExpenditureNoteItemViewModel
                    {
                        Id = item.Id,
                        UnitDOItemId = 1,
                        Quantity = 10,
                        IsSave = true,
                    },

                    new GarmentUnitExpenditureNoteItemViewModel
                    {
                        Id = item.Id,
                        UnitDOItemId = 1,
                        Quantity = 100,
                        IsSave = true,
                        
                    },

                    new GarmentUnitExpenditureNoteItemViewModel
                    {
                        Id = item.Id,
                        UnitDOItemId = 1,
                        Quantity = 0,
                        IsSave = true
                    },
                }
            };

            Mock<IGarmentUnitExpenditureNoteFacade> garmentUnitExpenditreMock = new Mock<IGarmentUnitExpenditureNoteFacade>();
            garmentUnitExpenditreMock.Setup(s => s.ReadById(1))
                .Returns(garmentUnitExpenditureNote);
            garmentUnitExpenditreMock.Setup(s => s.ReadById(It.IsAny<int>()))
                .Returns(garmentUnitExpenditureNote);

            serviceProvider.
                Setup(x => x.GetService(typeof(IGarmentUnitExpenditureNoteFacade)))
                .Returns(garmentUnitExpenditreMock.Object);
            System.ComponentModel.DataAnnotations.ValidationContext garmentUnitDeliveryOrderValidate = new System.ComponentModel.DataAnnotations.ValidationContext(garmentUnitExpenditureNote, serviceProvider.Object, null);
            Assert.True(garmentUnitExpenditureNote.Validate(garmentUnitDeliveryOrderValidate).Count() > 0);
        }
    }
}
