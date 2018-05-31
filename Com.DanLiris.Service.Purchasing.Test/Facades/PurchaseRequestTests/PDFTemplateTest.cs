using Com.DanLiris.Service.Purchasing.Lib.PDFTemplates;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.PurchaseRequestTests
{
    [Collection("ServiceProviderFixture Collection")]
    public class PDFTemplateTest
    {
        private IServiceProvider ServiceProvider { get; set; }

        public PDFTemplateTest(ServiceProviderFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;
        }

        private PurchaseRequestDataUtil DataUtil
        {
            get { return (PurchaseRequestDataUtil)ServiceProvider.GetService(typeof(PurchaseRequestDataUtil)); }
        }

        [Fact]
        public void Should_Success_Generate_PdfTemplate()
        {
            PurchaseRequestViewModel viewModel = DataUtil.GetNewDataViewModel();
            PurchaseRequestPDFTemplate PdfTemplate = new PurchaseRequestPDFTemplate();
            MemoryStream stream = PdfTemplate.GeneratePdfTemplate(viewModel);

            Assert.True(stream.CanRead);
        }
    }
}
