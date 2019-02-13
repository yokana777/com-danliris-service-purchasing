using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade
{
    public class UnitPaymentQuantityCorrectionNoteFacade : IUnitPaymentQuantityCorrectionNoteFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitPaymentCorrectionNote> dbSet;

        public UnitPaymentQuantityCorrectionNoteFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitPaymentCorrectionNote>();
        }

        public Tuple<List<UnitPaymentCorrectionNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<UnitPaymentCorrectionNote> Query = this.dbSet;

            Query = Query
                .Select(s => new UnitPaymentCorrectionNote
                {
                    Id = s.Id,
                    UPCNo = s.UPCNo,
                    CorrectionDate = s.CorrectionDate,
                    CorrectionType = s.CorrectionType,
                    UPOId = s.UPOId,
                    UPONo = s.UPONo,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    InvoiceCorrectionNo = s.InvoiceCorrectionNo,
                    InvoiceCorrectionDate = s.InvoiceCorrectionDate,
                    useVat = s.useVat,
                    useIncomeTax = s.useIncomeTax,
                    ReleaseOrderNoteNo = s.ReleaseOrderNoteNo,
                    DueDate = s.DueDate,
                    Items = s.Items.Select(
                        q => new UnitPaymentCorrectionNoteItem
                        {
                            Id = q.Id,
                            UPCId = q.UPCId,
                            UPODetailId = q.UPODetailId,
                            URNNo = q.URNNo,
                            EPONo = q.EPONo,
                            PRId = q.PRId,
                            PRNo = q.PRNo,
                            PRDetailId = q.PRDetailId,
                        }
                    )
                    .ToList(),
                    CreatedBy = s.CreatedBy,
                    LastModifiedUtc = s.LastModifiedUtc
                }).Where(k => k.CorrectionType == "Jumlah")
                .OrderByDescending(j => j.LastModifiedUtc);

            List<string> searchAttributes = new List<string>()
            {
                "UPCNo", "UPONo", "SupplierName", "InvoiceCorrectionNo"
            };

            Query = QueryHelper<UnitPaymentCorrectionNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<UnitPaymentCorrectionNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<UnitPaymentCorrectionNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<UnitPaymentCorrectionNote> pageable = new Pageable<UnitPaymentCorrectionNote>(Query, Page - 1, Size);
            List<UnitPaymentCorrectionNote> Data = pageable.Data.ToList<UnitPaymentCorrectionNote>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public UnitPaymentCorrectionNote ReadById(int id)
        {
            var a = this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
            return a;
        }

        public async Task<int> Create(UnitPaymentCorrectionNote m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);
                    var supplier = GetSupplier(m.SupplierId);
                    var supplierImport = false;
                    m.SupplierNpwp = null;
                    if (supplier != null)
                    {
                        m.SupplierNpwp = supplier.npwp;
                        supplierImport = supplier.import;
                    }
                    m.UPCNo = await GenerateNo(m, clientTimeZoneOffset, supplierImport, m.DivisionName);
                    if(m.useVat==true)
                    {
                        m.ReturNoteNo = await GeneratePONo(m, clientTimeZoneOffset);
                    }
                    UnitPaymentOrder unitPaymentOrder = this.dbContext.UnitPaymentOrders.Where(s => s.Id == m.UPOId).Include(p => p.Items).ThenInclude(i => i.Details).FirstOrDefault();
                    unitPaymentOrder.IsCorrection = true;

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                        foreach (var itemSpb in unitPaymentOrder.Items)
                        {
                            foreach(var detailSpb in itemSpb.Details)
                            {
                                if (item.UPODetailId == detailSpb.Id)
                                {
                                    detailSpb.QuantityCorrection = detailSpb.QuantityCorrection - item.Quantity;
                                    ExternalPurchaseOrderDetail epoDetail = dbContext.ExternalPurchaseOrderDetails.FirstOrDefault(a => a.Id.Equals(detailSpb.EPODetailId));
                                    epoDetail.DOQuantity -= item.Quantity;
                                }
                                
                            }
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

        async Task<string> GenerateNo(UnitPaymentCorrectionNote model, int clientTimeZoneOffset, bool supplierImport, string divisionName)
        {
            string Year = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Month = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string supplier_imp;
            char division_name;
            if (supplierImport == true)
            {
                supplier_imp = "NRI";
            }
            else
            {
                supplier_imp = "NRL";
            }
            if (divisionName.ToUpper() == "GARMENT")
            {
                division_name = 'G';
            }
            else
            {
                division_name = 'T';
            }


            string no = $"{Year}-{Month}-{division_name}-{supplier_imp}-";
            int Padding = 3;
            var upcno = "";

            var lastNo = await this.dbSet.Where(w => w.UPCNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.UPCNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                upcno = no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.UPCNo.Replace(no, "")) + 1;
                upcno = no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
            return upcno;
        }

        async Task<string> GeneratePONo(UnitPaymentCorrectionNote model, int clientTimeZoneOffset)
        {
            string Year = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Month = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");

            string no = $"{Year}-{Month}-NR-";
            int Padding = 3;
            var pono = "";

            var lastNo = await this.dbSet.Where(w => w.ReturNoteNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.ReturNoteNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                pono = no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.ReturNoteNo.Replace(no, "")) + 1;
                pono = no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
            return pono;
        }

        public SupplierViewModel GetSupplier(string supplierId)
        {
            string supplierUri = "master/suppliers";
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient!=null)
            {
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{supplierUri}/{supplierId}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                SupplierViewModel viewModel = JsonConvert.DeserializeObject<SupplierViewModel>(result.GetValueOrDefault("data").ToString());
                return viewModel;
            }
            else
            {
                SupplierViewModel viewModel = null;
                return viewModel;
            }
            
        }

        public UnitReceiptNote ReadByURNNo(string uRNNo)
        {
            var a = dbContext.UnitReceiptNotes.Where(p => p.URNNo == uRNNo)
                .Include(p => p.Items)
                .FirstOrDefault();
            return a;
        }
        //public UnitReceiptNote ReadByURNNo(string uRNNo)
        //{
        //    return dbContext.UnitReceiptNotes.Where(m => m.URNNo == uRNNo)
        //        .Include(p => p.Items)
        //        .FirstOrDefault();
        //}

        //public async Task<int> Update(int id, UnitPaymentCorrectionNote unitPaymentCorrectionNote, string user)
        //{
        //    int Updated = 0;

        //    using (var transaction = this.dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var m = this.dbSet.AsNoTracking()
        //                .Include(d => d.Items)
        //                .Single(pr => pr.Id == id && !pr.IsDeleted);

        //            if (m != null && id == unitPaymentCorrectionNote.Id)
        //            {

        //                EntityExtension.FlagForUpdate(unitPaymentCorrectionNote, user, USER_AGENT);

        //                foreach (var item in unitPaymentCorrectionNote.Items)
        //                {
        //                    if (item.Id == 0)
        //                    {
        //                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
        //                    }
        //                    else
        //                    {
        //                        EntityExtension.FlagForUpdate(item, user, USER_AGENT);
        //                    }
        //                }

        //                this.dbContext.Update(unitPaymentCorrectionNote);

        //                foreach (var item in m.Items)
        //                {
        //                    UnitPaymentCorrectionNoteItem unitPaymentCorrectionNoteItem = unitPaymentCorrectionNote.Items.FirstOrDefault(i => i.Id.Equals(item.Id));
        //                    if (unitPaymentCorrectionNoteItem == null)
        //                    {
        //                        EntityExtension.FlagForDelete(item, user, USER_AGENT);
        //                        this.dbContext.UnitPaymentCorrectionNoteItems.Update(item);
        //                    }
        //                }

        //                Updated = await dbContext.SaveChangesAsync();
        //                transaction.Commit();
        //            }
        //            else
        //            {
        //                throw new Exception("Invalid Id");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            transaction.Rollback();
        //            throw new Exception(e.Message);
        //        }
        //    }

        //    return Updated;
        //}

        //public int Delete(int id, string user)
        //{
        //    int Deleted = 0;

        //    using (var transaction = this.dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var m = this.dbSet
        //                .Include(d => d.Items)
        //                .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

        //            EntityExtension.FlagForDelete(m, user, USER_AGENT);

        //            foreach (var item in m.Items)
        //            {
        //                EntityExtension.FlagForDelete(item, user, USER_AGENT);
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

    }
}