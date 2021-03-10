using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDispositionPurchaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase
{
    public class FormItemDto
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
        public List<FormDetailDto> Details { get; set; }
    }
}
