using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels
{
    public class FlowDetailPenerimaanViewModels : BaseViewModel
    {
        public string kdbarang { get; set; }
        public int no { get; set; }
        public string nmbarang { get; set; }
        public string nopo { get; set; }
        public string keterangan { get; set; }
        public string noro { get; set; }
        public string artikel { get; set; }
        public string kdbuyer { get; set; }
        public string asal { get; set; }
        public string nobukti { get; set; }
        public DateTimeOffset tanggal { get; set; }
        public double jumlahbeli{ get; set; }
        public string satuanbeli { get; set; }
        public double jumlahterima { get; set; }
        public string satuanterima { get; set; }
        public double jumlah { get; set; }
    }
}
