using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel
{
    public class ExternalPurchaseOrderGenerateDataViewModel : BaseViewModel
    {
        public string EPONo { get; set; }
        public DateTimeOffset EPODate { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public DateTimeOffset DeliveryDate { get; set; }
        public string FreightCostBy { get; set; }
        public string PaymentMethod { get; set; }
        public string POCashType { get; set; }
        public string PaymentDueDays { get; set; }
        public string CurrencyCode { get; set; }
        public double CurrencyRate { get; set; }
        public string UseVat { get; set; }
        public string UseIncomeTax { get; set; }
        public string IncomeTaxRate { get; set; }
        public string Remark { get; set; }
        public string PRNo { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UOMUnit { get; set; }
        public double DealQuantity { get; set; }
        public double PricePerDealUnit { get; set; }
        public double Amount { get; set; }
        public string UserCreated { get; set; }
        public string IsPosted { get; set; }
    }
}
