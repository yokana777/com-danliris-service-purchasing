using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel
{
    public class GarmentInternNoteItemViewModel : BaseViewModel
    {
        public GarmentInvoiceViewModel garmentInvoice { get; set; }
        public double totalAmount { get; set; }
        public List<GarmentInternNoteDetailViewModel> detail { get; set; }
    }
}
