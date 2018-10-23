using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils
{
    public class GarmentDeliveryOrderDataUtil
    {
        private readonly GarmentDeliveryOrderFacade facade;

        public GarmentDeliveryOrderDataUtil(GarmentDeliveryOrderFacade facade)
        {
            this.facade = facade;
        }

        public GarmentDeliveryOrder GetNewData()
        {
            Random rnd = new Random();
            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            string nowTicksB = $"{nowTicks}b";

            return new GarmentDeliveryOrder
            {
                DONo = $"{nowTicksA}",

                SupplierId = nowTicks,
                SupplierCode = $"BuyerCode{nowTicksA}",
                SupplierName = $"BuyerName{nowTicksA}",

                DODate = DateTimeOffset.Now,
                ArrivalDate = DateTimeOffset.Now,

                ShipmentType = $"ShipmentType{nowTicksA}",
                ShipmentNo = $"ShipmentNo{nowTicksA}",

                Remark = $"Remark{nowTicksA}",

                IsClosed = false,
                IsCustoms = false,
                IsInvoice = false,

                CustomsId = nowTicks,
                PaymentBill = $"{nowTicksB}",
                BillNo = $"{nowTicksB}",

                TotalAmount = nowTicks,
                TotalQuantity = nowTicks
            };
        }

        public async Task<GarmentDeliveryOrder> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data, "Unit Test");
            return data;
        }
    }
}