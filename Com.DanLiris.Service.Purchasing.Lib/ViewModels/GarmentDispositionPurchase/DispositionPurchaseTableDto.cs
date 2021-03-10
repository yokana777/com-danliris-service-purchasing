using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase
{
    public class DispositionPurchaseTableDto
    {
        public string DispositionNo { get; set; }
        public DateTimeOffset DispositionDate { get; set; }
        public string Category { get; set; }
        public string Supplier { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string Currency { get; set; }
        public double AmountDisposition { get; set; }
    }
}
