namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public class BillNoPaymentBillAutoCompleteDto
    {
        public BillNoPaymentBillAutoCompleteDto(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }
    }
}