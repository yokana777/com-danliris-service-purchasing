using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO
{
    public class PurchaseMonitoringService : IPurchaseMonitoringService
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly DbSet<PurchaseRequest> _purchaseRequestDbSet;
        private readonly DbSet<PurchaseRequestItem> _purchaseRequestItemDbSet;
        private readonly DbSet<InternalPurchaseOrder> _internalPurchaseOrderDbSet;
        private readonly DbSet<InternalPurchaseOrderItem> _internalPurchaseOrderItemDbSet;
        private readonly DbSet<InternalPurchaseOrderFulFillment> _internalPurchaseOrderFulfillmentDbSet;
        private readonly DbSet<ExternalPurchaseOrder> _externalPurchaseOrderDbSet;
        private readonly DbSet<ExternalPurchaseOrderItem> _externalPurchaseOrderItemDbSet;
        private readonly DbSet<ExternalPurchaseOrderDetail> _externalPurchaseOrderDetailDbSet;
        private readonly DbSet<DeliveryOrder> _deliveryOderDbSet;
        private readonly DbSet<DeliveryOrderItem> _deliveryOrderItemDbSet;
        private readonly DbSet<DeliveryOrderDetail> _deliveryOrderDetailDbSet;
        private readonly DbSet<UnitReceiptNote> _unitReceiptNoteDbSet;
        private readonly DbSet<UnitReceiptNoteItem> _unitReceiptNoteItemDbSet;
        private readonly DbSet<UnitPaymentOrder> _unitPaymentOrderDbSet;
        private readonly DbSet<UnitPaymentOrderItem> _unitPaymentOrderItemDbSet;
        private readonly DbSet<UnitPaymentOrderDetail> _unitPaymentOrderDetailDbSet;
        private readonly DbSet<UnitPaymentCorrectionNoteItem> _correctionItemDbSet;

        public PurchaseMonitoringService(PurchasingDbContext dbContext)
        {
            _dbContext = dbContext;
            //_dbContext.Database.SetCommandTimeout(1000 * 60 * 2);
            //if (_dbContext.Database.IsSqlServer())
            //    _dbContext.Database.SetCommandTimeout(1000 * 60 * 2);

            _purchaseRequestDbSet = dbContext.Set<PurchaseRequest>();
            _purchaseRequestItemDbSet = dbContext.Set<PurchaseRequestItem>();

            _internalPurchaseOrderDbSet = dbContext.Set<InternalPurchaseOrder>();
            _internalPurchaseOrderItemDbSet = dbContext.Set<InternalPurchaseOrderItem>();
            _internalPurchaseOrderFulfillmentDbSet = dbContext.Set<InternalPurchaseOrderFulFillment>();

            _externalPurchaseOrderDbSet = dbContext.Set<ExternalPurchaseOrder>();
            _externalPurchaseOrderItemDbSet = dbContext.Set<ExternalPurchaseOrderItem>();
            _externalPurchaseOrderDetailDbSet = dbContext.Set<ExternalPurchaseOrderDetail>();

            _deliveryOderDbSet = dbContext.Set<DeliveryOrder>();
            _deliveryOrderItemDbSet = dbContext.Set<DeliveryOrderItem>();
            _deliveryOrderDetailDbSet = dbContext.Set<DeliveryOrderDetail>();

            _unitReceiptNoteDbSet = dbContext.Set<UnitReceiptNote>();
            _unitReceiptNoteItemDbSet = dbContext.Set<UnitReceiptNoteItem>();

            _unitPaymentOrderDbSet = dbContext.Set<UnitPaymentOrder>();
            _unitPaymentOrderItemDbSet = dbContext.Set<UnitPaymentOrderItem>();
            _unitPaymentOrderDetailDbSet = dbContext.Set<UnitPaymentOrderDetail>();

            _correctionItemDbSet = dbContext.Set<UnitPaymentCorrectionNoteItem>();
        }

        public IQueryable<PurchaseMonitoringReportViewModel> GetReportQuery(string unitId, string categoryId, string divisionId, string budgetId, long prId, string createdBy, string status, DateTimeOffset startDate, DateTimeOffset endDate, long poExtId, string supplierId)
        {
            var purchaseRequestItems = _purchaseRequestItemDbSet.Include(prItem => prItem.PurchaseRequest).Where(w => w.PurchaseRequest.Date >= startDate && w.PurchaseRequest.Date <= endDate);
            purchaseRequestItems = FilterPurchaseRequest(unitId, categoryId, divisionId, budgetId, prId, purchaseRequestItems);

            var internalPurchaseOrderFulfillments = _internalPurchaseOrderFulfillmentDbSet.AsQueryable();
            var internalPurchaseOrderItems = _internalPurchaseOrderItemDbSet.Include(ipoItem => ipoItem.InternalPurchaseOrder).AsQueryable();
            var externalPurchaseOrderDetails = _externalPurchaseOrderDetailDbSet.Include(epoDetail => epoDetail.ExternalPurchaseOrderItem).ThenInclude(epoItem => epoItem.ExternalPurchaseOrder).AsQueryable();
            //var deliveryOrderDetails = _deliveryOrderDetailDbSet.Include(doDetail => doDetail.DeliveryOrderItem).ThenInclude(doItem => doItem.DeliveryOrder).AsQueryable();
            //var unitReceiptNoteItems = _unitReceiptNoteItemDbSet.Include(urnItem => urnItem.UnitReceiptNote).AsQueryable();
            //var unitPaymentOrderDetails = _unitPaymentOrderDetailDbSet.Include(upoDetail => upoDetail.UnitPaymentOrderItem).ThenInclude(upoItem => upoItem.UnitPaymentOrder).AsQueryable();

            var query = (from purchaseRequestItem in purchaseRequestItems

                         join internalPurchaseOrderItem in internalPurchaseOrderItems on new { PRNo = purchaseRequestItem.PurchaseRequest.No, PRItemId = purchaseRequestItem.Id } equals new { internalPurchaseOrderItem.InternalPurchaseOrder.PRNo, internalPurchaseOrderItem.PRItemId } into joinIPO
                         from ipoItem in joinIPO.DefaultIfEmpty()

                         join externalPurchaseOrderDetail in externalPurchaseOrderDetails on new { ipoItem.POId, POItemId = ipoItem.Id } equals new { externalPurchaseOrderDetail.ExternalPurchaseOrderItem.POId, externalPurchaseOrderDetail.POItemId } into joinEPO
                         from epoDetail in joinEPO.DefaultIfEmpty()

                         join internalPOFulfillment in internalPurchaseOrderFulfillments on ipoItem.Id equals internalPOFulfillment.POItemId into joinFulfillment
                         from ipoFulfillment in joinFulfillment.DefaultIfEmpty()

                         select new PurchaseMonitoringReportViewModel()
                         {
                             PurchaseRequestId = purchaseRequestItem.PurchaseRequestId,
                             PurchaseRequestNo = purchaseRequestItem.PurchaseRequest.No,
                             PurchaseRequestDate = purchaseRequestItem.PurchaseRequest.Date.Date,
                             PurchaseRequestCreatedDate = purchaseRequestItem.PurchaseRequest.CreatedUtc,
                             CategoryId = purchaseRequestItem.PurchaseRequest.CategoryId,
                             CategoryName = purchaseRequestItem.PurchaseRequest.CategoryName,
                             DivisionId = purchaseRequestItem.PurchaseRequest.DivisionId,
                             DivisionName = purchaseRequestItem.PurchaseRequest.DivisionName,
                             BudgetId = purchaseRequestItem.PurchaseRequest.BudgetId,
                             BudgetName = purchaseRequestItem.PurchaseRequest.BudgetName,
                             PurchaseRequestItemId = purchaseRequestItem.Id,
                             ProductId = purchaseRequestItem.ProductId,
                             ProductName = purchaseRequestItem.ProductName,
                             ProductCode = purchaseRequestItem.ProductCode,
                             OrderQuantity = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.DealQuantity : 0,
                             UOMId = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.DealUomId : "",
                             UOMUnit = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.DealUomUnit : "-",
                             Price = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.PricePerDealUnit : 0,
                             PriceTotal = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.PricePerDealUnit * epoDetail.DealQuantity : 0,
                             CurrencyId = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyId : "",
                             CurrencyCode = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyCode : "",
                             CurrencyRate = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyRate : 0,
                             InternalPurchaseOrderId = ipoItem == null ? 0 : ipoItem.POId,
                             InternalPurchaseOrderNo = ipoItem == null ? "" : ipoItem.InternalPurchaseOrder.PONo,
                             InternalPurchaseOrderCreatedDate = ipoItem == null ? (DateTime?)null : ipoItem.InternalPurchaseOrder.CreatedUtc,
                             InternalPurchaseOrderLastModifiedDate = ipoItem == null ? (DateTime?)null : ipoItem.InternalPurchaseOrder.LastModifiedUtc,
                             InternalPurchaseOrderStaff = ipoItem != null ? ipoItem.InternalPurchaseOrder.CreatedBy : "",
                             InternalPurchaseOrderItemId = ipoItem == null ? 0 : ipoItem.Id,
                             InternalPurchaseOrderStatus = ipoItem != null ? ipoItem.Status : "",
                             ExternalPurchaseOrderId = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.EPOId : 0,
                             ExternalPurchaseOrderCreatedDate = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CreatedUtc : (DateTime?)null,
                             ExternalPurchaseOrderDate = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.OrderDate.Date : (DateTimeOffset?)null,
                             ExternalPurchaseOrderExpectedDeliveryDate = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted && ipoItem != null ? ipoItem.InternalPurchaseOrder.ExpectedDeliveryDate.Date : (DateTimeOffset?)null,
                             ExternalPurchaseOrderDeliveryDate = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted && ipoItem != null ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.DeliveryDate.Date : (DateTimeOffset?)null,
                             ExternalPurchaseOrderNo = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.EPONo : "",
                             SupplierId = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.SupplierId : "",
                             SupplierCode = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.SupplierCode : "",
                             SupplierName = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.SupplierName : "",
                             ExternalPurchaseOrderPaymentDueDays = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.PaymentDueDays : "",
                             ExternalPurchaseOrderRemark = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.Remark : "",
                             ExternalPurchaseOrderItemId = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.ExternalPurchaseOrderItem.Id : 0,
                             ExternalPurchaseOrderDetailId = epoDetail != null && epoDetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IsPosted ? epoDetail.Id : 0,
                             DeliveryOrderId = ipoFulfillment == null ? 0 : ipoFulfillment.DeliveryOrderId,
                             DeliveryOrderDate = ipoFulfillment != null ? ipoFulfillment.SupplierDODate.Date : (DateTimeOffset?)null,
                             DeliveryOrderArrivalDate = ipoFulfillment != null ? ipoFulfillment.DeliveryOrderDate.Date : (DateTimeOffset?)null,
                             DeliveryOrderNo = ipoFulfillment != null ? ipoFulfillment.DeliveryOrderNo : "",
                             DeliveryOrderItemId = ipoFulfillment == null ? 0 : ipoFulfillment.DeliveryOrderItemId,
                             DelveryOrderDetailId = ipoFulfillment == null ? 0 : ipoFulfillment.DeliveryOrderDetailId,
                             UnitReceiptNoteId = ipoFulfillment == null ? 0 : ipoFulfillment.UnitReceiptNoteId,
                             UnitReceiptNoteDate = ipoFulfillment != null && ipoFulfillment.UnitReceiptNoteDate != DateTimeOffset.MinValue ? ipoFulfillment.UnitReceiptNoteDate.Date : (DateTimeOffset?)null,
                             UnitReceiptNoteNo = ipoFulfillment != null ? ipoFulfillment.UnitReceiptNoteNo : "",
                             UnitReceiptNoteItemId = ipoFulfillment != null ? ipoFulfillment.UnitReceiptNoteItemId : 0,
                             UnitReceiptNoteQuantity = ipoFulfillment != null ? ipoFulfillment.UnitReceiptNoteDeliveredQuantity : 0,
                             UnitReceiptNoteUomId = ipoFulfillment != null ? ipoFulfillment.UnitReceiptNoteUomId : "",
                             UnitReceiptNoteUomUnit = ipoFulfillment != null ? ipoFulfillment.UnitReceiptNoteUom : "",
                             UnitPaymentOrderId = ipoFulfillment == null ? 0 : ipoFulfillment.UnitPaymentOrderId,
                             UnitPaymentOrderInvoiceDate = ipoFulfillment != null && ipoFulfillment.InvoiceDate != DateTimeOffset.MinValue ? ipoFulfillment.InvoiceDate : (DateTimeOffset?)null,
                             UnitPaymentOrderInvoiceNo = ipoFulfillment != null ? ipoFulfillment.InvoiceNo : "",
                             UnitPaymentOrderDate = ipoFulfillment != null && ipoFulfillment.InterNoteDate != DateTimeOffset.MinValue ? ipoFulfillment.InterNoteDate : (DateTimeOffset?)null,
                             UnitPaymentOrderNo = ipoFulfillment != null ? ipoFulfillment.InterNoteNo : "",
                             UnitPaymentOrderDueDate = ipoFulfillment != null && ipoFulfillment.InterNoteDueDate != DateTimeOffset.MinValue ? ipoFulfillment.InterNoteDueDate : (DateTimeOffset?)null,
                             UnitPaymentOrderItemId = ipoFulfillment == null ? 0 : ipoFulfillment.UnitPaymentOrderItemId,
                             UnitPaymentOrderDetailId = ipoFulfillment == null ? 0 : ipoFulfillment.UnitPaymentOrderDetailId,
                             UnitPaymentOrderTotalPrice = ipoFulfillment != null ? ipoFulfillment.InterNoteValue : 0,
                             UnitPaymentOrderVATDate = ipoFulfillment != null && ipoFulfillment.UnitPaymentOrderUseVat && ipoFulfillment.UnitPaymentOrderVatDate!= DateTimeOffset.MinValue ? ipoFulfillment.UnitPaymentOrderVatDate : (DateTimeOffset?)null,
                             UnitPaymentOrderVATNo = ipoFulfillment != null && ipoFulfillment.UnitPaymentOrderUseVat ? ipoFulfillment.UnitPaymentOrderVatNo : "",
                             UnitPaymentOrderVAT = ipoFulfillment != null && ipoFulfillment.UnitPaymentOrderUseVat ? 0.1 * ipoFulfillment.InterNoteValue : 0,
                             UnitPaymentOrderIncomeTaxDate = ipoFulfillment != null && ipoFulfillment.UnitPaymentOrderUseIncomeTax && ipoFulfillment.UnitPaymentOrderIncomeTaxDate != DateTimeOffset.MinValue ? ipoFulfillment.UnitPaymentOrderIncomeTaxDate : (DateTimeOffset?)null,
                             UnitPaymentOrderIncomeTaxNo = ipoFulfillment != null && ipoFulfillment.UnitPaymentOrderUseIncomeTax ? ipoFulfillment.UnitPaymentOrderIncomeTaxNo : "",
                             UnitPaymentOrderIncomeTax = ipoFulfillment != null && ipoFulfillment.UnitPaymentOrderUseIncomeTax ? ipoFulfillment.UnitPaymentOrderIncomeTaxRate * ipoFulfillment.InterNoteValue : 0
                         });

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(w => w.InternalPurchaseOrderStaff == createdBy);

            if (poExtId > 0)
                query = query.Where(w => w.ExternalPurchaseOrderId == poExtId);

            if (!string.IsNullOrWhiteSpace(supplierId))
                query = query.Where(w => w.SupplierId.Equals(supplierId));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(w => w.InternalPurchaseOrderStatus.Equals(status));

            return query;
        }

        public async Task<List<PurchaseMonitoringReportViewModel>> MapCorrections(List<PurchaseMonitoringReportViewModel> data)
        {
            var upoDetailIds = data.Select(datum => datum.UnitPaymentOrderDetailId).ToList();
            var corrections = await _correctionItemDbSet
                .Include(item => item.UnitPaymentCorrectionNote)
                .Where(item => upoDetailIds.Contains(item.UPODetailId))
                .Select(item => new
                {
                    item.UnitPaymentCorrectionNote.UPCNo,
                    item.UnitPaymentCorrectionNote.CorrectionType,
                    item.UnitPaymentCorrectionNote.CorrectionDate,
                    item.PriceTotalAfter,
                    item.PriceTotalBefore,
                    item.Quantity,
                    item.UPODetailId
                }).ToListAsync();

            //var result = new List<PurchaseMonitoringReportViewModel>();

            foreach (var correction in corrections)
            {
                var datum = data.FirstOrDefault(f => f.UnitPaymentOrderDetailId == correction.UPODetailId);

                if (datum != null)
                {
                    datum.CorrectionDate += $"- {correction.CorrectionDate.ToString("dd MMMM yyyy")}\n";
                    datum.CorrectionNo += $"- {correction.UPCNo}\n";
                    datum.CorrectionType += $"- {correction.CorrectionType}\n";

                    switch (correction.CorrectionType)
                    {
                        case "Harga Total":
                            datum.CorrectionNominal += $"- {string.Format("{0:N2}", correction.PriceTotalAfter - correction.PriceTotalBefore)}\n";
                            break;
                        case "Harga Satuan":
                            datum.CorrectionNominal += $"- {string.Format("{0:N2}", (correction.PriceTotalAfter - correction.PriceTotalBefore) * correction.Quantity)}\n";
                            break;
                        case "Jumlah":
                            datum.CorrectionNominal += $"- {string.Format("{0:N2}", correction.PriceTotalAfter)}\n";
                            break;
                        default:
                            break;
                    }
                }
            }

            return data;

        }

        public async Task<ReportFormatter> GetReport(string unitId, string categoryId, string divisionId, string budgetId, long prId, string createdBy, string status, DateTimeOffset startDate, DateTimeOffset endDate, long poExtId, string supplierId, int page, int size)
        {
            var query = GetReportQuery(unitId, categoryId, divisionId, budgetId, prId, createdBy, status, startDate, endDate, poExtId, supplierId);

            var result = await query.OrderByDescending(order => order.InternalPurchaseOrderLastModifiedDate).Skip((page - 1) * size).Take(size).ToListAsync();
            result = await MapCorrections(result);

            return new ReportFormatter()
            {
                Data = result,
                Total = await query.Select(s => s.PurchaseRequestId).CountAsync()
            };
        }

        private IQueryable<PurchaseRequestItem> FilterPurchaseRequest(string unitId, string categoryId, string divisionId, string budgetId, long prId, IQueryable<PurchaseRequestItem> purchaseRequestItems)
        {
            if (!string.IsNullOrWhiteSpace(unitId))
                purchaseRequestItems = purchaseRequestItems.Where(w => w.PurchaseRequest.UnitId.Equals(unitId));

            if (!string.IsNullOrWhiteSpace(categoryId))
                purchaseRequestItems = purchaseRequestItems.Where(w => w.PurchaseRequest.CategoryId.Equals(categoryId));

            if (!string.IsNullOrWhiteSpace(divisionId))
                purchaseRequestItems = purchaseRequestItems.Where(w => w.PurchaseRequest.DivisionId.Equals(divisionId));

            if (!string.IsNullOrWhiteSpace(budgetId))
                purchaseRequestItems = purchaseRequestItems.Where(w => w.PurchaseRequest.BudgetId.Equals(budgetId));

            if (prId > 0)
                purchaseRequestItems = purchaseRequestItems.Where(w => w.PurchaseRequest.Id.Equals(prId));

            //if (!string.IsNullOrWhiteSpace(createdBy))
            //    purchaseRequestItems = purchaseRequestItems.Where(w => w.PurchaseRequest.CreatedBy.Equals(createdBy));

            return purchaseRequestItems;
        }

        public async Task<MemoryStream> GenerateExcel(string unitId, string categoryId, string divisionId, string budgetId, long prId, string createdBy, string status, DateTimeOffset startDate, DateTimeOffset endDate, long poExtId, string supplierId, int timezoneOffset)
        {

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Purchase Request", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Pembuatan PR", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Purchase Request", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Divisi", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Budget", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Barang", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(string) });

            result.Columns.Add(new DataColumn() { ColumnName = "Harga Barang", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Total", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Terima PO Internal", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Terima PO Eksternal", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Pembuatan PO Eksternal", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Diminta Datang PO Eksternal", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Target Datang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO Eksternal", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Surat Jalan", DataType = typeof(string) });

            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Datang Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Bon Terima Unit", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Bon Terima Unit", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Diminta", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Diminta", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tempo Pembayaran", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Invoice", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Nota Intern", DataType = typeof(string) });

            result.Columns.Add(new DataColumn() { ColumnName = "No Nota Intern", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai Nota Intern", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Jatuh Tempo", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal PPN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PPN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal PPH", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PPH", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPH", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Koreksi", DataType = typeof(string) });

            result.Columns.Add(new DataColumn() { ColumnName = "No Koreksi", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai Koreksi", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Koreksi", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Status", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Staff Pembelian", DataType = typeof(string) });

            var query = GetReportQuery(unitId, categoryId, divisionId, budgetId, prId, createdBy, status, startDate, endDate, poExtId, supplierId);
            var queryResult = await query.ToListAsync();

            if (queryResult.Count == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", 0, "", 0, 0, "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", "", "", "", "", "", 0, "", "", "", "", "", "", 0, "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                var upoDetailIds = queryResult.Select(item => item.UnitPaymentOrderDetailId).ToList();
                var corrections = await _correctionItemDbSet
                    .Include(item => item.UnitPaymentCorrectionNote)
                    .Where(item => upoDetailIds.Contains(item.UPODetailId))
                    .Select(item => new
                    {
                        item.UnitPaymentCorrectionNote.UPCNo,
                        item.UnitPaymentCorrectionNote.CorrectionType,
                        item.UnitPaymentCorrectionNote.CorrectionDate,
                        item.PriceTotalAfter,
                        item.PriceTotalBefore,
                        item.Quantity,
                        item.UPODetailId
                    }).ToListAsync();

                int index = 0;
                foreach (var item in queryResult)
                {
                    index++;

                    var selectedCorrections = corrections.Where(correction => correction.UPODetailId == item.UnitPaymentOrderDetailId).ToList();

                    item.CorrectionDate = string.Join("\n", selectedCorrections.Select(correction => $"- {correction.CorrectionDate.ToString("dd MMMM yyyy")}"));
                    item.CorrectionNo = string.Join("\n", selectedCorrections.Select(correction => $"- {correction.UPCNo}"));
                    item.CorrectionType = string.Join("\n", selectedCorrections.Select(correction => $"- {correction.CorrectionType}"));
                    item.CorrectionNominal = string.Join("\n", selectedCorrections.Select(correction =>
                    {
                        var nominalResult = "";
                        switch (correction.CorrectionType)
                        {
                            case "Harga Total":
                                nominalResult = $"- {string.Format("{0:N2}", correction.PriceTotalAfter - correction.PriceTotalBefore)}";
                                break;
                            case "Harga Satuan":
                                nominalResult = $"- {string.Format("{0:N2}", (correction.PriceTotalAfter - correction.PriceTotalBefore) * correction.Quantity)}";
                                break;
                            case "Jumlah":
                                nominalResult = $"- {string.Format("{0:N2}", correction.PriceTotalAfter)}";
                                break;
                            default:
                                break;
                        }
                        return nominalResult;
                    }));
                    //string prDate = item.prDate == null ? "-" : item.prDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string prCreatedDate = item.createdDatePR == null ? "-" : item.createdDatePR.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string receiptDatePO = item.receivedDatePO == null ? "-" : item.receivedDatePO.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string epoDate = item.epoDate == new DateTime(1970, 1, 1) ? "-" : item.epoDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string epoCreatedDate = item.epoCreatedDate == new DateTime(1970, 1, 1) ? "-" : item.epoCreatedDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string epoExpectedDeliveryDate = item.epoExpectedDeliveryDate == new DateTime(1970, 1, 1) ? "-" : item.epoExpectedDeliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string epoDeliveryDate = item.epoDeliveryDate == new DateTime(1970, 1, 1) ? "-" : item.epoDeliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));

                    //string doDate = item.doDate == new DateTime(1970, 1, 1) ? "-" : item.doDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string doDeliveryDate = item.doDeliveryDate == new DateTime(1970, 1, 1) ? "-" : item.doDeliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));

                    //string urnDate = item.urnDate == new DateTime(1970, 1, 1) ? "-" : item.urnDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string invoiceDate = item.invoiceDate == new DateTime(1970, 1, 1) ? "-" : item.invoiceDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string upoDate = item.upoDate == new DateTime(1970, 1, 1) ? "-" : item.upoDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string dueDate = item.dueDate == new DateTime(1970, 1, 1) ? "-" : item.dueDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string vatDate = item.vatDate == new DateTime(1970, 1, 1) ? "-" : item.vatDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));

                    //string correctionDate = item.correctionDate == new DateTime(1970, 1, 1) ? "-" : item.correctionDate.ToOffset(new TimeSpan(offset, 0, 0)).Tostring("dd MMM yyyy", new CultureInfo("id-ID"));

                    var internalPurchaseOrderCreatedDate = item.InternalPurchaseOrderCreatedDate.HasValue ? item.InternalPurchaseOrderCreatedDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var externalPurchaseOrderDate = item.ExternalPurchaseOrderDate.HasValue ? item.ExternalPurchaseOrderDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var externalPurchaseOrderCreatedDate = item.ExternalPurchaseOrderCreatedDate.HasValue ? item.ExternalPurchaseOrderCreatedDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var externalPurchaseOrderExpectedDeliveryDate = item.ExternalPurchaseOrderExpectedDeliveryDate.HasValue ? item.ExternalPurchaseOrderExpectedDeliveryDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var externalPurchaseOrderDeliveryDate = item.ExternalPurchaseOrderDeliveryDate.HasValue ? item.ExternalPurchaseOrderDeliveryDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var deliveryOrderDate = item.DeliveryOrderDate.HasValue ? item.DeliveryOrderDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var deliveryOrderArrivalDate = item.DeliveryOrderArrivalDate.HasValue ? item.DeliveryOrderArrivalDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var unitReceiptNoteDate = item.UnitReceiptNoteDate.HasValue ? item.UnitReceiptNoteDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var unitPaymentOrderDate = item.UnitPaymentOrderDate.HasValue ? item.UnitPaymentOrderDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var unitPaymentOrderDueDate = item.UnitPaymentOrderDueDate.HasValue ? item.UnitPaymentOrderDueDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var unitPaymentOrderVATDate = item.UnitPaymentOrderVATDate.HasValue ? item.UnitPaymentOrderVATDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";
                    var unitPaymentOrderIncomeTaxDate = item.UnitPaymentOrderIncomeTaxDate.HasValue ? item.UnitPaymentOrderIncomeTaxDate.Value.AddHours(timezoneOffset).ToString("dd MMMM yyyy") : "";


                    result.Rows.Add(index.ToString(), item.PurchaseRequestDate.ToString("dd MMMM yyyy"), item.PurchaseRequestCreatedDate.ToString("dd MMMM yyyy"), item.PurchaseRequestNo, item.CategoryName, item.DivisionName, item.BudgetName, item.ProductName, item.ProductCode, item.OrderQuantity, item.UOMUnit,
                        item.Price, item.PriceTotal, item.CurrencyCode, item.SupplierCode, item.SupplierName, internalPurchaseOrderCreatedDate, externalPurchaseOrderDate, externalPurchaseOrderCreatedDate, externalPurchaseOrderExpectedDeliveryDate, externalPurchaseOrderDeliveryDate, item.ExternalPurchaseOrderNo, deliveryOrderDate,
                        deliveryOrderArrivalDate, item.DeliveryOrderNo, unitReceiptNoteDate, item.UnitReceiptNoteNo, item.UnitReceiptNoteQuantity, item.UnitReceiptNoteUomUnit, item.ExternalPurchaseOrderPaymentDueDays, item.UnitPaymentOrderInvoiceDate, item.UnitPaymentOrderInvoiceNo, unitPaymentOrderDate,
                        item.UnitPaymentOrderNo, item.UnitPaymentOrderTotalPrice, unitPaymentOrderDueDate, unitPaymentOrderVATDate, item.UnitPaymentOrderVATNo, item.UnitPaymentOrderVAT, unitPaymentOrderIncomeTaxDate, item.UnitPaymentOrderIncomeTaxNo, item.UnitPaymentOrderIncomeTax, item.CorrectionDate,
                        item.CorrectionNo, item.CorrectionNominal, item.CorrectionType, item.ExternalPurchaseOrderRemark, item.InternalPurchaseOrderStatus, item.InternalPurchaseOrderStaff);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "sheet 1") }, true);
        }

    }


    public interface IPurchaseMonitoringService
    {
        Task<ReportFormatter> GetReport(string unitId, string categoryId, string divisionId, string budgetId, long prId, string createdBy, string status, DateTimeOffset startDate, DateTimeOffset endDate, long poExtId, string supplierId, int page, int size);
        Task<MemoryStream> GenerateExcel(string unitId, string categoryId, string divisionId, string budgetId, long prId, string createdBy, string status, DateTimeOffset startDate, DateTimeOffset endDate, long poExtId, string supplierId, int timezoneOffset);
    }

    public class ReportFormatter
    {
        public ReportFormatter()
        {

        }

        public List<PurchaseMonitoringReportViewModel> Data { get; set; }
        public int Total { get; set; }
    }

    public class PurchaseMonitoringReportViewModel
    {
        public PurchaseMonitoringReportViewModel()
        {
            CorrectionDate = "";
            CorrectionNo = "";
            CorrectionNominal = "";
            CorrectionType = "";
        }

        public long PurchaseRequestId { get; set; }
        public string PurchaseRequestNo { get; set; }
        public DateTimeOffset PurchaseRequestDate { get; set; }
        public DateTime PurchaseRequestCreatedDate { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }
        public long PurchaseRequestItemId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public double OrderQuantity { get; set; }
        public string UOMId { get; set; }
        public string UOMUnit { get; set; }
        public double Price { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public double CurrencyRate { get; set; }
        public long InternalPurchaseOrderId { get; set; }
        public DateTime? InternalPurchaseOrderCreatedDate { get; set; }
        public DateTime? InternalPurchaseOrderLastModifiedDate { get; set; }
        public string InternalPurchaseOrderStaff { get; set; }
        public long InternalPurchaseOrderItemId { get; set; }
        public string InternalPurchaseOrderStatus { get; set; }
        public long ExternalPurchaseOrderId { get; set; }
        public DateTime? ExternalPurchaseOrderCreatedDate { get; set; }
        public DateTimeOffset? ExternalPurchaseOrderDate { get; set; }
        public DateTimeOffset? ExternalPurchaseOrderExpectedDeliveryDate { get; set; }
        public string ExternalPurchaseOrderNo { get; set; }
        public string SupplierId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ExternalPurchaseOrderPaymentDueDays { get; set; }
        public string ExternalPurchaseOrderRemark { get; set; }
        public long ExternalPurchaseOrderItemId { get; set; }
        public long ExternalPurchaseOrderDetailId { get; set; }
        public long DeliveryOrderId { get; set; }
        public DateTimeOffset? DeliveryOrderDate { get; set; }
        public DateTimeOffset? DeliveryOrderArrivalDate { get; set; }
        public string DeliveryOrderNo { get; set; }
        public long DeliveryOrderItemId { get; set; }
        public long DelveryOrderDetailId { get; set; }
        public long UnitReceiptNoteId { get; set; }
        public DateTimeOffset? UnitReceiptNoteDate { get; set; }
        public string UnitReceiptNoteNo { get; set; }
        public long UnitReceiptNoteItemId { get; set; }
        public double UnitReceiptNoteQuantity { get; set; }
        public string UnitReceiptNoteUomId { get; set; }
        public string UnitReceiptNoteUomUnit { get; set; }
        public long UnitPaymentOrderId { get; set; }
        public DateTimeOffset? UnitPaymentOrderInvoiceDate { get; set; }
        public string UnitPaymentOrderInvoiceNo { get; set; }
        public DateTimeOffset? UnitPaymentOrderDate { get; set; }
        public string UnitPaymentOrderNo { get; set; }
        public DateTimeOffset? UnitPaymentOrderDueDate { get; set; }
        public long UnitPaymentOrderItemId { get; set; }
        public long UnitPaymentOrderDetailId { get; set; }
        public double UnitPaymentOrderTotalPrice { get; set; }
        public DateTimeOffset? UnitPaymentOrderVATDate { get; set; }
        public string UnitPaymentOrderVATNo { get; set; }
        public double UnitPaymentOrderVAT { get; set; }
        public DateTimeOffset? UnitPaymentOrderIncomeTaxDate { get; set; }
        public string UnitPaymentOrderIncomeTaxNo { get; set; }
        public double UnitPaymentOrderIncomeTax { get; set; }
        public string CorrectionDate { get; set; }
        public string CorrectionNo { get; set; }
        public string CorrectionType { get; set; }
        public string CorrectionNominal { get; set; }
        public double PriceTotal { get; internal set; }
        public DateTimeOffset? ExternalPurchaseOrderDeliveryDate { get; internal set; }
        public string InternalPurchaseOrderNo { get; internal set; }
    }


}
