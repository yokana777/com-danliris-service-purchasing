using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
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
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
                    if (m.useVat == true)
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
                            foreach (var detailSpb in itemSpb.Details)
                            {
                                if (item.UPODetailId == detailSpb.Id)
                                {
                                    if (detailSpb.QuantityCorrection <= 0)
                                    {
                                        detailSpb.QuantityCorrection = detailSpb.ReceiptQuantity;
                                    }

                                    detailSpb.QuantityCorrection = detailSpb.QuantityCorrection - item.Quantity;
                                    ExternalPurchaseOrderDetail epoDetail = dbContext.ExternalPurchaseOrderDetails.FirstOrDefault(a => a.Id.Equals(detailSpb.EPODetailId));
                                    epoDetail.DOQuantity -= item.Quantity;
                                }
                            }
                        }
                    }

                    this.dbSet.Add(m);
                    Created = await dbContext.SaveChangesAsync();
                    Created += await AddCorrections(m, user);
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
            if (httpClient != null)
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
        #region Monitoring Correction Jumlah 
        public IQueryable<UnitPaymentQuantityCorrectionNoteReportViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = (from a in dbContext.UnitPaymentCorrectionNotes
                         join i in dbContext.UnitPaymentCorrectionNoteItems on a.Id equals i.UPCId
                         join j in dbContext.UnitReceiptNotes on i.URNNo equals j.URNNo
                         where a.IsDeleted == false
                             && a.CorrectionType == "Jumlah"
                             && i.IsDeleted == false
                             // && (a.CorrectionType == "Harga Total" || a.CorrectionType == "Harga Satuan")
                             && a.CorrectionDate.AddHours(offset).Date >= DateFrom.Date
                             && a.CorrectionDate.AddHours(offset).Date <= DateTo.Date
                         select new UnitPaymentQuantityCorrectionNoteReportViewModel
                         {
                             upcNo = a.UPCNo,
                             epoNo = i.EPONo,
                             upoNo = a.UPONo,
                             prNo = i.PRNo,
                             notaRetur = a.ReturNoteNo,
                             vatTaxCorrectionNo = a.VatTaxCorrectionNo,
                             vatTaxCorrectionDate = a.VatTaxCorrectionDate,
                             correctionDate = a.CorrectionDate,
                             unit = j.UnitName,
                             category = a.CategoryName,
                             supplier = a.SupplierName,
                             productCode = i.ProductCode,
                             productName = i.ProductName,
                             jumlahKoreksi = i.Quantity,
                             satuanKoreksi = i.UomUnit,
                             hargaSatuanKoreksi = i.PricePerDealUnitAfter,
                             hargaTotalKoreksi = i.PriceTotalAfter,
                             user = a.CreatedBy,
                             jenisKoreksi = a.CorrectionType,

                         });
            return Query;
        }

        public Tuple<List<UnitPaymentQuantityCorrectionNoteReportViewModel>, int> GetReport(DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetReportQuery(dateFrom, dateTo, offset);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            }


            Pageable<UnitPaymentQuantityCorrectionNoteReportViewModel> pageable = new Pageable<UnitPaymentQuantityCorrectionNoteReportViewModel>(Query, page - 1, size);
            List<UnitPaymentQuantityCorrectionNoteReportViewModel> Data = pageable.Data.ToList<UnitPaymentQuantityCorrectionNoteReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery(dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Nota Debet", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Debet", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No SPB", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO Eks", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nota Retur", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Faktur Pajak PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Faktur Pajak PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kd Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nm Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Koreksi", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Satuan Koreksi", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Total Koreksi", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "User Input", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Koreksi", DataType = typeof(String) });
            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", 0, 0, "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string correctionDate = item.correctionDate == null ? "-" : item.correctionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    DateTimeOffset date = item.vatTaxCorrectionDate ?? new DateTime(1970, 1, 1);
                    string vatDate = date == new DateTime(1970, 1, 1) ? "-" : date.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    result.Rows.Add(index, item.upcNo, correctionDate, item.upoNo, item.epoNo, item.prNo, item.notaRetur, item.vatTaxCorrectionNo, vatDate, item.unit, item.category, item.supplier, item.productCode, item.productName, item.jumlahKoreksi, item.satuanKoreksi, item.hargaSatuanKoreksi, item.hargaTotalKoreksi, item.user, item.jenisKoreksi);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
        #endregion 

        private async Task<int> AddCorrections(UnitPaymentCorrectionNote model, string username)
        {
            var internalPOFacade = serviceProvider.GetService<InternalPurchaseOrderFacade>();
            int count = 0;
            foreach (var item in model.Items)
            {

                var fulfillment = await dbContext.InternalPurchaseOrderFulfillments.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UnitPaymentOrderId == model.UPOId && x.UnitPaymentOrderDetailId == item.UPODetailId);

                if (fulfillment != null)
                {
                    fulfillment.Corrections.Add(new InternalPurchaseOrderCorrection()
                    {
                        CorrectionDate = model.CorrectionDate,
                        CorrectionNo = model.UPCNo,
                        CorrectionPriceTotal = item.PriceTotalAfter,
                        CorrectionQuantity = item.Quantity,
                        CorrectionRemark = model.Remark,
                        UnitPaymentCorrectionId = model.Id,
                        UnitPaymentCorrectionItemId = item.Id
                    });

                    count += await internalPOFacade.UpdateFulfillmentAsync(fulfillment.Id, fulfillment, username);
                }
            }

            return count;
        }

        public async Task<CorrectionState> GetCorrectionStateByUnitPaymentOrderId(int unitPaymentOrderId)
        {
            return new CorrectionState()
            {
                IsHavingPricePerUnitCorrection = await dbSet.AnyAsync(entity => entity.UPOId == unitPaymentOrderId && entity.CorrectionType == "Harga Satuan"),
                IsHavingPriceTotalCorrection = await dbSet.AnyAsync(entity => entity.UPOId == unitPaymentOrderId && entity.CorrectionType == "Harga Total"),
                IsHavingQuantityCorrection = await dbSet.AnyAsync(entity => entity.UPOId == unitPaymentOrderId && entity.CorrectionType == "Jumlah")
            };
        }
    }
}