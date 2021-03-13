using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDispositionPurchaseModel
{
    public class GarmentDispositionPurchaseItem : StandardEntity
    {
        public string EPONo { get; set; }
        public int EPOId { get; set; }
        public bool IsVAT { get; set; }
        public double VATAmount { get; set; }
        public bool IsIncomeTax { get; set; }
        public double IncomeTaxAmount { get; set; }
        public bool IsDispositionCreated { get; set; }
        public bool IsDispositionPaid { get; set; }
        public int GarmentDispositionPurchaseId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public double CurrencyRate { get; set; }
        [ForeignKey("GarmentDispositionPurchaseId")]
        public virtual GarmentDispositionPurchase GarmentDispositionPurchase { get; set; }
        public virtual List<GarmentDispositionPurchaseDetail> GarmentDispositionPurchaseDetails { get; set; }


    }
}
