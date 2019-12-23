using Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Utils
{
    public class CoreDataTest
    {
        [Fact]
        public void Should_Success_Build_ExternalPurchaseDeliveryOrderDurationReportViewModel()
        {
            var viewModel = new ExternalPurchaseDeliveryOrderDurationReportViewModel();
            Assert.NotNull(viewModel);
        }
    }
}
