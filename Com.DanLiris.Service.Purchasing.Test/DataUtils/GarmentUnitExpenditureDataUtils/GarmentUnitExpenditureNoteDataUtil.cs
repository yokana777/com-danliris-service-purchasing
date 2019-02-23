using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitExpenditureNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitDeliveryOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitExpenditureDataUtils
{
    public class GarmentUnitExpenditureNoteDataUtil
    {
        private readonly GarmentUnitExpenditureNoteFacade facade;
        private readonly GarmentUnitDeliveryOrderDataUtil garmentUnitDeliveryOrderDataUtil;

        public GarmentUnitExpenditureNoteDataUtil(GarmentUnitExpenditureNoteFacade facade, GarmentUnitDeliveryOrderDataUtil garmentUnitDeliveryOrderDataUtil)
        {
            this.facade = facade;
            this.garmentUnitDeliveryOrderDataUtil = garmentUnitDeliveryOrderDataUtil;
        }

        public GarmentUnitExpenditureNote GetNewData()
        {
            long nowTicks = DateTimeOffset.Now.Ticks;

            var garmentUnitDeliveryOrder = Task.Run(() => garmentUnitDeliveryOrderDataUtil.GetTestDataMultipleItem()).Result;

            var garmentUnitExpenditureNote = new GarmentUnitExpenditureNote
            {
                UnitSenderId = garmentUnitDeliveryOrder.UnitSenderId,
                UnitSenderCode = garmentUnitDeliveryOrder.UnitSenderCode,
                UnitSenderName = garmentUnitDeliveryOrder.UnitSenderName,

                UnitRequestId = garmentUnitDeliveryOrder.UnitRequestId,
                UnitRequestCode = garmentUnitDeliveryOrder.UnitRequestCode,
                UnitRequestName = garmentUnitDeliveryOrder.UnitRequestName,

                UnitDOId = garmentUnitDeliveryOrder.Id,
                UnitDONo = garmentUnitDeliveryOrder.UnitDONo,

                StorageId = garmentUnitDeliveryOrder.StorageId,
                StorageCode = garmentUnitDeliveryOrder.StorageCode,
                StorageName = garmentUnitDeliveryOrder.StorageName,

                StorageRequestId = garmentUnitDeliveryOrder.StorageRequestId,
                StorageRequestCode = garmentUnitDeliveryOrder.StorageRequestCode,
                StorageRequestName = garmentUnitDeliveryOrder.StorageRequestName,

                ExpenditureType = "PROSES",
                ExpenditureTo = "PROSES",
                UENNo = "UENNO12345",

                ExpenditureDate = DateTimeOffset.Now,

                Items = new List<GarmentUnitExpenditureNoteItem>()
            };

            foreach (var item in garmentUnitDeliveryOrder.Items)
            {
                var garmentUnitExpenditureNoteItem = new GarmentUnitExpenditureNoteItem
                {
                    IsSave = false,
                    DODetailId = item.DODetailId,
                    
                    EPOItemId = item.EPOItemId,

                    URNItemId = item.URNItemId,
                    UnitDOItemId = item.Id,
                    PRItemId = item.PRItemId,

                    FabricType = item.FabricType,
                    POItemId = item.POItemId,
                    POSerialNumber = item.POSerialNumber,

                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    ProductRemark = item.ProductRemark,
                    Quantity = 5,

                    RONo = item.RONo,

                    UomId = item.UomId,
                    UomUnit = item.UomUnit,

                    PricePerDealUnit = item.PricePerDealUnit,
                };
                new GarmentUnitExpenditureNoteItem
                {
                    Id = 0,
                    IsSave = false,
                    DODetailId = item.DODetailId,

                    EPOItemId = item.EPOItemId,

                    URNItemId = item.URNItemId,
                    UnitDOItemId = item.Id,
                    PRItemId = item.PRItemId,

                    FabricType = item.FabricType,
                    POItemId = item.POItemId,
                    POSerialNumber = item.POSerialNumber,

                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    ProductRemark = item.ProductRemark,
                    Quantity = item.Quantity,

                    RONo = item.RONo,

                    UomId = item.UomId,
                    UomUnit = item.UomUnit,

                    PricePerDealUnit = item.PricePerDealUnit,

                };

                garmentUnitExpenditureNote.Items.Add(garmentUnitExpenditureNoteItem);

            }

            return garmentUnitExpenditureNote;
        }

        public GarmentUnitExpenditureNote GetNewDataTypeTransfer()
        {
            long nowTicks = DateTimeOffset.Now.Ticks;

            var garmentUnitDeliveryOrder = Task.Run(() => garmentUnitDeliveryOrderDataUtil.GetTestDataMultipleItem()).Result;

            var garmentUnitExpenditureNote = new GarmentUnitExpenditureNote
            {
                UnitSenderId = garmentUnitDeliveryOrder.UnitSenderId,
                UnitSenderCode = garmentUnitDeliveryOrder.UnitSenderCode,
                UnitSenderName = garmentUnitDeliveryOrder.UnitSenderName,

                UnitRequestId = garmentUnitDeliveryOrder.UnitRequestId,
                UnitRequestCode = garmentUnitDeliveryOrder.UnitRequestCode,
                UnitRequestName = garmentUnitDeliveryOrder.UnitRequestName,

                UnitDOId = garmentUnitDeliveryOrder.Id,
                UnitDONo = garmentUnitDeliveryOrder.UnitDONo,

                StorageId = garmentUnitDeliveryOrder.StorageId,
                StorageCode = garmentUnitDeliveryOrder.StorageCode,
                StorageName = garmentUnitDeliveryOrder.StorageName,

                StorageRequestId = garmentUnitDeliveryOrder.StorageRequestId,
                StorageRequestCode = garmentUnitDeliveryOrder.StorageRequestCode,
                StorageRequestName = garmentUnitDeliveryOrder.StorageRequestName,

                ExpenditureType = "TRANSFER",
                ExpenditureTo = "TRANSFER",
                UENNo = "UENNO12345",

                ExpenditureDate = DateTimeOffset.Now,

                Items = new List<GarmentUnitExpenditureNoteItem>()
            };

            foreach (var item in garmentUnitDeliveryOrder.Items)
            {
                var garmentUnitExpenditureNoteItem = new GarmentUnitExpenditureNoteItem
                {
                    IsSave = false,
                    DODetailId = item.DODetailId,

                    EPOItemId = item.EPOItemId,

                    URNItemId = item.URNItemId,
                    UnitDOItemId = item.Id,
                    PRItemId = item.PRItemId,

                    FabricType = item.FabricType,
                    POItemId = item.POItemId,
                    POSerialNumber = item.POSerialNumber,

                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    ProductRemark = item.ProductRemark,
                    Quantity = 10,

                    RONo = item.RONo,

                    UomId = item.UomId,
                    UomUnit = item.UomUnit,

                    PricePerDealUnit = item.PricePerDealUnit,
                };
                new GarmentUnitExpenditureNoteItem
                {
                    Id = 0,
                    IsSave = false,
                    DODetailId = item.DODetailId,

                    EPOItemId = item.EPOItemId,

                    URNItemId = item.URNItemId,
                    UnitDOItemId = item.Id,
                    PRItemId = item.PRItemId,

                    FabricType = item.FabricType,
                    POItemId = item.POItemId,
                    POSerialNumber = item.POSerialNumber,

                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    ProductRemark = item.ProductRemark,
                    Quantity = item.Quantity,

                    RONo = item.RONo,

                    UomId = item.UomId,
                    UomUnit = item.UomUnit,

                    PricePerDealUnit = item.PricePerDealUnit,

                };

                garmentUnitExpenditureNote.Items.Add(garmentUnitExpenditureNoteItem);

            }

            return garmentUnitExpenditureNote;
        }
        public void SetDataWithStorage(GarmentUnitExpenditureNote garmentUnitExpenditureNote, long? unitId = null)
        {
            long nowTicks = unitId ?? DateTimeOffset.Now.Ticks;

            garmentUnitExpenditureNote.StorageId = nowTicks;
            garmentUnitExpenditureNote.StorageCode = string.Concat("StorageCode", nowTicks);
            garmentUnitExpenditureNote.StorageName = string.Concat("StorageName", nowTicks);
        }


        public GarmentUnitExpenditureNote GetNewDataWithStorage(long? ticks = null)
        {
            var data = GetNewDataTypeTransfer();
            SetDataWithStorage(data, ticks);

            return data;
        }
        public void SetDataWithStorageRequest(GarmentUnitExpenditureNote garmentUnitExpenditureNote, long? unitId = null)
        {
            long nowTicks = unitId ?? DateTimeOffset.Now.Ticks;

            garmentUnitExpenditureNote.StorageRequestId = nowTicks;
            garmentUnitExpenditureNote.StorageRequestCode = string.Concat("StorageCode", nowTicks);
            garmentUnitExpenditureNote.StorageRequestName = string.Concat("StorageName", nowTicks);
        }


        public GarmentUnitExpenditureNote GetNewDataWithStorageRequest(long? ticks = null)
        {
            var data = GetNewDataTypeTransfer();
            SetDataWithStorageRequest(data, ticks);

            return data;
        }

        public async Task<GarmentUnitExpenditureNote> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data);
            return data;
        }

        public async Task<GarmentUnitExpenditureNote> GetTestDataAcc()
        {
            var data = GetNewDataTypeTransfer();
            await facade.Create(data);
            return data;
        }

        public async Task<GarmentUnitExpenditureNote> GetTestDataWithStorage(long? ticks = null)
        {
            var data = GetNewDataWithStorage(ticks);
            await facade.Create(data);
            return data;
        }
        public async Task<GarmentUnitExpenditureNote> GetTestDataWithStorageReqeust(long? ticks = null)
        {
            var data = GetNewDataWithStorageRequest(ticks);
            await facade.Create(data);
            return data;
        }
    }
}
