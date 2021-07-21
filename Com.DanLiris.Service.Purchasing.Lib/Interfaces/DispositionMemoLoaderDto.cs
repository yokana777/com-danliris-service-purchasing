using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public class DispositionMemoLoaderDto
    {
        public List<UnitPaymentOrderDto> UnitPaymentOrders { get; set; }
        public List<UnitReceiptNoteDto> UnitReceiptNotes { get; set; }
    }
}