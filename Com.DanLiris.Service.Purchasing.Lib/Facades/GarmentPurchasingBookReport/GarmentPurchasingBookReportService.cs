using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public class GarmentPurchasingBookReportService : IGarmentPurchasingBookReportService
    {
        private readonly PurchasingDbContext _dbContext;

        public GarmentPurchasingBookReportService(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
        }

        public List<BillNoPaymentBillAutoCompleteDto> GetBillNos(string keyword)
        {
            var query = _dbContext.GarmentDeliveryOrders.Select(entity => entity.BillNo).Distinct();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(entity => entity.Contains(keyword));

            return query.Take(10).Select(entity => new BillNoPaymentBillAutoCompleteDto(entity)).ToList();
        }

        public List<BillNoPaymentBillAutoCompleteDto> GetPaymentBills(string keyword)
        {
            var query = _dbContext.GarmentDeliveryOrders.Select(entity => entity.PaymentBill).Distinct();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(entity => entity.Contains(keyword));

            return query.Take(10).Select(entity => new BillNoPaymentBillAutoCompleteDto(entity)).ToList();
        }

        public ReportDto GetReport(string billNo, string paymentBill, string garmentCategory, DateTimeOffset startDate, DateTimeOffset endDate, bool isForeignCurrency, bool isImportSupplier)
        {
            var query = from garmentDeliveryOrders in _dbContext.GarmentDeliveryOrders.Include(entity => entity.Items).ThenInclude(entity => entity.Details).AsQueryable()

                        join customsItems in _dbContext.GarmentBeacukaiItems.AsQueryable() on garmentDeliveryOrders.Id equals customsItems.GarmentDOId into doCustoms
                        from deliveryOrderCustoms in doCustoms.DefaultIfEmpty()

                        join garmentDeliveryOrderItems in _dbContext.GarmentDeliveryOrderItems.AsQueryable() on garmentDeliveryOrders.Id equals garmentDeliveryOrderItems.GarmentDOId into garmentDOItems
                        from deliveryOrderItems in garmentDOItems.DefaultIfEmpty()

                            //join garmentDeliveryOrderDetails in _dbContext.GarmentDeliveryOrderDetails.AsQueryable() on deliveryOrderItems.Id equals garmentDeliveryOrderDetails.GarmentDOItemId into garmentDODetails
                            //from deliveryOrderDetails in garmentDODetails.DefaultIfEmpty()

                        join invoiceItems in _dbContext.GarmentInvoiceItems.AsQueryable() on garmentDeliveryOrders.Id equals invoiceItems.DeliveryOrderId into garmentDOInvoiceItems
                        from deliveryOrderInvoiceItems in garmentDOInvoiceItems.DefaultIfEmpty()

                        join invoices in _dbContext.GarmentInvoices.AsQueryable() on deliveryOrderInvoiceItems.InvoiceId equals invoices.Id into garmentDOInvoices
                        from deliveryOrderInvoices in garmentDOInvoices.DefaultIfEmpty()

                        join garmentExternalPurchaseOrders in _dbContext.GarmentExternalPurchaseOrders.AsQueryable() on deliveryOrderItems.EPOId equals garmentExternalPurchaseOrders.Id into doExternalPurchaseOrders
                        from deliveryOrderExternalPurchaseOrders in doExternalPurchaseOrders.DefaultIfEmpty()

                        join internalNoteDetails in _dbContext.GarmentInternNoteDetails.AsQueryable() on garmentDeliveryOrders.Id equals internalNoteDetails.DOId into doInternalNoteDetails
                        from deliveryOrderInternalNoteDetails in doInternalNoteDetails.DefaultIfEmpty()

                        join internalNoteItems in _dbContext.GarmentInternNoteItems.AsQueryable() on deliveryOrderInternalNoteDetails.GarmentItemINId equals internalNoteItems.Id into doInternalNoteItems
                        from deliveryOrderInternalNoteItems in doInternalNoteItems.DefaultIfEmpty()

                        join internalNotes in _dbContext.GarmentInternNotes.AsQueryable() on deliveryOrderInternalNoteItems.GarmentINId equals internalNotes.Id into doInternalNotes
                        from deliveryOrderInternalNotes in doInternalNotes.DefaultIfEmpty()

                        select new ReportIndexDto(garmentDeliveryOrders, deliveryOrderCustoms, deliveryOrderItems, deliveryOrderInvoiceItems, deliveryOrderInvoices, deliveryOrderExternalPurchaseOrders, deliveryOrderInternalNoteDetails, deliveryOrderInternalNoteItems, deliveryOrderInternalNotes);

            query = query.Where(entity => entity.CustomsArrivalDate >= startDate && entity.CustomsArrivalDate <= endDate);

            if (!string.IsNullOrWhiteSpace(billNo))
            {
                query = query.Where(entity => entity.BillNo == billNo);
            }

            if (!string.IsNullOrWhiteSpace(paymentBill))
            {
                query = query.Where(entity => entity.PaymentBill == paymentBill);
            }

            if (!string.IsNullOrWhiteSpace(garmentCategory))
            {
                query = query.Where(entity => entity.PurchasingCategoryName == garmentCategory);
            }

            if (isImportSupplier)
            {
                query = query.Where(entity => entity.IsImportSupplier);
            }
            else
            {
                query = query.Where(entity => !entity.IsImportSupplier);
            }

            if (isForeignCurrency)
            {
                query = query.Where(entity => entity.CurrencyCode != "IDR");
            }

            var data = query.ToList();

            var reportCategories = data
                .GroupBy(element => element.AccountingCategoryName)
                .Select(element => new ReportCategoryDto(0, element.Key, element.Sum(sum => sum.Total)))
                .ToList();

            var reportCurrencies = data
                .GroupBy(element => element.CurrencyId)
                .Select(element => new ReportCurrencyDto(element.Key, element.FirstOrDefault(e => element.Key == e.CurrencyId).CurrencyCode, element.FirstOrDefault(e => element.Key == e.CurrencyId).CurrencyRate, element.Sum(sum => sum.Total)))
                .ToList();

            return new ReportDto(data, reportCategories, reportCurrencies);
        }
    }
}
