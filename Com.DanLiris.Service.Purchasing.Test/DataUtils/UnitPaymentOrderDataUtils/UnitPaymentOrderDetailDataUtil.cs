using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils
{
    public class UnitPaymentOrderDetailDataUtil
    {
        public UnitPaymentOrderDetail GetNewData()
        {
            return new UnitPaymentOrderDetail
            {
                Id = 0,
                UPOItemId = 0,
                URNDetailId = 0,
                EPONo = null,
                PRId = 0,
                PRNo = null,
                PRItemId = 0,
                ProductId = null,
                ProductCode = null,
                ProductName = null,
                ReceiptQuantity = 0,
                UomId = null,
                UomUnit = null,
                PricePerDealUnit = 0,
                PriceTotal = 0,
                PricePerDealUnitCorrection = 0,
                PriceTotalCorrection = 0,
                ProductRemark = null,
            };
        }
    }
}
