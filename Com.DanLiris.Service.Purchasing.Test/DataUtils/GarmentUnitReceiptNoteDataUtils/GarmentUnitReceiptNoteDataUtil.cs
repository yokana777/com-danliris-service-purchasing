using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils
{
    public class GarmentUnitReceiptNoteDataUtil
    {
        private readonly GarmentUnitReceiptNoteFacade facade;
        private readonly GarmentDeliveryOrderDataUtil garmentDeliveryOrderDataUtil;

        public GarmentUnitReceiptNoteDataUtil(GarmentUnitReceiptNoteFacade facade, GarmentDeliveryOrderDataUtil garmentDeliveryOrderDataUtil)
        {
            this.facade = facade;
            this.garmentDeliveryOrderDataUtil = garmentDeliveryOrderDataUtil;
        }

        public async Task<GarmentUnitReceiptNote> GetNewData(long? ticks = null)
        {
            long nowTicks = ticks ?? DateTimeOffset.Now.Ticks;

            var garmentDeliveryOrder = await Task.Run(() => garmentDeliveryOrderDataUtil.GetTestData());

            var garmentUnitReceiptNote = new GarmentUnitReceiptNote
            {
                UnitId = nowTicks,
                UnitCode = string.Concat("UnitCode", nowTicks),
                UnitName = string.Concat("UnitName", nowTicks),

                SupplierId = garmentDeliveryOrder.SupplierId,
                SupplierCode = garmentDeliveryOrder.SupplierCode,
                SupplierName = garmentDeliveryOrder.SupplierName,

                DOId = garmentDeliveryOrder.Id,
                DONo = garmentDeliveryOrder.DONo,

                ReceiptDate = DateTimeOffset.Now,

                Items = new List<GarmentUnitReceiptNoteItem>()
            };

            foreach (var item in garmentDeliveryOrder.Items)
            {
                foreach (var detail in item.Details)
                {
                    var garmentUnitReceiptNoteItem = new GarmentUnitReceiptNoteItem
                    {
                        DODetailId = detail.Id,

                        EPOItemId = detail.EPOItemId,

                        PRId = detail.PRId,
                        PRNo = detail.PRNo,
                        PRItemId = detail.PRItemId,

                        POId = detail.POId,
                        POItemId = detail.POItemId,
                        POSerialNumber = detail.POSerialNumber,

                        ProductId = detail.ProductId,
                        ProductCode = detail.ProductCode,
                        ProductName = detail.ProductName,
                        ProductRemark = detail.ProductRemark,

                        RONo = detail.RONo,

                        ReceiptQuantity = (decimal)detail.ReceiptQuantity,

                        UomId = long.Parse(detail.UomId),
                        UomUnit = detail.UomUnit,

                        PricePerDealUnit = (decimal)detail.PricePerDealUnit,

                        DesignColor = string.Concat("DesignColor", nowTicks),

                        SmallQuantity = (decimal)detail.SmallQuantity,

                        SmallUomId = long.Parse(detail.SmallUomId),
                        SmallUomUnit = detail.SmallUomUnit,
                    };

                garmentUnitReceiptNote.Items.Add(garmentUnitReceiptNoteItem);
            }
        }

            return garmentUnitReceiptNote;
        }

        public void SetDataWithStorage(GarmentUnitReceiptNote garmentUnitReceiptNote, long? unitId = null)
        {
            long nowTicks = unitId ?? DateTimeOffset.Now.Ticks;

            garmentUnitReceiptNote.IsStorage = true;
            garmentUnitReceiptNote.StorageId = nowTicks;
            garmentUnitReceiptNote.StorageCode = string.Concat("StorageCode", nowTicks);
            garmentUnitReceiptNote.StorageName = string.Concat("StorageName", nowTicks);
        }


        public async Task<GarmentUnitReceiptNote> GetNewDataWithStorage(long? ticks = null)
        {
            var data = await GetNewData(ticks);
            SetDataWithStorage(data, data.UnitId);

            return data;
        }

        public async Task<GarmentUnitReceiptNote> GetTestData(long? ticks = null)
        {
            var data = await GetNewData(ticks);
            await facade.Create(data);
            return data;
        }

        public async Task<GarmentUnitReceiptNote> GetTestDataWithStorage(long? ticks = null)
        {
            var data = await GetNewDataWithStorage(ticks);
            await facade.Create(data);
            return data;
        }
    }
}
