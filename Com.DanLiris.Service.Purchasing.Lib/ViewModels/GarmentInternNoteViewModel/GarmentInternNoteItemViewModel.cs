using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternNoteViewModel
{
    public class GarmentInternNoteItemViewModel : BaseViewModel
    {
        public Invoice Invoice { get; set; }
        public List<GarmentInternNoteFulfillmentViewModel> fulfillments { get; set; }
    }
    public class Invoice
    {
        public long Id { get; set; }
        public string no { get; set; }
        public DateTimeOffset invoiceDate { get; set; }
        public double totalAmount { get; set; }
    }
}
