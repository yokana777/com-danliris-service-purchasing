using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
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
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentInternNote> dbSet;
        private readonly DbSet<GarmentExternalPurchaseOrderItem> dbSetExternalPurchaseOrderItem;
        public readonly IServiceProvider serviceProvider;
        private string USER_AGENT = "Facade";

        public GarmentInternNoteFacades(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentInternNote>();
            dbSetExternalPurchaseOrderItem = dbContext.Set<GarmentExternalPurchaseOrderItem>();
            this.serviceProvider = serviceProvider;
        }

        public async Task<int> Create(GarmentInternNote m, bool isImport, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    m.INNo = await GenerateNo(m,isImport, clientTimeZoneOffset);
                    m.INDate = DateTimeOffset.Now;

                    foreach (var item in m.Items)
                    {
                        GarmentInvoice garmentInvoice = this.dbContext.GarmentInvoices.FirstOrDefault(s => s.Id == item.InvoiceId);
                        if (garmentInvoice != null)
                            garmentInvoice.HasInternNote = true;
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                        foreach (var detail in item.Details)
                        {
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.RONo.Equals(detail.RONo));
                            detail.UnitId = internalPurchaseOrder.UnitId;
                            detail.UnitCode = internalPurchaseOrder.UnitCode;
                            detail.UnitName = internalPurchaseOrder.UnitName;
                            EntityExtension.FlagForCreate(detail, user, USER_AGENT);
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

        public int Delete(int id, string username)
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
                        GarmentInvoice garmentInvoice = this.dbContext.GarmentInvoices.FirstOrDefault(s => s.Id == item.InvoiceId);
                        
                        if (garmentInvoice != null)
                        {
                            garmentInvoice.HasInternNote = false;
                        }
                        EntityExtension.FlagForDelete(item, username, USER_AGENT);
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForDelete(detail, username, USER_AGENT);
                        }
                    }

                    Deleted = dbContext.SaveChanges();
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
            IQueryable<GarmentInternNote> Query = dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "INNo", "SupplierName", "Items.InvoiceNo"
            };

            Query = QueryHelper<GarmentInternNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query.Select(s => new GarmentInternNote
            {
                Id = s.Id,
                INNo = s.INNo,
                INDate = s.INDate,
                SupplierName = s.SupplierName,
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc,
                Items = s.Items.Select(i => new GarmentInternNoteItem
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNo = i.InvoiceNo,
                    InvoiceDate = i.InvoiceDate,
                    Details = i.Details.Select(d => new GarmentInternNoteDetail
                    {
                        DOId = d.DOId
                    }).ToList()
                }).ToList()
            });

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

        public HashSet<long> GetGarmentInternNoteId(long id)
        {
            return new HashSet<long>(dbContext.GarmentInternNoteItems.Where(d => d.InternNote.Id == id).Select(d => d.Id));
        }

        public async Task<int> Update(int id, GarmentInternNote m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (m.Items != null)
                    {
                        double total = 0;
                        HashSet<long> detailIds = GetGarmentInternNoteId(id);
                        foreach (var itemId in detailIds)
                        {
                            GarmentInternNoteItem data = m.Items.FirstOrDefault(prop => prop.Id.Equals(itemId));
                            if (data == null)
                            {
                                GarmentInternNoteItem dataItem = dbContext.GarmentInternNoteItems.FirstOrDefault(prop => prop.Id.Equals(itemId));
                                EntityExtension.FlagForDelete(dataItem, user, USER_AGENT);
                                var Details = dbContext.GarmentInvoiceDetails.Where(prop => prop.InvoiceItemId.Equals(itemId)).ToList();
                                GarmentInvoice deliveryOrder = dbContext.GarmentInvoices.FirstOrDefault(s => s.Id.Equals(dataItem.InvoiceId));
                                deliveryOrder.HasInternNote = false;
                                foreach (GarmentInvoiceDetail detail in Details)
                                {

                                    EntityExtension.FlagForDelete(detail, user, USER_AGENT);
                                }
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(data, user, USER_AGENT);
                            }

                            foreach (GarmentInternNoteItem item in m.Items)
                            {
                                total += item.TotalAmount;
                                if (item.Id <= 0)
                                {
                                    GarmentInvoice garmentInvoice = this.dbContext.GarmentInvoices.FirstOrDefault(s => s.Id == item.InvoiceId);
                                    
                                    if (garmentInvoice != null)
                                        garmentInvoice.HasInternNote = true;
                                    EntityExtension.FlagForCreate(item, user, USER_AGENT);
                                }
                                else
                                {
                                    GarmentInvoice garmentInvoice = this.dbContext.GarmentInvoices.FirstOrDefault(s => s.Id == item.InvoiceId);

                                    if (garmentInvoice != null)
                                        garmentInvoice.HasInternNote = false;
                                    EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                                }
                                foreach (GarmentInternNoteDetail detail in item.Details)
                                {
                                    if (item.Id <= 0)
                                    {
                                        EntityExtension.FlagForCreate(detail, user, USER_AGENT);
                                    }
                                    else
                                    {
                                        GarmentExternalPurchaseOrderItem eksternalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.GarmentEPOId == detail.EPOId);
                                        GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.RONo == detail.RONo);
                                        //GarmentInternalPurchaseOrderItem internalpurchaseorderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(p => p.GPOId.Equals(internalPurchaseOrder.Id));
                                        detail.UnitId = internalPurchaseOrder.UnitId;
                                        detail.UnitCode = internalPurchaseOrder.UnitCode;
                                        detail.UnitName = internalPurchaseOrder.UnitName;

                                        EntityExtension.FlagForUpdate(detail, user, USER_AGENT);
                                    }
                                }
                            }
                        }
                    }
                    EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                    this.dbSet.Update(m);
                    Updated = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                
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
            DateTimeOffset dateTimeOffsetNow = DateTimeOffset.Now;
            string Month = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string Year = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Supplier = isImport ? "I" : "L";

            string no = $"NI{Year}{Month}";
            int Padding = 4;
            
            var lastNo = await this.dbSet.Where(w => w.INNo.StartsWith(no) && w.INNo.EndsWith(Supplier) && !w.IsDeleted).OrderByDescending(o => o.INNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0') + Supplier;
            }
            else
            {
                //int lastNoNumber = Int32.Parse(lastNo.INNo.Replace(no, "")) + 1;
                int.TryParse(lastNo.INNo.Replace(no, "").Replace(Supplier,""), out int lastno1);
                int lastNoNumber = lastno1 + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0') +Supplier;
                
            }
        }
    }
}
