using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentClosingDateModels
{
    public class GarmentClosingDate : BaseModel
    {
        public DateTimeOffset ClosingDate { get; set; }
    }
}
