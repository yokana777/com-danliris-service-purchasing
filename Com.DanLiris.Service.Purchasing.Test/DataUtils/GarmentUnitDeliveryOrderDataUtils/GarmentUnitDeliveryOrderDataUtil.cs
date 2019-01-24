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

        public GarmentUnitDeliveryOrder GetNewData()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long nowTicks = now.Ticks;

            var garmentUnitReceiptNote = Task.Run(() => UNDataUtil.GetTestData()).Result;

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
                StorageId = 1,
                StorageCode = $"SupplierCode{nowTicks}",
                StorageName = $"SupplierName{nowTicks}",
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
            var data = GetNewData();
            await facade.Create(data);
            return data;
        }

    }
}
