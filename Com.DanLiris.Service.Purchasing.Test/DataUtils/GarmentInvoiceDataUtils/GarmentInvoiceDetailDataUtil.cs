using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils
{
    public class GarmentInvoiceDetailDataUtil
    {
        public GarmentInvoiceDetail GetNewData(List<GarmentDeliveryOrderItem> item) => new GarmentInvoiceDetail
        {

            EPOId = item[0].Id,
            EPONo = item[0].EPONo,
           
            ProductCode = "ProductCode",
            ProductName = "ProductName",
            PricePerDealUnit=500,
            DOQuantity=300
        };

    }
}


