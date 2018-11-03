using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils
{
    public class GarmentInvoiceDetailDataUtil
    {
		public List<GarmentInvoiceDetail> GetNewData(List<GarmentDeliveryOrderItem> garmentDeliveryOrderItems)
		{
			List<GarmentInvoiceDetail> deliveryOrderDetails = new List<GarmentInvoiceDetail>();
			foreach (var item in garmentDeliveryOrderItems)
			{
				foreach (var detail in item.Details)
				{
					deliveryOrderDetails.Add(new GarmentInvoiceDetail
					{
						IPOId = detail.POId,
						POSerialNumber=detail.POSerialNumber,
						PRItemId = detail.PRItemId,
						ProductId = detail.ProductId,
						ProductCode = detail.ProductCode,
						ProductName = detail.ProductName,
						DOQuantity = detail.DOQuantity 
					});
				}
			}
			return deliveryOrderDetails;
		}

		public List<GarmentInvoiceDetailViewModel> GetNewDataViewModel(List<GarmentDeliveryOrderItem> garmentDeliveryOrderItems)
		{
			List<GarmentInvoiceDetailViewModel> garmentInvoiceDetailViewModels = new List<GarmentInvoiceDetailViewModel>();
			foreach (var item in garmentDeliveryOrderItems)
			{
				foreach (var detail in item.Details)
				{
					garmentInvoiceDetailViewModels.Add(new GarmentInvoiceDetailViewModel
					{
						pOId = detail.POId,
						pOSerialNumber = detail.POSerialNumber,
						pRItemId = detail.PRItemId,
						product = new GarmentProductViewModel
						{
							Id = detail.ProductId,
							Code = detail.ProductCode,
							Name = detail.ProductName,
						},
						doQuantity = detail.DOQuantity,
						uoms = new UomViewModel
						{
							Id = detail.UomId,
							Unit = detail.UomUnit,
						}
					});
				}
			}
			return garmentInvoiceDetailViewModels;
		}

	}
}


