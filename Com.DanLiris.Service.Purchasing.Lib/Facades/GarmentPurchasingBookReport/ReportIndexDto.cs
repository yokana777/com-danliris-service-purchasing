using System;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public class ReportIndexDto
    {
        public ReportIndexDto(DateTimeOffset customsArrivalDate, int supplierId, string supplierName, bool isImportSupplier, string productName, int garmentDeliveryOrderId, string garmentDeliveryOrderNo, string billNo, string paymentBill, int invoiceId, string invoiceNo, string vatNo, int internalNoteId, string internalNoteNo, int purchasingCategoryId, string purchasingCategoryName, int accountingCategoryId, string accountingCategoryName, double internalNoteQuantity, int currencyId, string currencyCode, double currencyRate, double dppAmount, bool isUseVAT, bool isPayVAT, bool isUseIncomeTax, bool isIncomeTaxPaidBySupplier, double incomeTaxRate)
        {
            Total = dppAmount;

            var vatAmount = 0.0;
            if (isUseVAT && isPayVAT)
            {
                vatAmount = dppAmount * 0.1;
                Total += vatAmount;
            }

            var incomeTaxAmount = 0.0;
            if (isIncomeTaxPaidBySupplier)
            {
                incomeTaxAmount = dppAmount * incomeTaxRate;
                Total -= incomeTaxAmount;
            }

            CustomsArrivalDate = customsArrivalDate;
            SupplierId = supplierId;
            SupplierName = supplierName;
            IsImportSupplier = isImportSupplier;
            ProductName = productName;
            GarmentDeliveryOrderId = garmentDeliveryOrderId;
            GarmentDeliveryOrderNo = garmentDeliveryOrderNo;
            BillNo = billNo;
            PaymentBill = paymentBill;
            InvoiceId = invoiceId;
            InvoiceNo = invoiceNo;
            VATNo = vatNo;
            InternalNoteId = internalNoteId;
            InternalNoteNo = internalNoteNo;
            PurchasingCategoryId = purchasingCategoryId;
            PurchasingCategoryName = purchasingCategoryName;
            AccountingCategoryId = accountingCategoryId;
            AccountingCategoryName = accountingCategoryName;
            InternalNoteQuantity = internalNoteQuantity;
            CurrencyId = currencyId;
            CurrencyCode = currencyCode;
            CurrencyRate = currencyRate;
            DPPAmount = dppAmount;
            VATAmount = vatAmount;
            IncomeTaxAmount = incomeTaxAmount;
            IsIncomeTaxPaidBySupplier = isIncomeTaxPaidBySupplier;
        }

        public double Total { get; private set; }
        public DateTimeOffset CustomsArrivalDate { get; private set; }
        public int SupplierId { get; private set; }
        public string SupplierName { get; private set; }
        public bool IsImportSupplier { get; private set; }
        public string ProductName { get; private set; }
        public int GarmentDeliveryOrderId { get; private set; }
        public string GarmentDeliveryOrderNo { get; private set; }
        public string BillNo { get; private set; }
        public string PaymentBill { get; private set; }
        public int InvoiceId { get; private set; }
        public string InvoiceNo { get; private set; }
        public string VATNo { get; private set; }
        public int InternalNoteId { get; private set; }
        public string InternalNoteNo { get; private set; }
        public int PurchasingCategoryId { get; private set; }
        public string PurchasingCategoryName { get; private set; }
        public int AccountingCategoryId { get; private set; }
        public string AccountingCategoryName { get; private set; }
        public double InternalNoteQuantity { get; private set; }
        public int CurrencyId { get; private set; }
        public string CurrencyCode { get; private set; }
        public double CurrencyRate { get; private set; }
        public double DPPAmount { get; private set; }
        public double VATAmount { get; private set; }
        public double IncomeTaxAmount { get; private set; }
        public bool IsIncomeTaxPaidBySupplier { get; private set; }
    }
}