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

                UseVat = datas.IsUseVat,
                UseIncomeTax = datas.IsIncomeTax,
                IncomeTaxId = Convert.ToInt32(datas.IncomeTaxId),
                IncomeTaxName = datas.IncomeTaxName,
                IncomeTaxRate = Convert.ToDouble(datas.IncomeTaxRate),

                IsCorrection = false,

                CustomsId = nowTicks,
                PaymentBill = "BB181122003",
                BillNo = "BP181122142947000001",
				PaymentType = datas.PaymentType,
                PaymentMethod = datas.PaymentMethod,
                DOCurrencyId = datas.CurrencyId,
                DOCurrencyCode = datas.CurrencyCode,
                DOCurrencyRate = datas.CurrencyRate,

                TotalAmount = nowTicks,

                Items = new List<GarmentDeliveryOrderItem>
                {
                    new GarmentDeliveryOrderItem
                    {
                        EPOId = datas.Id,
                        EPONo = datas.EPONo,
                        CurrencyId = datas.CurrencyId,
                        CurrencyCode = "USD",
                        PaymentDueDays = datas.PaymentDueDays,

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
                                ReceiptQuantity = 0,
                                QuantityCorrection = EPOItem[0].DOQuantity,
                                PricePerDealUnitCorrection = EPOItem[0].PricePerDealUnit,
                                PriceTotalCorrection = EPOItem[0].PricePerDealUnit,
                            }
                        }
                    }
                }
            };
        }

        public GarmentDeliveryOrder GetNewData2()
        {
            var datas = Task.Run(() => garmentExternalPurchaseOrderDataUtil.GetTestDataForDo()).Result;
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

                UseVat = datas.IsUseVat,
                UseIncomeTax = datas.IsIncomeTax,
                IncomeTaxId = Convert.ToInt32(datas.IncomeTaxId),
                IncomeTaxName = datas.IncomeTaxName,
                IncomeTaxRate = Convert.ToDouble(datas.IncomeTaxRate),

                IsCorrection = false,

                CustomsId = nowTicks,
                PaymentBill = $"{nowTicksB}",
                BillNo = $"{nowTicksB}",
                PaymentType = datas.PaymentType,
                PaymentMethod = datas.PaymentMethod,
                DOCurrencyId = datas.CurrencyId,
                DOCurrencyCode = datas.CurrencyCode,
                DOCurrencyRate = datas.CurrencyRate,

                TotalAmount = nowTicks,

                Items = new List<GarmentDeliveryOrderItem>
                {
                    new GarmentDeliveryOrderItem
                    {
                        EPOId = datas.Id,
                        EPONo = datas.EPONo,
                        CurrencyId = datas.CurrencyId,
                        CurrencyCode = "USD",
                        PaymentDueDays = datas.PaymentDueDays,

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
                                ReceiptQuantity = 0,
                                QuantityCorrection = EPOItem[0].DOQuantity,
                                PricePerDealUnitCorrection = EPOItem[0].PricePerDealUnit,
                                PriceTotalCorrection = EPOItem[0].PricePerDealUnit,
                            }
                        }
                    }
                }
            };
        }

        public GarmentDeliveryOrder GetNewData3()
        {
            var datas = Task.Run(() => garmentExternalPurchaseOrderDataUtil.GetTestDataForDo2()).Result;
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

                UseVat = datas.IsUseVat,
                UseIncomeTax = datas.IsIncomeTax,
                IncomeTaxId = Convert.ToInt32(datas.IncomeTaxId),
                IncomeTaxName = datas.IncomeTaxName,
                IncomeTaxRate = Convert.ToDouble(datas.IncomeTaxRate),

                IsCorrection = false,

                CustomsId = nowTicks,
                PaymentBill = $"{nowTicksB}",
                BillNo = $"{nowTicksB}",
                PaymentType = datas.PaymentType,
                PaymentMethod = datas.PaymentMethod,
                DOCurrencyId = datas.CurrencyId,
                DOCurrencyCode = datas.CurrencyCode,
                DOCurrencyRate = datas.CurrencyRate,

                TotalAmount = nowTicks,

                Items = new List<GarmentDeliveryOrderItem>
                {
                    new GarmentDeliveryOrderItem
                    {
                        EPOId = datas.Id,
                        EPONo = datas.EPONo,
                        CurrencyId = datas.CurrencyId,
                        CurrencyCode = "USD",
                        PaymentDueDays = datas.PaymentDueDays,

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
                                ReceiptQuantity = 0,
                                QuantityCorrection = EPOItem[0].DOQuantity,
                                PricePerDealUnitCorrection = EPOItem[0].PricePerDealUnit,
                                PriceTotalCorrection = EPOItem[0].PricePerDealUnit,
                            }
                        }
                    }
                }
            };
        }

        public GarmentDeliveryOrder GetNewData4()
        {
            var datas = Task.Run(() => garmentExternalPurchaseOrderDataUtil.GetTestDataForDo2()).Result;
            List<GarmentExternalPurchaseOrderItem> EPOItem = new List<GarmentExternalPurchaseOrderItem>(datas.Items);
            Random rnd = new Random();
            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            string nowTicksB = $"{nowTicks}b";

            return new GarmentDeliveryOrder
            {
                DONo = $"{nowTicksB}",

                SupplierId = nowTicks,
                SupplierCode = $"BuyerCode{nowTicksB}",
                SupplierName = $"BuyerName{nowTicksB}",

                DODate = DateTimeOffset.Now,
                ArrivalDate = DateTimeOffset.Now,

                ShipmentType = $"ShipmentType{nowTicksB}",
                ShipmentNo = $"ShipmentNo{nowTicksB}",

                Remark = $"Remark{nowTicksB}",

                IsClosed = false,
                IsCustoms = false,
                IsInvoice = false,

                UseVat = datas.IsUseVat,
                UseIncomeTax = datas.IsIncomeTax,
                IncomeTaxId = Convert.ToInt32(datas.IncomeTaxId),
                IncomeTaxName = datas.IncomeTaxName,
                IncomeTaxRate = Convert.ToDouble(datas.IncomeTaxRate),

                IsCorrection = false,

                CustomsId = nowTicks,
                PaymentBill = $"{nowTicksA}",
                BillNo = $"{nowTicksA}",
                PaymentType = datas.PaymentType,
                PaymentMethod = datas.PaymentMethod,
                DOCurrencyId = datas.CurrencyId,
                DOCurrencyCode = datas.CurrencyCode,
                DOCurrencyRate = datas.CurrencyRate,

                TotalAmount = nowTicks,

                Items = new List<GarmentDeliveryOrderItem>
                {
                    new GarmentDeliveryOrderItem
                    {
                        EPOId = datas.Id,
                        EPONo = datas.EPONo,
                        CurrencyId = datas.CurrencyId,
                        CurrencyCode = "USD",
                        PaymentDueDays = datas.PaymentDueDays,

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
                                UnitId =  $"{nowTicksB}",
                                UnitCode = $"{nowTicksB}",
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
                                ReceiptQuantity = 0,
                                QuantityCorrection = EPOItem[0].DOQuantity,
                                PricePerDealUnitCorrection = EPOItem[0].PricePerDealUnit,
                                PriceTotalCorrection = EPOItem[0].PricePerDealUnit,
                            }
                        }
                    },
                    new GarmentDeliveryOrderItem
                    {
                        EPOId = datas.Id,
                        EPONo = datas.EPONo,
                        CurrencyId = datas.CurrencyId,
                        CurrencyCode = "USD",
                        PaymentDueDays = datas.PaymentDueDays,

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
                                UnitId =  $"{nowTicksB}",
                                UnitCode = $"{nowTicksB}",
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
                                ReceiptQuantity = 0,
                                QuantityCorrection = EPOItem[0].DOQuantity,
                                PricePerDealUnitCorrection = EPOItem[0].PricePerDealUnit,
                                PriceTotalCorrection = EPOItem[0].PricePerDealUnit,
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

        public async Task<GarmentDeliveryOrder> GetTestData2()
        {
            var data = GetNewData2();
            await facade.Create(data, "Unit Test");
            return data;
        }

        public async Task<GarmentDeliveryOrder> GetTestData3()
        {
            var data = GetNewData3();
            await facade.Create(data, "Unit Test");
            return data;
        }

        public async Task<GarmentDeliveryOrder> GetTestData4()
        {
            var data = GetNewData4();
            await facade.Create(data, "Unit Test");
            return data;
        }

        public async Task<GarmentDeliveryOrder> GetNewData(string user)
		{
			var data = GetNewData();
			await facade.Create(data, "Unit Test");
			return data;
		}
		public async Task<GarmentDeliveryOrder> GetDatas(string user)
		{
			GarmentDeliveryOrder garmentDeliveryOrder =  GetNewData();
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