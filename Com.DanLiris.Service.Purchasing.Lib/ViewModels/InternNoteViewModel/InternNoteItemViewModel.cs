using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternNoteViewModel
{
    public class InternNoteItemViewModel : BaseViewModel
    {
        public Invoice Invoice { get; set; }
        public List<InternNoteFulfillmentViewModel> fulfillments { get; set; }
    }
    public class Invoice
    {
        public long Id { get; set; }
        public string name { get; set; }
        public DateTimeOffset invoiceDate { get; set; }
        public double totalAmount { get; set; }

    }
}
