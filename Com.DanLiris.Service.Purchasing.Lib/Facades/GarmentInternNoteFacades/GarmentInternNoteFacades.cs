using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternNoteFacades
{
    public class GarmentInternNoteFacades : IGarmentInternNoteFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentInternNote> dbSet;
        public readonly IServiceProvider serviceProvider;

        public GarmentInternNoteFacades(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentInternNote>();
            this.serviceProvider = serviceProvider;
        }

        public async Task<int> Create(GarmentInternNote m,bool isImport, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    m.INNo = await GenerateNo(m,isImport, clientTimeZoneOffset);

                    foreach (var item in m.Items)
                    {
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForCreate(detail, user, "Facade");
                        }
                        GarmentInvoice garmentInvoice = this.dbContext.GarmentInvoices.FirstOrDefault(s => s.Id == item.InvoiceId);
                        garmentInvoice.HasInternNote = true;

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

        public async Task<int> Delete(int id, string username)
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

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);
                    foreach (var item in model.Items)
                    {
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForDelete(model, username, USER_AGENT);
                        }
                        GarmentInvoice garmentInvoice = this.dbContext.GarmentInvoices.FirstOrDefault(s => s.Id == item.InvoiceId);
                        garmentInvoice.HasInternNote = false;

                        EntityExtension.FlagForDelete(model, username, USER_AGENT);
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

        public Tuple<List<GarmentInternNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentInternNote> Query = this.dbSet.Include(m => m.Items);

            List<string> searchAttributes = new List<string>()
            {
                "INNo", "INDate", "SupplierName", "Items.InvoiceNo", "CreatedBy"
            };

            Query = QueryHelper<GarmentInternNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentInternNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentInternNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentInternNote> pageable = new Pageable<GarmentInternNote>(Query, Page - 1, Size);
            List<GarmentInternNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentInternNote ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                    .ThenInclude(i => i.Details)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Update(int id, GarmentInternNote m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = this.dbSet.AsNoTracking()
                               .SingleOrDefault(gi => gi.Id == id && !gi.IsDeleted);

                    if (oldM != null && oldM.Id == id)
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                        foreach (var item in m.Items)
                        {
                            foreach (var detail in item.Details)
                            {
                                EntityExtension.FlagForCreate(detail, user, "Facade");
                            }
                            GarmentInvoice garmentInvoice = this.dbContext.GarmentInvoices.FirstOrDefault(s => s.Id == item.InvoiceId);
                            garmentInvoice.HasInternNote = true;

                            EntityExtension.FlagForCreate(item, user, USER_AGENT);
                        }

                        dbSet.Update(m);

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
        async Task<string> GenerateNo(GarmentInternNote model,bool isImport, int clientTimeZoneOffset)
        {
            var viewmodel = new GarmentInternNoteViewModel();
            DateTimeOffset dateTimeOffsetNow = DateTimeOffset.Now;
            string Month = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string Year = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("YY");
            string Supplier = isImport ? "I" : "L";

            string no = $"NI{Year}{Month}{Supplier}";
            int Padding = 4;
            
            var lastNo = await this.dbSet.Where(w => w.INNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.INNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.INNo.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
                
            }
        }
    }
}
