using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils
{
    public class GarmentDeliveryOrderDataUtil
    {
        private readonly GarmentDeliveryOrderFacade facade;
        private readonly GarmentExternalPurchaseOrderDataUtil garmentExternalPurchaseOrderDataUtil;

        public GarmentDeliveryOrderDataUtil(GarmentDeliveryOrderFacade facade, GarmentExternalPurchaseOrderDataUtil garmentExternalPurchaseOrderDataUtil)
        {
            this.facade = facade;
            this.garmentExternalPurchaseOrderDataUtil = garmentExternalPurchaseOrderDataUtil;
        }

        public GarmentDeliveryOrder GetNewData()
        {
            var datas = Task.Run(() => garmentExternalPurchaseOrderDataUtil.GetTestDataFabric()).Result;
            List<GarmentExternalPurchaseOrderItem> EPOItem = new List<GarmentExternalPurchaseOrderItem>(datas.Items);
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
                        EPOId = datas.Id,
                        EPONo = datas.EPONo,
                        PaymentType = datas.PaymentType,
                        PaymentMethod = datas.PaymentMethod,
                        PaymentDueDays = datas.PaymentDueDays,
                        CurrencyId = datas.CurrencyId,
                        CurrencyCode = datas.CurrencyCode,
                        UseVat = datas.IsUseVat,
                        UseIncomeTax = datas.IsIncomeTax,
                        IncomeTaxId = Convert.ToInt32(datas.IncomeTaxId),
                        IncomeTaxName = datas.IncomeTaxName,
                        IncomeTaxRate = Convert.ToDouble(datas.IncomeTaxRate),
                        Details = new List<GarmentDeliveryOrderDetail>
                        {
                            new GarmentDeliveryOrderDetail
                            {
                                EPOItemId = EPOItem[0].Id,
                                POId = EPOItem[0].POId,
                                POItemId = (int)nowTicks,
                                PRId = EPOItem[0].PRId,
                                PRNo = EPOItem[0].PRNo,
                                PRItemId = nowTicks,
                                POSerialNumber = EPOItem[0].PO_SerialNumber,
                                UnitId =  $"{nowTicksA}",
                                UnitCode = $"{nowTicksA}",
                                ProductId = EPOItem[0].ProductId,
                                ProductCode = EPOItem[0].ProductCode,
                                ProductName = EPOItem[0].ProductName,
                                ProductRemark = EPOItem[0].Remark,
                                DOQuantity = EPOItem[0].DOQuantity,
                                DealQuantity = EPOItem[0].DealQuantity,
                                Conversion = EPOItem[0].Conversion,
                                UomId = EPOItem[0].DealUomId.ToString(),
                                UomUnit = EPOItem[0].DealUomUnit,
                                SmallQuantity = EPOItem[0].SmallQuantity,
                                SmallUomId = EPOItem[0].SmallUomId.ToString(),
                                SmallUomUnit = EPOItem[0].SmallUomUnit,
                                PricePerDealUnit = EPOItem[0].PricePerDealUnit,
                                PriceTotal = EPOItem[0].PricePerDealUnit,
                                RONo = EPOItem[0].RONo,
                                ReceiptQuantity = 0
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