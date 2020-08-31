using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Clauses.ResultOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class VBRequestPOExternalService : IVBRequestPOExternalService
    {
        private readonly PurchasingDbContext _dbContext;

        public VBRequestPOExternalService(PurchasingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<POExternalDto> ReadPOExternal(string keyword, string division, string currencyCode)
        {
            var result = new List<POExternalDto>();

            if (!string.IsNullOrWhiteSpace(division) && division.ToUpper() == "GARMENT")
            {
                var query = _dbContext.GarmentExternalPurchaseOrders.Where(entity => entity.PaymentType == "CASH" && entity.IsPosted).Include(entity => entity.Items).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(entity => entity.EPONo.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(currencyCode))
                {
                    query = query.Where(entity => entity.CurrencyCode == currencyCode);
                }

                var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                var epoIdAndPOIds = queryResult.SelectMany(element => element.Items).Select(element => new EPOIdAndPOId() { EPOId = element.GarmentEPOId, POId = element.POId }).ToList();
                var poIds = epoIdAndPOIds.Select(element => element.POId).ToList();
                var purchaseOrders = _dbContext.GarmentInternalPurchaseOrders.Where(entity => poIds.Contains(entity.Id)).ToList();
                //var internalPOs = _dbContext

                result = queryResult.Select(entity => new POExternalDto(entity, purchaseOrders)).ToList();
            }
            else
            {
                var query = _dbContext.ExternalPurchaseOrders.Where(entity => entity.POCashType == "VB" && entity.IsPosted).Include(entity => entity.Items).ThenInclude(entity => entity.Details).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(entity => entity.EPONo.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(currencyCode))
                {
                    query = query.Where(entity => entity.CurrencyCode == currencyCode);
                }

                var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                result = queryResult.Select(entity => new POExternalDto(entity)).ToList();
            }

            return result;
        }

        public List<SPBDto> ReadSPB(string keyword, string division, List<int> epoIds)

        {
            var result = new List<SPBDto>();

            if (!string.IsNullOrWhiteSpace(division) && division.ToUpper() == "GARMENT")
            {
                if (epoIds.Count <= 0)
                {
                    epoIds = _dbContext.GarmentExternalPurchaseOrders.Where(entity => entity.PaymentType == "CASH" && entity.IsPosted).Select(entity => (int)entity.Id).ToList();
                }

                var internNoteItemIds = _dbContext.GarmentInternNoteDetails.Where(entity => epoIds.Contains((int)entity.EPOId)).Select(entity => entity.GarmentItemINId).ToList();
                var internNoteItems = _dbContext.GarmentInternNoteItems.Where(entity => internNoteItemIds.Contains(entity.Id)).ToList();
                var internNoteIds = internNoteItems.Select(entity => entity.Id).ToList();



                var query = _dbContext.GarmentInternNotes.Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => internNoteIds.Contains(entity.Id)).AsQueryable();
                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(entity => entity.INNo.Contains(keyword));

                var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                var invoiceIds = queryResult.SelectMany(element => element.Items).Select(element => element.InvoiceId).ToList();
                var invoices = _dbContext.GarmentInvoices.Where(entity => invoiceIds.Contains(entity.Id)).ToList();

                internNoteIds = queryResult.Select(element => element.Id).ToList();
                internNoteItems = _dbContext.GarmentInternNoteItems.Where(entity => internNoteIds.Contains(entity.GarmentINId)).ToList();
                internNoteItemIds = internNoteItems.Select(element => element.Id).ToList();
                var internNoteDetails = _dbContext.GarmentInternNoteDetails.Where(entity => internNoteItemIds.Contains(entity.GarmentItemINId)).ToList();

                result = queryResult.Select(element => new SPBDto(element, invoices, internNoteItems, internNoteDetails)).ToList();

            }
            else
            {
                if (epoIds.Count <= 0)
                {
                    epoIds = _dbContext.ExternalPurchaseOrders.Where(entity => entity.PaymentMethod == "CASH" && entity.POCashType == "VB" && entity.IsPosted).Select(entity => (int)entity.Id).ToList();
                }

                var epoItemIds = _dbContext.ExternalPurchaseOrderItems.Where(entity => epoIds.Contains((int)entity.EPOId)).Select(entity => entity.Id).ToList();
                var epoDetailIds = _dbContext.ExternalPurchaseOrderDetails.Where(entity => epoItemIds.Contains(entity.EPOItemId)).Select(entity => entity.Id).ToList();
                var spbItemIds = _dbContext.UnitPaymentOrderDetails.Where(entity => epoDetailIds.Contains(entity.EPODetailId)).Select(entity => entity.UPOItemId).ToList();
                var spbIds = _dbContext.UnitPaymentOrderItems.Where(entity => spbItemIds.Contains(entity.Id)).Select(entity => entity.UPOId).ToList();

                var query = _dbContext.UnitPaymentOrders.Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => spbIds.Contains(entity.Id)).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(entity => entity.UPONo.Contains(keyword));

                var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                spbIds = queryResult.Select(element => element.Id).ToList();
                var spbItems = _dbContext.UnitPaymentOrderItems.Where(entity => spbIds.Contains(entity.UPOId)).ToList();
                spbItemIds = spbItems.Select(element => element.Id).ToList();
                var spbDetails = _dbContext.UnitPaymentOrderDetails.Where(entity => spbItemIds.Contains(entity.UPOItemId)).ToList();
                var unitReceiptNoteItemIds = spbDetails.Select(element => element.URNItemId).ToList();
                var unitReceiptNoteItems = _dbContext.UnitReceiptNoteItems.Where(entity => unitReceiptNoteItemIds.Contains(entity.Id)).ToList();
                var unitReceiptNoteIds = unitReceiptNoteItems.Select(entity => entity.URNId).ToList();
                var unitReceiptNotes = _dbContext.UnitReceiptNotes.Where(entity => unitReceiptNoteIds.Contains(entity.Id)).ToList();

                result = queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList();
            }

            return result;
        }
    }

    public class EPOIdAndPOId
    {
        public long EPOId { get; set; }
        public long POId { get; set; }
    }
}
