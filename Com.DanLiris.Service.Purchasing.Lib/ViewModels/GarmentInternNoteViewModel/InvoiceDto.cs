using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using System;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel
{
    public class InvoiceDto
    {
        public InvoiceDto(GarmentInvoice internalNoteInvoice)
        {
            DocumentNo = internalNoteInvoice.InvoiceNo;
            Date = internalNoteInvoice.InvoiceDate;
            ProductNames = string.Join("\n", internalNoteInvoice.Items.SelectMany(item => item.Details).Select(detail => detail.ProductName));
            Category = new CategoryDto();
            Amount = internalNoteInvoice.TotalAmount;
            Id = (int)internalNoteInvoice.Id;
        }

        public string DocumentNo { get; set; }
        public DateTimeOffset Date { get; set; }
        public string ProductNames { get; set; }
        public CategoryDto Category { get; set; }
        public double Amount { get; set; }
        public int Id { get; set; }
    }
}