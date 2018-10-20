using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
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

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades
{
    public class GarmentExternalPurchaseOrderFacade : IGarmentExternalPurchaseOrderFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentExternalPurchaseOrder> dbSet;
        public readonly IServiceProvider serviceProvider;

        public GarmentExternalPurchaseOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentExternalPurchaseOrder>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<GarmentExternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet;

            Query = Query.Select(s => new GarmentExternalPurchaseOrder
            {
                Id = s.Id,
                UId = s.UId,
                IsPosted=s.IsPosted,
                SupplierName=s.SupplierName,
                SupplierCode=s.SupplierCode,
                OrderDate=s.OrderDate,
                EPONo=s.EPONo,
                SupplierImport=s.SupplierImport,
                Items=s.Items.Select(a=> new GarmentExternalPurchaseOrderItem
                {
                    PRNo=a.PRNo,
                    PRId=a.PRId
                }).ToList(),
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "EPONo", "Items.PRNo", "SupplierName", "UnitName"
            };

            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentExternalPurchaseOrder> pageable = new Pageable<GarmentExternalPurchaseOrder>(Query, Page - 1, Size);
            List<GarmentExternalPurchaseOrder> Data = pageable.Data.ToList<GarmentExternalPurchaseOrder>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentExternalPurchaseOrder ReadById(int id)
        {
            var a = this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
            return a;
        }

        public async Task<int> Create(GarmentExternalPurchaseOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in m.Items)
                    {
                        if (item.IsOverBudget)
                        {
                            m.IsOverBudget = true;
                            break;
                        }
                    }
                    m.EPONo = await GenerateNo(m, clientTimeZoneOffset);

                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    foreach (var item in m.Items)
                    {

                        GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(item.POId));
                        internalPurchaseOrder.IsPosted = true;

                        GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                        if (item.ProductId.ToString() == IPOItem.ProductId)
                        {

                            IPOItem.Status = "Sudah dibuat PO";
                        }

                        var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();

                        foreach (var a in ipoItems)
                        {
                            a.RemainingBudget -= item.UsedBudget;
                        }


                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
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

        public async Task<int> Update(int id, GarmentExternalPurchaseOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    m.IsOverBudget = false;
                    foreach (var item in m.Items)
                    {
                        if (item.IsOverBudget)
                        {
                            m.IsOverBudget = true;
                            break;
                        }
                    }

                    if (oldM != null && oldM.Id == id)
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                        foreach (var Olditem in oldM.Items)
                        {
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(Olditem.POId));
                            //internalPurchaseOrder.IsPosted = true;

                            GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(Olditem.POId));

                            var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(Olditem.ProductId.ToString())).ToList();
                            //returning Values
                            foreach (var a in ipoItems)
                            {
                                a.RemainingBudget += Olditem.UsedBudget;
                            }
                        }

                        foreach (var item in m.Items)
                        {
                            if (item.Id == 0)
                            {
                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(item.POId));
                                internalPurchaseOrder.IsPosted = true;

                                GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));
                                
                                if (item.ProductId.ToString() == IPOItem.ProductId)
                                {
                                    
                                    IPOItem.Status = "Sudah dibuat PO";
                                }

                                var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();

                                foreach(var a in ipoItems)
                                {
                                    a.RemainingBudget -= item.UsedBudget;
                                }


                                EntityExtension.FlagForCreate(item, user, USER_AGENT);
                            }
                            else
                            {
                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(item.POId));
                                internalPurchaseOrder.IsPosted = true;

                                GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                                var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();

                                foreach (var a in ipoItems)
                                {
                                    a.RemainingBudget -= item.UsedBudget;
                                }
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            }
                        }

                        dbSet.Update(m);

                        foreach (var oldItem in oldM.Items)
                        {
                            var newItem = m.Items.FirstOrDefault(i => i.Id.Equals(oldItem.Id));
                            if (newItem == null)
                            {
                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(oldItem.POId));
                                internalPurchaseOrder.IsPosted = false;

                                GarmentInternalPurchaseOrderItem IPOItems = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(oldItem.POId));

                                if (oldItem.ProductId.ToString() == IPOItems.ProductId)
                                {
                                    IPOItems.Status = "PO Internal belum diorder";
                                }
                                
                                EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
                                dbContext.GarmentExternalPurchaseOrderItems.Update(oldItem);
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

        async Task<string> GenerateNo(GarmentExternalPurchaseOrder model, int clientTimeZoneOffset)
        {
            DateTimeOffset Now = model.OrderDate;
            string Year = Now.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy"); ;
            string Month = Now.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM"); ;

            string no = $"PO-{Year}-{Month}-";
            int Padding = 5;

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

        public int Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForDelete(m, user, "Facade");

                    foreach (var item in m.Items)
                    {

                        GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id == item.POId);
                        internalPurchaseOrder.IsPosted = false;
                        
                        GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                        var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();
                        //returning Values
                        foreach (var a in ipoItems)
                        {
                            a.RemainingBudget += item.UsedBudget;
                        }

                        EntityExtension.FlagForDelete(item, user, "Facade");
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

        public int EPOPost(List<GarmentExternalPurchaseOrder> ListEPO, string user)
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
                        .ToList();
                    listData.ForEach(m =>
                    {
                        EntityExtension.FlagForUpdate(m, user, "Facade");
                        m.IsPosted = true;

                        foreach (var item in m.Items)
                        {
                            GarmentInternalPurchaseOrderItem IPOItems = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                            if (item.ProductId.ToString() == IPOItems.ProductId)
                            {
                                IPOItems.RemainingBudget += item.UsedBudget;
                                IPOItems.Status = "Sudah diorder ke Supplier";
                            }

                            GarmentPurchaseRequestItem PRItems = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(a => a.Id.Equals(IPOItems.GPRItemId));
                            PRItems.Status = "Sudah diorder ke Supplier";

                            EntityExtension.FlagForUpdate(item, user, "Facade");
                        }
                        //foreach (var item in m.Items)
                        //{
                        //    EntityExtension.FlagForUpdate(item, user, "Facade");

                        //}
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

        public int EPOUnpost(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForUpdate(m, user, "Facade");
                    m.IsPosted = false;

                    foreach (var item in m.Items)
                    {
                        GarmentInternalPurchaseOrderItem IPOItems = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                        if (item.ProductId.ToString() == IPOItems.ProductId)
                        {
                            IPOItems.RemainingBudget += item.UsedBudget;
                            IPOItems.Status = "Sudah dibuat PO Eksternal";
                        }

                        GarmentPurchaseRequestItem PRItems = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(a => a.Id.Equals(IPOItems.GPRItemId));
                        PRItems.Status = "Sudah diterima Pembelian";

                        EntityExtension.FlagForUpdate(item, user, "Facade");

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

        //public int EPOCancel(int id, string user)
        //{
        //    int Updated = 0;

        //    using (var transaction = this.dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var m = this.dbSet
        //                .Include(d => d.Items)
        //                .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

        //            EntityExtension.FlagForUpdate(m, user, "Facade");
        //            m.IsCanceled = true;

        //            foreach (var item in m.Items)
        //            {
        //                EntityExtension.FlagForUpdate(item, user, "Facade");

        //            }

        //            Updated = dbContext.SaveChanges();
        //            transaction.Commit();
        //        }
        //        catch (Exception e)
        //        {
        //            transaction.Rollback();
        //            throw new Exception(e.Message);
        //        }
        //    }

        //    return Updated;
        //}

        //public int EPOClose(int id, string user)
        //{
        //    int Updated = 0;

        //    using (var transaction = this.dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var m = this.dbSet
        //                .Include(d => d.Items)
        //                .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

        //            EntityExtension.FlagForUpdate(m, user, "Facade");
        //            m.IsClosed = true;

        //            foreach (var item in m.Items)
        //            {
        //                EntityExtension.FlagForUpdate(item, user, "Facade");
        //            }

        //            Updated = dbContext.SaveChanges();
        //            transaction.Commit();
        //        }
        //        catch (Exception e)
        //        {
        //            transaction.Rollback();
        //            throw new Exception(e.Message);
        //        }
        //    }

        //    return Updated;
        //}



        public SupplierViewModel GetSupplier(long supplierId)
        {
            string supplierUri = "master/garment-suppliers";
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{supplierUri}/{supplierId}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                SupplierViewModel viewModel = JsonConvert.DeserializeObject<SupplierViewModel>(result.GetValueOrDefault("data").ToString());
                return viewModel;
            }
            else
            {
                SupplierViewModel viewModel = null;
                return viewModel;
            }

        }

        public GarmentProductViewModel GetProduct(long productId)
        {
            string productUri = "master/garmentProducts";
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{productUri}/{productId}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                GarmentProductViewModel viewModel = JsonConvert.DeserializeObject<GarmentProductViewModel>(result.GetValueOrDefault("data").ToString());
                return viewModel;
            }
            else
            {
                GarmentProductViewModel viewModel = null;
                return viewModel;
            }

        }
    }
}
