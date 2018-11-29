using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils
{
    public class GarmentExternalPurchaseOrderDataUtil
    {
        private readonly GarmentExternalPurchaseOrderFacade facade;
        private readonly GarmentInternalPurchaseOrderDataUtil garmentPurchaseOrderDataUtil;

        public GarmentExternalPurchaseOrderDataUtil(GarmentExternalPurchaseOrderFacade facade, GarmentInternalPurchaseOrderDataUtil garmentPurchaseOrderDataUtil)
        {
            this.facade = facade;
            this.garmentPurchaseOrderDataUtil = garmentPurchaseOrderDataUtil;
        }

        public GarmentExternalPurchaseOrder GetNewDataFabric()
        {
            var datas= Task.Run(() => garmentPurchaseOrderDataUtil.GetTestDataByTags()).Result;
            return new GarmentExternalPurchaseOrder
            {
                SupplierId = 1,
                SupplierCode = "Supplier1",
                SupplierImport = true,
                SupplierName = "supplier1",

                Category = "FABRIC",
                DarkPerspiration = "dark",
                WetRubbing = "wet",
                DryRubbing = "dry",
                LightMedPerspiration = "light",
                Washing = "wash",
                Shrinkage = "shrink",
                QualityStandardType = "quality",
                PieceLength = "piece",
                PaymentMethod = "pay",
                PaymentType = "payType",
                IncomeTaxId = "1",
                IncomeTaxName = "income1",
                IncomeTaxRate = "1",

                DeliveryDate = new DateTimeOffset(),
                OrderDate = new DateTimeOffset(),

                CurrencyId = 1,
                CurrencyCode = "currency1",
                CurrencyRate = 1,

                IsApproved = false,
                IsOverBudget = false,
                IsPosted = false,


                Remark = "Remark1",

                Items = new List<GarmentExternalPurchaseOrderItem>
                {
                    new GarmentExternalPurchaseOrderItem
                    {
                        PO_SerialNumber = "PO_SerialNumber1",
                        POId=(int)datas[0].Id,
                        PONo=datas[0].PONo,
                        PRNo=datas[0].PRNo,
                        PRId=1,
                        ProductId = 1,
                        ProductCode = "ProductCode1",
                        ProductName = "ProductName1",

                        DealQuantity = 5,
                        BudgetPrice = 5,

                        DealUomId = 1,
                        DealUomUnit = "UomUnit1",

                        DefaultQuantity=5,
                        DefaultUomId=1,
                        DefaultUomUnit="unit1",

                        UsedBudget=1,

                        PricePerDealUnit=1,
                        Conversion=1,
                        RONo=datas[0].RONo,

                        Remark = "ProductRemark",
                        IsOverBudget=true,
                        OverBudgetRemark="TestRemarkOB",

                        ReceiptQuantity = 0,
                        DOQuantity = 5,
                    }
                }
            };
        }

        public GarmentExternalPurchaseOrder GetNewDataACC()
        {
            var datas = Task.Run(() => garmentPurchaseOrderDataUtil.GetTestDataByTags()).Result;
            return new GarmentExternalPurchaseOrder
            {
                SupplierId = 1,
                SupplierCode = "Supplier1",
                SupplierImport = true,
                SupplierName = "supplier1",

                Category = "ACCESORIES",
                

                IncomeTaxId = "1",
                IncomeTaxName = "income1",
                IncomeTaxRate = "1",

                DeliveryDate = new DateTimeOffset(),
                OrderDate = new DateTimeOffset(),

                CurrencyId = 1,
                CurrencyCode = "currency1",
                CurrencyRate = 1,

                IsApproved = true,
                IsOverBudget = true,
                IsPosted = false,


                Remark = "Remark1",

                Items = new List<GarmentExternalPurchaseOrderItem>
                {
                    new GarmentExternalPurchaseOrderItem
                    {
                        PO_SerialNumber = "PO_SerialNumber1",
                        POId=(int)datas[0].Id,
                        PONo=datas[0].PONo,
                        PRNo=datas[0].PRNo,
                        PRId=1,
                        ProductId = 1,
                        ProductCode = "ProductCode1",
                        ProductName = "ProductName1",

                        DealQuantity = 5,
                        BudgetPrice = 5,

                        DealUomId = 1,
                        DealUomUnit = "UomUnit1",

                        DefaultQuantity=5,
                        DefaultUomId=1,
                        DefaultUomUnit="unit1",
                        

                        UsedBudget=1,

                        PricePerDealUnit=1,
                        Conversion=1,
                        RONo=datas[0].RONo,

                        Remark = "ProductRemark"
                    }
                }
            };
        }

        public GarmentExternalPurchaseOrder GetDataForDo()
        {
            var datas = Task.Run(() => garmentPurchaseOrderDataUtil.GetTestDataByTags()).Result;
            return new GarmentExternalPurchaseOrder
            {
                SupplierId = 1,
                SupplierCode = "Supplier1",
                SupplierImport = true,
                SupplierName = "supplier1",

                Category = "FABRIC",
                DarkPerspiration = "dark",
                WetRubbing = "wet",
                DryRubbing = "dry",
                LightMedPerspiration = "light",
                Washing = "wash",
                Shrinkage = "shrink",
                QualityStandardType = "quality",
                PieceLength = "piece",
                PaymentMethod = "pay",
                PaymentType = "payType",
                IncomeTaxId = "1",
                IncomeTaxName = "income1",
                IncomeTaxRate = "1",

                DeliveryDate = new DateTimeOffset(),
                OrderDate = new DateTimeOffset(),

                CurrencyId = 1,
                CurrencyCode = "currency1",
                CurrencyRate = 1,

                IsApproved = false,
                IsOverBudget = false,
                IsPosted = false,


                Remark = "Remark1",

                Items = new List<GarmentExternalPurchaseOrderItem>
                {
                    new GarmentExternalPurchaseOrderItem
                    {
                        PO_SerialNumber = "PO_SerialNumber1",
                        POId=(int)datas[0].Id,
                        PONo=datas[0].PONo,
                        PRNo=datas[0].PRNo,
                        PRId=1,
                        ProductId = 1,
                        ProductCode = "ProductCode1",
                        ProductName = "ProductName1",

                        DealQuantity = 5,
                        BudgetPrice = 5,

                        DealUomId = 1,
                        DealUomUnit = "UomUnit1",

                        DefaultQuantity=5,
                        DefaultUomId=1,
                        DefaultUomUnit="unit1",

                        UsedBudget=1,

                        PricePerDealUnit=1,
                        Conversion=1,
                        RONo=datas[0].RONo,

                        Remark = "ProductRemark",
                        IsOverBudget=true,
                        OverBudgetRemark="TestRemarkOB",

                        ReceiptQuantity = 0,
                        DOQuantity = 1,
                        
                    }
                }
            };
        }

        public GarmentExternalPurchaseOrder GetDataForDo2()
        {
            var datas = Task.Run(() => garmentPurchaseOrderDataUtil.GetTestDataByTags()).Result;
            return new GarmentExternalPurchaseOrder
            {
                SupplierId = 1,
                SupplierCode = "Supplier1",
                SupplierImport = true,
                SupplierName = "supplier1",

                Category = "FABRIC",
                DarkPerspiration = "dark",
                WetRubbing = "wet",
                DryRubbing = "dry",
                LightMedPerspiration = "light",
                Washing = "wash",
                Shrinkage = "shrink",
                QualityStandardType = "quality",
                PieceLength = "piece",
                PaymentMethod = "pay",
                PaymentType = "payType",
                IncomeTaxId = "1",
                IncomeTaxName = "income1",
                IncomeTaxRate = "1",

                DeliveryDate = new DateTimeOffset(),
                OrderDate = new DateTimeOffset(),

                CurrencyId = 1,
                CurrencyCode = "currency1",
                CurrencyRate = 1,

                IsApproved = false,
                IsOverBudget = false,
                IsPosted = false,


                Remark = "Remark1",

                Items = new List<GarmentExternalPurchaseOrderItem>
                {
                    new GarmentExternalPurchaseOrderItem
                    {
                        PO_SerialNumber = "PO_SerialNumber1",
                        POId=(int)datas[0].Id,
                        PONo=datas[0].PONo,
                        PRNo=datas[0].PRNo,
                        PRId=1,
                        ProductId = 1,
                        ProductCode = "ProductCode1",
                        ProductName = "ProductName1",

                        DealQuantity = 5,
                        BudgetPrice = 5,

                        DealUomId = 1,
                        DealUomUnit = "UomUnit1",

                        DefaultQuantity=5,
                        DefaultUomId=1,
                        DefaultUomUnit="unit1",

                        UsedBudget=1,

                        PricePerDealUnit=1,
                        Conversion=1,
                        RONo=datas[0].RONo,

                        Remark = "ProductRemark",
                        IsOverBudget=true,
                        OverBudgetRemark="TestRemarkOB",

                        ReceiptQuantity = 0,
                        DOQuantity = 0,

                    }
                }
            };
        }

        public async Task<GarmentExternalPurchaseOrder> GetTestDataFabric()
        {
            var data = GetNewDataFabric();
            await facade.Create(data, "Unit Test");
            return data;
        }

        public async Task<GarmentExternalPurchaseOrder> GetTestDataAcc()
        {
            var data = GetNewDataACC();
            await facade.Create(data, "Unit Test");
            return data;
        }

        public async Task<GarmentExternalPurchaseOrder> GetTestDataForDo()
        {
            var data = GetDataForDo();
            await facade.Create(data, "Unit Test");
            return data;
        }
        public async Task<GarmentExternalPurchaseOrder> GetTestDataForDo2()
        {
            var data = GetDataForDo2();
            await facade.Create(data, "Unit Test");
            return data;
        }
    }
}
