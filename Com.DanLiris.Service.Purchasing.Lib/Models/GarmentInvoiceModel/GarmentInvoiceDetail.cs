using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel
{
    public class GarmentInvoiceDetail : StandardEntity<long>
    {
        public long EPOId { get; set; }
        public string EPONo { get; set; }
        public long IPOId { get; set; }
        public string IPONo { get; set; }
        public string ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UomId { get; set; }
        public string UomUnit { get; set; }
        public double DOQuantity { get; set; }
        public double PricePerDealUnit { get; set; }
        public virtual long InvoiceItemId { get; set; }
        [ForeignKey("InvoiceItemId")]
        public virtual GarmentInvoiceItem GarmentInvoiceItem { get; set; }
    }
}
