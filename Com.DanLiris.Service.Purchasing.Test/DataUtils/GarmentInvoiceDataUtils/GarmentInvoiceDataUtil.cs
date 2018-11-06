using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Moq;
//using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils
{
    public class GarmentInvoiceDataUtil
    {
        private GarmentInvoiceItemDataUtil garmentInvoiceItemDataUtil;
		private GarmentDeliveryOrderDataUtil garmentDeliveryOrderDataUtil;
		private readonly GarmentInvoiceFacade facade;
        private GarmentInvoiceDetailDataUtil GarmentInvoiceDetailDataUtil;

        public GarmentInvoiceDataUtil(GarmentInvoiceItemDataUtil GarmentInvoiceItemDataUtil, GarmentInvoiceDetailDataUtil GarmentInvoiceDetailDataUtil, GarmentDeliveryOrderDataUtil GarmentDeliveryOrderDataUtil, GarmentInvoiceFacade facade)
        {
            this.garmentInvoiceItemDataUtil = GarmentInvoiceItemDataUtil;
            this.GarmentInvoiceDetailDataUtil = GarmentInvoiceDetailDataUtil;
            this.garmentDeliveryOrderDataUtil = GarmentDeliveryOrderDataUtil;
            this.facade = facade;
        }
		public async Task<GarmentInvoice> GetNewData(string user)
		{
			long nowTicks = DateTimeOffset.Now.Ticks;
			return new GarmentInvoice
			{
				InvoiceNo = "InvoiceNo",
				InvoiceDate = DateTimeOffset.Now,
				SupplierId = It.IsAny<int>(),
				SupplierCode = "codeS",
				SupplierName = "nameS",
				IncomeTaxId = It.IsAny<int>(),
				IncomeTaxName = "name",
				IncomeTaxNo = "Inc",
				IncomeTaxDate = DateTimeOffset.Now,
				IncomeTaxRate = 2,
				VatNo = "vat",
				VatDate = DateTimeOffset.Now,
				UseIncomeTax = true,
				UseVat = true,
				IsPayTax = true,
				HasInternNote = false,
				CurrencyId = It.IsAny<int>(),
				CurrencyCode = "TEST",
				Items = new List<GarmentInvoiceItem>
					{
						new GarmentInvoiceItem
						{

						   DeliveryOrderId =It.IsAny<int>(),
						   DODate=DateTimeOffset.Now,
						   DeliveryOrderNo="dono",
						   ArrivalDate  =  DateTimeOffset.Now,
						   TotalAmount =2000,
						   CurrencyId=It.IsAny<int>(),
							Details= new List<GarmentInvoiceDetail>
							{
								new GarmentInvoiceDetail
								{
									EPOId=It.IsAny<int>(),
									EPONo="epono",
									IPOId=It.IsAny<int>(),
									PRItemId=It.IsAny<int>(),
									PRNo="prno",
									RONo="12343",
									ProductId= It.IsAny<int>(),
									ProductCode="code",
									ProductName="name",
									UomId=It.IsAny<int>(),
									UomUnit="ROLL",
									DOQuantity=40,
									PricePerDealUnit=5000,
									PaymentType="type",
									PaymentDueDays=2,
									PaymentMethod="method",
									POSerialNumber="PM132434"

								}
							}
						}
				}
			};
		}
		public async Task<GarmentInvoice> GetTestData(string user)
		{
			GarmentInvoice model = await GetNewDataViewModel(user);

			await facade.Create(model, user);

			return model;
		}
		public async Task<GarmentInvoice> GetNewDataViewModel(string user)
		{
			var garmentDeliveryOrder = await garmentDeliveryOrderDataUtil.GetNewData(user);
			DateTime dateWithoutOffset = new DateTime(2010, 7, 16, 13, 32, 00);
			return new GarmentInvoice 
			{
				InvoiceNo = "InvoiceNo",
				InvoiceDate = dateWithoutOffset,
				SupplierId = It.IsAny<int>(),
				SupplierCode = "codeS",
				SupplierName = "nameS",
				IncomeTaxId = It.IsAny<int>(),
				IncomeTaxName = "name",
				IncomeTaxNo = "Inc",
				IncomeTaxDate = DateTimeOffset.Now,
				IncomeTaxRate = 2,
				VatNo = "vat",
				VatDate = DateTimeOffset.Now,
				UseIncomeTax = true,
				UseVat = true,
				IsPayTax = true,
				HasInternNote = false,
				CurrencyId = It.IsAny<int>(),
				CurrencyCode = "TEST",
				Items = new List<GarmentInvoiceItem> { garmentInvoiceItemDataUtil.GetNewDataViewModel(garmentDeliveryOrder) }
			};
		}
	}
}
