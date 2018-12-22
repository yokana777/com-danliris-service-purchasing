//using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderFacades;
//using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
//using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
//using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitReceiptNoteDataUtils;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentUnitDeliveryOrderDataUtils
//{
//    public class GarmentUnitDeliveryOrderDataUtil
//    {
//        private readonly GarmentUnitReceiptNoteDataUtil UNDataUtil;
//        private readonly GarmentUnitDeliveryOrderFacade facade;
//        public GarmentUnitDeliveryOrderDataUtil(GarmentUnitDeliveryOrderFacade facade, GarmentUnitReceiptNoteDataUtil UNDataUtil)
//        {
//            this.facade = facade;
//            this.UNDataUtil = UNDataUtil;
//        }

//        public GarmentUnitDeliveryOrder GetNewData()
//        {
//            var GarmentUnitReceiptNote = Task.Run(() => UNDataUtil.GetTestData()).Result;
//            GarmentUnitDeliveryOrder garmentUnitDeliveryOrder = new GarmentUnitDeliveryOrder
//            {
//                UnitDONo = "NK1234L",
//                UnitDOType = "Jumlah",
//                UnitDODate = DateTimeOffset.Now,
//                UnitSenderId = 1,
//                UnitRequestCode = "unitno",
//                UnitRequestName = "unitname",
//                UnitRequestId =1,
//                UnitSenderCode = "sendercode",
//                UnitSenderName = "sendername",
//                StorageId = 1,
//                StorageCode = "storagecode",
//                StorageName = "storagename",
//                RONo = "RONO",
//                Article = "Article",
//                Items = new List<GarmentUnitDeliveryOrderItem>()
//            };

//            foreach (var item in GarmentUnitReceiptNote.Items)
//            {
//                garmentUnitDeliveryOrder.Items.Add(
//                    new GarmentUnitDeliveryOrderItem
//                    {
//                        DODetailId = item.DODetailId,
//                        EPOItemId = item.EPOItemId,
//                        POItemId = item.POItemId,
//                        PRItemId = item.PRItemId,
//                        FabricType = "fabric",
//                        URNId = GarmentUnitReceiptNote.Id,
//                        URNItemId = item.Id,
//                        URNNo = GarmentUnitReceiptNote.URNNo,
//                        POSerialNumber = item.POSerialNumber,
//                        RONo = item.RONo,
//                        ProductId = item.ProductId,
//                        ProductCode = item.ProductCode,
//                        ProductName = item.ProductName,
//                        Quantity = 8,
//                        UomId = Convert.ToInt32(item.UomId),
//                        UomUnit = item.UomUnit,
//                    });
//            }

//            return garmentUnitDeliveryOrder;
//        }

//        public async Task<GarmentUnitDeliveryOrder> GetTestData()
//        {
//            var data = GetNewData();
//            await facade.Create(data, "Unit Test");
//            return data;
//        }

//    }
//}
