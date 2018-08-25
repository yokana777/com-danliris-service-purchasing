using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote
{
    public class LocalPurchasingBookReportViewModel : BaseViewModel
    {
        public DateTimeOffset receiptDate { get; set; }
        public string uRNNo { get; set; }
        public string productName { get; set; }
        public string invoiceNo { get; set; }
        public string categoryName { get; set; }
        public string unitName { get; set; }
        public string dpp { get; set; }
        public string ppn { get; set; }
        public string total { get; set; }
    }
}