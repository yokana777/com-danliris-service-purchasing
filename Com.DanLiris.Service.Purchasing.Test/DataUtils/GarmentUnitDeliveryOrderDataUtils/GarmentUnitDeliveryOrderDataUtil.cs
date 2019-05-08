using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitDeliveryOrderDataUtils
{
    public class GarmentUnitDeliveryOrderDataUtil
    {
        private readonly GarmentUnitReceiptNoteDataUtil UNDataUtil;
        private readonly GarmentUnitDeliveryOrderFacade facade;
        public GarmentUnitDeliveryOrderDataUtil(GarmentUnitDeliveryOrderFacade facade, GarmentUnitReceiptNoteDataUtil UNDataUtil)
        {
            this.facade = facade;
            this.UNDataUtil = UNDataUtil;
        }

        public async Task<GarmentUnitDeliveryOrder> GetNewData()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long nowTicks = now.Ticks;

            var garmentUnitReceiptNote = await Task.Run(() => UNDataUtil.GetTestDataWithStorage(nowTicks));

            GarmentUnitDeliveryOrder garmentUnitDeliveryOrder = new GarmentUnitDeliveryOrder
            {
                UnitDOType = "SAMPLE",
                UnitDODate = DateTimeOffset.Now,
                UnitSenderId = garmentUnitReceiptNote.UnitId,
                UnitRequestCode = garmentUnitReceiptNote.UnitCode,
                UnitRequestName = garmentUnitReceiptNote.UnitName,
                UnitRequestId = garmentUnitReceiptNote.UnitId,
                UnitSenderCode = garmentUnitReceiptNote.UnitCode,
                UnitSenderName = garmentUnitReceiptNote.UnitName,
                StorageId = garmentUnitReceiptNote.StorageId,
                StorageCode = garmentUnitReceiptNote.StorageCode,
                StorageName = garmentUnitReceiptNote.StorageName,
                RONo = garmentUnitReceiptNote.Items.Select(i => i.RONo).FirstOrDefault(),
                Article = $"Article{nowTicks}",
                Items = new List<GarmentUnitDeliveryOrderItem>()
            };

            foreach (var item in garmentUnitReceiptNote.Items)
            {
                garmentUnitDeliveryOrder.Items.Add(
                    new GarmentUnitDeliveryOrderItem
                    {
                        IsSave = true,
                        DODetailId = item.DODetailId,
                        EPOItemId = item.EPOItemId,
                        POItemId = item.POItemId,
                        PRItemId = item.PRItemId,
                        FabricType = "FABRIC",
                        URNId = garmentUnitReceiptNote.Id,
                        URNItemId = item.Id,
                        URNNo = garmentUnitReceiptNote.URNNo,
                        POSerialNumber = item.POSerialNumber,
                        RONo = item.RONo,
                        ProductId = item.ProductId,
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        Quantity = (double)(item.SmallQuantity - item.OrderQuantity),
                        UomId = item.UomId,
                        UomUnit = item.UomUnit,
                    });
            }

            return garmentUnitDeliveryOrder;
        }

        public async Task<GarmentUnitDeliveryOrder> GetTestData()
        {
            var data = await GetNewData();
            await facade.Create(data);
            return data;
        }
        public async Task<GarmentUnitDeliveryOrder> GetNewDataMultipleItem()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long nowTicks = now.Ticks;

            var garmentUnitReceiptNote1 = await Task.Run(() => UNDataUtil.GetTestDataWithStorage());
            var garmentUnitReceiptNote2 = await Task.Run(() => UNDataUtil.GetTestDataWithStorage(nowTicks + 1));
            GarmentUnitDeliveryOrder garmentUnitDeliveryOrder = new GarmentUnitDeliveryOrder
            {
                UnitDOType = "SAMPLE",
                UnitDODate = DateTimeOffset.Now,
                UnitSenderId = garmentUnitReceiptNote1.UnitId,
                UnitRequestCode = garmentUnitReceiptNote1.UnitCode,
                UnitRequestName = garmentUnitReceiptNote1.UnitName,
                UnitRequestId = garmentUnitReceiptNote1.UnitId,
                UnitSenderCode = garmentUnitReceiptNote1.UnitCode,
                UnitSenderName = garmentUnitReceiptNote1.UnitName,
                StorageId = garmentUnitReceiptNote1.StorageId,
                StorageCode = garmentUnitReceiptNote1.StorageCode,
                StorageName = garmentUnitReceiptNote1.StorageName,
                RONo = garmentUnitReceiptNote1.Items.Select(i => i.RONo).FirstOrDefault(),
                Article = $"Article{nowTicks}",
                Items = new List<GarmentUnitDeliveryOrderItem>()
            };

            foreach (var item in garmentUnitReceiptNote1.Items)
            {
                garmentUnitDeliveryOrder.Items.Add(
                    new GarmentUnitDeliveryOrderItem
                    {
                        IsSave = true,
                        DODetailId = item.DODetailId,
                        EPOItemId = item.EPOItemId,
                        POItemId = item.POItemId,
                        PRItemId = item.PRItemId,
                        FabricType = "FABRIC",
                        URNId = garmentUnitReceiptNote1.Id,
                        URNItemId = item.Id,
                        URNNo = garmentUnitReceiptNote1.URNNo,
                        POSerialNumber = item.POSerialNumber,
                        RONo = item.RONo,
                        ProductId = item.ProductId,
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        Quantity = (double)(item.SmallQuantity - item.OrderQuantity),
                        UomId = item.UomId,
                        UomUnit = item.UomUnit,
                        DOCurrencyRate = garmentUnitReceiptNote1.DOCurrencyRate
                    });
            }


            

            foreach (var item in garmentUnitReceiptNote2.Items)
            {
                garmentUnitDeliveryOrder.Items.Add(
                    new GarmentUnitDeliveryOrderItem
                    {
                        IsSave = true,
                        DODetailId = item.DODetailId,
                        EPOItemId = item.EPOItemId,
                        POItemId = item.POItemId,
                        PRItemId = item.PRItemId,
                        FabricType = "FABRIC",
                        URNId = garmentUnitReceiptNote1.Id,
                        URNItemId = item.Id,
                        URNNo = garmentUnitReceiptNote1.URNNo,
                        POSerialNumber = item.POSerialNumber,
                        RONo = item.RONo,
                        ProductId = item.ProductId+1,
                        ProductCode = item.ProductCode + $"{nowTicks}",
                        ProductName = item.ProductName + $"{nowTicks}",
                        Quantity = (double)(item.SmallQuantity - item.OrderQuantity),
                        UomId = item.UomId,
                        UomUnit = item.UomUnit,
                        DOCurrencyRate = garmentUnitReceiptNote1.DOCurrencyRate
                    });
            }


            return garmentUnitDeliveryOrder;
        }

        public async Task<GarmentUnitDeliveryOrder> GetTestDataMultipleItem()
        {
            var data = await GetNewDataMultipleItem();
            await facade.Create(data);
            return data;
        }

    }
}
