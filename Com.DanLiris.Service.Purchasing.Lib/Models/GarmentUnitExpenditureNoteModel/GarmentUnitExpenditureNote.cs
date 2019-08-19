using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel
{
    public class GarmentUnitExpenditureNote : BaseModel
    {
        public string UENNo { get; set; }
        public DateTimeOffset ExpenditureDate { get; set; }
        public string ExpenditureType { get; set; }
        public string ExpenditureTo { get; set; }
        public long UnitDOId { get; set; }
        public string UnitDONo { get; set; }
        public long UnitSenderId { get; set; }
        public string UnitSenderCode { get; set; }
        public string UnitSenderName { get; set; }
        public long StorageId { get; set; }
        public string StorageCode { get; set; }
        public string StorageName { get; set; }
        public long UnitRequestId { get; set; }
        public string UnitRequestCode { get; set; }
        public string UnitRequestName { get; set; }
        public long StorageRequestId { get; set; }
        public string StorageRequestCode { get; set; }
        public string StorageRequestName { get; set; }

        public bool IsPreparing { get; set; }

        public bool IsTransfered { get; set; }

        public virtual ICollection<GarmentUnitExpenditureNoteItem> Items { get; set; }

    }
}
