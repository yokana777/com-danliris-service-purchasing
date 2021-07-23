using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public class DispositionMemoLoaderDto
    {
        public DispositionMemoLoaderDto(UnitPaymentOrderDto unitPaymentOrders, List<UnitReceiptNoteDto> unitReceiptNotes, double purchaseAmount)
        {
            UnitPaymentOrder = unitPaymentOrders;
            UnitReceiptNotes = unitReceiptNotes;
            PurchaseAmount = purchaseAmount;
        }

        public UnitPaymentOrderDto UnitPaymentOrder { get; set; }
        public List<UnitReceiptNoteDto> UnitReceiptNotes { get; set; }
        public double PurchaseAmount { get; set; }
    }
}