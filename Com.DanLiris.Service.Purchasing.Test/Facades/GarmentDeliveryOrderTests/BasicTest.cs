using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentDeliveryOrderTests
{
    public class BasicTest
    {
        private const string ENTITY = "GarmentDeliveryOrder";

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

        private GarmentDeliveryOrderDataUtil dataUtil(GarmentDeliveryOrderFacade facade, string testName)
        {
            return new GarmentDeliveryOrderDataUtil(facade);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(_dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.Create(model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(_dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(null, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            var Response = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Update_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            Exception errorInvalidId = await Assert.ThrowsAsync<Exception>(async () => await facade.Update(0, model, USERNAME));
            Assert.NotNull(errorInvalidId.Message);
        }

        [Fact]
        public async void Should_Success_Get_All_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public void Should_Success_Validate_Data()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),

            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data_Where_DoDate_GreaterThan_ArrivalDate()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                doDate = new DateTimeOffset(DateTime.Today).AddDays(7),
                arrivalDate = new DateTimeOffset(DateTime.Today).AddDays(1),
                supplier = new SupplierViewModel(),
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data_Item()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),
                items = new List<GarmentDeliveryOrderItemViewModel>
                {
                    new GarmentDeliveryOrderItemViewModel
                    {
                        purchaseOrderExternal = null,
                        pOId = 1,
                        pONo = "test"
                    }
                }

            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public async void Should_Success_Validate_Data_Duplicate()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),
            };
            viewModel.Id = model.Id + 1;
            viewModel.doNo = model.DONo;
            viewModel.supplier.Id = model.SupplierId;
            viewModel.doDate = model.DODate;
            viewModel.arrivalDate = model.ArrivalDate;

            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.
                Setup(x => x.GetService(typeof(PurchasingDbContext)))
                .Returns(_dbContext(GetCurrentMethod()));

            ValidationContext validationContext = new ValidationContext(viewModel, serviceProvider.Object, null);

            var validationResultCreate = viewModel.Validate(validationContext).ToList();

            var errorDuplicateDONo = validationResultCreate.SingleOrDefault(r => r.ErrorMessage.Equals("DoNo is already exist"));
            Assert.NotNull(errorDuplicateDONo);
        }
    }
}
