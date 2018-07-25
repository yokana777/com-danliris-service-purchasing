using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel
{
    public class UnitPaymentCorrectionNoteItemViewModel : BaseViewModel
    {
        public long uPODetailId { get; set; }
        public string uRNNo { get; set; }
        public string ePONo { get; set; }
        public long pRId { get; set; }
        public string pRNo { get; set; }
        public long pRDetailId { get; set; }
        public ProductViewModel product { get; set; }
        public long quantity { get; set; }
        public UomViewModel uom { get; set; }
        public long pricePerDealUnitBefore { get; set; }
        public long pricePerDealUnitAfter { get; set; }
        public long priceTotalBefore { get; set; }
        public long priceTotalAfter { get; set; }
        public CurrencyViewModel currency { get; set; }
        public virtual long uPCId { get; set; }
    }
}
