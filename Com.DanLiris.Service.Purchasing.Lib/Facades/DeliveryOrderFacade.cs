using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
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

namespace Com.DanLiris.Service.Purchasing.Lib.Facades
{
    public class DeliveryOrderFacade
    {

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<DeliveryOrder> dbSet;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";

        public DeliveryOrderFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<DeliveryOrder>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<DeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<DeliveryOrder> Query = this.dbSet;

            Query = Query.Select(s => new DeliveryOrder
            {
                Id = s.Id,
                UId = s.UId,
                DONo = s.DONo,
                DODate = s.DODate,
                ArrivalDate = s.ArrivalDate,
                SupplierName = s.SupplierName,
                SupplierId=s.SupplierId,
                IsClosed = s.IsClosed,
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc,
                Items = s.Items.Select(i => new DeliveryOrderItem
                {
                    EPOId = i.EPOId,
                    EPONo = i.EPONo,
                    Details=i.Details.ToList()
                }).ToList()
            });

            List<string> searchAttributes = new List<string>()
            {
                "DONo", "SupplierName", "Items.EPONo"
            };

            //Query = QueryHelper<DeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dan dengan searchAttributes ada "." maka case sensitive, kalo tanpa "." tidak masalah
            Query = QueryHelper<DeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword, true); // bisa make ToLower()

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<DeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<DeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<DeliveryOrder> pageable = new Pageable<DeliveryOrder>(Query, Page - 1, Size);
            List<DeliveryOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public DeliveryOrder ReadById(int id)
        {
            var Result = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                    .ThenInclude(i => i.Details)
                .FirstOrDefault();
            return Result;
        }

