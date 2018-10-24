using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternNoteViewModel
{
    public class InternNoteFulfillmentViewModel : BaseViewModel
    {
        public long iNDetailId { get; set; }
        public string doNo { get; set; }
        public long ePONo { get; set; }
        public string poSerialNumber { get; set; }
        [MaxLength(255)]
        public string roNo { get; set; }
        public string termOfPayment { get; set; }
        public string paymentType { get; set; }

        /*Product*/
        public ProductViewModel product { get; set; }

        public long Quantity { get; set; }


        /* Unit */
        public UnitViewModel unit { get; set; }

        public double PricePerDealUnit { get; set; }
        public double PriceTotal { get; set; }
    }
}
