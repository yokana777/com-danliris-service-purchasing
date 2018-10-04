using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
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

        public GarmentPurchaseRequest ReadByRONo(string rono)
        {
            var a = this.dbSet.Where(p => p.RONo.Equals(rono))
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

        public async Task<int> Update(int id, GarmentPurchaseRequest m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    if (oldM != null && oldM.Id == id)
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);

                        foreach (var item in m.Items)
                        {
                            if (item.Id == 0)
                            {
                                EntityExtension.FlagForCreate(item, user, USER_AGENT);
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            }
                        }

                        dbSet.Update(m);

                        foreach (var oldItem in oldM.Items)
                        {
                            var newItem = oldM.Items.FirstOrDefault(i => i.Id.Equals(oldItem.Id));
                            if (newItem == null)
                            {
                                EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
                                dbContext.GarmentPurchaseRequestItems.Update(oldItem);
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

        public List<GarmentInternalPurchaseOrder> GetByTags(string tags, DateTimeOffset shipmentDateFrom, DateTimeOffset shipmentDateTo)
        {
            IQueryable<GarmentPurchaseRequest> Models = this.dbSet.AsQueryable();

            if (shipmentDateFrom != DateTimeOffset.MinValue && shipmentDateTo != DateTimeOffset.MinValue)
            {
                Models = Models.Where(m => m.ShipmentDate >= shipmentDateFrom && m.ShipmentDate <= shipmentDateTo);
            }

            string[] stringKeywords = new string[3];

            if (tags != null)
            {
                List<string> Keywords = new List<string>();

                if (tags.Contains("#"))
                {
                    Keywords = tags.Split("#").ToList();
                    Keywords.RemoveAt(0);
                    Keywords = Keywords.Take(stringKeywords.Length).ToList();
                }
                else
                {
                    Keywords.Add(tags);
                }

                for (int n = 0; n < Keywords.Count; n++)
                {
                    stringKeywords[n] = Keywords[n].Trim().ToLower();
                }
            }

            Models = Models
                .Where(m =>
                    (string.IsNullOrWhiteSpace(stringKeywords[0]) || m.UnitName.ToLower().Contains(stringKeywords[0])) &&
                    (string.IsNullOrWhiteSpace(stringKeywords[1]) || m.BuyerName.ToLower().Contains(stringKeywords[1])) &&
                    //m.Items.Any(i => i.IsUsed == false) &&
                    m.IsUsed == false
                    )
                .Select(m => new GarmentPurchaseRequest
                {
                    Date = m.Date,
                    PRNo = m.PRNo,
                    RONo = m.RONo,
                    BuyerId = m.BuyerId,
                    BuyerCode = m.BuyerCode,
                    BuyerName = m.BuyerName,
                    Article = m.Article,
                    ExpectedDeliveryDate = m.ExpectedDeliveryDate.GetValueOrDefault(),
                    ShipmentDate = m.ShipmentDate,
                    UnitId = m.UnitId,
                    UnitCode = m.UnitCode,
                    UnitName = m.UnitName,
                    Items = m.Items
                        .Where(i =>
                            //i.IsPosted == false &&
                            (string.IsNullOrWhiteSpace(stringKeywords[2]) || i.CategoryName.ToLower().Contains(stringKeywords[2]))
                            )
                        .ToList()
                })
                .Where(m => m.Items.Count > 0);

            var IPOModels = new List<GarmentInternalPurchaseOrder>();

            foreach (var model in Models)
            {
                foreach (var item in model.Items)
                {
                    var IPOModel = new GarmentInternalPurchaseOrder
                    {
                        PRDate = model.Date,
                        PRNo = model.PRNo,
                        RONo = model.RONo,
                        BuyerId = model.BuyerId,
                        BuyerCode = model.BuyerCode,
                        BuyerName = model.BuyerName,
                        Article = model.Article,
                        ExpectedDeliveryDate = model.ExpectedDeliveryDate.GetValueOrDefault(),
                        ShipmentDate = model.ShipmentDate,
                        UnitId = model.UnitId,
                        UnitCode = model.UnitCode,
                        UnitName = model.UnitName,
                        //IsPosted = false,
                        //IsClosed = false,
                        //Remark = "",
                        Items = new List<GarmentInternalPurchaseOrderItem>
                        {
                            new GarmentInternalPurchaseOrderItem
                            {
                                GPRItemId = item.Id,
                                PO_SerialNumber = item.PO_SerialNumber,
                                ProductId = item.ProductId,
                                ProductCode = item.ProductCode,
                                ProductName = item.ProductName,
                                Quantity = item.Quantity,
                                BudgetPrice = item.BudgetPrice,
                                UomId = item.UomId,
                                UomUnit = item.UomUnit,
                                CategoryId = item.CategoryId,
                                CategoryName = item.CategoryName,
                                ProductRemark = item.ProductRemark,
                                //Status = "PO Internal belum diorder"
                            }
                        }
                    };
                    IPOModels.Add(IPOModel);
                }
            }

            return IPOModels;
        }
    }
}
