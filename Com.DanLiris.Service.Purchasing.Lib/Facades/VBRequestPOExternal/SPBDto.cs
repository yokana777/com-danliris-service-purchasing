using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.Moonlay.EntityFrameworkCore;
using iTextSharp.text;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class SPBDto
    {
        public SPBDto(GarmentInternNote element, List<GarmentInvoice> invoices)
        {
            Id = element.Id;
            No = element.INNo;
            Date = element.INDate;

            var invoiceIds = element.Items.Select(item => item.InvoiceId).ToList();
            var elementInvoice = invoices.FirstOrDefault(entity => invoiceIds.Contains(entity.Id));

            Amount = element.Items.SelectMany(item => item.Details).Sum(detail => detail.PriceTotal);

            if (elementInvoice != null)
            {
                UseVat = elementInvoice.UseVat;
                UseIncomeTax = elementInvoice.UseIncomeTax;
                IncomeTax = new IncomeTaxDto(elementInvoice.IncomeTaxId, elementInvoice.IncomeTaxName, elementInvoice.IncomeTaxRate);
                IncomeTaxBy = elementInvoice.IsPayTax ? "Dan Liris" : "Supplier";
            }
        }

        public SPBDto(UnitPaymentOrder element)
        {
            Id = element.Id;
            No = element.UPONo;
            Date = element.Date;

            Amount = element.Items.SelectMany(item => item.Details).Sum(detail => detail.PriceTotal);

            UseVat = element.UseVat;
            UseIncomeTax = element.UseIncomeTax;
            IncomeTax = new IncomeTaxDto(element.IncomeTaxId, element.IncomeTaxName, element.IncomeTaxRate);
            IncomeTaxBy = element.IncomeTaxBy;

            UnitCosts = new List<UnitCostDto>();
        }

        public SPBDto(UnitPaymentOrder element, List<UnitPaymentOrderDetail> spbDetails, List<UnitPaymentOrderItem> spbItems, List<UnitReceiptNoteItem> unitReceiptNoteItems, List<UnitReceiptNote> unitReceiptNotes)
        {
            Id = element.Id;
            No = element.UPONo;
            Date = element.Date;
            

            Supplier = new SupplierDto(element);
            Division = new DivisionDto(element.DivisionCode, element.DivisionId, element.DivisionName);

            UseVat = element.UseVat;
            UseIncomeTax = element.UseIncomeTax;
            IncomeTax = new IncomeTaxDto(element.IncomeTaxId, element.IncomeTaxName, element.IncomeTaxRate);
            IncomeTaxBy = element.IncomeTaxBy;

            //var selectedSPBDetails = spbDetails.Where(detail =)
            var selectedSPBItems = spbItems.Where(item => item.UPOId == element.Id).ToList();
            var selectedSPBItemIds = selectedSPBItems.Select(item => item.Id).ToList();
            var selectedSPBDetails = spbDetails.Where(detail => selectedSPBItemIds.Contains(detail.UPOItemId)).ToList();


            UnitCosts = selectedSPBDetails.Select(detail => new UnitCostDto(detail, selectedSPBItems, unitReceiptNoteItems, unitReceiptNotes, element)).ToList();
            //Amount = selectedSPBDetails.Sum(detail => detail.PriceTotal);
            Amount = selectedSPBDetails.Sum(detail => {
                var quantity = detail.ReceiptQuantity;
                if (detail.QuantityCorrection > 0)
                    quantity = detail.QuantityCorrection;

                var price = detail.PricePerDealUnit;
                if (detail.PricePerDealUnitCorrection > 0)
                    price = detail.PricePerDealUnitCorrection;

                var result = quantity * price;
                if (detail.PriceTotalCorrection > 0)
                    result = detail.PriceTotalCorrection;

                var total = result;
                if (element != null)
                {
                    if (element.UseVat)
                    {
                        result += total * 0.1;
                    }

                    if (element.UseIncomeTax && (element.IncomeTaxBy == "Supplier" || element.IncomeTaxBy == "SUPPLIER"))
                    {
                        result -= total * (element.IncomeTaxRate / 100);
                    }
                }

                return result;
            });
        }

        public SPBDto(GarmentInternNote element, List<GarmentInvoice> invoices, List<GarmentInternNoteItem> internNoteItems, List<GarmentInternNoteDetail> internNoteDetails)
        {
            Id = element.Id;
            No = element.INNo;
            Date = element.INDate;

            var invoiceIds = element.Items.Select(item => item.InvoiceId).ToList();
            var elementInvoice = invoices.FirstOrDefault(entity => invoiceIds.Contains(entity.Id));


            Supplier = new SupplierDto(element);
            

            if (elementInvoice != null)
            {
                Division = new DivisionDto("", "0", "GARMENT");
                UseVat = elementInvoice.UseVat;
                UseIncomeTax = elementInvoice.UseIncomeTax;
                IncomeTax = new IncomeTaxDto(elementInvoice.IncomeTaxId, elementInvoice.IncomeTaxName, elementInvoice.IncomeTaxRate);
                //IncomeTaxBy = elementInvoice.IsPayTax ? "Dan Liris" : "Supplier";
                IncomeTaxBy = "Supplier";
            }

            var selectedInternNoteItems = internNoteItems.Where(item => item.GarmentINId == element.Id).ToList();
            var selectedInternNoteItemIds = selectedInternNoteItems.Select(item => item.Id).ToList();
            var selectedInternNoteDetails = internNoteDetails.Where(detail => selectedInternNoteItemIds.Contains(detail.GarmentItemINId)).ToList();

            UnitCosts = selectedInternNoteDetails.Select(detail => new UnitCostDto(detail, selectedInternNoteItems, element, elementInvoice)).ToList();
            Amount = selectedInternNoteDetails.Sum(detail => detail.PriceTotal);
        }

        public long Id { get; private set; }
        public string No { get; private set; }
        public DateTimeOffset Date { get; private set; }
        public List<SPBDtoItem> Items { get; private set; }
        public double Amount { get; private set; }
        public SupplierDto Supplier { get; private set; }
        public DivisionDto Division { get; private set; }
        public bool UseVat { get; private set; }
        public bool UseIncomeTax { get; private set; }
        public IncomeTaxDto IncomeTax { get; private set; }
        public string IncomeTaxBy { get; private set; }
        public List<UnitCostDto> UnitCosts { get; private set; }
    }
}