using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDispositionPurchaseModel
{
    public class GarmentDispositionPurchaseDetail: StandardEntity
    {
        public int ROId { get; set; }
        public string RONo { get; set; }
        public string IPONo { get; set; }
        public int IPOId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string UnitCode { get; set; }
        public double QTYOrder { get; set; }
        public string QTYUnit { get; set; }
        public double QTYRemains { get; set; }
        public double PricePerQTY { get; set; }
        public double PriceTotal { get; set; }
        public double QTYPaid { get; set; }
        public double PaidPrice { get; set; }
        public double PercentageOverQTY { get; set; }
        public int EPO_POId { get; set; }
        public double DispositionAmountPaid { get; set; }
        public double DispositionAmountCreated { get; set; }
        public double DispositionQuantityCreated { get; set; }
        public double DispositionQuantityPaid { get; set; }

        public int GarmentDispositionPurchaseItemId { get; set; }
        [ForeignKey("GarmentDispositionPurchaseItemId")]
        public GarmentDispositionPurchaseItem GarmentDispositionPurchaseItem { get; set; }

    }
}
