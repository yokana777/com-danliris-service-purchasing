using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
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

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade
{
    public class UnitReceiptNoteFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitReceiptNote> dbSet;

        public UnitReceiptNoteFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitReceiptNote>();
        }

        public Tuple<List<UnitReceiptNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<UnitReceiptNote> Query = this.dbSet;

            Query = Query.Select(s => new UnitReceiptNote
            {
                Id = s.Id,
                UId = s.UId,
                URNNo = s.URNNo,
                ReceiptDate = s.ReceiptDate,
                UnitName = s.UnitName,
                DivisionName = s.DivisionName,
                SupplierName = s.SupplierName,
                DONo = s.DONo,
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "URNNo", "UnitName", "SupplierName", "DONo"
            };

            Query = QueryHelper<UnitReceiptNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<UnitReceiptNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<UnitReceiptNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<UnitReceiptNote> pageable = new Pageable<UnitReceiptNote>(Query, Page - 1, Size);
            List<UnitReceiptNote> Data = pageable.Data.ToList<UnitReceiptNote>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public UnitReceiptNote ReadById(int id)
        {
            var a = this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
            return a;
        }


        async Task<string> GenerateNo(UnitReceiptNote model)
        {
            string Year = model.ReceiptDate.ToString("yy");
            string Month = model.ReceiptDate.ToString("MM");


            string no = $"{Year}-{Month}-{model.URNNo}-{model.UnitCode}-";
            int Padding = 3;

            var lastNo = await this.dbSet.Where(w => w.URNNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.URNNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.URNNo.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public async Task<int> Create(UnitReceiptNote m, string user)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, "Facade");

                    m.URNNo = await GenerateNo(m);

                    foreach (var item in m.Items)
                    {

                        EntityExtension.FlagForCreate(item, user, "Facade");
                        ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.FirstOrDefault(s => s.Id == item.EPODetailId);
                        PurchaseRequestItem prItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.PRItemId);
                        InternalPurchaseOrderItem poItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.POItemId);
                        DeliveryOrderDetail doDetail = dbContext.DeliveryOrderDetails.FirstOrDefault(s => s.Id == item.DODetailId);

                        doDetail.ReceiptQuantity += item.ReceiptQuantity;
                        externalPurchaseOrderDetail.ReceiptQuantity += item.ReceiptQuantity;
                        if (externalPurchaseOrderDetail.DOQuantity>= externalPurchaseOrderDetail.DealQuantity)
                        {
                            if (externalPurchaseOrderDetail.ReceiptQuantity < externalPurchaseOrderDetail.DealQuantity)
                            {
                                //prItem.Status = "Barang sudah diterima Unit parsial";
                                poItem.Status = "Barang sudah diterima Unit parsial";
                            }
                            else
                            {
                                //prItem.Status = "Barang sudah diterima Unit semua";
                                poItem.Status = "Barang sudah diterima Unit semua";
                            }
                        }
                        else
                        {
                            //prItem.Status = "Barang sudah diterima Unit parsial";
                            poItem.Status = "Barang sudah diterima Unit parsial";
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


        public async Task<int> Update(int id, UnitReceiptNote unitReceiptNote, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .Single(pr => pr.Id == id && !pr.IsDeleted);

                    if (m != null && !id.Equals(unitReceiptNote.Id))
                    {

                        EntityExtension.FlagForUpdate(unitReceiptNote, user, USER_AGENT);

                        foreach (var item in unitReceiptNote.Items)
                        {
                            if (item.Id == 0)
                            {
                                EntityExtension.FlagForCreate(item, user, USER_AGENT);
                                ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.FirstOrDefault(s => s.Id == item.EPODetailId);
                                PurchaseRequestItem prItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.PRItemId);
                                InternalPurchaseOrderItem poItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.POItemId);
                                DeliveryOrderDetail doDetail = dbContext.DeliveryOrderDetails.FirstOrDefault(s => s.Id == item.DODetailId);

                                doDetail.ReceiptQuantity += item.ReceiptQuantity;
                                externalPurchaseOrderDetail.ReceiptQuantity += item.ReceiptQuantity;
                                if (externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                {
                                    if (externalPurchaseOrderDetail.ReceiptQuantity < externalPurchaseOrderDetail.DealQuantity)
                                    {
                                        //prItem.Status = "Barang sudah diterima Unit parsial";
                                        poItem.Status = "Barang sudah diterima Unit parsial";
                                    }
                                    else
                                    {
                                        //prItem.Status = "Barang sudah diterima Unit semua";
                                        poItem.Status = "Barang sudah diterima Unit semua";
                                    }
                                }
                                else
                                {
                                    //prItem.Status = "Barang sudah diterima Unit parsial";
                                    poItem.Status = "Barang sudah diterima Unit parsial";
                                }
                            }
                            EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                        }

                        this.dbContext.Update(unitReceiptNote);

                        foreach (var item in m.Items)
                        {
                            
                            ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.FirstOrDefault(s => s.Id == item.EPODetailId);
                            PurchaseRequestItem prItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.PRItemId);
                            InternalPurchaseOrderItem poItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.POItemId);
                            DeliveryOrderDetail doDetail = dbContext.DeliveryOrderDetails.FirstOrDefault(s => s.Id == item.DODetailId);

                            UnitReceiptNoteItem unitReceiptNoteItem = unitReceiptNote.Items.FirstOrDefault(i => i.Id.Equals(item.Id));
                            if (unitReceiptNoteItem == null)
                            {
                                EntityExtension.FlagForDelete(item, user, USER_AGENT);
                                doDetail.ReceiptQuantity -= item.ReceiptQuantity;
                                externalPurchaseOrderDetail.ReceiptQuantity -= item.ReceiptQuantity;
                                if (externalPurchaseOrderDetail.ReceiptQuantity == 0)
                                {
                                    if (externalPurchaseOrderDetail.DOQuantity > 0 && externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                    {
                                        //prItem.Status = "Barang sudah diterima Unit semua";
                                        poItem.Status = "Barang sudah diterima Unit semua";
                                    }
                                    else if (externalPurchaseOrderDetail.DOQuantity > 0 && externalPurchaseOrderDetail.DOQuantity < externalPurchaseOrderDetail.DealQuantity)
                                    {
                                        //prItem.Status = "Barang sudah diterima Unit parsial";
                                        poItem.Status = "Barang sudah diterima Unit parsial";
                                    }
                                }
                                else if (externalPurchaseOrderDetail.ReceiptQuantity > 0)
                                {
                                    if (externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                    {
                                        if (externalPurchaseOrderDetail.ReceiptQuantity < externalPurchaseOrderDetail.DealQuantity)
                                        {
                                            //prItem.Status = "Barang sudah diterima Unit parsial";
                                            poItem.Status = "Barang sudah diterima Unit parsial";
                                        }
                                        else if (externalPurchaseOrderDetail.ReceiptQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                        {
                                            //prItem.Status = "Barang sudah diterima Unit semua";
                                            poItem.Status = "Barang sudah diterima Unit semua";
                                        }
                                    }
                                }
                                this.dbContext.UnitReceiptNoteItems.Update(item);
                            }
                            else
                            {
                                doDetail.ReceiptQuantity -= unitReceiptNoteItem.ReceiptQuantity;
                                externalPurchaseOrderDetail.ReceiptQuantity -= unitReceiptNoteItem.ReceiptQuantity;

                                doDetail.ReceiptQuantity += item.ReceiptQuantity;
                                externalPurchaseOrderDetail.ReceiptQuantity += item.ReceiptQuantity;
                                if (externalPurchaseOrderDetail.ReceiptQuantity == 0)
                                {
                                    if (externalPurchaseOrderDetail.DOQuantity > 0 && externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                    {
                                        //prItem.Status = "Barang sudah diterima Unit semua";
                                        poItem.Status = "Barang sudah diterima Unit semua";
                                    }
                                    else if (externalPurchaseOrderDetail.DOQuantity > 0 && externalPurchaseOrderDetail.DOQuantity < externalPurchaseOrderDetail.DealQuantity)
                                    {
                                        //prItem.Status = "Barang sudah diterima Unit parsial";
                                        poItem.Status = "Barang sudah diterima Unit parsial";
                                    }
                                }
                                else if (externalPurchaseOrderDetail.ReceiptQuantity > 0)
                                {
                                    if (externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                    {
                                        if (externalPurchaseOrderDetail.ReceiptQuantity < externalPurchaseOrderDetail.DealQuantity)
                                        {
                                            //prItem.Status = "Barang sudah diterima Unit parsial";
                                            poItem.Status = "Barang sudah diterima Unit parsial";
                                        }
                                        else if (externalPurchaseOrderDetail.ReceiptQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                        {
                                            //prItem.Status = "Barang sudah diterima Unit semua";
                                            poItem.Status = "Barang sudah diterima Unit semua";
                                        }
                                    }
                                }
                                this.dbContext.UnitReceiptNoteItems.Update(item);
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

        public int Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(m, user, USER_AGENT);

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForDelete(item, user, USER_AGENT);

                        ExternalPurchaseOrderDetail externalPurchaseOrderDetail = this.dbContext.ExternalPurchaseOrderDetails.FirstOrDefault(s => s.Id == item.EPODetailId);
                        PurchaseRequestItem prItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.PRItemId);
                        InternalPurchaseOrderItem poItem = this.dbContext.InternalPurchaseOrderItems.FirstOrDefault(s => s.Id == externalPurchaseOrderDetail.POItemId);
                        DeliveryOrderDetail doDetail = dbContext.DeliveryOrderDetails.FirstOrDefault(s => s.Id == item.DODetailId);

                        doDetail.ReceiptQuantity -= item.ReceiptQuantity;
                        externalPurchaseOrderDetail.ReceiptQuantity -= item.ReceiptQuantity;
                        if (externalPurchaseOrderDetail.ReceiptQuantity == 0)
                        {
                            if (externalPurchaseOrderDetail.DOQuantity>0 && externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
                            {
                                //prItem.Status = "Barang sudah diterima Unit semua";
                                poItem.Status = "Barang sudah diterima Unit semua";
                            }
                            else if(externalPurchaseOrderDetail.DOQuantity > 0 && externalPurchaseOrderDetail.DOQuantity < externalPurchaseOrderDetail.DealQuantity)
                            {
                                //prItem.Status = "Barang sudah diterima Unit parsial";
                                poItem.Status = "Barang sudah diterima Unit parsial";
                            }
                        }
                        else if(externalPurchaseOrderDetail.ReceiptQuantity>0)
                        {
                            if (externalPurchaseOrderDetail.DOQuantity >= externalPurchaseOrderDetail.DealQuantity)
                            {
                                if (externalPurchaseOrderDetail.ReceiptQuantity < externalPurchaseOrderDetail.DealQuantity)
                                {
                                    //prItem.Status = "Barang sudah diterima Unit parsial";
                                    poItem.Status = "Barang sudah diterima Unit parsial";
                                }
                                else if (externalPurchaseOrderDetail.ReceiptQuantity >= externalPurchaseOrderDetail.DealQuantity)
                                {
                                    //prItem.Status = "Barang sudah diterima Unit semua";
                                    poItem.Status = "Barang sudah diterima Unit semua";
                                }
                            }
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
    }
}
