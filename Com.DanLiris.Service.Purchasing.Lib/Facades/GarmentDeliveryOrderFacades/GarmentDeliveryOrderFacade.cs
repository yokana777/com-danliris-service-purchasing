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
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
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
                        m.TotalAmount = 0;
                        foreach (var vmItem in vm.items)
                        {

                            foreach (var modelItem in m.Items.Where(i => i.Id == vmItem.Id))
                            {
                                if (modelItem.Id == 0)
                                {
                                    EntityExtension.FlagForCreate(modelItem, user, USER_AGENT);

                                    CurrencyViewModel garmentCurrencyViewModel = GetCurrency(modelItem.CurrencyCode, m.DODate);
                                    m.DOCurrencyId = garmentCurrencyViewModel.Id;
                                    m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                                    m.DOCurrencyRate = garmentCurrencyViewModel.Rate;
                                    foreach (var vmDetail in vmItem.fulfillments)
                                    {
                                        foreach (var modelDetail in modelItem.Details.Where(j => j.POId == vmDetail.pOId))
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
                                                        m.TotalAmount += modelDetail.PriceTotal;

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
                            var newItem = m.Items.FirstOrDefault(i => i.EPOId.Equals(oldItem.EPOId));
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
                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForDelete(item, user, USER_AGENT);

                        foreach (var detail in item.Details)
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
                    .ThenInclude(i => i.Details).Where(s => s.IsInvoice == false && !string.IsNullOrWhiteSpace(s.BillNo));
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
                .ThenInclude(i => i.Details).Where(s => s.BillNo == null);
            //}

            return Query;
        }


        public int IsReceived(List<int> id)
        {
            int isReceived = 0;
            foreach (var no in id)
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

        private CurrencyViewModel GetCurrency(string currencyCode, DateTimeOffset doDate)
        {
            string currencyUri = "master/garment-currencies/byCode";
            IHttpClientService httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));

            var response = httpClient.GetAsync($"{APIEndpoint.Core}{currencyUri}/{currencyCode}").Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                List<CurrencyViewModel> viewModel = JsonConvert.DeserializeObject<List<CurrencyViewModel>>(result.GetValueOrDefault("data").ToString());
                return viewModel.OrderByDescending(s => s.Date).FirstOrDefault(s => s.Date < doDate.AddDays(1)); ;
            }
            else
            {
                return null;
            }
        }

        public ReadResponse<object> ReadForUnitReceiptNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long filterSupplierId = FilterDictionary.ContainsKey("SupplierId") ? long.Parse(FilterDictionary["SupplierId"]) : 0;
            FilterDictionary.Remove("SupplierId");

            var filterUnitId = FilterDictionary.ContainsKey("UnitId") ? FilterDictionary["UnitId"] : string.Empty;
            FilterDictionary.Remove("UnitId");

            IQueryable<GarmentDeliveryOrder> Query = dbSet
                .Where(m => m.DONo.Contains(Keyword ?? "") && (filterSupplierId == 0 ? true : m.SupplierId == filterSupplierId) && m.BillNo != null && m.Items.Any(i => i.Details.Any(d => d.ReceiptQuantity == 0 && (string.IsNullOrWhiteSpace(filterUnitId) ? true : d.UnitId == filterUnitId))))
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
                            productRemark = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == d.ePOItemId).Select(m => m.Remark).FirstOrDefault(),

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

            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }

        public ReadResponse<object> ReadForCorrectionNoteQuantity(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            IQueryable<GarmentDeliveryOrder> Query = dbSet

                .Where(m => m.DONo.Contains(Keyword ?? "") && m.BillNo !=null && m.Items.Any(i => i.Details.Any(d => d.ReceiptQuantity > 0 )))
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
                    DOCurrencyId = m.DOCurrencyId,
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
                            d.doQuantity,
                            d.purchaseOrderUom,
                            d.quantityCorrection,
                            d.priceTotalCorrection,

                            d.pricePerDealUnit,
                            d.pricePerDealUnitCorrection,

                            d.returQuantity,
                            receiptCorrection = dbContext.GarmentUnitReceiptNoteItems.Where(m => m.DODetailId == d.Id && m.IsDeleted == false).Select(m => m.ReceiptCorrection).FirstOrDefault()
                        }).ToList()
                    }).ToList()
                }).ToList()
            );
            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }

        public IQueryable<AccuracyOfArrivalReportViewModel> GetReportQuery(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            List<string> Category = null;
            List<string> Product = null;
            var Status = new[] { "" };
            var Supplier = new[] { "MADEIRA", "MARATHON" };

            switch (category)
            {
                case "BB":
                    Status = new[] { "FABRIC", "INTERLINING" };
                    break;
                case "BP":
                    Status = new[] { "FABRIC", "INTERLINING", "PLISKET", "PRINT", "QUILTING", "WASH" };
                    break;
                default:
                    Status = new[] { "" };
                    break;
            }
            // if (category == "")
            // {
            //  var categoryAll = "[\"BB\",\"BP\"]";
            //  Category = JsonConvert.DeserializeObject<List<string>>(categoryAll);
            // }

            List<AccuracyOfArrivalReportViewModel> listAccuracyOfArrival = new List<AccuracyOfArrivalReportViewModel>();

            var Query = (from a in dbContext.GarmentDeliveryOrders
                         join b in dbContext.GarmentDeliveryOrderItems on a.Id equals b.GarmentDOId
                         join c in dbContext.GarmentDeliveryOrderDetails on b.Id equals c.GarmentDOItemId
                         join d in dbContext.GarmentInternalPurchaseOrders on c.POId equals d.Id
                         join e in dbContext.GarmentInternalPurchaseOrderItems on d.Id equals e.GPOId
                         join f in dbContext.GarmentPurchaseRequests on c.PRId equals f.Id
                         join g in dbContext.GarmentPurchaseRequestItems on f.Id equals g.GarmentPRId
                         join h in dbContext.GarmentExternalPurchaseOrders on b.EPOId equals h.Id
                         join i in dbContext.GarmentExternalPurchaseOrderItems on h.Id equals i.GarmentEPOId
                         where a.IsDeleted == false
                             && d.IsDeleted == false
                             && f.IsDeleted == false
                             && h.IsDeleted == false
                             && ((DateFrom != new DateTime(1970, 1, 1)) ? (a.DODate.Date >= DateFrom && a.DODate.Date <= DateTo) : true)
                             && (category == "BB" ? Status.Contains(c.ProductName) : (category == "BP" ? !Status.Contains(c.ProductName) || Supplier.Contains(a.SupplierName) : c.ProductName == c.ProductName))
                             && !c.RONo.EndsWith("S")
                         //  && (category == "" ? Category.Contains(c.CodeRequirment) : c.CodeRequirment==category)
                         select new AccuracyOfArrivalReportViewModel
                         {
                             supplier = new SupplierViewModel
                             {
                                 Code = a.SupplierCode,
                                 Id = a.SupplierId,
                                 Name = a.SupplierName
                             },
                             poSerialNumber = c.POSerialNumber,
                             prDate = f.Date,    //distinct garmentdodetailid
                             poDate = d.CreatedUtc,
                             epoDate = h.OrderDate,
                             product = new GarmentProductViewModel
                             {
                                 Code = c.ProductCode,
                                 Id = c.ProductId,
                                 Name = c.ProductName,
                                 Remark = c.ProductRemark,
                             },
                             article = i.Article,
                             roNo = c.RONo,
                             shipmentDate = f.ShipmentDate,
                             doDate = a.DODate,
                             staff = a.CreatedBy,
                             category = category,
                             doNo = a.DONo,
                             ok_notOk = "NOT OK",
                             LastModifiedUtc = i.LastModifiedUtc
                         }).Distinct();

            Query = Query.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate);
            var suppTemp = "";
            var percentOK = 0;
            var percentNotOk = 0;
            var jumlah = 0;
            
            foreach (var item in Query)
            {
                var ShipmentDate = new DateTimeOffset(item.shipmentDate.Date, TimeSpan.Zero);
                var DODate = new DateTimeOffset(item.doDate.Date, TimeSpan.Zero);
                var jumlahOk = 0;
                var datediff = ((TimeSpan)(ShipmentDate - DODate)).Days;
               
                if(item.category == "BB")
                {
                    if (datediff >= 27)
                    {
                        item.ok_notOk = "OK";
                    }
                } else if(item.category == "BP")
                {
                    if (datediff >= 20)
                    {
                        item.ok_notOk = "OK";
                    }
                }
                if (suppTemp == "")
                {
                    jumlah += 1;
                    suppTemp = item.supplier.Code;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }
                   
                }
                else if(suppTemp == item.supplier.Code)
                {
                    jumlah += 1;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }
                }
                else if (suppTemp != item.supplier.Code)
                {
                    var perOk = 0;
                    var perNotOk = 0;
                    suppTemp = item.supplier.Code;
                    jumlah = 1;
                    jumlahOk = perOk;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK = perOk + 1;
                        percentNotOk = perNotOk;
                    }
                    else
                    {
                        percentNotOk = perNotOk + 1;
                        percentOK = perOk;
                    }
                }
                jumlahOk = percentOK + percentNotOk;
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    category = item.category,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = (percentOK *100) / jumlah,
                    jumlahOk = percentOK,
                    jumlah = jumlah,
                    dateDiff = datediff,
                    LastModifiedUtc = item.LastModifiedUtc
                };
                listAccuracyOfArrival.Add(_new);
            }
            return listAccuracyOfArrival.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate).AsQueryable();
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportHeaderAccuracyofArrival(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var ctg = "";
            if(category== "Bahan Baku")
            {
                ctg = "BB";
            } else if(category == "Bahan Pendukung")
            {
                ctg = "BP";
            }

            var QuerySupplier = GetReportQuery(ctg, dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();
            List<AccuracyOfArrivalReportViewModel> Data2 = new List<AccuracyOfArrivalReportViewModel>();

            var SuppTemp = "";
            foreach(var item in QuerySupplier.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.jumlah))
            {
                if (SuppTemp == "" || SuppTemp != item.supplier.Code)
                {
                    SuppTemp = item.supplier.Code;

                    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                    {
                        supplier = item.supplier,
                        poSerialNumber = item.poSerialNumber,
                        prDate = item.prDate,
                        poDate = item.poDate,
                        epoDate = item.epoDate,
                        product = item.product,
                        article = item.article,
                        roNo = item.roNo,
                        shipmentDate = item.shipmentDate,
                        doDate = item.doDate,
                        staff = item.staff,
                        category = item.category,
                        doNo = item.doNo,
                        ok_notOk = item.ok_notOk,
                        percentOk_notOk = item.percentOk_notOk,
                        jumlah = item.jumlah,
                        jumlahOk = item.jumlahOk,
                        dateDiff = item.dateDiff,
                        LastModifiedUtc = item.LastModifiedUtc
                    };
                    Data.Add(_new);
                }
            }
            foreach (var items in Data.OrderBy(b => b.percentOk_notOk).ThenByDescending(b => b.jumlah))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = items.supplier,
                    poSerialNumber = items.poSerialNumber,
                    prDate = items.prDate,
                    poDate = items.poDate,
                    epoDate = items.epoDate,
                    product = items.product,
                    article = items.article,
                    roNo = items.roNo,
                    shipmentDate = items.shipmentDate,
                    doDate = items.doDate,
                    staff = items.staff,
                    category = items.category,
                    doNo = items.doNo,
                    ok_notOk = items.ok_notOk,
                    percentOk_notOk = items.percentOk_notOk,
                    jumlah = items.jumlah,
                    jumlahOk = items.jumlahOk,
                    dateDiff = items.dateDiff,
                    LastModifiedUtc = items.LastModifiedUtc
                };
                Data2.Add(_new);
            }
            return Tuple.Create(Data2, Data.Count);
        }

        public MemoryStream GenerateExcelArrivalHeader(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var ctg = "";
            if (category == "Bahan Baku")
            {
                ctg = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                ctg = "BP";
            }
            var Query = GetReportQuery(ctg, dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();
            List<AccuracyOfArrivalReportViewModel> Data2 = new List<AccuracyOfArrivalReportViewModel>();

            var SuppTemp = "";
            foreach (var item in Query.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.jumlah))
            {
                if (SuppTemp == "" || SuppTemp != item.supplier.Code)
                {
                    SuppTemp = item.supplier.Code;

                    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                    {
                        supplier = item.supplier,
                        poSerialNumber = item.poSerialNumber,
                        prDate = item.prDate,
                        poDate = item.poDate,
                        epoDate = item.epoDate,
                        product = item.product,
                        article = item.article,
                        roNo = item.roNo,
                        shipmentDate = item.shipmentDate,
                        doDate = item.doDate,
                        staff = item.staff,
                        category = item.category,
                        doNo = item.doNo,
                        ok_notOk = item.ok_notOk,
                        percentOk_notOk = item.percentOk_notOk,
                        jumlah = item.jumlah,
                        jumlahOk = item.jumlahOk,
                        dateDiff = item.dateDiff,
                        LastModifiedUtc = item.LastModifiedUtc
                    };
                    Data.Add(_new);
                }
            }
            foreach (var items in Data.OrderBy(b => b.percentOk_notOk).ThenByDescending(b => b.jumlah))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = items.supplier,
                    poSerialNumber = items.poSerialNumber,
                    prDate = items.prDate,
                    poDate = items.poDate,
                    epoDate = items.epoDate,
                    product = items.product,
                    article = items.article,
                    roNo = items.roNo,
                    shipmentDate = items.shipmentDate,
                    doDate = items.doDate,
                    staff = items.staff,
                    category = items.category,
                    doNo = items.doNo,
                    ok_notOk = items.ok_notOk,
                    percentOk_notOk = items.percentOk_notOk,
                    jumlah = items.jumlah,
                    jumlahOk = items.jumlahOk,
                    dateDiff = items.dateDiff,
                    LastModifiedUtc = items.LastModifiedUtc
                };
                Data2.Add(_new);
            }

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "OK %", DataType = typeof(int) });
            result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH", DataType = typeof(int) });

            if (Data2.ToArray().Count() == 0)
                result.Rows.Add("", "", 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Data2)
                {
                    index++;
                    result.Rows.Add(index, item.supplier.Name, item.percentOk_notOk, item.jumlah);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportDetailAccuracyofArrival(string supplier, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var ctg = "";
            if (category == "Bahan Baku")
            {
                ctg = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                ctg = "BP";
            }

            var QuerySupplier = GetReportQuery(ctg, dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            foreach (var item in QuerySupplier.Where(b=>b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                     supplier = item.supplier,
                     poSerialNumber = item.poSerialNumber,
                     prDate = item.prDate,
                     poDate = item.poDate,
                     epoDate = item.epoDate,
                     product = item.product,
                     article = item.article,
                     roNo = item.roNo,
                     shipmentDate = item.shipmentDate,
                     doDate = item.doDate,
                     staff = item.staff,
                     category = item.category,
                     doNo = item.doNo,
                     ok_notOk = item.ok_notOk,
                     percentOk_notOk = item.percentOk_notOk,
                     jumlah = item.jumlah,
                     jumlahOk = item.jumlahOk,
                     dateDiff = item.dateDiff,
                     LastModifiedUtc = item.LastModifiedUtc
                 };
                 Data.Add(_new);
            }
            return Tuple.Create(Data, Data.Count);
        }

        public MemoryStream GenerateExcelArrivalDetail(string supplier, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var ctg = "";
            if (category == "Bahan Baku")
            {
                ctg = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                ctg = "BP";
            }
            var QuerySupplier = GetReportQuery(ctg, dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            foreach (var item in QuerySupplier.Where(b => b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    category = item.category,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = item.percentOk_notOk,
                    jumlah = item.jumlah,
                    jumlahOk = item.jumlahOk,
                    dateDiff = item.dateDiff,
                    LastModifiedUtc = item.LastModifiedUtc
                };
                Data.Add(_new);
            }

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PLAN PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PURCHASE REQUEST", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PO INTERNAL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PEMBELIAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO SJ", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "ARTIKEL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL SHIPMENT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "+/- DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "STAFF", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KATEGORI", DataType = typeof(String) });

            if (Data.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Data)
                {
                    index++;
                    string prDate = item.prDate == null ? "-" : item.prDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poDate = item.poDate == null ? "-" : item.poDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoDate = item.epoDate == null ? "-" : item.epoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string shipmentDate = item.shipmentDate == null ? "-" : item.shipmentDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string doDate = item.doDate == null ? "-" : item.doDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.supplier.Name, item.poSerialNumber, prDate, poDate, epoDate, item.doNo, item.product.Code, item.product.Name, item.product.Remark, item.article, item.roNo,
                        shipmentDate, doDate, item.ok_notOk, item.staff, item.product.Name);
                }
            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }


        public IQueryable<AccuracyOfArrivalReportViewModel> GetReportQuery2(DateTime? dateFrom, DateTime? dateTo, int offset)
        {

            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            List<AccuracyOfArrivalReportViewModel> listAccuracyOfArrival = new List<AccuracyOfArrivalReportViewModel>();

            var Query = (from a in dbContext.GarmentDeliveryOrders
                         join b in dbContext.GarmentDeliveryOrderItems on a.Id equals b.GarmentDOId
                         join c in dbContext.GarmentDeliveryOrderDetails on b.Id equals c.GarmentDOItemId
                         join d in dbContext.GarmentInternalPurchaseOrders on c.POId equals d.Id
                         join e in dbContext.GarmentInternalPurchaseOrderItems on d.Id equals e.GPOId
                         join f in dbContext.GarmentPurchaseRequests on c.PRId equals f.Id
                         join g in dbContext.GarmentPurchaseRequestItems on f.Id equals g.GarmentPRId
                         join h in dbContext.GarmentExternalPurchaseOrders on b.EPOId equals h.Id
                         join i in dbContext.GarmentExternalPurchaseOrderItems on h.Id equals i.GarmentEPOId
                         where a.IsDeleted == false
                             && d.IsDeleted == false
                             && f.IsDeleted == false
                             && h.IsDeleted == false
                             && ((DateFrom != new DateTime(1970, 1, 1)) ? (a.DODate.Date >= DateFrom && a.DODate.Date <= DateTo) : true)
                         select new AccuracyOfArrivalReportViewModel
                         {
                             supplier = new SupplierViewModel
                             {
                                 Code = a.SupplierCode,
                                 Id = a.SupplierId,
                                 Name = a.SupplierName
                             },
                             poSerialNumber = c.POSerialNumber,
                             prDate = f.Date,
                             poDate = d.CreatedUtc,
                             epoDate = h.OrderDate,
                             product = new GarmentProductViewModel
                             {
                                 Code = c.ProductCode,
                                 Id = c.ProductId,
                                 Name = c.ProductName,
                                 Remark = c.ProductRemark,
                             },
                             article = i.Article,
                             roNo = c.RONo,
                             shipmentDate = h.DeliveryDate,
                             doDate = a.DODate,
                             staff = a.CreatedBy,
                             doNo = a.DONo,
                             ok_notOk = "NOT OK",
                             LastModifiedUtc = i.LastModifiedUtc
                         }).Distinct();
            Query = Query.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate);
            var suppTemp = "";
            var percentOK = 0;
            var percentNotOk = 0;
            var jumlah = 0;
            
            foreach (var item in Query)
            {
                var ShipmentDate = new DateTimeOffset(item.shipmentDate.Date, TimeSpan.Zero);
                var DODate = new DateTimeOffset(item.doDate.Date, TimeSpan.Zero);
                var jumlahOk = 0;
                var datediff = ((TimeSpan)(DODate - ShipmentDate)).Days;

                if (datediff <= 7)
                {
                    item.ok_notOk = "OK";
                }

                if (suppTemp == "")
                {
                    jumlah += 1;
                    suppTemp = item.supplier.Code;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }

                }
                else if (suppTemp == item.supplier.Code)
                {
                    jumlah += 1;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }
                }
                else if (suppTemp != item.supplier.Code)
                {
                    var perOk = 0;
                    var perNotOk = 0;
                    suppTemp = item.supplier.Code;
                    jumlah = 1;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK = perOk + 1;
                        percentNotOk = perNotOk;
                    }
                    else
                    {
                        percentNotOk = perNotOk + 1;
                        percentOK = perOk;
                    }
                }
                jumlahOk = percentOK + percentNotOk;
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = (percentOK*100) / jumlah,
                    jumlah = jumlah,
                    jumlahOk = percentOK,
                    dateDiff = datediff,
                    LastModifiedUtc = item.LastModifiedUtc
                };
                listAccuracyOfArrival.Add(_new);
            }
            return listAccuracyOfArrival.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate).AsQueryable();
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportHeaderAccuracyofDelivery(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var QuerySupplier = GetReportQuery2(dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            var SuppTemp = "";
            foreach (var item in QuerySupplier.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.jumlah))
            {
                if (SuppTemp == "" || SuppTemp != item.supplier.Code)
                {
                    SuppTemp = item.supplier.Code;

                    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                    {
                        supplier = item.supplier,
                        poSerialNumber = item.poSerialNumber,
                        prDate = item.prDate,
                        poDate = item.poDate,
                        epoDate = item.epoDate,
                        product = item.product,
                        article = item.article,
                        roNo = item.roNo,
                        shipmentDate = item.shipmentDate,
                        doDate = item.doDate,
                        staff = item.staff,
                        doNo = item.doNo,
                        ok_notOk = item.ok_notOk,
                        percentOk_notOk = item.percentOk_notOk,
                        jumlah = item.jumlah,
                        jumlahOk = item.jumlahOk,
                        dateDiff = item.dateDiff,
                        LastModifiedUtc = item.LastModifiedUtc
                    };
                    Data.Add(_new);
                }
            }
            return Tuple.Create(Data, Data.Count);
        }

        public MemoryStream GenerateExcelDeliveryHeader(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery2(dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            var SuppTemp = "";
            foreach (var item in Query.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.jumlah))
            {
                if (SuppTemp == "" || SuppTemp != item.supplier.Code)
                {
                    SuppTemp = item.supplier.Code;

                    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                    {
                        supplier = item.supplier,
                        poSerialNumber = item.poSerialNumber,
                        prDate = item.prDate,
                        poDate = item.poDate,
                        epoDate = item.epoDate,
                        product = item.product,
                        article = item.article,
                        roNo = item.roNo,
                        shipmentDate = item.shipmentDate,
                        doDate = item.doDate,
                        staff = item.staff,
                        doNo = item.doNo,
                        ok_notOk = item.ok_notOk,
                        percentOk_notOk = item.percentOk_notOk,
                        jumlah = item.jumlah,
                        jumlahOk = item.jumlahOk,
                        dateDiff = item.dateDiff,
                        LastModifiedUtc = item.LastModifiedUtc
                    };
                    Data.Add(_new);
                }
            }

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "OK %", DataType = typeof(int) });
            result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH", DataType = typeof(int) });

            if (Data.ToArray().Count() == 0)
                result.Rows.Add("", "", 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Data)
                {
                    index++;
                    result.Rows.Add(index, item.supplier.Name, item.percentOk_notOk, item.jumlah);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportDetailAccuracyofDelivery(string supplier, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var QuerySupplier = GetReportQuery2(dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            foreach (var item in QuerySupplier.Where(b => b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = item.percentOk_notOk,
                    jumlah = item.jumlah,
                    jumlahOk = item.jumlahOk,
                    dateDiff = item.dateDiff,
                    LastModifiedUtc = item.LastModifiedUtc
                };
                Data.Add(_new);
            }
            return Tuple.Create(Data, Data.Count);
        }

        public MemoryStream GenerateExcelDeliveryDetail(string supplier, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var QuerySupplier = GetReportQuery2(dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            foreach (var item in QuerySupplier.Where(b => b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    category = item.category,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = item.percentOk_notOk,
                    jumlah = item.jumlah,
                    jumlahOk = item.jumlahOk,
                    dateDiff = item.dateDiff,
                    LastModifiedUtc = item.LastModifiedUtc
                };
                Data.Add(_new);
            }

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PLAN PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PURCHASE REQUEST", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PO INTERNAL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PEMBELIAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO SJ", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "ARTIKEL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL ESTIMASI DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "+/- DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "STAFF", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KATEGORI", DataType = typeof(String) });

            if (Data.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Data)
                {
                    index++;
                    string prDate = item.prDate == null ? "-" : item.prDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poDate = item.poDate == null ? "-" : item.poDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoDate = item.epoDate == null ? "-" : item.epoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string shipmentDate = item.shipmentDate == null ? "-" : item.shipmentDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string doDate = item.doDate == null ? "-" : item.doDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.supplier.Name, item.poSerialNumber, prDate, poDate, epoDate, item.doNo, item.product.Code, item.product.Name, item.product.Remark, item.article, item.roNo,
                        shipmentDate, doDate, item.ok_notOk, item.staff, item.product.Name);
                }
            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }


        public IQueryable<GarmentDeliveryOrderReportViewModel> GetReportQueryDO(string no, string poEksNo, long supplierId, DateTime? dateFrom, DateTime? dateTo, int offset)

        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = (from a in dbContext.GarmentDeliveryOrders
                         join i in dbContext.GarmentDeliveryOrderItems on a.Id equals i.GarmentDOId
                         join j in dbContext.GarmentDeliveryOrderDetails on i.Id equals j.GarmentDOItemId
                        
                         where a.IsDeleted == false
                             && i.IsDeleted == false
                             && j.IsDeleted == false
                            
                             && a.DONo == (string.IsNullOrWhiteSpace(no) ? a.DONo : no)
                           
                            && a.SupplierId == (supplierId == 0 ? a.SupplierId : supplierId)
                            && a.DODate.AddHours(offset).Date >= DateFrom.Date
                             && a.DODate.AddHours(offset).Date <= DateTo.Date
                             && i.EPONo == (string.IsNullOrWhiteSpace(poEksNo) ? i.EPONo : poEksNo)
                         select new GarmentDeliveryOrderReportViewModel
                         {
                             no = a.DONo,
                             supplierDoDate = a.DODate == null ? new DateTime(1970, 1, 1) : a.DODate,
                             date = a.ArrivalDate,
                             supplierName = a.SupplierName,
                             supplierCode = a.SupplierCode,
                             shipmentNo = a.ShipmentNo,
                             shipmentType = a.ShipmentType,
                             createdBy = a.CreatedBy,
                             doCurrencyCode = a.DOCurrencyCode,
                             isCustoms = a.IsCustoms,
                             price = j.PricePerDealUnit,
                             ePONo = i.EPONo,
                             productCode = j.ProductCode,
                             productName = j.ProductName,
                             productRemark = j.ProductRemark,
                             prRefNo = j.POSerialNumber,
                             roNo = j.RONo,
                             prNo = j.PRNo,
                             remark = a.Remark,
                             dOQuantity = j.DOQuantity,
                             dealQuantity = j.DealQuantity,
                             uomUnit = j.UomUnit,
                             createdUtc = j.CreatedUtc,
                           
                         });
            Dictionary<string, double> q = new Dictionary<string, double>();
            List<GarmentDeliveryOrderReportViewModel> urn = new List<GarmentDeliveryOrderReportViewModel>();
            foreach (GarmentDeliveryOrderReportViewModel data in Query.ToList())
            {
                double value;
                if (q.TryGetValue(data.productCode + data.ePONo + data.ePODetailId, out value))
                {
                    q[data.productCode + data.ePONo + data.ePODetailId] -= data.dOQuantity;
                    data.remainingQuantity = q[data.productCode + data.ePONo + data.ePODetailId];
                    urn.Add(data);
                }
                else
                {
                    q[data.productCode + data.ePONo + data.ePODetailId] = data.remainingQuantity - data.dOQuantity;
                    data.remainingQuantity = q[data.productCode + data.ePONo + data.ePODetailId];
                    urn.Add(data);
                }
            }
            return Query = urn.AsQueryable();
        }


        public Tuple<List<GarmentDeliveryOrderReportViewModel>, int> GetReportDO(string no, string poEksNo, long supplierId, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)

        {
            var Query = GetReportQueryDO(no, poEksNo, supplierId, dateFrom, dateTo, offset);


            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.supplierDoDate).ThenByDescending(b => b.createdUtc);
            }


            Pageable<GarmentDeliveryOrderReportViewModel> pageable = new Pageable<GarmentDeliveryOrderReportViewModel>(Query, page - 1, size);
            List<GarmentDeliveryOrderReportViewModel> Data = pageable.Data.ToList<GarmentDeliveryOrderReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcelDO(string no, string poEksNo, long supplierId, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQueryDO(no, poEksNo, supplierId, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.supplierDoDate).ThenByDescending(b => b.createdUtc);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Tiba", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Pengiriman", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor BL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Dikenakan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Referensi PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Dipesan", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Diterima", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Staff Pembelian", DataType = typeof(String) });

            if (Query.ToArray().Count() == 0)
                // result.Rows.Add("", "", "", "", "", "", "", "", "", "", 0, 0, 0, ""); // to allow column name to be generated properly for empty data as template
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, 0, "", 0, "", "", "");
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string date = item.date == null ? "-" : item.date.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string supplierDoDate = item.supplierDoDate == new DateTime(1970, 1, 1) ? "-" : item.supplierDoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string dikenakan = item.isCustoms == true ? "Ya" : "Tidak";
                    string jenissupp = item.shipmentType == "" ? "Local" : "Import";
                    // result.Rows.Add(index, item.supplierCode, item.supplierName, item.no, supplierDoDate, date, item.ePONo, item.productCode, item.productName, item.productRemark, item.dealQuantity, item.dOQuantity, item.remainingQuantity, item.uomUnit);
                    result.Rows.Add(index, item.no, supplierDoDate, date, item.supplierName, jenissupp, item.shipmentType, item.shipmentNo, dikenakan, item.ePONo, item.prNo, item.roNo, item.prRefNo, item.productCode, item.productName, item.dealQuantity, item.dOQuantity, item.uomUnit, item.price, item.doCurrencyCode, item.productRemark, item.createdBy);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
    }
}