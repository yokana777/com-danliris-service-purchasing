using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils
{
    public class GarmentInvoiceItemDataUtil
    {
        private GarmentInvoiceDetailDataUtil garmentInvoiceDetailDataUtil;

        public GarmentInvoiceItemDataUtil(GarmentInvoiceDetailDataUtil garmentInvoiceDetailDataUtil)
        {
            this.garmentInvoiceDetailDataUtil = garmentInvoiceDetailDataUtil;
        }

        public GarmentInvoiceItem GetNewData(GarmentDeliveryOrder garmentDeliveryOrder)
        {
            return new GarmentInvoiceItem
            {
                DeliveryOrderId = garmentDeliveryOrder.Id,
                DeliveryOrderNo = garmentDeliveryOrder.DONo,
                ArrivalDate= garmentDeliveryOrder.ArrivalDate

            };
        }
    }
}
