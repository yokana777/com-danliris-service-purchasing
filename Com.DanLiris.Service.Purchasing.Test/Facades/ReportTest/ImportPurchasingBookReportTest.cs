using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNote;
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

        private UnitReceiptNoteBsonDataUtil DataUtil
        {
            get { return (UnitReceiptNoteBsonDataUtil)ServiceProvider.GetService(typeof(UnitReceiptNoteBsonDataUtil)); }
        }

        private ImportPurchasingBookReportFacade Facade
        {
            get { return (ImportPurchasingBookReportFacade)ServiceProvider.GetService(typeof(ImportPurchasingBookReportFacade)); }
        }

        //[Fact]
        //public void Should_Success_Get_Report_Data()
        //{
        //    BsonDocument data = DataUtil.GetTestData();
        //    var Response = this.Facade.GetReport(GetBsonValue.ToString(data, "no"), GetBsonValue.ToString(data, "unit.code"), GetBsonValue.ToString(data, "items.purchaseOrder.category.code"), null, null);
        //    Assert.NotEqual(Response.Item2, 0);
            
        //}

        //[Fact]
        //public void Should_Success_Get_Report_Data_Null_Parameter()
        //{
        //    BsonDocument data = DataUtil.GetTestData();
        //    var Response = this.Facade.GetReport(null, null, null, data["date"].ToUniversalTime(), data["date"].ToUniversalTime());
        //    Assert.NotEqual(Response.Item2, 0);
            
        //}

        //[Fact]
        //public void Should_Success_Get_Report_Data_Excel()
        //{
        //    BsonDocument data = DataUtil.GetTestData();
        //    var xls = this.Facade.GenerateExcel(GetBsonValue.ToString(data, "no"), GetBsonValue.ToString(data, "unit.code"), GetBsonValue.ToString(data, "items.purchaseOrder.category.code"), null, null);
        //    Assert.IsType(typeof(System.IO.MemoryStream), xls);
            
        //}

        //[Fact]
        //public void Should_Success_Get_Report_Data_Excel_Null_Parameter()
        //{
        //    BsonDocument data = DataUtil.GetTestData();
        //    var xls = this.Facade.GenerateExcel(null, null, null, data["date"].ToUniversalTime(), data["date"].ToUniversalTime());
        //    byte[] xlsInBytes = xls.ToArray();
        //    Assert.IsType(typeof(System.IO.MemoryStream), xls);
             
        //}
    }
}
