using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
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
            return new GarmentPurchaseRequest
            {
                RONo = "RODA881",

                BuyerId = "1",
                BuyerCode = "BuyerCode1",
                BuyerName = "BuyerName1",

                Article = "Article1",

                Date = DateTimeOffset.Now,
                ShipmentDate = DateTimeOffset.Now,

                UnitId = "1",
                UnitCode = "UnitCode1",
                UnitName = "UnitName1",

                Remark = "Remark1",

                Items = new List<GarmentPurchaseRequestItem>
                {
                    new GarmentPurchaseRequestItem
                    {
                        PO_SerialNumber = "PO_SerialNumber1",

                        ProductId = "1",
                        ProductCode = "ProductCode1",
                        ProductName = "ProductName1",

                        Quantity = 5,
                        BudgetPrice = 5,

                        UomId = "1",
                        UomUnit = "UomUnit1",

                        CategoryId = "1",
                        CategoryName = "CategoryName1",

                        ProductRemark = "ProductRemark"
                    },
                    new GarmentPurchaseRequestItem
                    {
                        PO_SerialNumber = "PO_SerialNumber2",

                        ProductId = "2",
                        ProductCode = "ProductCode2",
                        ProductName = "ProductName2",

                        Quantity = 5,
                        BudgetPrice = 5,

                        UomId = "2",
                        UomUnit = "UomUnit2",

                        CategoryId = "2",
                        CategoryName = "CategoryName2",

                        ProductRemark = "ProductRemark"
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

    }
}
