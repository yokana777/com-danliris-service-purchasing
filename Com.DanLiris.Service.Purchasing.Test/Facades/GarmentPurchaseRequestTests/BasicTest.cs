using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentPurchaseRequestViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
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

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentPurchaseRequestTests
{
    public class BasicTest
    {
        private const string ENTITY = "GarmentPurchaseRequest";

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

        private GarmentPurchaseRequestDataUtil dataUtil(GarmentPurchaseRequestFacade facade, string testName)
        {
            return new GarmentPurchaseRequestDataUtil(facade);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.Create(model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            model.Items = null;
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(model, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var item = model.Items.First();

            model.Items.Add(new GarmentPurchaseRequestItem
            {
                PO_SerialNumber = item.PO_SerialNumber,
                ProductId = item.ProductId,
                ProductCode = item.ProductCode,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                BudgetPrice = item.BudgetPrice,
                UomId = item.UomId,
                UomUnit = item.UomUnit,
                CategoryId = item.CategoryId,
                CategoryName = item.CategoryName,
                ProductRemark = item.ProductRemark,
                Status = item.Status,
            });

            var Response = await facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Update_Data()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            Exception errorInvalidId = await Assert.ThrowsAsync<Exception>(async () => await facade.Update(0, model, USERNAME));
            Assert.NotNull(errorInvalidId.Message);

            model.Items = null;
            Exception errorNullItems = await Assert.ThrowsAsync<Exception>(async () => await facade.Update((int)model.Id, model, USERNAME));
            Assert.NotNull(errorNullItems.Message);
        }

        [Fact]
        public async void Should_Success_Get_All_Data()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int) model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_RONo()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadByRONo(model.RONo);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Tags()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            var Response = facade.ReadByTags($"#{model.UnitName} #{model.BuyerName}", model.ShipmentDate.AddDays(-1), model.ShipmentDate.AddDays(1));
            Assert.NotNull(Response);

            var ResponseWhiteSpace = facade.ReadByTags("", DateTimeOffset.MinValue, DateTimeOffset.MinValue);
            Assert.NotNull(ResponseWhiteSpace);
        }

        [Fact]
        public void Should_Success_Validate_Data()
        {
            GarmentPurchaseRequestViewModel nullViewModel = new GarmentPurchaseRequestViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentPurchaseRequestViewModel viewModel = new GarmentPurchaseRequestViewModel
            {
                Buyer = new BuyerViewModel(),
                Unit = new UnitViewModel(),
                Items = new List<GarmentPurchaseRequestItemViewModel>
                {
                    new GarmentPurchaseRequestItemViewModel(),
                    new GarmentPurchaseRequestItemViewModel
                    {
                        Product = new ProductViewModel(),
                        Uom = new UomViewModel(),
                        Category = new CategoryViewModel()
                    }
                }
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public async void Should_Success_Validate_Data_Duplicate()
        {
            GarmentPurchaseRequestFacade facade = new GarmentPurchaseRequestFacade(_dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            GarmentPurchaseRequestViewModel viewModel = new GarmentPurchaseRequestViewModel();
            viewModel.RONo = model.RONo;

            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.
                Setup(x => x.GetService(typeof(PurchasingDbContext)))
                .Returns(_dbContext(GetCurrentMethod()));

            ValidationContext validationContext = new ValidationContext(viewModel, serviceProvider.Object, null);

            var validationResultCreate = viewModel.Validate(validationContext).ToList();

            var errorDuplicateRONo = validationResultCreate.SingleOrDefault(r => r.ErrorMessage.Equals("RONo sudah ada"));
            Assert.NotNull(errorDuplicateRONo);

            viewModel.Id = model.Id;
            viewModel.Items = new List<GarmentPurchaseRequestItemViewModel>();
            viewModel.Items.AddRange(model.Items.Select(i => new GarmentPurchaseRequestItemViewModel
            {
                PO_SerialNumber = i.PO_SerialNumber
            }));

            var validationResultUpdate = viewModel.Validate(validationContext).ToList();
            var errorItems = validationResultUpdate.SingleOrDefault(r => r.MemberNames.Contains("Items"));
            List<Dictionary<string, object>> errorItemsMessage = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(errorItems.ErrorMessage);
            var errorDuplicatePO_SerialNumber = errorItemsMessage.FirstOrDefault(m => m.ContainsValue("PO_SerialNumber sudah ada"));
            Assert.NotNull(errorDuplicatePO_SerialNumber);
        }
    }
}
