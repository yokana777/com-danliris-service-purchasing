using System;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingExpedition
{
    public class GarmentInternalNoteDto
    {
        public GarmentInternalNoteDto(int id, string documentNo, DateTimeOffset date, DateTimeOffset dueDate, int supplierId, string supplierName, double vat, double incomeTax, double totalPaid, int currencyId, string currencyCode)
        {
            Id = id;
            DocumentNo = documentNo;
            Date = date;
            DueDate = dueDate;
            SupplierId = supplierId;
            SupplierName = supplierName;
            VAT = vat;
            IncomeTax = incomeTax;
            TotalPaid = totalPaid;
            CurrencyId = currencyId;
            CurrencyCode = currencyCode;
        }

        public int Id { get; private set; }
        public string DocumentNo { get; private set; }
        public DateTimeOffset Date { get; private set; }
        public DateTimeOffset DueDate { get; private set; }
        public int SupplierId { get; private set; }
        public string SupplierName { get; private set; }
        public double VAT { get; private set; }
        public double IncomeTax { get; private set; }
        public double TotalPaid { get; private set; }
        public int CurrencyId { get; private set; }
        public string CurrencyCode { get; private set; }
    }
}