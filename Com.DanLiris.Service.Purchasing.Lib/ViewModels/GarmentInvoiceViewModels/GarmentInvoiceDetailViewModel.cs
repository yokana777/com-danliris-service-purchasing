using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels
{
    public class GarmentInvoiceDetailViewModel
    {
        public long EPOId { get; set; }
        public string EPONo { get; set; }
        public long IPOId { get; set; }
        public string IPONo { get; set; }
        public ProductViewModel Products{ get; set; }
        public UomViewModel Uoms { get; set; }
        public double DOQuantity { get; set; }
        public double PricePerDealUnit { get; set; }
        
    }
}
