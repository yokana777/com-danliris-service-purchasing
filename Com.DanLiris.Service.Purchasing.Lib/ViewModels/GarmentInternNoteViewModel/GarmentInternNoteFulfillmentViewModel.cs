using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternNoteViewModel
{
    public class GarmentInternNoteFulfillmentViewModel : BaseViewModel
    {
        public long iNDetailId { get; set; }
        public string doNo { get; set; }
        public string poSerialNumber { get; set; }
        [MaxLength(255)]
        public string roNo { get; set; }
        public string termOfPayment { get; set; }
        public string paymentType { get; set; }
        public double PricePerDealUnit { get; set; }
        public double PriceTotal { get; set; }
        public PurchaseOrder purchaseOrder { get; set; }

        /*Product*/
        public ProductViewModel product { get; set; }

        public long Quantity { get; set; }

        public class PurchaseOrder
        {
            public PurchaseRequest purchaseRequest { get; set; }
        }
        public class PurchaseRequest
        {
            public PurchaseExternal purchaseExternal { get; set; }
        }
        public class PurchaseExternal
        {
            public long Id { get; set; }
            public int no { get; set; }
            public UnitViewModel unit { get; set; }
        }
        

    }
}
