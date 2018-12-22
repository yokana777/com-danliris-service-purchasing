using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderFacades
{
    public class GarmentUnitDeliveryOrderFacade : IGarmentUnitDeliveryOrder
    {
        //private string USER_AGENT = "Facade";
        //private readonly PurchasingDbContext dbContext;
        //public readonly IServiceProvider serviceProvider;
        //private readonly DbSet<GarmentUnitDeliveryOrder> dbSet;

        //public GarmentUnitDeliveryOrderFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        //{
        //    this.dbContext = dbContext;
        //    this.serviceProvider = serviceProvider;
        //    dbSet = dbContext.Set<GarmentUnitDeliveryOrder>();
        //}

        //public Tuple<List<GarmentUnitDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        //{
        //    IQueryable<GarmentUnitDeliveryOrder> Query = dbSet;

        //    Query = Query.Select(m => new GarmentUnitDeliveryOrder
        //    {
        //        Id = m.Id,
        //        UnitDONo = m.UnitDONo,
        //        UnitDODate = m.UnitDODate,
        //        UnitDOType = m.UnitDOType,
        //        UnitRequestName = m.UnitRequestName,
        //        UnitSenderName = m.UnitSenderName,
        //        StorageName = m.StorageName,
        //        RONo = m.RONo,
        //        Article = m.Article,
        //        CreatedBy = m.CreatedBy,
        //        LastModifiedUtc = m.LastModifiedUtc
        //    });

        //    List<string> searchAttributes = new List<string>()
        //    {
        //        "UnitDONo", "RONo", "UnitDOType", "Article","UnitDODate","UnitRequestName","StorageName"
        //    };

        //    Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

        //    Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
        //    Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

        //    Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
        //    Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

        //    Pageable<GarmentUnitDeliveryOrder> pageable = new Pageable<GarmentUnitDeliveryOrder>(Query, Page - 1, Size);
        //    List<GarmentUnitDeliveryOrder> Data = pageable.Data.ToList();
        //    int TotalData = pageable.TotalCount;

        //    return Tuple.Create(Data, TotalData, OrderDictionary);
        //}

        //public GarmentUnitDeliveryOrder ReadById(int id)
        //{
        //    var model = dbSet.Where(m => m.Id == id)
        //        .Include(m => m.Items)
        //        .FirstOrDefault();
        //    return model;
        //}
        //public async Task<int> Create(GarmentUnitDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        //{
        //    int Created = 0;

        //    using (var transaction = this.dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            EntityExtension.FlagForCreate(m, user, USER_AGENT);

        //            //m.UnitDONo = await GenerateNo(m, isImport, clientTimeZoneOffset);
        //            //m.UnitDODate = DateTimeOffset.Now;

        //            foreach (var item in m.Items)
        //            {
        //                GarmentUnitReceiptNoteItem garmentUnitReceiptNote = this.dbContext.GarmentUnitReceiptNoteItems.FirstOrDefault(s => s.Id == item.URNItemId);
        //                if (garmentUnitReceiptNote != null)
        //                    garmentUnitReceiptNote.OrderQuantity = garmentUnitReceiptNote.OrderQuantity + (decimal)item.Quantity;
        //                EntityExtension.FlagForCreate(item, user, USER_AGENT);
        //            }

        //            this.dbSet.Add(m);

        //            Created = await dbContext.SaveChangesAsync();
        //            transaction.Commit();
        //        }
        //        catch (Exception e)
        //        {
        //            transaction.Rollback();
        //            throw new Exception(e.Message);
        //        }
        //    }

        //    return Created;
        //}

        //public int Delete(int id, string username)
        //{
        //    int Deleted = 0;

        //    using (var transaction = this.dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var model = this.dbSet
        //                        .Include(m => m.Items)
        //                        .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

        //            EntityExtension.FlagForDelete(model, username, USER_AGENT);
        //            foreach (var item in model.Items)
        //            {
        //                GarmentUnitReceiptNoteItem garmentUnitReceiptNote = this.dbContext.GarmentUnitReceiptNoteItems.FirstOrDefault(s => s.Id == item.URNItemId);
        //                if (garmentUnitReceiptNote != null)
        //                    garmentUnitReceiptNote.OrderQuantity = garmentUnitReceiptNote.OrderQuantity - (decimal)item.Quantity;
        //                EntityExtension.FlagForDelete(item, username, USER_AGENT);
        //            }

        //            Deleted = dbContext.SaveChanges();
        //            transaction.Commit();
        //        }
        //        catch (Exception e)
        //        {
        //            transaction.Rollback();
        //            throw new Exception(e.Message);
        //        }
        //    }

        //    return Deleted;
        //}

        //public Task<int> Update(int id, GarmentUnitDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
