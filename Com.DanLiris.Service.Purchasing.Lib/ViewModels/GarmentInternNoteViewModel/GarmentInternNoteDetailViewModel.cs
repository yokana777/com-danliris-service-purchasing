using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.Moonlay.Models;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel
{
    public class GarmentInternNoteDetailViewModel : BaseViewModel
    {
        public long ePOId { get; set; }
        public string ePONo { get; set; }
        public long dOId { get; set; }
        public string dONo { get; set; }
        public DateTimeOffset dODate { get; set; }
        public string poSerialNumber { get; set; }
        public long rOId { get; set; }
        public string rONo { get; set; }
        public string termOfPayment { get; set; }
        public string paymentType { get; set; }
        public double pricePerDealUnit { get; set; }
        public double priceTotal { get; set; }
        public double receiptQuantity { get; set; }
        public double paymentDueDays { get; set; }
        public long Quantity { get; set; }

        /*Product*/
        public ProductViewModel product { get; set; }

        public UnitViewModel unit { get; set; }
    }
}
