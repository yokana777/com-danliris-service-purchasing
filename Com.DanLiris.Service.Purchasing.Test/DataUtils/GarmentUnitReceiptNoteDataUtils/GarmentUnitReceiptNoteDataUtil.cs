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

        public GarmentUnitReceiptNote GetNewData()
        {
            long nowTicks = DateTimeOffset.Now.Ticks;

            var gdo = Task.Run(() => garmentDeliveryOrderDataUtil.GetTestData()).Result;

            var garmentUnitReceiptNote = new GarmentUnitReceiptNote
            {
                UnitId = nowTicks,
                UnitCode = string.Concat("UnitCode", nowTicks),
                UnitName = string.Concat("UnitName", nowTicks),

                SupplierId = gdo.SupplierId,
                SupplierCode = gdo.SupplierCode,
                SupplierName = gdo.SupplierName,

                DOId = gdo.Id,
                DONo = gdo.DONo,

                ReceiptDate = DateTimeOffset.Now,

                Items = new List<GarmentUnitReceiptNoteItem>()
            };

            foreach (var item in gdo.Items)
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

        public GarmentUnitReceiptNote GetNewDataWithStorage()
        {
            long nowTicks = DateTimeOffset.Now.Ticks;

            var garmentUnitReceiptNote = GetNewData();

            garmentUnitReceiptNote.IsStorage = true;
            garmentUnitReceiptNote.StorageId = nowTicks;
            garmentUnitReceiptNote.StorageCode = string.Concat("StorageCode", nowTicks);
            garmentUnitReceiptNote.StorageName = string.Concat("StorageName", nowTicks);

            return garmentUnitReceiptNote;
        }

        public async Task<GarmentUnitReceiptNote> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data);
            return data;
        }

        public async Task<GarmentUnitReceiptNote> GetTestDataWithStorage()
        {
            var data = GetNewDataWithStorage();
            await facade.Create(data);
            return data;
        }
    }
}
