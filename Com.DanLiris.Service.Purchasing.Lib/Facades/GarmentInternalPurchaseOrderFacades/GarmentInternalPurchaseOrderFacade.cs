using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades
{
    public class GarmentInternalPurchaseOrderFacade : IGarmentInternalPurchaseOrderFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentInternalPurchaseOrder> dbSet;

        public GarmentInternalPurchaseOrderFacade(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentInternalPurchaseOrder>();
        }

        public Tuple<List<GarmentInternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentInternalPurchaseOrder> Query = this.dbSet.Include(m => m.Items);

            List<string> searchAttributes = new List<string>()
            {
                "PRNo", "RONo", "BuyerName", "Items.ProductName", "Items.UomUnit", "CreatedBy"
            };

            Query = QueryHelper<GarmentInternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentInternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentInternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);
            //Query = Query
            //    .Select(m => new
            //    {
            //        Data = m,
            //        ProductName = m.Items.Select(i => i.ProductName).FirstOrDefault()
            //    })
            //    .OrderByDescending(m => m.ProductName)
            //    .AsEnumerable()
            //    .Select(m => m.Data)
            //    .AsQueryable();

            Pageable<GarmentInternalPurchaseOrder> pageable = new Pageable<GarmentInternalPurchaseOrder>(Query, Page - 1, Size);
            List<GarmentInternalPurchaseOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentInternalPurchaseOrder ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public bool CheckDuplicate(GarmentInternalPurchaseOrder model)
        {
            var countPOIntByPRAndRefNo = dbSet.Count(m => m.PRNo == model.PRNo && m.Items.Any(i => i.PO_SerialNumber == model.Items.Single().PO_SerialNumber));
            return countPOIntByPRAndRefNo > 1;
        }

        public async Task<int> CreateMultiple(List<GarmentInternalPurchaseOrder> ListModel, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var model in ListModel)
                    {
                        EntityExtension.FlagForCreate(model, user, USER_AGENT);

                        do
                        {
                            model.PONo = CodeGenerator.Generate();
                        }
                        while (ListModel.Count(m => m.PONo == model.PONo) > 1 || dbSet.Any(m => m.PONo.Equals(model.PONo)));
                        model.IsPosted = false;
                        model.IsClosed = false;

                        foreach (var item in model.Items)
                        {
                            EntityExtension.FlagForCreate(item, user, USER_AGENT);

                            item.Status = "PO Internal belum diorder";
                            item.RemainingBudget = item.BudgetPrice * item.Quantity;

                            var garmentPurchaseRequestItem = dbContext.GarmentPurchaseRequestItems.Single(i => i.Id == item.GPRItemId);
                            garmentPurchaseRequestItem.IsUsed = true;
                            garmentPurchaseRequestItem.Status = "Sudah diterima Pembelian";
                            EntityExtension.FlagForUpdate(garmentPurchaseRequestItem, user, USER_AGENT);

                            var garmentPurchaseRequest = dbContext.GarmentPurchaseRequests.Include(m => m.Items).Single(i => i.Id == model.PRId);
                            garmentPurchaseRequest.IsUsed = garmentPurchaseRequest.Items.All(i => i.IsUsed == true);
                            EntityExtension.FlagForUpdate(garmentPurchaseRequest, user, USER_AGENT);
                        }

                        dbSet.Add(model);
                    }

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

        public async Task<int> Split(int id, GarmentInternalPurchaseOrder model, string user, int clientTimeZoneOffset = 7)
        {
            int Splited = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldModel = dbSet.SingleOrDefault(m => m.Id == id);

                    EntityExtension.FlagForUpdate(oldModel, user, USER_AGENT);
                    foreach (var oldItem in oldModel.Items)
                    {
                        EntityExtension.FlagForUpdate(oldItem, user, USER_AGENT);
                        var newQuantity = model.Items.Single(i => i.Id == oldItem.Id).Quantity;
                        oldItem.Quantity -= newQuantity;
                    }

                    model.Id = 0;
                    foreach (var item in model.Items)
                    {
                        item.Id = 0;
                    }

                    EntityExtension.FlagForCreate(model, user, USER_AGENT);
                    do
                    {
                        model.PONo = CodeGenerator.Generate();
                    }
                    while (dbSet.Any(m => m.PONo.Equals(model.PONo)));

                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                    }

                    dbSet.Add(model);

                    Splited = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Splited;
        }

        public async Task<int> Delete(int id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);
                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForDelete(item, username, USER_AGENT);

                        if (!CheckDuplicate(model))
                        {
                            var garmentPurchaseRequestItem = dbContext.GarmentPurchaseRequestItems.Single(i => i.Id == item.GPRItemId);
                            garmentPurchaseRequestItem.IsUsed = false;
                            garmentPurchaseRequestItem.Status = "Belum diterima Pembelian";
                            EntityExtension.FlagForUpdate(garmentPurchaseRequestItem, username, USER_AGENT);

                            var garmentPurchaseRequest = dbContext.GarmentPurchaseRequests.Single(i => i.Id == model.PRId);
                            garmentPurchaseRequest.IsUsed = false;
                            EntityExtension.FlagForUpdate(garmentPurchaseRequest, username, USER_AGENT);
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
    }
}
