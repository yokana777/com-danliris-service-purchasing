using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels
{
    public class GarmentInvoiceDetailViewModel
    {
        public long ePOId { get; set; }
        public string ePONo { get; set; }
        public long iPOId { get; set; }
        public string iPONo { get; set; }
        public ProductViewModel product{ get; set; }
        public UomViewModel uoms { get; set; }
        public double dOQuantity { get; set; }
        public double pricePerDealUnit { get; set; }
        public string paymentType { get; set; }
        public string paymentMethod { get; set; }
        public int paymentDueDays { get; set; }
    }
}
