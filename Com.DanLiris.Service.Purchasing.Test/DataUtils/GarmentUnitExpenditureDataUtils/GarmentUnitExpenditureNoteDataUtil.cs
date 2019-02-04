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

            var garmentUnitDeliveryOrder = Task.Run(() => garmentUnitDeliveryOrderDataUtil.GetTestData()).Result;

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
                    IsSave = true,
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

                garmentUnitExpenditureNote.Items.Add(garmentUnitExpenditureNoteItem);

            }

            return garmentUnitExpenditureNote;
        }

        public async Task<GarmentUnitExpenditureNote> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data);
            return data;
        }
    }
}
