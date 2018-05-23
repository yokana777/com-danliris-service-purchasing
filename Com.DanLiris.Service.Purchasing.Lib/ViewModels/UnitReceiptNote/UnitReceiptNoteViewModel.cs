using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote
{
    public class UnitReceiptNoteViewModel
    {
        public string no { get; set; }
        public DateTimeOffset date { get; set; }
        public UnitViewModel unit { get; set; }
        public string pibNo { get; set; }
        public List<UnitReceiptNoteItemViewModel> items { get; set; }

    }
}
