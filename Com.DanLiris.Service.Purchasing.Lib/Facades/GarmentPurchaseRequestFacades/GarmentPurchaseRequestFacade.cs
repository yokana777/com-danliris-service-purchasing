using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades
{
    public class GarmentPurchaseRequestFacade : IGarmentPurchaseRequestFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentPurchaseRequest> dbSet;

        public GarmentPurchaseRequestFacade(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentPurchaseRequest>();
        }

        public Tuple<List<GarmentPurchaseRequest>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentPurchaseRequest> Query = this.dbSet;

            Query = Query.Select(s => new GarmentPurchaseRequest
            {
                Id = s.Id,
                UId = s.UId,
                RONo = s.RONo,
                PRNo = s.PRNo,
                Article = s.Article,
                Date = s.Date,
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                ShipmentDate = s.ShipmentDate,
                BuyerId = s.BuyerId,
                BuyerCode = s.BuyerCode,
                BuyerName = s.BuyerName,
                UnitId = s.UnitId,
                UnitCode = s.UnitCode,
                UnitName = s.UnitName,
                IsPosted = s.IsPosted,
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "PRNo", "RONo", "BuyerName", "UnitName"
            };

            Query = QueryHelper<GarmentPurchaseRequest>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentPurchaseRequest>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentPurchaseRequest>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentPurchaseRequest> pageable = new Pageable<GarmentPurchaseRequest>(Query, Page - 1, Size);
            List<GarmentPurchaseRequest> Data = pageable.Data.ToList<GarmentPurchaseRequest>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentPurchaseRequest ReadById(int id)
        {
            var a = this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
            return a;
        }

        public async Task<int> Create(GarmentPurchaseRequest m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using(var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    m.PRNo = $"PR{m.RONo}";
                    m.IsPosted = true;
                    m.IsUsed = false;

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                        item.Status = "Belum diterima Pembelian";
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
    }
}
