using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using System.Collections.Generic;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class UnitCostDto
    {
        public UnitCostDto(GarmentInternNoteDetail detail, List<GarmentInternNoteItem> internNoteItems, GarmentInternNote element, GarmentInvoice elementInvoice)
        {
            Unit = new UnitDto(detail.UnitId, detail.UnitCode, detail.UnitName, "", "0", "");

            var total = detail.PriceTotal;
            if (elementInvoice != null)
            {
                if (elementInvoice.UseVat)
                {
                    total += detail.PriceTotal * 0.1;
                }

                if (elementInvoice.UseIncomeTax)
                {
                    total += detail.PriceTotal * (elementInvoice.IncomeTaxRate / 100);
                }
            }

            Amount = total;
        }

        public UnitCostDto(UnitPaymentOrderDetail detail, List<UnitReceiptNote> unitReceiptNotes, List<UnitReceiptNoteItem> unitReceiptNoteItems)
        {
            var unitReceiptNoteItem = unitReceiptNoteItems.FirstOrDefault(item => item.Id == detail.URNItemId);
            var unitReceiptNote = unitReceiptNotes.FirstOrDefault(item => item.Id == unitReceiptNoteItem.URNId);

            Unit = new UnitDto(unitReceiptNote.UnitId, unitReceiptNote.UnitCode, unitReceiptNote.UnitName, unitReceiptNote.DivisionCode, unitReceiptNote.DivisionId, unitReceiptNote.DivisionName);
            Amount = detail.PriceTotal;
        }

        public UnitCostDto(UnitPaymentOrderDetail detail, List<UnitPaymentOrderItem> spbItems, List<UnitReceiptNoteItem> unitReceiptNoteItems, List<UnitReceiptNote> unitReceiptNotes)
        {
            var unitReceiptNoteItem = unitReceiptNoteItems.FirstOrDefault(item => item.Id == detail.URNItemId);
            var unitReceiptNote = unitReceiptNotes.FirstOrDefault(item => item.Id == unitReceiptNoteItem.URNId);

            Unit = new UnitDto(unitReceiptNote.UnitId, unitReceiptNote.UnitCode, unitReceiptNote.UnitName, unitReceiptNote.DivisionCode, unitReceiptNote.DivisionId, unitReceiptNote.DivisionName);
            Amount = detail.PriceTotal;
        }

        public UnitCostDto(UnitPaymentOrderDetail detail, List<UnitPaymentOrderItem> spbItems, List<UnitReceiptNoteItem> unitReceiptNoteItems, List<UnitReceiptNote> unitReceiptNotes, UnitPaymentOrder element)
        {
            var unitReceiptNoteItem = unitReceiptNoteItems.FirstOrDefault(item => item.Id == detail.URNItemId);
            var unitReceiptNote = unitReceiptNotes.FirstOrDefault(item => item.Id == unitReceiptNoteItem.URNId);

            Unit = new UnitDto(unitReceiptNote.UnitId, unitReceiptNote.UnitCode, unitReceiptNote.UnitName, unitReceiptNote.DivisionCode, unitReceiptNote.DivisionId, unitReceiptNote.DivisionName);

            var total = detail.PriceTotal;
            if (element != null)
            {
                if (element.UseVat)
                {
                    total += detail.PriceTotal * 0.1;
                }

                if (element.UseIncomeTax)
                {
                    total += detail.PriceTotal * (element.IncomeTaxRate / 100);
                }
            }

            Amount = total;
        }

        public UnitDto Unit { get; private set; }
        public double Amount { get; private set; }
    }
}