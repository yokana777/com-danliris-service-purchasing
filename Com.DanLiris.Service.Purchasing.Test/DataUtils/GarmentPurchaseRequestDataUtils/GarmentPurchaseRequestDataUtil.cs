using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils
{
    public class GarmentPurchaseRequestDataUtil
    {
        private readonly GarmentPurchaseRequestFacade facade;

        public GarmentPurchaseRequestDataUtil(GarmentPurchaseRequestFacade facade)
        {
            this.facade = facade;
        }

        public GarmentPurchaseRequest GetNewData()
        {
            Random rnd = new Random();
            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            string nowTicksB = $"{nowTicks}b";

            return new GarmentPurchaseRequest
            {
                RONo = $"RO{nowTicksA}",

                BuyerId = $"{nowTicksA}",
                BuyerCode = $"BuyerCode{nowTicksA}",
                BuyerName = $"BuyerName{nowTicksA}",

                Article = $"Article{nowTicksA}",

                Date = DateTimeOffset.Now,
                ShipmentDate = DateTimeOffset.Now,

                UnitId = $"{nowTicksA}",
                UnitCode = $"UnitCode{nowTicksA}",
                UnitName = $"UnitName{nowTicksA}",

                Remark = $"Remark{nowTicksA}",

                Items = new List<GarmentPurchaseRequestItem>
                {
                    new GarmentPurchaseRequestItem
                    {
                        PO_SerialNumber = $"PO_SerialNumber{nowTicksA}",

                        ProductId = $"{nowTicksA}",
                        ProductCode = $"ProductCode{nowTicksA}",
                        ProductName = $"ProductName{nowTicksA}",

                        Quantity = rnd.Next(2, 100),
                        BudgetPrice = rnd.Next(2, 100),

                        UomId = $"{nowTicksA}",
                        UomUnit = $"UomUnit{nowTicksA}",

                        CategoryId = $"{nowTicksA}",
                        CategoryName = $"CategoryName{nowTicksA}",

                        ProductRemark = $"ProductRemark{nowTicksA}"
                    },
                    new GarmentPurchaseRequestItem
                    {
                        PO_SerialNumber = $"PO_SerialNumber{nowTicksB}",

                        ProductId = $"{nowTicksB}",
                        ProductCode = $"ProductCode{nowTicksB}",
                        ProductName = $"ProductName{nowTicksB}",

                        Quantity = rnd.Next(2, 100),
                        BudgetPrice = rnd.Next(2, 100),

                        UomId = $"{nowTicksB}",
                        UomUnit = $"UomUnit{nowTicksB}",

                        CategoryId = $"{nowTicksB}",
                        CategoryName = $"CategoryName{nowTicksB}",

                        ProductRemark = $"ProductRemark{nowTicksB}"
                    }
                }
            };
        }

        public async Task<GarmentPurchaseRequest> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data, "Unit Test");
            return data;
        }

        public async Task<List<GarmentInternalPurchaseOrder>> GetTestDataByTags()
        {
            var testData = await GetTestData();
            return facade.ReadByTags($"#{testData.UnitName}#{testData.BuyerName}", DateTimeOffset.MinValue, DateTimeOffset.MinValue);
        }

    }
}
