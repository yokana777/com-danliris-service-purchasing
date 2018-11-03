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

                Items = new List<GarmentDeliveryOrderItem>
                {
                    new GarmentDeliveryOrderItem
                    {
                        EPOId = nowTicks,
                        EPONo = $"{nowTicksA}",
                        PaymentType = $"{nowTicksA}",
                        PaymentMethod = $"{nowTicksA}",
                        PaymentDueDays = (int)nowTicks,
                        CurrencyId = nowTicks,
                        CurrencyCode = $"{nowTicksA}",
                        UseVat = false,
                        UseIncomeTax = false,
                        IncomeTaxId = (int)nowTicks,
                        IncomeTaxName = $"{nowTicksA}",
                        IncomeTaxRate = nowTicks,
                        Details = new List<GarmentDeliveryOrderDetail>
                        {
                            new GarmentDeliveryOrderDetail
                            {
                                EPOItemId = nowTicks,
                                POId = (int)nowTicks,
                                POItemId = (int)nowTicks,
                                PRId = nowTicks,
                                PRNo = $"{nowTicksA}",
                                PRItemId = nowTicks,
                                POSerialNumber = $"{nowTicksA}",
                                UnitId = $"{nowTicksA}",
                                UnitCode = $"{nowTicksA}",
                                ProductId = nowTicks,
                                ProductCode = $"{nowTicksA}",
                                ProductName = $"{nowTicksA}",
                                ProductRemark = $"{nowTicksA}",
                                DOQuantity = nowTicks,
                                DealQuantity = nowTicks,
                                Conversion = nowTicks,
                                UomId = $"{nowTicksA}",
                                UomUnit = $"{nowTicksA}",
                                SmallQuantity = nowTicks,
                                SmallUomId = $"{nowTicksA}",
                                SmallUomUnit = $"{nowTicksA}",
                                PricePerDealUnit = nowTicks,
                                PriceTotal = nowTicks,
                                RONo = $"{nowTicksA}",
                                ReceiptQuantity = nowTicks
                            }
                        }
                    }
                }
            };
        }

        public async Task<GarmentDeliveryOrder> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data, "Unit Test");
            return data;
        }

		public async Task<GarmentDeliveryOrder> GetNewData(string user)
		{
			var data = GetNewData();
			await facade.Create(data, "Unit Test");
			return data;
		}
		public async Task<GarmentDeliveryOrder> GetTestDataUnused(string user)
		{
			GarmentDeliveryOrder garmentDeliveryOrder = await  GetNewData(user);
			garmentDeliveryOrder.IsInvoice = false;
			foreach (var item in garmentDeliveryOrder.Items)
			{
				foreach (var detail in item.Details)
				{
					detail.DOQuantity = 0;
					detail.DealQuantity = 2;
				}
			}

			await facade.Create(garmentDeliveryOrder, user, 7);

			return garmentDeliveryOrder;
		}
	}
}