using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel
{
    public class UnitPaymentCorrectionNoteItemViewModel : BaseViewModel
    {
        public long UPODetailId { get; set; }
        public string URNNo { get; set; }
        public string EPONo { get; set; }
        public long PRId { get; set; }
        public string PRNo { get; set; }
        public long PRDetailId { get; set; }
        public ProductViewModel product { get; set; }
        public long Quantity { get; set; }
        public UomViewModel uom { get; set; }
        public long PricePerDealUnitBefore { get; set; }
        public long PricePerDealUnitAfter { get; set; }
        public long PriceTotalBefore { get; set; }
        public long PriceTotalAfter { get; set; }
        public CurrencyViewModel currency { get; set; }
        public virtual long UPCId { get; set; }
    }
}
