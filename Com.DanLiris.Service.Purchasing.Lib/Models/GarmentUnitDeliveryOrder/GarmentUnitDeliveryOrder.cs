using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel
{
    public class GarmentUnitDeliveryOrder : BaseModel
    {
        public string UnitDOType { get; set; }
        public string UnitDONo { get; set; }
        public DateTimeOffset UnitDODate { get; set; }

        public long UnitRequestId { get; set; }
        public string UnitRequestCode { get; set; }
        public string UnitRequestName { get; set; }

        public long UnitSenderId { get; set; }
        public string UnitSenderCode { get; set; }
        public string UnitSenderName { get; set; }

        public long StorageId { get; set; }
        public string StorageCode { get; set; }
        public string StorageName { get; set; }

        public string RONo { get; set; }
        public string Article { get; set; }
        public bool IsUsed { get; set; }

        public long DOId { get; set; }
        public string DONo { get; set; }

        public long CorrectionId { get; set; }
        public string CorrectionNo { get; set; }


        public long StorageRequestId { get; set; }
        public string StorageRequestCode { get; set; }
        public string StorageRequestName { get; set; }

        public virtual List<GarmentUnitDeliveryOrderItem> Items { get; set; }
    }
}
