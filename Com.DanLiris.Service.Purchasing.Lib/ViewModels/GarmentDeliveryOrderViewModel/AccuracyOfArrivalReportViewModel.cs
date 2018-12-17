using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel
{
    public class AccuracyOfArrivalReportViewModel : BaseViewModel
    {
        public SupplierViewModel supplier { get; set; } //SJ
        public string poSerialNumber { get; set; } //SJ
        public DateTimeOffset prDate { get; set; } //date PR
        public DateTimeOffset poDate { get; set; } //createdDate PO
        public DateTimeOffset epoDate { get; set; } //date EPO
        public ProductViewModel product { get; set; } //SJ
        public string article { get; set; } // article EPO
        public string roNo { get; set; } //SJ
        public DateTimeOffset shipmentDate { get; set; } //PR
        public DateTimeOffset doDate { get; set; } //dodate SJ
        public string status { get; set; } //OK ? NotOK
        public string staff { get; set; } //CreatedBy SJ
        public string category { get; set; } //GarmentCategory Based On Product 
        public string doNo { get; set; } //SJ
        public List<ProductViewModel> productByCategory { get; set; }
    }
}
