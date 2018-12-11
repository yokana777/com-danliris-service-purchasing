using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
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

        private readonly IMapper mapper;

        public GarmentDeliveryOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentDeliveryOrder>();
            this.serviceProvider = serviceProvider;

            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
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

                        CurrencyViewModel garmentCurrencyViewModel = GetCurrency(item.CurrencyCode, m.DODate);
                        m.DOCurrencyId = garmentCurrencyViewModel.Id;
                        m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                        m.DOCurrencyRate = garmentCurrencyViewModel.Rate;

                        foreach (var detail in item.Details)
                        {
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(detail.POId));
                            GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(internalPurchaseOrder.Id));

                            detail.POItemId = (int)internalPurchaseOrderItem.Id;
                            detail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                            detail.UnitId = internalPurchaseOrder.UnitId;
                            detail.UnitCode = internalPurchaseOrder.UnitCode;
                            EntityExtension.FlagForCreate(detail, user, USER_AGENT);

                            GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(detail.EPOItemId));
                            externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity + detail.DOQuantity;

                            if(externalPurchaseOrderItem.ReceiptQuantity == 0)
                            {
                                if(externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                }
                                else if(externalPurchaseOrderItem.DOQuantity>0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang semua";
                                }
                            }

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

        public async Task<int> Update(int id, GarmentDeliveryOrderViewModel vm, GarmentDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
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

                        foreach (var vmItem in vm.items)
                        {

                            foreach (var modelItem in m.Items.Where(i => i.Id == vmItem.Id))
                            {   
                                if(modelItem.Id == 0)
                                {
                                    EntityExtension.FlagForCreate(modelItem, user, USER_AGENT);

                                    CurrencyViewModel garmentCurrencyViewModel = GetCurrency(modelItem.CurrencyCode, m.DODate);
                                    m.DOCurrencyId = garmentCurrencyViewModel.Id;
                                    m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                                    m.DOCurrencyRate = garmentCurrencyViewModel.Rate;
                                    foreach (var vmDetail in vmItem.fulfillments)
                                    {
                                        foreach (var modelDetail in modelItem.Details)
                                        {
                                            if (vmDetail.isSave)
                                            {
                                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(modelDetail.POId));
                                                GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(internalPurchaseOrder.Id));

                                                modelDetail.POItemId = (int)internalPurchaseOrderItem.Id;
                                                modelDetail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                                                modelDetail.UnitId = internalPurchaseOrder.UnitId;
                                                modelDetail.UnitCode = internalPurchaseOrder.UnitCode;
                                                EntityExtension.FlagForCreate(modelDetail, user, USER_AGENT);

                                                GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(modelDetail.EPOItemId));
                                                externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity + modelDetail.DOQuantity;

                                                if (externalPurchaseOrderItem.ReceiptQuantity == 0)
                                                {
                                                    if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                                    {
                                                        internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                                    }
                                                    else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                                    {
                                                        internalPurchaseOrderItem.Status = "Barang sudah datang semua";
                                                    }
                                                }

                                                modelDetail.QuantityCorrection = modelDetail.DOQuantity;
                                                modelDetail.PricePerDealUnitCorrection = modelDetail.PricePerDealUnit;
                                                modelDetail.PriceTotalCorrection = modelDetail.PriceTotal;

                                                m.TotalAmount += modelDetail.PriceTotal;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var item in oldM.Items.Where(i => i.EPOId == modelItem.EPOId).ToList())
                                    {
                                        EntityExtension.FlagForUpdate(modelItem, user, USER_AGENT);

                                        CurrencyViewModel garmentCurrencyViewModel = GetCurrency(item.CurrencyCode, m.DODate);
                                        m.DOCurrencyId = garmentCurrencyViewModel.Id;
                                        m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                                        m.DOCurrencyRate = garmentCurrencyViewModel.Rate;

                                        foreach (var vmDetail in vmItem.fulfillments)
                                        {
                                            foreach (var modelDetail in modelItem.Details.Where(j => j.Id == vmDetail.Id))
                                            {
                                                foreach (var detail in item.Details.Where(j => j.EPOItemId == modelDetail.EPOItemId).ToList())
                                                {
                                                    GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(modelDetail.POId));
                                                    GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(modelDetail.POId));
                                                    GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(modelDetail.EPOItemId));

                                                    if (vmDetail.isSave == false)
                                                    {
                                                        externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity;
                                                        EntityExtension.FlagForDelete(modelDetail, user, USER_AGENT);
                                                    }
                                                    else
                                                    {
                                                        externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity + modelDetail.DOQuantity;
                                                        modelDetail.POItemId = (int)internalPurchaseOrderItem.Id;
                                                        modelDetail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                                                        modelDetail.UnitId = internalPurchaseOrder.UnitId;
                                                        modelDetail.UnitCode = internalPurchaseOrder.UnitCode;

                                                        modelDetail.QuantityCorrection = modelDetail.DOQuantity;
                                                        modelDetail.PricePerDealUnitCorrection = modelDetail.PricePerDealUnit;
                                                        modelDetail.PriceTotalCorrection = modelDetail.PriceTotal;
                                                        m.TotalAmount += oldM.TotalAmount - detail.PriceTotal + modelDetail.PriceTotal;

                                                        EntityExtension.FlagForUpdate(modelDetail, user, USER_AGENT);
                                                    }
                                                    if (externalPurchaseOrderItem.ReceiptQuantity == 0)
                                                    {
                                                        if (externalPurchaseOrderItem.DOQuantity == 0)
                                                        {
                                                            GarmentPurchaseRequestItem purchaseRequestItem = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(s => s.Id.Equals(modelDetail.PRItemId));
                                                            purchaseRequestItem.Status = "Sudah diorder ke Supplier";
                                                            internalPurchaseOrderItem.Status = "Sudah diorder ke Supplier";
                                                        }
                                                        else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                                        {
                                                            internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                                        }
                                                        else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                                        {
                                                            internalPurchaseOrderItem.Status = "Barang sudah datang semua";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        dbSet.Update(m);

                        foreach (var oldItem in oldM.Items)
                        {
                            var newItem = m.Items.FirstOrDefault(i => i.Id.Equals(oldItem.Id));
                            foreach (var oldDetail in oldItem.Details)
                            {
                                GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(oldDetail.EPOItemId));
                                if (newItem == null)
                                {
                                    EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
                                    dbContext.GarmentDeliveryOrderItems.Update(oldItem);
                                    EntityExtension.FlagForDelete(oldDetail, user, USER_AGENT);
                                    externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - oldDetail.DOQuantity;
                                    dbContext.GarmentDeliveryOrderDetails.Update(oldDetail);
                                }
                            }
                        }

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
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(detail.POId));
                            GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(detail.POId));

                            GarmentDeliveryOrderDetail deliveryOrderDetail = this.dbContext.GarmentDeliveryOrderDetails.FirstOrDefault(s => s.Id.Equals(detail.Id));
                            externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity;

                            if (externalPurchaseOrderItem.ReceiptQuantity == 0)
                            {
                                if (externalPurchaseOrderItem.DOQuantity == 0)
                                {
                                    GarmentPurchaseRequestItem purchaseRequestItem = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(s => s.Id.Equals(detail.PRItemId));
                                    purchaseRequestItem.Status = "Sudah diorder ke Supplier";
                                    internalPurchaseOrderItem.Status = "Sudah diorder ke Supplier";
                                }
                                else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                }
                                else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang Semua";
                                }
                            }

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
                    .ThenInclude(i => i.Details).Where(s=> s.IsInvoice == false && !string.IsNullOrWhiteSpace( s.BillNo));
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

        public CurrencyViewModel GetCurrency(string currencyCode, DateTimeOffset doDate)
        {
            try
            {
                IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
                //Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"date\":\"desc\"}");
                string gCurrencyUri = "master/garment-currencies?keyword="+currencyCode+"&order=%7B\"date\"%3A\"desc\"%7D&page=1&size=25";
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{gCurrencyUri}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                var jsonUOM = result.Single(p => p.Key.Equals("data")).Value;
                List<CurrencyViewModel> viewModel = JsonConvert.DeserializeObject<List<CurrencyViewModel>>(result.GetValueOrDefault("data").ToString());
                return viewModel.FirstOrDefault(s=> s.Date < doDate.AddDays(1));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ReadResponse ReadForUnitReceiptNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long filterSupplierId = FilterDictionary.ContainsKey("SupplierId") ? long.Parse(FilterDictionary["SupplierId"]) : 0;
            FilterDictionary.Remove("SupplierId");

            var filterUnitId = FilterDictionary.ContainsKey("UnitId") ? FilterDictionary["UnitId"] : string.Empty;
            FilterDictionary.Remove("UnitId");

            IQueryable<GarmentDeliveryOrder> Query = dbSet
                .Where(m => m.DONo.Contains(Keyword ?? "") && (filterSupplierId == 0 ? true : m.SupplierId == filterSupplierId) && m.Items.Any(i => i.Details.Any(d => d.ReceiptQuantity == 0 && (string.IsNullOrWhiteSpace(filterUnitId) ? true : d.UnitId == filterUnitId))))
                .Select(m => new GarmentDeliveryOrder
                {
                    Id = m.Id,
                    DONo = m.DONo,
                    LastModifiedUtc = m.LastModifiedUtc,
                    Items = m.Items.Select(i => new GarmentDeliveryOrderItem
                    {
                        Id = i.Id,
                        Details = i.Details.Where(d => d.ReceiptQuantity == 0 && (string.IsNullOrWhiteSpace(filterUnitId) ? true : d.UnitId == filterUnitId)).ToList()
                    }).ToList()
                });

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentDeliveryOrder> pageable = new Pageable<GarmentDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.doNo,
                    s.LastModifiedUtc,
                    items = s.items.Select(i => new
                    {
                        i.Id,
                        fulfillments = i.fulfillments.Select(d => new
                        {
                            d.Id,

                            d.ePOItemId,

                            d.pRId,
                            d.pRNo,
                            d.pRItemId,

                            d.pOId,
                            d.pOItemId,
                            d.poSerialNumber,

                            d.product,

                            d.rONo,

                            d.doQuantity,
                            d.receiptQuantity,

                            d.purchaseOrderUom,

                            d.pricePerDealUnit,
                            d.pricePerDealUnitCorrection,

                            d.conversion,

                            d.smallUom,

                            buyer = new {
                                name = dbContext.GarmentPurchaseRequests.Where(m => m.Id == d.pRId).Select(m => m.BuyerName).FirstOrDefault()
                            },
                            article = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == d.ePOItemId).Select(m => m.Article).FirstOrDefault()
                        }).ToList()
                    }).ToList()
                }).ToList()
            );

            return new ReadResponse(listData, Total, OrderDictionary);
        }

        public ReadResponse ReadForCorrectionNoteQuantity(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            IQueryable<GarmentDeliveryOrder> Query = dbSet
                .Where(m => m.DONo.Contains(Keyword ?? "") && m.BillNo !=null && m.IsInvoice == true && m.Items.Any(i => i.Details.Any(d => d.ReceiptQuantity > 0 )))
                .Select(m => new GarmentDeliveryOrder
                {
                    Id = m.Id,
                    DONo = m.DONo,
                    BillNo = m.BillNo,
                    IsInvoice = m.IsInvoice,
                    UseIncomeTax = m.UseIncomeTax,
                    SupplierName = m.SupplierName,
                    SupplierId = m.SupplierId,
                    SupplierCode = m.SupplierCode,
                    DOCurrencyCode = m.DOCurrencyCode,
                    UseVat = m.UseVat,
                    IncomeTaxId = m.IncomeTaxId,
                    IncomeTaxName = m.IncomeTaxName,
                    IncomeTaxRate = m.IncomeTaxRate,
                    LastModifiedUtc = m.LastModifiedUtc,
                    Items = m.Items.Select(i => new GarmentDeliveryOrderItem
                    {
                        Id = i.Id,
                        EPOId = i.EPOId,
                        EPONo = i.EPONo,
                        Details = i.Details.Where(d => d.ReceiptQuantity > 0).ToList()
                    }).ToList()
                });

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentDeliveryOrder> pageable = new Pageable<GarmentDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.doNo,
                    s.docurrency,
                    s.supplier,
                    s.useIncomeTax,
                    s.billNo,
                    s.isInvoice,
                    s.useVat,
                    s.incomeTax,
                    s.LastModifiedUtc,
                    items = s.items.Select(i => new
                    {
                        i.Id,
                        i.purchaseOrderExternal,
                        fulfillments = i.fulfillments.Select(d => new
                        {
                            d.Id,

                            d.pRId,
                            d.pRNo,

                            d.pOId,
                            d.poSerialNumber,

                            d.product,

                            d.rONo,

                            d.purchaseOrderUom,
                            d.quantityCorrection,
                            d.priceTotalCorrection,

                            d.pricePerDealUnit,
                            d.pricePerDealUnitCorrection,

                            receiptCorrection =  dbContext.GarmentUnitReceiptNoteItems.Where(m => m.DODetailId == d.Id && m.IsDeleted == false).Select(m => m.ReceiptCorrection).FirstOrDefault()
                        }).ToList()
                    }).ToList()
                }).ToList()
            );
            return new ReadResponse(listData, Total, OrderDictionary);
        }
    }
}