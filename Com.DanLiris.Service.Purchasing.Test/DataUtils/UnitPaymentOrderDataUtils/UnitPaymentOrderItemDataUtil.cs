using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils
{
    public class UnitPaymentOrderItemDataUtil
    {
        private UnitPaymentOrderDetailDataUtil unitPurchaseOrderDetailDataUtil;

        public UnitPaymentOrderItemDataUtil(UnitPaymentOrderDetailDataUtil unitPurchaseOrderDetailDataUtil)
        {
            this.unitPurchaseOrderDetailDataUtil = unitPurchaseOrderDetailDataUtil;
        }

        public UnitPaymentOrderItem GetNewData()
        {
            return new UnitPaymentOrderItem
            {
                Id = 0,
                UPOId = 0,
                URNId = 0,
                URNNo = null,
                DOId = 0,
                DONo = null,
                Details = new List<UnitPaymentOrderDetail> { unitPurchaseOrderDetailDataUtil.GetNewData() }
            };
        }
    }
}
