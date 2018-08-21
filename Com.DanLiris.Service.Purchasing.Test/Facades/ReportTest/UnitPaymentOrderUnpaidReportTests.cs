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
        public async void Should_Success_Get_SQL_Data()
        {
            var response = this.Facade.GetPurchasingDocumentExpedition(25, 1, "{}", null, null, DateTimeOffset.Now, DateTimeOffset.Now.AddMonths(-1));

            Assert.NotNull(response.Item1);
            Assert.NotNull(response.Item2);
        }

        [Fact]
        public void Should_Success_Get_Mongo_Data()
        {
            var result = this.Facade.GetReportMongo( "", "", DateTimeOffset.Now, DateTimeOffset.Now.AddMonths(-1));
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_Report_Data()
        {
            var result = this.Facade.GetReport(25, 1, "{}", null, null, null, null,0);
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_Mongo_Data_Parm()
        {
            BsonDocument data = DataUtil.GetTestData();
            var result = this.Facade.GetReportMongo(GetBsonValue.ToString(data, "no"), GetBsonValue.ToString(data, "supplier.code"), DateTimeOffset.Now.AddMonths(-1), DateTimeOffset.Now);
            Assert.NotEmpty(result);
            this.Facade.DeleteDataMongoByNo(data["no"].AsString);
        }

        [Fact]
        public void Should_Success_Get_Report_Data_Parm()
        {
            BsonDocument data = DataUtil.GetTestData();
            var result = this.Facade.GetReport(25, 1, "{}", GetBsonValue.ToString(data, "no"), GetBsonValue.ToString(data, "supplier.code"), null, null, 0);
            Assert.NotNull(result);
            this.Facade.DeleteDataMongoByNo(data["no"].AsString);
        }
    }
}
