using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
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
using System.IO;
using System.Data;
using System.Globalization;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO
{
    public class PurchaseOrderMonitoringAllFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<InternalPurchaseOrder> dbSet;

        public PurchaseOrderMonitoringAllFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<InternalPurchaseOrder>();
        }

        public IQueryable<PurchaseOrderMonitoringAllViewModel> GetReportQuery(string prNo, string supplierId, string unitId, string categoryId, string budgetId, string epoNo, string staff, DateTime? dateFrom, DateTime? dateTo, string status, int offset, string user)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = (from a in dbContext.InternalPurchaseOrders
                         join b in dbContext.InternalPurchaseOrderItems on a.Id equals b.POId
                         //PR
                         join c in dbContext.PurchaseRequests on a.PRId equals c.Id.ToString()
                         join d in dbContext.PurchaseRequestItems on c.Id equals d.PurchaseRequestId
                         //EPO
                         join e in dbContext.ExternalPurchaseOrderItems on b.POId equals e.POId into f
                         from epoItem in f.DefaultIfEmpty()
                         join g in dbContext.ExternalPurchaseOrders on epoItem.EPOId equals g.Id into h
                         from epo in h.DefaultIfEmpty()
                         join k in dbContext.ExternalPurchaseOrderDetails on b.Id equals k.POItemId into l
                         from epoDetail in l.DefaultIfEmpty()
                             //DO
                         join m in dbContext.DeliveryOrderItems on epo.Id equals m.EPOId into n
                         from doItem in n.DefaultIfEmpty()
                         join o in dbContext.DeliveryOrders on doItem.DOId equals o.Id into p
                         from DO in p.DefaultIfEmpty()
                             //URN
                         join q in dbContext.UnitReceiptNotes on DO.Id equals q.DOId into r
                         from urn in r.DefaultIfEmpty()
                         join s in dbContext.UnitReceiptNoteItems on urn.Id equals s.URNId into t
                         from urnItem in t.DefaultIfEmpty()
                             //UPO
                         join u in dbContext.UnitPaymentOrderItems on urn.Id equals u.URNId into v
                         from upoItem in v.DefaultIfEmpty()
                         join w in dbContext.UnitPaymentOrders on upoItem.UPOId equals w.Id into x
                         from upo in x.DefaultIfEmpty()
                         join y in dbContext.UnitPaymentOrderDetails on upoItem.Id equals y.UPOItemId into z
                         from upoDetail in z.DefaultIfEmpty()
                             //Correction
                         join aa in dbContext.UnitPaymentCorrectionNotes on upo.Id equals aa.UPOId into bb
                         from corr in bb.DefaultIfEmpty()
                         join cc in dbContext.UnitPaymentCorrectionNoteItems on corr.Id equals cc.UPCId into dd
                         from corrItem in dd.DefaultIfEmpty()
                         where a.IsDeleted == false && b.IsDeleted == false
                             && c.IsDeleted == false && d.IsDeleted == false
                             && epo.IsDeleted == false && epoDetail.IsDeleted == false && epoItem.IsDeleted == false
                             && DO.IsDeleted == false && doItem.IsDeleted == false
                             && urn.IsDeleted == false && urnItem.IsDeleted == false
                             && upo.IsDeleted == false && upoItem.IsDeleted == false && upoDetail.IsDeleted == false
                             && corr.IsDeleted == false && corrItem.IsDeleted == false
                             && a.UnitId == (string.IsNullOrWhiteSpace(unitId) ? a.UnitId : unitId)
                             && a.PRNo == (string.IsNullOrWhiteSpace(prNo) ? a.PRNo : prNo)
                             && a.CategoryId == (string.IsNullOrWhiteSpace(categoryId) ? a.CategoryId : categoryId)
                             && a.BudgetId == (string.IsNullOrWhiteSpace(budgetId) ? a.BudgetId : budgetId)
                             && epo.SupplierId == (string.IsNullOrWhiteSpace(supplierId) ? epo.SupplierId : supplierId)
                             && epo.EPONo == (string.IsNullOrWhiteSpace(epoNo) ? epo.EPONo : epoNo)
                             && b.Status == (string.IsNullOrWhiteSpace(status) ? b.Status : status)
                             && a.CreatedBy == (string.IsNullOrWhiteSpace(staff) ? a.CreatedBy : staff)
                             && a.PRDate.AddHours(offset).Date >= DateFrom.Date
                             && a.PRDate.AddHours(offset).Date <= DateTo.Date
                             && b.Quantity > 0
                             && a.CreatedBy == (string.IsNullOrWhiteSpace(user) ? a.CreatedBy : user)
                         select new PurchaseOrderMonitoringAllViewModel
                         {
                             createdDatePR = c.CreatedUtc,
                             prNo = a.PRNo,
                             prDate = a.PRDate,
                             category = a.CategoryName,
                             budget = a.BudgetName,
                             productName = b.ProductName,
                             productCode = b.ProductCode,
                             quantity = b.Quantity,
                             uom = b.UomUnit,
                             pricePerDealUnit = epoDetail != null ? epoDetail.PricePerDealUnit : 0,
                             priceTotal = epoDetail != null ? epoDetail.DealQuantity * epoDetail.PricePerDealUnit : 0,
                             supplierCode = epo.SupplierCode ?? "-",
                             supplierName = epo.SupplierName ?? "-",
                             receivedDatePO = a.CreatedUtc,
                             epoDate = epo == null ? new DateTime(1970, 1, 1) : epo.OrderDate,
                             epoCreatedDate = epo == null ? new DateTime(1970, 1, 1) : epo.CreatedUtc,
                             epoExpectedDeliveryDate = a.ExpectedDeliveryDate,
                             epoDeliveryDate = epo == null ? new DateTime(1970, 1, 1) : epo.DeliveryDate,
                             epoNo = epo.EPONo ?? "-",
                             doDate = DO == null ? new DateTime(1970, 1, 1) : DO.DODate,
                             doDeliveryDate = DO == null ? new DateTime(1970, 1, 1) : DO.ArrivalDate,
                             doNo = DO.DONo ?? "-",
                             urnDate = urn == null ? new DateTime(1970, 1, 1) : urn.ReceiptDate,
                             urnNo = urn.URNNo ?? "-",
                             urnQuantity = urnItem == null ? 0 : urnItem.ReceiptQuantity,
                             urnUom = urnItem.Uom ?? "-",
                             paymentDueDays = epo.PaymentDueDays ?? "-",
                             invoiceDate = upo == null ? new DateTime(1970, 1, 1) : upo.InvoiceDate,
                             invoiceNo = upo.InvoiceNo ?? "-",
                             upoDate = upo == null ? new DateTime(1970, 1, 1) : upo.Date,
                             upoNo = upo.UPONo ?? "-",
                             upoPriceTotal = upoDetail == null ? 0 : upoDetail.PriceTotal,
                             dueDate = upo == null ? new DateTime(1970, 1, 1) : upo.DueDate,
                             vatDate = upo != null  ? upo.UseVat ? upo.VatDate : new DateTime(1970, 1, 1) : new DateTime(1970, 1, 1),
                             vatNo = upo.VatNo ?? "-",
                             vatValue = upo != null ? upo.UseVat ? 0.1 * upoDetail.PriceTotal : 0:0,
                             incomeTaxDate = upo == null && !upo.UseIncomeTax ? null : upo.IncomeTaxDate,
                             incomeTaxNo = upo.IncomeTaxNo ?? null,
                             incomeTaxValue = upo != null ? upo.UseIncomeTax ? upo.IncomeTaxRate * upoDetail.PriceTotal : 0 : 0,
                             correctionDate = corr == null ? new DateTime(1970, 1, 1) : corr.CorrectionDate,
                             correctionNo = corr.UPCNo ?? null,
                             correctionType = corr.CorrectionType ?? null,
                             valueCorrection = corrItem == null ? 0 : corr.CorrectionType=="Harga Total"? corrItem.PriceTotalAfter - corrItem.PriceTotalBefore : corr.CorrectionType == "Harga Satuan"? (corrItem.PricePerDealUnitAfter - corrItem.PricePerDealUnitBefore) * corrItem.Quantity: corr.CorrectionType == null? corrItem.PriceTotalAfter * -1 : 0,
                             priceAfter = corrItem == null ? 0 : corrItem.PricePerDealUnitAfter,
                             priceBefore = corrItem == null ? 0 : corrItem.PricePerDealUnitBefore,
                             priceTotalAfter = corrItem == null ? 0 : corrItem.PriceTotalAfter,
                             priceTotalBefore = corrItem == null ? 0 : corrItem.PricePerDealUnitBefore,
                             qtyCorrection = corrItem == null ? 0 : corrItem.Quantity,
                             remark = epoDetail.ProductRemark ?? "",
                             status = b.Status,
                             staff = a.CreatedBy,
                             LastModifiedUtc= a.LastModifiedUtc,
                         }).Distinct();
            

            return Query;
        }

        public Tuple<List<PurchaseOrderMonitoringAllViewModel>, int> GetReport(string prNo, string supplierId, string unitId, string categoryId, string budgetId, string epoNo, string staff, DateTime? dateFrom, DateTime? dateTo, string status, int page, int size, string Order, int offset, string user)
        {
            var Query = GetReportQuery(prNo, supplierId, unitId, categoryId,budgetId,epoNo,  staff, dateFrom, dateTo,status, offset,user);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            }


            Pageable<PurchaseOrderMonitoringAllViewModel> pageable = new Pageable<PurchaseOrderMonitoringAllViewModel>(Query, page - 1, size);
            List<PurchaseOrderMonitoringAllViewModel> Data = pageable.Data.ToList<PurchaseOrderMonitoringAllViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcel(string prNo, string supplierId, string unitId, string categoryId, string budgetId, string epoNo, string staff, DateTime? dateFrom, DateTime? dateTo, string status, int offset, string user)
        {
            var Query = GetReportQuery(prNo, supplierId, unitId, categoryId, budgetId, epoNo, staff, dateFrom, dateTo, status, offset, user);
            Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Purchase Request", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Pembuatan PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Purchase Request", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Budget", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Barang", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "Harga Barang", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Total", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Terima PO Internal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Terima PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Pembuatan PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Diminta Datang PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Target Datang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Surat Jalan", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Datang Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Bon Terima Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Bon Terima Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Diminta", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Diminta", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tempo Pembayaran", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Nota Intern", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "No Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai Nota Intern", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Jatuh Tempo", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPN", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPH", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Koreksi", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "No Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai Koreksi", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Status", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Staff Pembelian", DataType = typeof(String) });
            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "","", "", "", "", "", "", 0, "",     0, 0, "", "","", "", "", "", "", "", "",     "", "", "", "", 0, "", "", "", "", "",     "", 0, "", "", "", 0, "", "", 0, "",     "",0,"","","","" ); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string prDate = item.prDate == null ? "-" : item.prDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string prCreatedDate = item.createdDatePR == null ? "-" : item.createdDatePR.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string receiptDatePO = item.receivedDatePO == null ? "-" : item.receivedDatePO.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoDate = item.epoDate == new DateTime(1970, 1, 1) ? "-" : item.epoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoCreatedDate = item.epoCreatedDate == new DateTime(1970, 1, 1) ? "-" : item.epoCreatedDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoExpectedDeliveryDate = item.epoExpectedDeliveryDate == new DateTime(1970, 1, 1) ? "-" : item.epoExpectedDeliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoDeliveryDate = item.epoDeliveryDate == new DateTime(1970, 1, 1) ? "-" : item.epoDeliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    string doDate = item.doDate == new DateTime(1970, 1, 1) ? "-" : item.doDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string doDeliveryDate = item.doDeliveryDate == new DateTime(1970, 1, 1) ? "-" : item.doDeliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    string urnDate = item.urnDate == new DateTime(1970, 1, 1) ? "-" : item.urnDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string invoiceDate = item.invoiceDate == new DateTime(1970, 1, 1) ? "-" : item.invoiceDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string upoDate = item.upoDate == new DateTime(1970, 1, 1) ? "-" : item.upoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string dueDate = item.dueDate == new DateTime(1970, 1, 1) ? "-" : item.dueDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string vatDate = item.vatDate == new DateTime(1970, 1, 1) ? "-" : item.vatDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    DateTimeOffset date = item.incomeTaxDate ?? new DateTime(1970, 1, 1);
                    string incomeTaxDate = date == new DateTime(1970, 1, 1) ? "-" : date.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    string correctionDate = item.correctionDate == new DateTime(1970, 1, 1) ? "-" : item.correctionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, prDate, prCreatedDate, item.prNo, item.category, item.budget, item.productName,item.productCode, item.quantity,item.uom, 
                        item.pricePerDealUnit, item.priceTotal, item.supplierCode, item.supplierName, receiptDatePO, epoDate, epoCreatedDate, epoExpectedDeliveryDate, epoDeliveryDate,item.epoNo, doDate, 
                        doDeliveryDate,item.doNo, urnDate, item.urnNo, item.urnQuantity, item.urnUom, item.paymentDueDays, invoiceDate, item.invoiceNo, upoDate, 
                        item.upoNo, item.upoPriceTotal, dueDate , vatDate , item.vatNo, item.vatValue , incomeTaxDate,item.incomeTaxNo , item.incomeTaxValue, correctionDate, 
                        item.correctionNo, item.valueCorrection, item.correctionType, item.remark, item.status,item.staff);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
    }
}
