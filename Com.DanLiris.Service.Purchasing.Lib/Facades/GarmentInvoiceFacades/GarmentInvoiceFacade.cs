using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades
{
    public class GarmentInvoiceFacade : IGarmentInvoice
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentInvoice> dbSet;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";
        public GarmentInvoiceFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentInvoice>();
            this.serviceProvider = serviceProvider;
        }
        public Tuple<List<GarmentInvoice>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentInvoice> Query = this.dbSet.Include(m => m.Items);

            List<string> searchAttributes = new List<string>()
            {
                "InvoiceNo", "InvoiceDate", "Suppliers.Name"
            };

            Query = QueryHelper<GarmentInvoice>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentInvoice>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentInvoice>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentInvoice> pageable = new Pageable<GarmentInvoice>(Query, Page - 1, Size);
            List<GarmentInvoice> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentInvoice ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                 .Include(m => m.Items)
                     .ThenInclude(i => i.Details)
                 .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentInvoice model, string username, int clientTimeZoneOffset = 7)
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
    }
}
