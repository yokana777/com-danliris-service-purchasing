using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel
{
    public class PurchaseOrderMonitoringAllViewModel : BaseViewModel
    {
        public string prNo { get; set; }
        public DateTimeOffset createdDatePR { get; set; }
        public DateTimeOffset prDate { get; set; }
        public string category { get; set; }
        public string budget { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public double quantity { get; set; }
        public string uom { get; set; }
        public double? pricePerDealUnit { get; set; }
        public double? priceTotal { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public DateTimeOffset receivedDatePO { get; set; }

        //EPO
        public DateTimeOffset epoDate { get; set; }
        public DateTimeOffset epoCreatedDate { get; set; }
        public DateTimeOffset epoExpectedDeliveryDate { get; set; }
        public DateTimeOffset epoDeliveryDate { get; set; }
        public string epoNo { get; set; }

        //DO
        public DateTimeOffset doDate { get; set; }
        public DateTimeOffset doDeliveryDate { get; set; }
        public string doNo { get; set; }
        public long doDetailId { get; set; }

        //URN
        public DateTimeOffset urnDate { get; set; }
        public string urnNo { get; set; }
        public double urnQuantity { get; set; }
        public string urnUom { get; set; }
        public string urnProductCode { get; set; }

        public string paymentDueDays { get; set; }
        public DateTimeOffset invoiceDate { get; set; }
        public string invoiceNo { get; set; }

        //UPO
        public DateTimeOffset upoDate { get; set; }
        public string upoNo { get; set; }
        public double upoPriceTotal { get; set; }
        public DateTimeOffset dueDate { get; set; }

        //vat
        public DateTimeOffset vatDate { get; set; }
        public string vatNo { get; set; }
        public double vatValue { get; set; }

        //incomeTax
        public DateTimeOffset? incomeTaxDate { get; set; }
        public string incomeTaxNo { get; set; }
        public double incomeTaxValue { get; set; }

        //correction
        public DateTimeOffset correctionDate { get; set; }
        public string correctionNo { get; set; }
        public string correctionType { get; set; }
        public double priceBefore { get; set; }
        public double priceAfter { get; set; }
        public double priceTotalAfter { get; set; }
        public double priceTotalBefore { get; set; }
        public double qtyCorrection { get; set; }
        public double valueCorrection { get; set; }
        public string correctionRemark { get; set; }
        public string correctionDates { get; set; }
        public string correctionQtys { get; set; }

        public string remark { get; set; }
        public string status { get; set; }
        public string staff { get; set; }
    }
}
