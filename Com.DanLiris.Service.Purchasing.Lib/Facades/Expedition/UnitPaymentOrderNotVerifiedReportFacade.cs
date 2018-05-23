using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class UnitPaymentOrderNotVerifiedReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<PurchasingDocumentExpedition> dbSet;
        public UnitPaymentOrderNotVerifiedReportFacade(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = this.dbContext.Set<PurchasingDocumentExpedition>();
        }

        public Tuple<List<UnitPaymentOrderNotVerifiedReportViewModel>, int> GetReport(string no, string supplier, string division, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            IQueryable<PurchasingDocumentExpedition> Query = this.dbSet;

            DateTime dateFromFilter = (dateFrom == null ? new DateTime(1970, 1, 1) : dateFrom.Value.Date);
            DateTime dateToFilter = (dateTo == null ? DateTime.UtcNow.AddHours(offset).Date : dateTo.Value.Date);

            Query = Query
                .Where(p => p.IsDeleted == false &&
                    p.UnitPaymentOrderNo == (string.IsNullOrWhiteSpace(no) ? p.UnitPaymentOrderNo : no) &&
                    p.SupplierCode == (string.IsNullOrWhiteSpace(supplier) ? p.SupplierCode : supplier) &&
                    p.DivisionCode == (string.IsNullOrWhiteSpace(division) ? p.DivisionCode : division) &&
                    p.VerifyDate >= dateFromFilter &&
                    p.VerifyDate <= dateToFilter 
                    && p.Position == (ExpeditionPosition)6
                );

            Query = Query
               .Select(s => new PurchasingDocumentExpedition
               {
                   Id = s.Id,
                   UnitPaymentOrderNo = s.UnitPaymentOrderNo,
                   DivisionName = s.DivisionName,
                   SupplierName = s.SupplierName,
                   VerifyDate = s.VerifyDate,
                   Currency=s.Currency,
                   UPODate=s.UPODate,
                   TotalPaid=s.TotalPaid
               });

            #region Order

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            }
            //else
            //{
            //    string Key = OrderDictionary.Keys.First();
            //    string OrderType = OrderDictionary[Key];

            //    Query = Query.OrderBy(string.Concat(Key, " ", OrderType));
            //}

            #endregion Order

            #region Paging

            Pageable<PurchasingDocumentExpedition> pageable = new Pageable<PurchasingDocumentExpedition>(Query, page - 1, size);
            List<PurchasingDocumentExpedition> Data = pageable.Data.ToList<PurchasingDocumentExpedition>();
            int TotalData = pageable.TotalCount;

            #endregion Paging

            List<UnitPaymentOrderNotVerifiedReportViewModel> list = Data
                    .Select(s => new UnitPaymentOrderNotVerifiedReportViewModel
                    {
                        UnitPaymentOrderNo = s.UnitPaymentOrderNo,
                        DivisionName = s.DivisionName,
                        SupplierName = s.SupplierName,
                        VerifyDate = s.VerifyDate,
                        Currency = s.Currency,
                        UPODate = s.UPODate,
                        TotalPaid = s.TotalPaid
                    }).ToList();

            return Tuple.Create(list, TotalData);
        }
    }
}
