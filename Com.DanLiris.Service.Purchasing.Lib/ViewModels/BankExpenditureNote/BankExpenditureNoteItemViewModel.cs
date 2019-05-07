using Com.DanLiris.Service.Purchasing.Lib.Utilities;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.BankExpenditureNote
{
    public class BankExpenditureNoteItemViewModel : BaseViewModel
    {
        public string UId { get; set; }
        public long UnitPaymentOrderItemId { get; set; }
        public double Price { get; set; }
        public string ProductCode { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public string UnitCode { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string Uom { get; set; }
    }
}
