using Com.DanLiris.Service.Purchasing.Lib.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Helpers
{
    public class COAGeneratorTest
    {
        [Fact]
        public void Should_Success_Get_COA_Spinning1()
        {
            var result = COAGenerator.GetDebtCOA(false, "SPINNING", "S1");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_Spinning2()
        {
            var result = COAGenerator.GetDebtCOA(false, "SPINNING", "S2");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_Spinning3()
        {
            var result = COAGenerator.GetDebtCOA(false, "SPINNING", "S3");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_SpinningMS()
        {
            var result = COAGenerator.GetDebtCOA(false, "SPINNING", "S4");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_SpinningOther()
        {
            var result = COAGenerator.GetDebtCOA(true, "SPINNING", "other");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_PurchasingWeavingKK()
        {
            var result = COAGenerator.GetPurchasingCOA("WEAVING", "KK", "EMBALAGE");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_PurchasingWeavingE()
        {
            var result = COAGenerator.GetPurchasingCOA("WEAVING", "E", "BAHANBAKU");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_PurchasingWeavingOther()
        {
            var result = COAGenerator.GetPurchasingCOA("WEAVING", "other", "BAHANPEMBANTU");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_FinishingPrintingF1()
        {
            var result = COAGenerator.GetPurchasingCOA("FINISHING&PRINTING", "F1", "BARANGJADI");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_StockFinishingPrintingF2()
        {
            var result = COAGenerator.GetStockCOA("FINISHING&PRINTING", "F2", "BAHANBAKU");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_StockFinishingPrintingOther()
        {
            var result = COAGenerator.GetStockCOA("FINISHING&PRINTING", "other", "BARANGJADI");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC1A()
        {
            var result = COAGenerator.GetIncomeTaxCOA("GARMENT", "C1A");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC1B()
        {
            var result = COAGenerator.GetIncomeTaxCOA("GARMENT", "C1B");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC2A()
        {
            var result = COAGenerator.GetIncomeTaxCOA("GARMENT", "C2A");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC2B()
        {
            var result = COAGenerator.GetIncomeTaxCOA("GARMENT", "C2B");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC2C()
        {
            var result = COAGenerator.GetIncomeTaxCOA("GARMENT", "C2C");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentOther()
        {
            var result = COAGenerator.GetIncomeTaxCOA("GARMENT", "other");
            Assert.NotNull(result);
        }
    }
}
