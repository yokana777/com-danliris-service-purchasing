using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
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
    public class GarmentCorrectionNoteQuantityFacade : IGarmentCorrectionNoteQuantityFacade
    {
        private string USER_AGENT = "Facade";
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentCorrectionNote> dbSet;

        public GarmentCorrectionNoteQuantityFacade(IServiceProvider serviceProvider ,PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.serviceProvider = serviceProvider;
            dbSet = dbContext.Set<GarmentCorrectionNote>();
        }

        public Tuple<List<GarmentCorrectionNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentCorrectionNote> Query = dbSet;

            Query = Query.Where(m => m.CorrectionType.StartsWith("Jumlah"));

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

        public GarmentCorrectionNote ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentCorrectionNote garmentCorrectionNote, bool isImport, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(garmentCorrectionNote, user, USER_AGENT);
                    var supplier = GetSupplier(garmentCorrectionNote.SupplierId);
                    var supplierImport = false;
                    if (supplier != null)
                    {
                        supplierImport = supplier.Import;
                    }
                    garmentCorrectionNote.CorrectionNo = await GenerateNo(garmentCorrectionNote, supplierImport, clientTimeZoneOffset);
                    garmentCorrectionNote.TotalCorrection = garmentCorrectionNote.Items.Sum(i => i.PriceTotalAfter - i.PriceTotalBefore);

                    var garmentDeliveryOrder = dbContext.GarmentDeliveryOrders.First(d => d.Id == garmentCorrectionNote.DOId);
                    if (garmentDeliveryOrder.UseIncomeTax == true)
                    {
                        garmentCorrectionNote.NKPH = await GenerateNKPH(garmentCorrectionNote, clientTimeZoneOffset);
                    }
                    else
                    {
                        garmentCorrectionNote.NKPH = "";
                    }
                    if (garmentDeliveryOrder.UseVat == true)
                    {
                        garmentCorrectionNote.NKPN = await GenerateNKPN(garmentCorrectionNote, clientTimeZoneOffset);
                    }
                    else
                    {
                        garmentCorrectionNote.NKPN = "";
                    }
                    //garmentDeliveryOrder.IsCorrection = true;
                    EntityExtension.FlagForUpdate(garmentDeliveryOrder, user, USER_AGENT);

                    foreach (var item in garmentCorrectionNote.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                        var garmentDeliveryOrderDetail = dbContext.GarmentDeliveryOrderDetails.First(d => d.Id == item.DODetailId);
                        
                        garmentDeliveryOrderDetail.QuantityCorrection = (double)item.Quantity;
                        garmentDeliveryOrderDetail.PriceTotalCorrection = (double)item.PriceTotalAfter;
                        
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
        async Task<string> GenerateNo(GarmentCorrectionNote model, bool isImport, int clientTimeZoneOffset)
        {
            DateTimeOffset dateTimeOffsetNow = DateTimeOffset.Now;
            string Month = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string Year = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Supplier = isImport ? "I" : "L";

            string no = $"NK{Year}{Month}";
            int Padding = 4;

            var lastNo = await this.dbSet.Where(w => w.CorrectionNo.StartsWith(no) && w.CorrectionNo.EndsWith(Supplier) && !w.IsDeleted).OrderByDescending(o => o.CorrectionNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0') + Supplier;
            }
            else
            {
                int.TryParse(lastNo.CorrectionNo.Replace(no, "").Replace(Supplier, ""), out int lastno1);
                int lastNoNumber = lastno1 + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0') + Supplier;
            }
        }
        async Task<string> GenerateNKPN(GarmentCorrectionNote model, int clientTimeZoneOffset)
        {
            DateTimeOffset dateTimeOffsetNow = DateTimeOffset.Now;
            string Month = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string Year = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");

            string no = $"NKPN{Year}{Month}";
            int Padding = 4;

            var lastNo = await this.dbSet.Where(w => w.CorrectionNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.CorrectionNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int.TryParse(lastNo.CorrectionNo.Replace(no, ""), out int lastno1);
                int lastNoNumber = lastno1 + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }
        async Task<string> GenerateNKPH(GarmentCorrectionNote model, int clientTimeZoneOffset)
        {
            DateTimeOffset dateTimeOffsetNow = DateTimeOffset.Now;
            string Month = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string Year = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");

            string no = $"NKPH{Year}{Month}";
            int Padding = 4;

            var lastNo = await this.dbSet.Where(w => w.CorrectionNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.CorrectionNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int.TryParse(lastNo.CorrectionNo.Replace(no, ""), out int lastno1);
                int lastNoNumber = lastno1 + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public SupplierViewModel GetSupplier(long supplierId)
        {
            string supplierUri = "master/garment-suppliers";
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{supplierUri}/{supplierId}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                List<SupplierViewModel> viewModel = JsonConvert.DeserializeObject<List<SupplierViewModel>>(result.GetValueOrDefault("data").ToString());
                return viewModel.First();
            }
            else
            {
                SupplierViewModel viewModel = null;
                return viewModel;
            }

        }

        public GarmentCorrectionNote ReadByDOId(int id)
        {
            var model = dbSet.Where(m => m.DOId == id)
                 .Include(m => m.Items)
                 .FirstOrDefault();
            return model;
        }
    }
}
