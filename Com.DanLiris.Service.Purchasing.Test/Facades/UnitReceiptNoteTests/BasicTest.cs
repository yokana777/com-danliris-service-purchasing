using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.UnitReceiptNoteTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class BasicTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public BasicTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;

            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
            identityService.Username = "Unit Test";
        }

        private UnitReceiptNoteDataUtil DataUtil
        {
            get { return (UnitReceiptNoteDataUtil)ServiceProvider.GetService(typeof(UnitReceiptNoteDataUtil)); }
        }

        private UnitReceiptNoteFacade Facade
        {
            get { return (UnitReceiptNoteFacade)ServiceProvider.GetService(typeof(UnitReceiptNoteFacade)); }
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            await DataUtil.GetTestData("Unit test");
            Tuple<List<UnitReceiptNote>, int, Dictionary<string, string>> Response = Facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            UnitReceiptNote model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {
            UnitReceiptNote model = await DataUtil.GetNewData("Unit test");
            var Response = await Facade.Create(model, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        //[Fact]
        //public async void Should_Success_Update_Data()
        //{
        //    UnitReceiptNote model = await DataUtil.GetTestData("Unit test");
        //    var Response = await Facade.Update((int)model.Id, model, "Unit Test");
        //    Assert.NotEqual(Response, 0);
        //}

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            UnitReceiptNote model = await DataUtil.GetTestData("Unit test");
            var Response = Facade.Delete((int)model.Id, "Unit Test");
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data_Null_Parameter()
        {
            Exception exception = await Assert.ThrowsAsync<Exception>(() => Facade.Create(null, "Unit Test"));
            Assert.Equal(exception.Message, "Object reference not set to an instance of an object.");
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            UnitReceiptNote model = await DataUtil.GetTestData("Unit Test");
            foreach (var item in model.Items)
            {
                
                    item.ReceiptQuantity -= 1;
                
            }
            var Response = await Facade.Update((int)model.Id, model, "Unit Test");
            Assert.NotEqual(Response, 0);

            UnitReceiptNoteItem oldItem = model.Items.FirstOrDefault();
            

            model.Items.Remove(oldItem);
            model.Items.Add(oldItem);
            var ResponseRemoveItemDetail = await Facade.Update((int)model.Id, model, "Unit Test");
            Assert.NotEqual(ResponseRemoveItemDetail, 0);
        }
    }
}
