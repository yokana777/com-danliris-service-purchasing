using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDispositionPurchaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase
{
    public class FormDto
    {
        public int Id { get; set; }
        public string DispositionNo { get; set; }
        public string Category { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public bool SupplierIsImport { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public string Bank { get; set; }
        public string ConfirmationOrderNo { get; set; }
        public string PaymentType { get; set; }
        public DateTimeOffset PaymentDueDate { get; set; }
        public string Remark { get; set; }
        public string ProformaNo { get; set; }
        public double DPP { get; set; }
        public double IncomeTaxValue { get; set; }
        public double VatValue { get; set; }
        public double MiscAmount { get; set; }
        public double Amount { get; set; }
        public List<FormItemDto> Items { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

    }
}
