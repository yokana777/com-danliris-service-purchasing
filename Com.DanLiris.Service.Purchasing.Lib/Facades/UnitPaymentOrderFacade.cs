using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades
{
    public class UnitPaymentOrderFacade : IUnitPaymentOrderFacade
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<UnitPaymentOrder> dbSet;

        private string USER_AGENT = "Facade";

        public UnitPaymentOrderFacade(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitPaymentOrder>();
        }

        public Tuple<List<UnitPaymentOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<UnitPaymentOrder> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "UPONo", "DivisionName", "SupplierName", "Items.URNNo", "Items.DONo"
            };

            Query = QueryHelper<UnitPaymentOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query.Select(s => new UnitPaymentOrder
            {
                Id = s.Id,
                DivisionId = s.DivisionId,
                DivisionCode = s.DivisionCode,
                DivisionName = s.DivisionName,
                SupplierId = s.SupplierId,
                SupplierCode = s.SupplierCode,
                SupplierName = s.SupplierName,
                Date = s.Date,
                UPONo = s.UPONo,
                Items = s.Items.Select(i => new UnitPaymentOrderItem
                {
                    URNNo = i.URNNo,
                    DONo = i.DONo,
                }).ToList(),
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc,
            });

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<UnitPaymentOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<UnitPaymentOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<UnitPaymentOrder> pageable = new Pageable<UnitPaymentOrder>(Query, Page - 1, Size);
            List<UnitPaymentOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public UnitPaymentOrder ReadById(int id)
        {
            var Result = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                    .ThenInclude(i => i.Details)
                .FirstOrDefault();

            return Result;
        }

        public async Task<int> Create(UnitPaymentOrder model, string username, bool isImport, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using(var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);
                    model.UPONo = await GenerateNo(model, isImport, clientTimeZoneOffset);
                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForCreate(item, username, USER_AGENT);
                        foreach (var detail in item.Details)
                        {
                            SetEPONo(detail);
                            EntityExtension.FlagForCreate(detail, username, USER_AGENT);

                            SetStatus(detail, username);
                        }
                        SetPaid(item, true, username);
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

        public async Task<int> Update(int id, UnitPaymentOrder model, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var existingModel = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    if (existingModel != null && id == model.Id)
                    {
                        EntityExtension.FlagForUpdate(model, user, USER_AGENT);

                        foreach (var item in model.Items)
                        {
                            if (item.Id == 0)
                            {
                                EntityExtension.FlagForCreate(item, user, USER_AGENT);
                                foreach (var detail in item.Details)
                                {
                                    SetEPONo(detail);
                                    EntityExtension.FlagForCreate(detail, user, USER_AGENT);

                                    SetStatus(detail, user);
                                }
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                                foreach (var detail in item.Details)
                                {
                                    EntityExtension.FlagForUpdate(detail, user, USER_AGENT);

                                    SetStatus(detail, user);
                                }
                            }

                            SetPaid(item, true, user);
                        }

                        this.dbContext.Update(model);

                        foreach (var existingItem in existingModel.Items)
                        {
                            var newItem = model.Items.FirstOrDefault(i => i.Id == existingItem.Id);
                            if (newItem == null)
                            {
                                EntityExtension.FlagForDelete(existingItem, user, USER_AGENT);
                                this.dbContext.UnitPaymentOrderItems.Update(existingItem);
                                foreach (var existingDetail in existingItem.Details)
                                {
                                    EntityExtension.FlagForDelete(existingDetail, user, USER_AGENT);
                                    this.dbContext.UnitPaymentOrderDetails.Update(existingDetail);

                                    SetStatus(existingDetail, user);
                                }

                                SetPaid(existingItem, false, user);
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

        public async Task<int> Delete(int id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);

                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForDelete(item, username, USER_AGENT);
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForDelete(detail, username, USER_AGENT);

                            SetStatus(detail, username);
                        }

                        SetPaid(item, false, username);
                    }

                    Deleted = await dbContext.SaveChangesAsync();
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

        async Task<string> GenerateNo(UnitPaymentOrder model, bool isImport, int clientTimeZoneOffset)
        {
            string Year = model.Date.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Month = model.Date.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string Supplier = isImport ? "NKI" : "NKL";
            string TG = "";
            if (model.DivisionName.ToUpper().Equals("GARMENT"))
            {
                TG = "G-";
            }
            else if(model.DivisionName.ToUpper().Equals("UMUM") || 
                model.DivisionName.ToUpper().Equals("SPINNING") ||
                model.DivisionName.ToUpper().Equals("FINISHING & PRINTING") ||
                model.DivisionName.ToUpper().Equals("UTILITY") ||
                model.DivisionName.ToUpper().Equals("WEAVING"))
            {
                TG = "T-";
            }

            string no = $"{Year}-{Month}-{TG}{Supplier}-";
            int Padding = isImport ? 3 : 4;

            var lastNo = await dbSet.Where(w => w.UPONo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.UPONo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.UPONo.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        private void SetEPONo(UnitPaymentOrderDetail detail)
        {
            detail.EPONo = dbContext.ExternalPurchaseOrders.Single(m => m.Items.Any(i => i.Details.Any(d => d.Id == detail.EPODetailId))).EPONo;
        }

        private void SetPaid(UnitPaymentOrderItem item, bool isPaid, string username)
        {
            UnitReceiptNote unitReceiptNote = dbContext.UnitReceiptNotes.Single(m => m.Id == item.URNId);
            unitReceiptNote.IsPaid = isPaid;
            EntityExtension.FlagForUpdate(unitReceiptNote, username, USER_AGENT);
        }

        private void SetStatus(UnitPaymentOrderDetail detail, string username)
        {
            // PurchaseRequestItem purchaseRequestItem = dbContext.PurchaseRequestItems.Single(m => m.Id == detail.PRItemId);
            // purchaseRequestItem.Status = "";
            // EntityExtension.FlagForUpdate(purchaseRequestItem, username, USER_AGENT);

            // //List<InternalPurchaseOrderItem> internalPurchaseOrderItems = dbContext.InternalPurchaseOrderItems.Where(m => m.PRItemId == detail.PRItemId).ToList();
            // //foreach (var item in internalPurchaseOrderItems)
            // //{
            // //    item.Status = "";
            // //    EntityExtension.FlagForUpdate(item, username, USER_AGENT);
            // //}

            // InternalPurchaseOrderItem internalPurchaseOrderItem = dbContext.InternalPurchaseOrderItems.Single(m => m.PRItemId == detail.PRItemId);
            // internalPurchaseOrderItem.Status = "";
            // EntityExtension.FlagForUpdate(internalPurchaseOrderItem, username, USER_AGENT);
        }
    }
}
