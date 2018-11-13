using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel
{
    public class PurchasingDispositionDetail :BaseModel
    {

        public long EPODetailId { get; set; }
        public long PRId { get; set; }
        public string PRNo { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public double DealQuantity { get; set; }
        public string DealUomUnit { get; set; }
        public int DealUomId { get; set; }
        public double PaidQuantity { get; set; }
        public double PricePerDealUnit { get; set; }
        public double PriceTotal { get; set; }
        public double PaidPrice { get; set; }
        public virtual long PurchasingDispositionItemId { get; set; }
        [ForeignKey("PurchasingDispositionItemId")]
        public virtual PurchasingDispositionItem PurchasingDispositionItem { get; set; }
    }
}
