using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentBeacukaiFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentBeacukaiModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentBeacukaiViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentBeacukaiDataUtils
{
	public class GarmentBeacukaiDataUtil
	{
		private GarmentDeliveryOrderDataUtil garmentDeliveryOrderDataUtil;
		private readonly GarmentBeacukaiFacade facade;
		public GarmentBeacukaiDataUtil( GarmentDeliveryOrderDataUtil GarmentDeliveryOrderDataUtil, GarmentBeacukaiFacade facade)
		{
			this.garmentDeliveryOrderDataUtil = GarmentDeliveryOrderDataUtil;
			this.facade = facade;
		}
		public async Task<GarmentBeacukai> GetNewData(string user)
		{
			long nowTicks = DateTimeOffset.Now.Ticks;
			var garmentDO = Task.Run(() => garmentDeliveryOrderDataUtil.GetNewData("User")).Result;
			
			return new GarmentBeacukai
			{
				BeacukaiNo = "BeacukaiNo",
				BeacukaiDate = DateTimeOffset.Now,
				SupplierId = It.IsAny<int>(),
				SupplierCode = "codeS",
				SupplierName = "nameS",
				BillNo = "BP181115160748000042",
				Bruto = 10,
				Netto = 10,
				Packaging = "COllY",
				PackagingQty = 2,
				CustomsType = "customsType",
				ValidationDate = DateTimeOffset.Now,
				CurrencyId = It.IsAny<int>(),
				CurrencyCode = "TEST",
				Items = new List<GarmentBeacukaiItem>
					{
						new GarmentBeacukaiItem
						{

						   GarmentDOId =garmentDO.Id,
						   DODate=garmentDO.DODate,
						   GarmentDONo=garmentDO.DONo,
						   ArrivalDate  =  garmentDO.ArrivalDate,
						   TotalAmount = (decimal)garmentDO.TotalAmount,
						   TotalQty=50
						
						}
				}
			};
		}
		public async Task<GarmentBeacukaiViewModel> GetViewModel(string user)
		{
			long nowTicks = DateTimeOffset.Now.Ticks;
			var garmentDO = Task.Run(() => garmentDeliveryOrderDataUtil.GetNewData("User")).Result;

			return new GarmentBeacukaiViewModel
			{
				beacukaiNo = "",
				beacukaiDate = DateTimeOffset.MinValue,
				supplier = { },
				customType = null,
				packagingQty = 0,
				netto = 0,
				bruto = 0,
				packaging = "",
				currency = { },

				items = { new GarmentBeacukaiItemViewModel {
					selected=true
				} ,
				new GarmentBeacukaiItemViewModel {
					selected=false
				}
				}
			
			};
		}
		public async Task<GarmentBeacukai> GetTestData(string user)
		{
			GarmentBeacukai model = await GetNewData(user);

			await facade.Create(model, user);

			return model;
		}
	}
}
