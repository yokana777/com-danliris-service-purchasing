using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.PurchasingDispositionFacades
{
    public class PurchasingDispositionFacade : IPurchasingDispositionFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<PurchasingDisposition> dbSet;

        public PurchasingDispositionFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<PurchasingDisposition>();
        }

        public Tuple<List<PurchasingDisposition>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<PurchasingDisposition> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "Bank"
            };

            Query = QueryHelper<PurchasingDisposition>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query
                .Select(s => new PurchasingDisposition
                {
                    Id = s.Id,
                    SupplierCode=s.SupplierCode,
                    SupplierId=s.SupplierId,
                    SupplierName=s.SupplierName,
                    Bank=s.Bank,
                    ConfirmationOrderNo=s.ConfirmationOrderNo,
                    InvoiceNo=s.InvoiceNo,
                    PaymentMethod=s.PaymentMethod,
                    
                });



            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<PurchasingDisposition>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<PurchasingDisposition>.ConfigureOrder(Query, OrderDictionary);

            Pageable<PurchasingDisposition> pageable = new Pageable<PurchasingDisposition>(Query, Page - 1, Size);
            List<PurchasingDisposition> Data = pageable.Data.ToList<PurchasingDisposition>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public PurchasingDisposition ReadModelById(int id)
        {
            var a = this.dbSet.Where(d => d.Id.Equals(id) && d.IsDeleted.Equals(false))
                .Include(p => p.Items)
                .ThenInclude(p => p.Details)
                .FirstOrDefault();
            return a;
        }

        public async Task<int> Create(PurchasingDisposition m, string user, int clientTimeZoneOffset)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, "Facade");

                    //m.EPONo = await GenerateNo(m, clientTimeZoneOffset);

                    foreach (var item in m.Items)
                    {

                        EntityExtension.FlagForCreate(item, user, "Facade");
                        foreach (var detail in item.Details)
                        {
                            
                            EntityExtension.FlagForCreate(detail, user, "Facade");

                            
                        }
                        
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

        //async Task<string> GenerateNo(PurchasingDisposition model, int clientTimeZoneOffset)
        //{
        //    DateTimeOffset Now = DateTime.UtcNow;
        //    string Year = Now.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy"); ;
        //    string Month = Now.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM"); ;

        //    string no = $"{Year}-{Month}-";
        //    int Padding = 3;

        //    var lastNo = await this.dbSet.Where(w => w.EPONo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.EPONo).FirstOrDefaultAsync();
        //    no = $"{no}";

        //    if (lastNo == null)
        //    {
        //        return no + "1".PadLeft(Padding, '0');
        //    }
        //    else
        //    {
        //        int lastNoNumber = Int32.Parse(lastNo.EPONo.Replace(no, "")) + 1;
        //        return no + lastNoNumber.ToString().PadLeft(Padding, '0');
        //    }
        //}
    }
}
