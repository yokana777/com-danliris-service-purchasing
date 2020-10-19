using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
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
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade
{
    public class UnitPaymentPriceCorrectionNoteFacade : IUnitPaymentPriceCorrectionNoteFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitPaymentCorrectionNote> dbSet;

        public UnitPaymentPriceCorrectionNoteFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
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
                }).Where(s=>s.CorrectionType== "Harga Satuan"|| s.CorrectionType == "Harga Total").OrderByDescending(j => j.LastModifiedUtc);

            List<string> searchAttributes = new List<string>()
            {
                "UPCNo", "UPONo", "SupplierName","InvoiceCorrectionNo"
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

        async Task<string> GenerateNo(UnitPaymentCorrectionNote model, bool supplierImport, int clientTimeZoneOffset)
        {
            string Year = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Month = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string supplier_imp = "NRL";
            if (supplierImport == true)
            {
                supplier_imp = "NRI";
            }
            string divisionName = model.DivisionName;
            string division_name = "T";
            if (divisionName.ToUpper() == "GARMENT")
            {
                division_name = "G";
            }

            string no = $"{Year}-{Month}-{division_name}-{supplier_imp}-";
            int Padding = 3;

            var lastNo = await this.dbSet.Where(w => w.UPCNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.UPCNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.UPCNo.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public async Task<int> Create(UnitPaymentCorrectionNote model, bool supplierImport, string username, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);

                    model.UPCNo = await GenerateNo(model, supplierImport, clientTimeZoneOffset);
                    if (model.useVat == true)
                    {
                        model.ReturNoteNo = await GeneratePONo(model, clientTimeZoneOffset);
                    }
                    UnitPaymentOrder unitPaymentOrder = this.dbContext.UnitPaymentOrders.FirstOrDefault(s => s.Id == model.UPOId);
                    unitPaymentOrder.IsCorrection = true;

                    foreach (var item in model.Items)
                    {
                        
                        UnitPaymentOrderDetail upoDetail = dbContext.UnitPaymentOrderDetails.FirstOrDefault(s => s.Id == item.UPODetailId);
                        item.PricePerDealUnitBefore = upoDetail.PricePerDealUnit;
                        item.PriceTotalBefore = upoDetail.PriceTotal;
                        upoDetail.PricePerDealUnitCorrection = item.PricePerDealUnitAfter;
                        upoDetail.PriceTotalCorrection = item.PriceTotalAfter;

                        if (item.PriceTotalAfter > 0 && item.PricePerDealUnitAfter <= 0)
                        {
                            upoDetail.PricePerDealUnitCorrection = upoDetail.PricePerDealUnit;
                        }
                            
                        EntityExtension.FlagForCreate(item, username, USER_AGENT);
                    }

                    this.dbSet.Add(model);
                    Created = await dbContext.SaveChangesAsync();
                    Created += await AddCorrections(model, username);
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
        async Task<string> GeneratePONo(UnitPaymentCorrectionNote model, int clientTimeZoneOffset)
        {
            string Year = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Month = model.CorrectionDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");



            string no = $"{Year}-{Month}-NR-";
            int Padding = 3;

            var lastNo = await this.dbSet.Where(w => w.ReturNoteNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.ReturNoteNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.ReturNoteNo.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public SupplierViewModel GetSupplier(string supplierId)
        {
            string supplierUri = "master/suppliers";
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var resu = httpClient.GetAsync($"{APIEndpoint.Core}{supplierUri}/{supplierId}").Result;
                var response = resu.Content.ReadAsStringAsync();
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
        public UnitReceiptNote GetUrn(string urnNo)
        {
            var urnModel = dbContext.UnitReceiptNotes.SingleOrDefault(s => s.IsDeleted == false && s.URNNo == urnNo);//_urnFacade.ReadById((int)item.URNId);
            return urnModel;
        }

        public IQueryable<UnitPaymentPriceCorrectionNoteReportViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = (from a in dbContext.UnitPaymentCorrectionNotes
                         join i in dbContext.UnitPaymentCorrectionNoteItems on a.Id equals i.UPCId
                         where a.IsDeleted == false
                             && i.IsDeleted == false
                             && (a.CorrectionType=="Harga Total" || a.CorrectionType == "Harga Satuan")
                             && a.CorrectionDate.AddHours(offset).Date >= DateFrom.Date
                             && a.CorrectionDate.AddHours(offset).Date <= DateTo.Date
                         select new UnitPaymentPriceCorrectionNoteReportViewModel
                         {
                             upcNo=a.UPCNo,
                             epoNo=i.EPONo,
                             upoNo=a.UPONo,
                             prNo = i.PRNo,
                             vatTaxCorrectionNo=a.VatTaxCorrectionNo,
                             vatTaxCorrectionDate=a.VatTaxCorrectionDate,
                             correctionDate=a.CorrectionDate,
                             correctionType=a.CorrectionType,
                             pricePerDealUnitAfter=i.PricePerDealUnitAfter,
                             priceTotalAfter=i.PriceTotalAfter,
                             supplier=a.SupplierName,
                             productCode=i.ProductCode,
                             productName = i.ProductName,
                             uom = i.UomUnit,
                             quantity = i.Quantity,
                             user = a.CreatedBy,
                             LastModifiedUtc = i.LastModifiedUtc
                         });
            return Query;
        }

        public Tuple<List<UnitPaymentPriceCorrectionNoteReportViewModel>, int> GetReport( DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetReportQuery(dateFrom, dateTo, offset);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            }


            Pageable<UnitPaymentPriceCorrectionNoteReportViewModel> pageable = new Pageable<UnitPaymentPriceCorrectionNoteReportViewModel>(Query, page - 1, size);
            List<UnitPaymentPriceCorrectionNoteReportViewModel> Data = pageable.Data.ToList<UnitPaymentPriceCorrectionNoteReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery(dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Nota Debet", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Nota Debet", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No SPB", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Purchase Request", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Faktur Pajak PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Faktur Pajak PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Koreksi", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Satuan Koreksi", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Total Koreksi", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "User Input", DataType = typeof(String) });
            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", 0, "", 0, 0, ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string correctionDate = item.correctionDate == null ? "-" : item.correctionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    DateTimeOffset date = item.vatTaxCorrectionDate ?? new DateTime(1970, 1, 1);
                    string vatDate = date == new DateTime(1970, 1, 1) ? "-" : date.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    result.Rows.Add(index, item.upcNo, correctionDate, item.upoNo,item.epoNo, item.prNo, item.vatTaxCorrectionNo,vatDate,item.supplier,item.correctionType,item.productCode, item.productName, item.quantity, item.uom, item.pricePerDealUnitAfter, item.priceTotalAfter, item.user);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        public IQueryable<UnitPaymentCorrectionNoteGenerateDataViewModel> GetDataReportQuery(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            var Query = (from a in dbContext.UnitPaymentCorrectionNotes
                         join b in dbContext.UnitPaymentCorrectionNoteItems on a.Id equals b.UPCId
                         join c in dbContext.UnitReceiptNotes on b.URNNo equals c.URNNo

                         where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false &&
                               a.CorrectionDate.AddHours(offset).Date >= DateFrom.Date &&
                               a.CorrectionDate.AddHours(offset).Date <= DateTo.Date
                         select new UnitPaymentCorrectionNoteGenerateDataViewModel
                         {
                             UPCNo = a.UPCNo,
                             UPCDate = a.CorrectionDate,
                             CorrectionType = a.CorrectionType,
                             UPONo = a.UPONo,
                             InvoiceCorrectionNo = a.InvoiceCorrectionNo,
                             InvoiceCorrectionDate = a.InvoiceCorrectionDate.GetValueOrDefault(),
                             VatTaxCorrectionNo = a.VatTaxCorrectionNo,
                             VatTaxCorrectionDate = a.VatTaxCorrectionDate.GetValueOrDefault(),
                             IncomeTaxCorrectionNo = a.IncomeTaxCorrectionNo,
                             IncomeTaxCorrectionDate = a.InvoiceCorrectionDate.GetValueOrDefault(),
                             SupplierCode = a.SupplierCode,
                             SupplierName = a.SupplierName,
                             SupplierAddress = "",
                             ReleaseOrderNoteNo = a.ReleaseOrderNoteNo,
                             UPCRemark = a.Remark,
                             EPONo = b.EPONo,
                             PRNo = b.PRNo,
                             AccountNo = "-",
                             ProductCode = b.ProductCode,
                             ProductName = b.ProductName,
                             Quantity = b.Quantity,
                             UOMUnit = b.UomUnit,
                             PricePerDealUnit = b.PricePerDealUnitAfter,
                             CurrencyCode = b.CurrencyCode,
                             CurrencyRate = b.CurrencyRate,
                             PriceTotal = b.PriceTotalAfter,
                             URNNo = b.URNNo,
                             URNDate = c.ReceiptDate,
                             UserCreated = a.CreatedBy,
                             UseVat = a.useVat ? "YA " : "TIDAK",
                             UseIncomeTax = a.useIncomeTax ? "YA " : "TIDAK",
                         }
                         );
            return Query;
        }

        public MemoryStream GenerateDataExcel(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetDataReportQuery(dateFrom, dateTo, offset);
            Query = Query.OrderBy(b => b.UPCNo);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR NOTA KOREKSI", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL NOTA KOREKSI", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "JENIS RETUR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR NOTA KREDIT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR INVOICE KOREKSI", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL INVOICE KOREKSI", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "FAKTUR PAJAK KOREKSI PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL FAKTUR PAJAK KOREKSI PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "FAKTUR PAJAK KOREKSI PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL FAKTUR PAJAK KOREKSI PPH", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "KODE SUPPLIER", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "ALAMAT SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR SURAT PENGANTAR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR PO EXTERNAL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR PURCHASE REQUEST", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR ACCOUNT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH BARANG", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "SATUAN BARANG", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "HARGA SATUAN BARANG", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "MATA UANG", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "RATE", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "HARGA TOTAL BARANG", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR BON TERIMA UNIT", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL BON TERIMA UNIT", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "USER INPUT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PPH", DataType = typeof(String) });

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", 0, "", "", 0, "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                var index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string UPCDate = item.UPCDate == new DateTime(1970, 1, 1) ? "-" : item.UPCDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));
                    string InvoiceCorrectionDate = item.InvoiceCorrectionDate == DateTimeOffset.MinValue ? "-" : item.InvoiceCorrectionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));
                    string VatTaxCorrectionDate = item.VatTaxCorrectionDate == DateTimeOffset.MinValue ? "-" : item.VatTaxCorrectionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));
                    string IncomeTaxCorrectionDate = item.IncomeTaxCorrectionDate == DateTimeOffset.MinValue ? "-" : item.IncomeTaxCorrectionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));
                    string URNDate = item.URNDate == null ? "-" : item.URNDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(item.UPCNo, UPCDate, item.CorrectionType, item.UPONo, item.InvoiceCorrectionNo, InvoiceCorrectionDate, item.VatTaxCorrectionNo, VatTaxCorrectionDate, item.IncomeTaxCorrectionNo,
                                    IncomeTaxCorrectionDate, item.SupplierCode, item.SupplierName, item.SupplierAddress, item.ReleaseOrderNoteNo, item.UPCRemark, item.EPONo, item.PRNo, item.AccountNo, item.ProductCode,
                                    item.ProductName, item.Quantity, item.UOMUnit, item.PricePerDealUnit, item.CurrencyCode, item.CurrencyRate, item.PriceTotal, item.URNNo, URNDate, item.UserCreated, item.UseVat, item.UseIncomeTax);
                }
            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Sheet1") }, true);
        }

        private async Task<int> AddCorrections(UnitPaymentCorrectionNote model, string username)
        {
            var internalPOFacade = serviceProvider.GetService<InternalPurchaseOrderFacade>();
            int count = 0;
            foreach (var item in model.Items)
            {

                var fulfillment = await dbContext.InternalPurchaseOrderFulfillments.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UnitPaymentOrderId == model.UPOId && x.UnitPaymentOrderDetailId == item.UPODetailId);

                if(fulfillment != null)
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

        
    }
}
