using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentCorrectionNoteFacades
{
    public class GarmentCorrectionNoteFacade : IGarmentCorrectionNoteFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentCorrectionNote> dbSet;

        public GarmentCorrectionNoteFacade(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentCorrectionNote>();
        }

        public Tuple<List<GarmentCorrectionNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentCorrectionNote> Query = dbSet;

            Query = Query.Select(m => new GarmentCorrectionNote
            {
                Id = m.Id,
                CorrectionNo = m.CorrectionNo,
                CorrectionType = m.CorrectionType,
                CorrectionDate = m.CorrectionDate,
                SupplierName = m.SupplierName,
                DONo = m.DONo,
                UseIncomeTax = m.UseIncomeTax,
                UseVat = m.UseVat,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "CorrectionNo", "CorrectionType", "SupplierName", "DONo"
            };

            Query = QueryHelper<GarmentCorrectionNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentCorrectionNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentCorrectionNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentCorrectionNote> pageable = new Pageable<GarmentCorrectionNote>(Query, Page - 1, Size);
            List<GarmentCorrectionNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentCorrectionNote ReadById(long id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentCorrectionNote garmentCorrectionNote, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(garmentCorrectionNote, user, USER_AGENT);
                    do
                    {
                        garmentCorrectionNote.CorrectionNo = CodeGenerator.Generate();
                    }
                    while (dbSet.Any(m => m.CorrectionNo == garmentCorrectionNote.CorrectionNo));

                    //garmentCorrectionNote.TotalCorrection = garmentCorrectionNote.Items.Sum(i => i.PriceTotalAfter - i.PriceTotalBefore);

                    var garmentDeliveryOrder = dbContext.GarmentDeliveryOrders.First(d => d.Id == garmentCorrectionNote.DOId);
                    garmentDeliveryOrder.IsCorrection = true;
                    EntityExtension.FlagForUpdate(garmentDeliveryOrder, user, USER_AGENT);

                    foreach (var item in garmentCorrectionNote.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                        var garmentDeliveryOrderDetail = dbContext.GarmentDeliveryOrderDetails.First(d => d.Id == item.DODetailId);
                        if ((garmentCorrectionNote.CorrectionType ?? "").ToUpper() == "HARGA SATUAN")
                        {
                            garmentDeliveryOrderDetail.PricePerDealUnitCorrection = (double)item.PricePerDealUnitAfter;
                            garmentDeliveryOrderDetail.PriceTotalCorrection = (double)item.PriceTotalAfter;
                        }
                        else if ((garmentCorrectionNote.CorrectionType ?? "").ToUpper() == "HARGA TOTAL")
                        {
                            garmentDeliveryOrderDetail.PriceTotalCorrection = (double)item.PriceTotalAfter;
                        }
                        EntityExtension.FlagForUpdate(garmentDeliveryOrderDetail, user, USER_AGENT);
                    }

                    dbSet.Add(garmentCorrectionNote);

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
