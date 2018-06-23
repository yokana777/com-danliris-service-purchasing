using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.DeliveryOrderTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class BasicTest
    {
        private const string USERNAME = "Unit Test";
        private IServiceProvider ServiceProvider { get; set; }

        public BasicTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = USERNAME;
        }

        private DeliveryOrderDataUtil DataUtil
        {
            get { return (DeliveryOrderDataUtil)ServiceProvider.GetService(typeof(DeliveryOrderDataUtil)); }
        }

        private DeliveryOrderFacade Facade
        {
            get { return (DeliveryOrderFacade)ServiceProvider.GetService(typeof(DeliveryOrderFacade)); }
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            DeliveryOrder model = await DataUtil.GetNewData(USERNAME);
            var Response = await Facade.Create(model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data_Null_Parameter()
        {
            Exception exception = await Assert.ThrowsAsync<Exception>(() => Facade.Create(null, USERNAME));
            Assert.Equal(exception.Message, "Object reference not set to an instance of an object.");
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            await DataUtil.GetTestData(USERNAME);
            Tuple<List<DeliveryOrder>, int, Dictionary<string, string>> Response = Facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
            var Response = Facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            DeliveryOrder model = await DataUtil.GetTestData(USERNAME);
            foreach (var item in model.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.DOQuantity -= 1; 
                }
            }
            var Response = await Facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(Response, 0);

            var newModelForAddDetail = await DataUtil.GetNewData(USERNAME);
            DeliveryOrderDetail newModelDetail = newModelForAddDetail.Items.FirstOrDefault().Details.FirstOrDefault();
            model.Items.FirstOrDefault().Details.Add(newModelDetail);
            var ResponseAddDetail = await Facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAddDetail, 0);

            var newModelForAddItem = await DataUtil.GetNewData(USERNAME);
            DeliveryOrderItem newModelItem = newModelForAddItem.Items.FirstOrDefault();
            model.Items.Add(newModelItem);
            var ResponseAddItem = await Facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseAddItem, 0);

            model.Items.Remove(newModelItem);
            model.Items.FirstOrDefault().Details.Remove(newModelDetail);
            var ResponseRemoveItemDetail = await Facade.Update((int)model.Id, model, USERNAME);
            Assert.NotEqual(ResponseRemoveItemDetail, 0);
        }

        [Fact]
        public async void Should_Error_Update_Data_Invalid_Id()
        {
            Exception exception = await Assert.ThrowsAsync<Exception>(() => Facade.Update(0, new DeliveryOrder(), USERNAME));
            Assert.Equal(exception.Message, "Invalid Id");
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            var model = await DataUtil.GetTestData(USERNAME);
            var Response = Facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }
    }
}
