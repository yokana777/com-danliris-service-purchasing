using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public GarmentInvoiceItemViewModel GetNewDataViewModel(GarmentDeliveryOrder garmentDeliveryOrder)
		{
			return new GarmentInvoiceItemViewModel
			{
				deliveryOrder = new GarmentDeliveryOrderViewModel
				{
					Id = garmentDeliveryOrder.Id,
					doNo = garmentDeliveryOrder.DONo,
					doDate=garmentDeliveryOrder.DODate
				},
				details = garmentInvoiceDetailDataUtil.GetNewDataViewModel(garmentDeliveryOrder.Items.ToList())
			};
		}
	}
}
