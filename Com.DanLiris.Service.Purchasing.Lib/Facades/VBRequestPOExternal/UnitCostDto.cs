using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using System.Collections.Generic;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class UnitCostDto
    {
        public UnitCostDto(GarmentInternNoteDetail detail, List<GarmentInternNoteItem> internNoteItems, GarmentInternNote element)
        {
            Unit = new UnitDto(detail.UnitId, detail.UnitCode, detail.UnitName, "", "0", "");
            Amount = detail.PriceTotal;
        }

        public UnitCostDto(UnitPaymentOrderDetail detail, List<UnitPaymentOrderItem> spbItems, List<UnitReceiptNoteItem> unitReceiptNoteItems, List<UnitReceiptNote> unitReceiptNotes)
        {
            var unitReceiptNoteItem = unitReceiptNoteItems.FirstOrDefault(item => item.Id == detail.URNItemId);
            var unitReceiptNote = unitReceiptNotes.FirstOrDefault(item => item.Id == unitReceiptNoteItem.URNId);

            Unit = new UnitDto(unitReceiptNote.UnitId, unitReceiptNote.UnitCode, unitReceiptNote.UnitName, unitReceiptNote.DivisionCode, unitReceiptNote.DivisionId, unitReceiptNote.DivisionName);
            Amount = detail.PriceTotal;
        }

        public UnitDto Unit { get; private set; }
        public double Amount { get; private set; }
    }
}