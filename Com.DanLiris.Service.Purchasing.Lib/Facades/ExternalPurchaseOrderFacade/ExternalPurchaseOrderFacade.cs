using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade
{
    public class ExternalPurchaseOrderFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<ExternalPurchaseOrder> dbSet;

        public ExternalPurchaseOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<ExternalPurchaseOrder>();
        }

        public Tuple<List<ExternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<ExternalPurchaseOrder> Query = this.dbSet;

            Query = Query
                .Select(s => new ExternalPurchaseOrder
                {
                    Id = s.Id,
                    EPONo = s.EPONo,
                    CurrencyCode = s.CurrencyCode,
                    CurrencyRate = s.CurrencyRate,
                    OrderDate = s.OrderDate,
                    DeliveryDate = s.DeliveryDate,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    DivisionCode = s.DivisionCode,
                    DivisionName = s.DivisionName,
                    LastModifiedUtc = s.LastModifiedUtc,
                    UnitName=s.UnitName,
                    UnitCode=s.UnitCode,
                    CreatedBy=s.CreatedBy,
                    IsPosted=s.IsPosted,
                    Items = s.Items.Select(
                        q => new ExternalPurchaseOrderItem
                        {
                            Id = q.Id,
                            POId = q.POId,
                            PRNo = q.PRNo
                        }
                    )
                    .ToList()
                });

            List<string> searchAttributes = new List<string>()
            {
                "EPONo", "SupplierName", "DivisionName","UnitName"
            };

            Query = QueryHelper<ExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<ExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<ExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<ExternalPurchaseOrder> pageable = new Pageable<ExternalPurchaseOrder>(Query, Page - 1, Size);
            List<ExternalPurchaseOrder> Data = pageable.Data.ToList<ExternalPurchaseOrder>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public ExternalPurchaseOrder ReadModelById(int id)
        {
            var a = this.dbSet.Where(d => d.Id.Equals(id) && d.IsDeleted.Equals(false))
                .Include(p => p.Items)
                .ThenInclude(p => p.Details)
                .FirstOrDefault();
            return a;
        }

        public async Task<int> Create(ExternalPurchaseOrder m, string user)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, "Facade");

                    m.EPONo = await GenerateNo(m);

                    foreach (var item in m.Items)
                    {
                        
                        EntityExtension.FlagForCreate(item, user, "Facade");
                        foreach (var detail in item.Details)
                        {
                            detail.PricePerDealUnit= detail.IncludePpn ? (100 * detail.PriceBeforeTax) / 110 : detail.PriceBeforeTax;
                            //PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == detail.PRItemId);
                            //purchaseRequestItem.Status = "Sudah diorder ke Supplier";
                            EntityExtension.FlagForCreate(detail, user, "Facade");

                            InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == detail.POItemId);
                            internalPurchaseOrderItem.Status = "Sudah dibuat PO Eksternal";
                        }
                        InternalPurchaseOrder internalPurchaseOrder = this.dbContext.InternalPurchaseOrders.FirstOrDefault(s => s.Id == item.POId);
                        internalPurchaseOrder.IsPosted = true;
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

        public async Task<int> Update(int id, ExternalPurchaseOrder externalPurchaseOrder, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var existingModel = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .ThenInclude(d=>d.Details)
                        .Single(epo => epo.Id == id && !epo.IsDeleted);

                    if (existingModel != null && id == externalPurchaseOrder.Id)
                    {
                        EntityExtension.FlagForUpdate(externalPurchaseOrder, user, "Facade");

                        foreach (var item in externalPurchaseOrder.Items.ToList())
                        {
                            var existingItem = existingModel.Items.SingleOrDefault(m => m.Id == item.Id);
                            List<ExternalPurchaseOrderItem> duplicateExternalPurchaseOrderItems = externalPurchaseOrder.Items.Where(i => i.POId == item.POId && i.Id != item.Id).ToList();

                            if (item.Id == 0)
                            {
                                if (duplicateExternalPurchaseOrderItems.Count <= 0)
                                {

                                    EntityExtension.FlagForCreate(item, user, "Facade");

                                    foreach (var detail in item.Details)
                                    {
                                        detail.PricePerDealUnit = detail.IncludePpn ? (100 * detail.PriceBeforeTax) / 110 : detail.PriceBeforeTax;
                                        EntityExtension.FlagForCreate(detail, user, "Facade");
                                        //PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == detail.PRItemId);
                                        //purchaseRequestItem.Status = "Sudah diorder ke Supplier";

                                        InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == detail.POItemId);
                                        internalPurchaseOrderItem.Status = "Sudah dibuat PO Eksternal";

                                    }
                                    InternalPurchaseOrder internalPurchaseOrder = this.dbContext.InternalPurchaseOrders.FirstOrDefault(s => s.Id == item.POId);
                                    internalPurchaseOrder.IsPosted = true;
                                }
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(item, user, "Facade");

                                if (duplicateExternalPurchaseOrderItems.Count > 0)
                                {
                                    foreach (var detail in item.Details.ToList())
                                    {
                                        if (detail.Id != 0)
                                        {
                                            EntityExtension.FlagForUpdate(detail, user, "Facade");

                                            foreach (var duplicateItem in duplicateExternalPurchaseOrderItems.ToList())
                                            {
                                                foreach (var duplicateDetail in duplicateItem.Details.ToList())
                                                {
                                                    if (detail.ProductId.Equals(duplicateDetail.ProductId))
                                                    {
                                                        detail.PricePerDealUnit = detail.IncludePpn ? (100 * detail.PriceBeforeTax) / 110 : detail.PriceBeforeTax;
                                                    }
                                                    else if (item.Details.Count(d => d.ProductId.Equals(duplicateDetail.ProductId)) < 1)
                                                    {
                                                        EntityExtension.FlagForCreate(duplicateDetail, user, "Facade");
                                                        item.Details.Add(duplicateDetail);
                                                        detail.PricePerDealUnit = detail.IncludePpn ? (100 * detail.PriceBeforeTax) / 110 : detail.PriceBeforeTax;
                                                        //PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == detail.PRItemId);
                                                        //purchaseRequestItem.Status = "Sudah diorder ke Supplier";

                                                        InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == detail.POItemId);
                                                        internalPurchaseOrderItem.Status = "Sudah dibuat PO Eksternal";
                                                    }
                                                }
                                                externalPurchaseOrder.Items.Remove(duplicateItem);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var detail in item.Details)
                                    {
                                        if (detail.Id != 0)
                                        {
                                            EntityExtension.FlagForUpdate(detail, user, "Facade");
                                            detail.PricePerDealUnit = detail.IncludePpn ? (100 * detail.PriceBeforeTax) / 110 : detail.PriceBeforeTax;

                                        }
                                    }
                                }
                            }
                        }

                        this.dbContext.Update(externalPurchaseOrder);

                        foreach (var existingItem in existingModel.Items)
                        {
                            var newItem = externalPurchaseOrder.Items.FirstOrDefault(i => i.Id == existingItem.Id);
                            if (newItem == null)
                            {
                                EntityExtension.FlagForDelete(existingItem, user, "Facade");
                                InternalPurchaseOrder internalPurchaseOrder = this.dbContext.InternalPurchaseOrders.FirstOrDefault(s => s.Id == existingItem.POId);
                                internalPurchaseOrder.IsPosted = false;
                                this.dbContext.ExternalPurchaseOrderItems.Update(existingItem);
                                foreach (var existingDetail in existingItem.Details)
                                {
                                    EntityExtension.FlagForDelete(existingDetail, user, "Facade");
                                    //PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == existingDetail.PRItemId);
                                    //purchaseRequestItem.Status = "Sudah diterima Pembelian";

                                    InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == existingDetail.POItemId);
                                    internalPurchaseOrderItem.Status = "PO Internal belum diorder";
                                    this.dbContext.ExternalPurchaseOrderDetails.Update(existingDetail);
                                }
                            }
                            else
                            {
                                foreach (var existingDetail in existingItem.Details)
                                {
                                    var newDetail = newItem.Details.FirstOrDefault(d => d.Id == existingDetail.Id);
                                    if (newDetail == null)
                                    {
                                        EntityExtension.FlagForDelete(existingDetail, user, "Facade");
                                        //PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == existingDetail.PRItemId);
                                        //purchaseRequestItem.Status = "Sudah diterima Pembelian";

                                        InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == existingDetail.POItemId);
                                        internalPurchaseOrderItem.Status = "PO Internal belum diorder";
                                        this.dbContext.ExternalPurchaseOrderDetails.Update(existingDetail);

                                    }
                                }
                            }
                        }

                        Updated = await dbContext.SaveChangesAsync();
                        transaction.Commit();
                        
                    }
                    else
                    {
                        throw new Exception("Error");
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

        public int Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .ThenInclude(d=>d.Details)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForDelete(m, user, "Facade");

                    foreach (var item in m.Items)
                    {

                        EntityExtension.FlagForDelete(item, user, "Facade");
                        foreach (var detail in item.Details)
                        {
                            //PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == detail.PRItemId);
                            //purchaseRequestItem.Status = "Sudah diterima Pembelian";
                            EntityExtension.FlagForDelete(detail, user, "Facade");

                            InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == detail.POItemId);
                            internalPurchaseOrderItem.Status = "PO Internal belum diorder";
                        }
                        InternalPurchaseOrder internalPurchaseOrder = this.dbContext.InternalPurchaseOrders.FirstOrDefault(s => s.Id == item.POId);
                        internalPurchaseOrder.IsPosted = false;

                    }

                    Deleted = dbContext.SaveChanges();
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

        public int EPOPost(List<ExternalPurchaseOrder> ListEPO, string user)
        {
            int Updated = 0;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var Ids = ListEPO.Select(d => d.Id).ToList();
                    var listData = this.dbSet
                        .Where(m => Ids.Contains(m.Id) && !m.IsDeleted)
                        .Include(d => d.Items)
                        .ThenInclude(d=>d.Details)
                        .ToList();
                    listData.ForEach(m =>
                    {
                        EntityExtension.FlagForUpdate(m, user, "Facade");
                        m.IsPosted = true;

                        foreach (var item in m.Items)
                        {
                            EntityExtension.FlagForUpdate(item, user, "Facade");
                            foreach (var detail in item.Details)
                            {
                                EntityExtension.FlagForUpdate(detail, user, "Facade");
                                InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == detail.POItemId);
                                internalPurchaseOrderItem.Status = "Sudah diorder ke Supplier";

                                PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == detail.PRItemId);
                                purchaseRequestItem.Status = "Sudah diorder ke Supplier";
                            }
                        }
                    });

                    Updated = dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public int EPOCancel(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .ThenInclude(d => d.Details)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted );

                    EntityExtension.FlagForUpdate(m, user, "Facade");
                    m.IsCanceled = true;

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForUpdate(item, user, "Facade");
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForUpdate(detail, user, "Facade");

                            InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == detail.POItemId);
                            internalPurchaseOrderItem.Status = "Dibatalkan";

                            PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == detail.PRItemId);
                            purchaseRequestItem.Status = "Dibatalkan";
                        }
                    }

                    Updated = dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public int EPOClose(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .ThenInclude(d => d.Details)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForUpdate(m, user, "Facade");
                    m.IsClosed = true;

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForUpdate(item, user, "Facade");
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForUpdate(detail, user, "Facade");
                        }
                        InternalPurchaseOrder internalPurchaseOrder = this.dbContext.InternalPurchaseOrders.FirstOrDefault(s => s.Id == item.POId);
                        internalPurchaseOrder.IsClosed = true;
                    }

                    Updated = dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public int EPOUnpost(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .ThenInclude(d => d.Details)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForUpdate(m, user, "Facade");
                    m.IsPosted = false;

                    foreach (var item in m.Items)
                    {
                        var existPR = this.dbContext.ExternalPurchaseOrderItems.Where(a => a.PRId == item.PRId && a.IsDeleted==false && a.EPOId!=item.EPOId);
                        EntityExtension.FlagForUpdate(item, user, "Facade");
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForUpdate(detail, user, "Facade");

                            if (existPR == null)
                            {
                                PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == detail.PRItemId);
                                purchaseRequestItem.Status = "Sudah diterima Pembelian";
                            }


                            InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == detail.POItemId);
                            internalPurchaseOrderItem.Status = "Sudah dibuat PO Eksternal";
                        }
                    }

                    Updated = dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        async Task<string> GenerateNo(ExternalPurchaseOrder model)
        {
            DateTimeOffset Now = model.OrderDate;
            string Year = Now.ToString("yy");
            string Month = Now.ToString("MM");

            string no = $"PE-{model.UnitCode}-{Year}-{Month}-";
            int Padding = 3;

            var lastNo = await this.dbSet.Where(w => w.EPONo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.EPONo).FirstOrDefaultAsync();
            no = $"{no}";

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.EPONo.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public List<ExternalPurchaseOrder> ReadUnused(string Keyword = null, string Filter = "{}")
        {
            IQueryable<ExternalPurchaseOrder> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "EPONo", "SupplierName", "DivisionName","UnitName"
            };

            Query = QueryHelper<ExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah

            Query = Query
                .Where(m => m.IsPosted == true && m.IsCanceled == false && m.IsClosed == false && m.IsDeleted == false)
                .Select(s => new ExternalPurchaseOrder
                {
                    Id = s.Id,
                    EPONo = s.EPONo,
                    CurrencyCode = s.CurrencyCode,
                    CurrencyRate = s.CurrencyRate,
                    OrderDate = s.OrderDate,
                    DeliveryDate = s.DeliveryDate,
                    SupplierId = s.SupplierId,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    DivisionCode = s.DivisionCode,
                    DivisionName = s.DivisionName,
                    LastModifiedUtc = s.LastModifiedUtc,
                    UnitName = s.UnitName,
                    UnitCode = s.UnitCode,
                    CreatedBy = s.CreatedBy,
                    IsPosted = s.IsPosted,
                    Items = s.Items
                        .Select(i => new ExternalPurchaseOrderItem
                        {
                            Id = i.Id,
                            PRId = i.PRId,
                            PRNo = i.PRNo,
                            UnitId = i.UnitId,
                            UnitCode = i.UnitCode,
                            UnitName = i.UnitName,
                            Details = i.Details
                                .Where(d => d.DOQuantity < d.DealQuantity && d.IsDeleted == false)
                                .Select(d => new ExternalPurchaseOrderDetail
                                {
                                    Id = d.Id,
                                    POItemId = d.POItemId,
                                    PRItemId = d.PRItemId,
                                    ProductId = d.ProductId,
                                    ProductCode = d.ProductCode,
                                    ProductName = d.ProductName,
                                    DealQuantity = d.DealQuantity,
                                    DealUomId = d.DealUomId,
                                    DealUomUnit = d.DealUomUnit,
                                    DOQuantity = d.DOQuantity,
                                    ProductRemark = d.ProductRemark,
                                })
                                .ToList()
                        })
                        .Where(i => i.Details.Count > 0)
                        .ToList()
                })
                .Where(m => m.Items.Count > 0);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<ExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            return Query.ToList();
        }

    }
}