        public async Task<int> Create(DeliveryOrder model, string username)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);

                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForCreate(item, username, USER_AGENT);

                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForCreate(detail, username, USER_AGENT);

                            ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.SingleOrDefault(m => m.Id == detail.EPODetailId);
                            externalPurchaseOrderDetail.DOQuantity += detail.DOQuantity;
                            EntityExtension.FlagForUpdate(externalPurchaseOrderDetail, username, USER_AGENT);
                            SetStatus(externalPurchaseOrderDetail, detail, username);
                        }
                    }

                    this.dbSet.Add(model);
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

        public async Task<int> Update(int id, DeliveryOrder model, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var existingModel = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    if (existingModel != null && id == model.Id)
                    {

                        EntityExtension.FlagForUpdate(model, user, USER_AGENT);

                        foreach (var item in model.Items)
                        {
                            if (item.Id == 0)
                            {
                                EntityExtension.FlagForCreate(item, user, USER_AGENT);

                                foreach (var detail in item.Details)
                                {
                                    EntityExtension.FlagForCreate(detail, user, USER_AGENT);

                                    ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.SingleOrDefault(m => m.Id == detail.EPODetailId);
                                    externalPurchaseOrderDetail.DOQuantity += detail.DOQuantity;
                                    EntityExtension.FlagForUpdate(externalPurchaseOrderDetail, user, USER_AGENT);
                                    SetStatus(externalPurchaseOrderDetail, detail, user);
                                }
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);

                                var existingItem = existingModel.Items.SingleOrDefault(m => m.Id == item.Id);
                                foreach (var detail in item.Details)
                                {
                                    if (detail.Id == 0)
                                    {
                                        EntityExtension.FlagForCreate(detail, user, USER_AGENT);

                                        ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.SingleOrDefault(m => m.Id == detail.EPODetailId);
                                        externalPurchaseOrderDetail.DOQuantity += detail.DOQuantity;
                                        EntityExtension.FlagForUpdate(externalPurchaseOrderDetail, user, USER_AGENT);
                                        SetStatus(externalPurchaseOrderDetail, detail, user);
                                    }
                                    else
                                    {
                                        EntityExtension.FlagForUpdate(detail, user, USER_AGENT);

                                        var existingDetail = existingItem.Details.SingleOrDefault(m => m.Id == detail.Id);

                                        ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.SingleOrDefault(m => m.Id == detail.EPODetailId);
                                        externalPurchaseOrderDetail.DOQuantity = externalPurchaseOrderDetail.DOQuantity - existingDetail.DOQuantity + detail.DOQuantity;
                                        EntityExtension.FlagForUpdate(externalPurchaseOrderDetail, user, USER_AGENT);
                                        SetStatus(externalPurchaseOrderDetail, detail, user);
                                    }
                                }
                            }
                        }

                        this.dbContext.Update(model);

                        foreach (var existingItem in existingModel.Items)
                        {
                            var newItem = model.Items.FirstOrDefault(i => i.Id == existingItem.Id);
                            if (newItem == null)
                            {
                                EntityExtension.FlagForDelete(existingItem, user, USER_AGENT);
                                this.dbContext.DeliveryOrderItems.Update(existingItem);
                                foreach (var existingDetail in existingItem.Details)
                                {
                                    EntityExtension.FlagForDelete(existingDetail, user, USER_AGENT);
                                    this.dbContext.DeliveryOrderDetails.Update(existingDetail);

                                    ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.SingleOrDefault(m => m.Id == existingDetail.EPODetailId);
                                    externalPurchaseOrderDetail.DOQuantity -= existingDetail.DOQuantity;
                                    EntityExtension.FlagForUpdate(externalPurchaseOrderDetail, user, USER_AGENT);
                                    SetStatus(externalPurchaseOrderDetail, existingDetail, user);
                                }
                            }
                            else
                            {
                                foreach (var existingDetail in existingItem.Details)
                                {
                                    var newDetail = newItem.Details.FirstOrDefault(d => d.Id == existingDetail.Id);
                                    if (newDetail == null)
                                    {
                                        EntityExtension.FlagForDelete(existingDetail, user, USER_AGENT);
                                        this.dbContext.DeliveryOrderDetails.Update(existingDetail);

                                        ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.SingleOrDefault(m => m.Id == existingDetail.EPODetailId);
                                        externalPurchaseOrderDetail.DOQuantity -= existingDetail.DOQuantity;
                                        EntityExtension.FlagForUpdate(externalPurchaseOrderDetail, user, USER_AGENT);
                                        SetStatus(externalPurchaseOrderDetail, existingDetail, user);
                                    }
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

        public int Delete(int id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);

                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForDelete(item, username, USER_AGENT);
                        foreach (var detail in item.Details)
                        {
                            ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.SingleOrDefault(m => m.Id == detail.EPODetailId);
                            externalPurchaseOrderDetail.DOQuantity -= detail.DOQuantity;
                            EntityExtension.FlagForUpdate(externalPurchaseOrderDetail, username, USER_AGENT);
                            SetStatus(externalPurchaseOrderDetail, detail, username);

                            EntityExtension.FlagForDelete(detail, username, USER_AGENT);
                        }
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

        private void SetStatus(ExternalPurchaseOrderDetail externalPurchaseOrderDetail, DeliveryOrderDetail detail, string username)
        {
            PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.SingleOrDefault(i => i.Id == detail.PRItemId);
            InternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.InternalPurchaseOrderItems.SingleOrDefault(i => i.Id == detail.POItemId);

            if (externalPurchaseOrderDetail.DOQuantity == 0)
            {
                purchaseRequestItem.Status = "Sudah diorder ke Supplier";
                internalPurchaseOrderItem.Status = "Sudah diorder ke Supplier";

                EntityExtension.FlagForUpdate(purchaseRequestItem, username, USER_AGENT);
                EntityExtension.FlagForUpdate(internalPurchaseOrderItem, username, USER_AGENT);
            }
            else if (externalPurchaseOrderDetail.DOQuantity > 0 && externalPurchaseOrderDetail.DOQuantity < externalPurchaseOrderDetail.DealQuantity)
            {
                purchaseRequestItem.Status = "Barang sudah datang parsial";
                internalPurchaseOrderItem.Status = "Barang sudah datang parsial";

                EntityExtension.FlagForUpdate(purchaseRequestItem, username, USER_AGENT);
                EntityExtension.FlagForUpdate(internalPurchaseOrderItem, username, USER_AGENT);
            }
            else if (externalPurchaseOrderDetail.DOQuantity > 0 && externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
            {
                purchaseRequestItem.Status = "Barang sudah datang semua";
                internalPurchaseOrderItem.Status = "Barang sudah datang semua";

                EntityExtension.FlagForUpdate(purchaseRequestItem, username, USER_AGENT);
                EntityExtension.FlagForUpdate(internalPurchaseOrderItem, username, USER_AGENT);
            }
        }

        public List<DeliveryOrder> ReadBySupplier(string Keyword = null, string unitId="", string supplierId="")
        {
            IQueryable<DeliveryOrder> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "DONo", "SupplierName", "Items.EPONo"
            };

            Query = QueryHelper<DeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah

            Query = Query
                .Where(m => m.IsClosed == false && m.IsDeleted == false && m.SupplierId==supplierId)
                .Select(s => new DeliveryOrder
                {
                    Id = s.Id,
                    UId = s.UId,
                    DONo = s.DONo,
                    DODate = s.DODate,
                    ArrivalDate = s.ArrivalDate,
                    SupplierName = s.SupplierName,
                    SupplierId = s.SupplierId,
                    IsClosed = s.IsClosed,
                    CreatedBy = s.CreatedBy,
                    LastModifiedUtc = s.LastModifiedUtc,
                    Items = s.Items.Select(i => new DeliveryOrderItem
                    {
                        EPOId = i.EPOId,
                        EPONo = i.EPONo,
                        DOId=i.DOId,
                        Details = i.Details
                                .Select(d => new DeliveryOrderDetail
                                {
                                    Id = d.Id,
                                    POItemId = d.POItemId,
                                    PRItemId = d.PRItemId,
                                    PRId=d.PRId,
                                    PRNo=d.PRNo,
                                    ProductId = d.ProductId,
                                    ProductCode = d.ProductCode,
                                    ProductName = d.ProductName,
                                    DealQuantity = d.DealQuantity,
                                    DOQuantity = d.DOQuantity,
                                    ProductRemark = d.ProductRemark,
                                    UnitId=d.UnitId,
                                    EPODetailId=d.EPODetailId,
                                    DOItemId=d.DOItemId,
                                    ReceiptQuantity=d.ReceiptQuantity,
                                    UomId=d.UomId,
                                    UomUnit=d.UomUnit
                                }).Where(d=> d.UnitId==unitId)
                                .ToList()
                        })
                        .Where(i => i.Details.Count > 0)
                        .ToList()
                })
                .Where(m => m.Items.Count > 0);

            //Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            //Query = QueryHelper<DeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            return Query.ToList();
        }
    }
}