using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public class DispositionMemoLoaderDto
    {
        public DispositionMemoLoaderDto(UnitPaymentOrderDto unitPaymentOrders, List<UnitReceiptNoteDto> unitReceiptNotes)
        {
            UnitPaymentOrders = unitPaymentOrders;
            UnitReceiptNotes = unitReceiptNotes;
        }

        public UnitPaymentOrderDto UnitPaymentOrders { get; set; }
        public List<UnitReceiptNoteDto> UnitReceiptNotes { get; set; }
    }
}