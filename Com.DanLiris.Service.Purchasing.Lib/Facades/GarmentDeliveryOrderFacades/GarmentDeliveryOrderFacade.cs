using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades
{
    public class GarmentDeliveryOrderFacade : IGarmentDeliveryOrderFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSet;
		private readonly DbSet<GarmentDeliveryOrderItem> dbSetItem; 

		public GarmentDeliveryOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentDeliveryOrder>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<GarmentDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentDeliveryOrder> Query = this.dbSet.Include(m => m.Items);

            List<string> searchAttributes = new List<string>()
            {
                "DONo", "SupplierName", "Items.EPONo"
            };

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentDeliveryOrder> pageable = new Pageable<GarmentDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentDeliveryOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentDeliveryOrder ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                    .ThenInclude(i => i.Details)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);
                    
                    m.IsClosed = false;
                    m.IsCorrection = false;
                    m.IsCustoms = false;

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                        CurrencyViewModel garmentCurrencyViewModel = GetCurrency(item.CurrencyCode);
                        m.DOCurrencyId = garmentCurrencyViewModel.Id;
                        m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                        m.DOCurrencyRate = garmentCurrencyViewModel.Rate;

                        foreach (var detail in item.Details)
                        {
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(detail.POId));
                            GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(detail.POId));

                            detail.POItemId = (int)internalPurchaseOrderItem.Id;
                            detail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                            detail.UnitId = internalPurchaseOrder.UnitId;
                            detail.UnitCode = internalPurchaseOrder.UnitCode;
                            EntityExtension.FlagForCreate(detail, user, USER_AGENT);

                            GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(detail.EPOItemId));
                            externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity + detail.DOQuantity;
                            EntityExtension.FlagForUpdate(externalPurchaseOrderItem, user, USER_AGENT);

                            detail.QuantityCorrection = detail.DOQuantity;
                            detail.PricePerDealUnitCorrection = detail.PricePerDealUnit;
                            detail.PriceTotalCorrection = detail.PriceTotal;

                            m.TotalAmount += detail.PriceTotal;
                            
                        }
                    }

                    this.dbSet.Add(m);

                    Created = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Created;
        }

        public async Task<int> Update(int id, GarmentDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                               .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    if (oldM != null && oldM.Id == id)
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);

                        foreach (var modelItem in m.Items)
                        {
                            foreach (var item in oldM.Items.Where(i => i.EPOId == modelItem.EPOId).ToList())
                            {
                                EntityExtension.FlagForUpdate(modelItem, user, USER_AGENT);

                                CurrencyViewModel garmentCurrencyViewModel = GetCurrency(item.CurrencyCode);
                                m.DOCurrencyId = garmentCurrencyViewModel.Id;
                                m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                                m.DOCurrencyRate = garmentCurrencyViewModel.Rate;
                                foreach (var modelDetail in modelItem.Details)
                                {
                                    foreach (var detail in item.Details.Where(j => j.EPOItemId == modelDetail.EPOItemId).ToList())
                                    {
                                        GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(modelDetail.POId));
                                        GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(modelDetail.POId));
                                        GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(modelDetail.EPOItemId));
                                        externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity + modelDetail.DOQuantity;
                                        modelDetail.POItemId = (int)internalPurchaseOrderItem.Id;
                                        modelDetail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                                        modelDetail.UnitId = internalPurchaseOrder.UnitId;
                                        modelDetail.UnitCode = internalPurchaseOrder.UnitCode;

                                        modelDetail.QuantityCorrection = modelDetail.DOQuantity;
                                        modelDetail.PricePerDealUnitCorrection = modelDetail.PricePerDealUnit;
                                        modelDetail.PriceTotalCorrection = modelDetail.PriceTotal;

                                        EntityExtension.FlagForUpdate(modelDetail, user, USER_AGENT);
                                    }
                                }
                            }
                        }


                        dbSet.Update(m);

                        Updated = await dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception("Invalid Id");
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public async Task<int> Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                                .Include(m => m.Items)
                                .ThenInclude(i => i.Details)
                                .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    EntityExtension.FlagForDelete(model, user, USER_AGENT);
                    foreach(var item in model.Items)
                    {
                        EntityExtension.FlagForDelete(item, user, USER_AGENT);

                        foreach(var detail in item.Details)
                        {
                            GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(detail.EPOItemId));
                            GarmentDeliveryOrderDetail deliveryOrderDetail = this.dbContext.GarmentDeliveryOrderDetails.FirstOrDefault(s => s.Id.Equals(detail.Id));
                            externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity;

                            EntityExtension.FlagForDelete(detail, user, USER_AGENT);
                        }
                    }
                    Deleted = await dbContext.SaveChangesAsync();

                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Deleted;
        }

        public IQueryable<GarmentDeliveryOrder> ReadBySupplier(string Keyword, string Filter)
        {
           IQueryable<GarmentDeliveryOrder> Query = this.dbSet;

			List<string> searchAttributes = new List<string>()
            {
                "DONo"
            };

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);
            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{}");

            if (OrderDictionary.Count > 0 && OrderDictionary.Keys.First().Contains("."))
            {
                string Key = OrderDictionary.Keys.First();
                string SubKey = Key.Split(".")[1];
                string OrderType = OrderDictionary[Key];

                Query = Query.Include(m => m.Items)
                    .ThenInclude(i => i.Details);
            }
            else
            {
				
                Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary).Include(m => m.Items)
                    .ThenInclude(i => i.Details).Where(s=> s.IsInvoice == false);
            }

            return Query;
        }
		public IQueryable<GarmentDeliveryOrder> DOForCustoms(string Keyword, string Filter)
		{
			IQueryable<GarmentDeliveryOrder> Query = this.dbSet;

			List<string> searchAttributes = new List<string>()
			{
				"DONo"
			};

			Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah
			Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
			Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);
			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{}");

			//if (OrderDictionary.Count > 0 && OrderDictionary.Keys.First().Contains("."))
			//{
			//	string Key = OrderDictionary.Keys.First();
			//	string SubKey = Key.Split(".")[1];
			//	string OrderType = OrderDictionary[Key];

			//	Query = Query.Include(m => m.Items)
			//		.ThenInclude(i => i.Details);
			//}
			//else
			//{

				Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary).Include(m => m.Items)
					.ThenInclude(i => i.Details).Where(s => s.BillNo ==null );
			//}

			return Query;
		}


		public int  IsReceived(List<int> id)
		{
			int isReceived = 0;
			foreach(var no in id)
			{
				var model = dbSet.Where(m => m.Id == no)
							   .Include(m => m.Items)
								   .ThenInclude(i => i.Details)
							   .FirstOrDefault();
				if (model.IsInvoice == true)
				{
					isReceived = 1;
					break;
				}
				else
				{
					foreach (var item in model.Items)
					{
						foreach (var detail in item.Details)
						{
							if (detail.ReceiptQuantity > 0)
								isReceived = 1;
							break;
						}
					}
				}
			}
			
			return isReceived;
		}
		public CurrencyViewModel GetCurrency(string currencyCode)
        {
            try
            {
                IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
                //Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"date\":\"desc\"}");
                string gCurrencyUri = "master/garment-currencies?keyword="+currencyCode+"&order=%7B\"date\"%3A\"desc\"%7D&page=1&size=1";
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{gCurrencyUri}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                var jsonUOM = result.Single(p => p.Key.Equals("data")).Value;
                List<CurrencyViewModel> viewModel = JsonConvert.DeserializeObject<List<CurrencyViewModel>>(result.GetValueOrDefault("data").ToString());
                return viewModel.First();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}