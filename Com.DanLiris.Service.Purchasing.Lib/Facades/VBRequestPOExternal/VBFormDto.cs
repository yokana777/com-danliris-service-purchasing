using iTextSharp.text;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class VBFormDto
    {
        public DateTimeOffset? Date { get; set; }
        public string DocumentNo { get; set; }
        public List<long> EPOIds { get; set; }
        public List<UPOAndAmountDto> UPOIds { get; set; }
        public double? Amount { get; set; }
    }
}