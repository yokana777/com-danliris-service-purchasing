using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
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

                        model.IsPosted = false;
                        model.IsClosed = false;

                        foreach (var item in model.Items)
                        {
                            EntityExtension.FlagForCreate(item, user, USER_AGENT);

                            item.Status = "PO Internal belum diorder";
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
    }
}
