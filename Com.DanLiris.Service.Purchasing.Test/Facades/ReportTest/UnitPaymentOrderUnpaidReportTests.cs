using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil;
using MongoDB.Bson;
using System;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.ReportTest
{
    [Collection("ServiceProviderFixture Collection")]
    public class UnitPaymentOrderUnpaidReportTests
    {
        private IServiceProvider ServiceProvider { get; set; }

        public UnitPaymentOrderUnpaidReportTests(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;
        }

        private UnitPaymentOrderUnpaidReportDataUtil DataUtil
        {
            get { return (UnitPaymentOrderUnpaidReportDataUtil)ServiceProvider.GetService(typeof(UnitPaymentOrderUnpaidReportDataUtil)); }
        }

        private UnitPaymentOrderUnpaidReportFacade Facade
        {
            get { return (UnitPaymentOrderUnpaidReportFacade)ServiceProvider.GetService(typeof(UnitPaymentOrderUnpaidReportFacade)); }
        }

        [Fact]
        public void Should_Success_Get_SQL_Data()
        {
            var result = this.Facade.GetPurchasingDocumentExpedition(25, 1, null, null, DateTimeOffset.Now.AddMonths(-1), DateTimeOffset.Now);
            Assert.NotNull(result);
        }

        [Fact]
        public async void Should_Success_Get_Mongo_Data()
        {
            var result = await this.Facade.GetReportMongo( "", "", DateTimeOffset.Now.AddMonths(-1), DateTimeOffset.Now);
            Assert.NotNull(result);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data()
        {
            var result = await this.Facade.GetReport(25, 1, "{}", null, null, null, null,0);
            Assert.NotNull(result);
        }

        [Fact]
        public async void Should_Success_Get_Mongo_Data_Parm()
        {
            var data = DataUtil.GetTestData();
            var date = data.Item1["dueDate"].ToUniversalTime();
            var result = await this.Facade.GetReportMongo(GetBsonValue.ToString(data.Item1, "no"), GetBsonValue.ToString(data.Item1, "supplier.code"), date.AddDays(-15), date.AddDays(15));
            Assert.NotEmpty(result);
            this.Facade.DeleteDataMongoByNoUPO(data.Item1["no"].AsString);
            this.Facade.DeleteDataMongoByNoURN(data.Item2["no"].AsString);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Parm()
        {
            var data = DataUtil.GetTestData();
            var date = data.Item1["dueDate"].ToUniversalTime();
            var result = await this.Facade.GetReport(25, 1, "{}", "", "", date.AddDays(-15), date.AddDays(15), 0);
            Assert.NotNull(result);
            this.Facade.DeleteDataMongoByNoUPO(data.Item1["no"].AsString);
            this.Facade.DeleteDataMongoByNoURN(data.Item2["no"].AsString);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Parm_Order()
        {
            var data = DataUtil.GetTestData();
            var date = data.Item1["dueDate"].ToUniversalTime();
            var result = await this.Facade.GetReport(25, 1, "{\"UnitPaymentOrderNo\":\"asc\"}", "", "", date.AddDays(-15), date.AddDays(15), 0);
            Assert.NotNull(result);
            this.Facade.DeleteDataMongoByNoUPO(data.Item1["no"].AsString);
            this.Facade.DeleteDataMongoByNoURN(data.Item2["no"].AsString);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Parm_Order_From()
        {
            var data = DataUtil.GetTestData();
            var date = data.Item1["dueDate"].ToUniversalTime();
            var result = await this.Facade.GetReport(25, 1, "{\"UnitPaymentOrderNo\":\"asc\"}", GetBsonValue.ToString(data.Item1, "no"), GetBsonValue.ToString(data.Item1, "supplier.code"), date.AddDays(-15), null, 0);
            Assert.NotNull(result);
            this.Facade.DeleteDataMongoByNoUPO(data.Item1["no"].AsString);
            this.Facade.DeleteDataMongoByNoURN(data.Item2["no"].AsString);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Parm_Order_To()
        {
            var data = DataUtil.GetTestData();
            var date = data.Item1["dueDate"].ToUniversalTime();
            var result = await this.Facade.GetReport(25, 1, "{\"UnitPaymentOrderNo\":\"asc\"}", GetBsonValue.ToString(data.Item1, "no"), GetBsonValue.ToString(data.Item1, "supplier.code"), null,date.AddDays(15), 0);
            Assert.NotNull(result);
            this.Facade.DeleteDataMongoByNoUPO(data.Item1["no"].AsString);
            this.Facade.DeleteDataMongoByNoURN(data.Item2["no"].AsString);
        }

        [Fact]
        public async void Should_Success_Get_Report_Data_Parm_All()
        {
            var data = DataUtil.GetTestData();
            var date = data.Item1["dueDate"].ToUniversalTime();
            var result = await this.Facade.GetReport(25, 1, "{\"UnitPaymentOrderNo\":\"asc\"}", GetBsonValue.ToString(data.Item1, "no"), GetBsonValue.ToString(data.Item1, "supplier.code"), date.AddDays(-15), date.AddDays(15), 0);
            Assert.NotNull(result);
            this.Facade.DeleteDataMongoByNoUPO(data.Item1["no"].AsString);
            this.Facade.DeleteDataMongoByNoURN(data.Item2["no"].AsString);
        }
    }
}
