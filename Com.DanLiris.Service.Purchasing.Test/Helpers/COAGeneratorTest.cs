using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentExternalPurchaseOrderViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Helpers
{
    public class COAGeneratorTest
    {
        [Fact]
        public void Should_Success_Generate_UnitAndDivision()
        {
            var result = COAGenerator.GetDivisionAndUnitCOACode("", "");
            Assert.NotNull(result);
        }

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
            var result = COAGenerator.GetIncomeTaxCOA("Final", "GARMENT", "C1A");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC1B()
        {
            var result = COAGenerator.GetIncomeTaxCOA("PASAL21", "GARMENT", "C1B");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC2A()
        {
            var result = COAGenerator.GetIncomeTaxCOA("PASAL23", "GARMENT", "C2A");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC2B()
        {
            var result = COAGenerator.GetIncomeTaxCOA("PASAL26", "GARMENT", "C2B");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentC2C()
        {
            var result = COAGenerator.GetIncomeTaxCOA("PASAL26", "GARMENT", "C2C");
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Success_Get_COA_IncomeTaxGarmentOther()
        {
            var result = COAGenerator.GetIncomeTaxCOA("PASAL26", "GARMENT", "other");
            Assert.NotNull(result);
        }

        [Fact]
        public void Set_Purchase_Order_Delivery_Order_Duration_Report_ViewModel()
        {
            var result = new GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel()
            {
                artikelNo = "",
                buyerName = "",
                category = "",
                dateDiff = 0,
                deliveryOrderNo = "",
                doCreatedDate = DateTime.Now,
                expectedDate = DateTime.Now,
                planPO = "",
                poEksCreatedDate = DateTime.Now,
                poEksNo = "",
                poIntCreatedDate = DateTime.Now,
                poIntNo = "",
                productCode = "",
                productName = "",
                productPrice = 0,
                productQuantity = 0,
                productUom = "",
                roNo = "",
                staff = "",
                supplierCode = "",
                supplierDoDate = DateTime.Now,
                supplierName = "",
                unit = ""
            };

            Assert.NotNull(result);
        }

        [Fact]
        public void Set_Garment_External_Purchase_Order_Over_Budget_Monitoring_ViewModel()
        {
            var result = new GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel()
            {
                budgetPrice = 0,
                no = 1,
                overBudgetRemark = "",
                overBudgetValue = 0,
                overBudgetValuePercentage = 0,
                poExtDate = "",
                poExtNo = "",
                prDate = "",
                prNo = "",
                prRefNo = "",
                price = 0,
                productCode = "",
                productDesc = "",
                productName = "",
                quantity = 0,
                status = "",
                supplierCode = "",
                supplierName = "",
                totalBudgetPrice = 0,
                totalPrice = 0,
                unit = "",
                uom = "",
            };

            Assert.NotNull(result);
        }
    }
}
