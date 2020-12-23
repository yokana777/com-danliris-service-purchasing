using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.Moonlay.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingExpedition
{
    public class GarmentPurchasingExpeditionService : IGarmentPurchasingExpeditionService
    {
        private const string UserAgent = "purchasing-service";
        private readonly PurchasingDbContext _dbContext;
        private readonly IdentityService _identityService;

        public GarmentPurchasingExpeditionService(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        public List<GarmentInternalNoteDto> GetGarmentInternalNotes(string keyword)
        {
            var internalNoteQuery = _dbContext.GarmentInternNotes.Where(entity => entity.Position <= PurchasingGarmentExpeditionPosition.Purchasing || entity.Position == PurchasingGarmentExpeditionPosition.SendToPurchasing);

            if (!string.IsNullOrWhiteSpace(keyword))
                internalNoteQuery = internalNoteQuery.Where(entity => entity.INNo.Contains(keyword));

            var internalNotes = internalNoteQuery.Select(entity => new
            {
                entity.Id,
                entity.INNo,
                entity.INDate,
                entity.SupplierId,
                entity.SupplierName,
                entity.CurrencyCode,
                entity.CurrencyId
            }).Take(10).ToList();

            var internalNoteIds = internalNotes.Select(element => element.Id).ToList();
            var internalNoteItems = _dbContext.GarmentInternNoteItems.Where(entity => internalNoteIds.Contains(entity.GarmentINId)).Select(entity => new { entity.Id, entity.GarmentINId, entity.InvoiceId }).ToList();
            var internalNoteItemIds = internalNoteItems.Select(element => element.Id).ToList();
            var internalNoteDetails = _dbContext.GarmentInternNoteDetails.Where(entity => internalNoteItemIds.Contains(entity.GarmentItemINId)).Select(entity => new { entity.Id, entity.GarmentItemINId, entity.PaymentDueDate, entity.DOId }).ToList();

            var doIds = internalNoteDetails.Select(element => element.DOId).ToList();
            var corrections = _dbContext.GarmentCorrectionNotes.Where(entity => doIds.Contains(entity.DOId)).Select(entity => new { entity.Id, entity.TotalCorrection, entity.CorrectionType, entity.DOId });
            var correctionIds = corrections.Select(element => element.Id).ToList();
            var correctionItems = _dbContext.GarmentCorrectionNoteItems.Where(entity => correctionIds.Contains(entity.GCorrectionId)).Select(entity => new { entity.Id, entity.PricePerDealUnitAfter, entity.Quantity, entity.GCorrectionId });

            var invoiceIds = internalNoteItems.Select(element => element.InvoiceId).ToList();
            var invoices = _dbContext.GarmentInvoices.Where(entity => invoiceIds.Contains(entity.Id)).Select(entity => new { entity.Id, entity.IsPayTax, entity.UseIncomeTax, entity.UseVat, entity.IncomeTaxRate, entity.TotalAmount }).ToList();

            var result = internalNotes.Select(internalNote =>
            {
                var selectedInternalNoteItems = internalNoteItems.Where(element => element.GarmentINId == internalNote.Id).ToList();
                var selectedInternalNoteItemIds = selectedInternalNoteItems.Select(element => element.Id).ToList();
                var selectedInvoiceIds = selectedInternalNoteItems.Select(element => element.InvoiceId).ToList();
                var internalNoteDetail = internalNoteDetails.Where(element => selectedInternalNoteItemIds.Contains(element.GarmentItemINId)).OrderByDescending(element => element.PaymentDueDate).FirstOrDefault();

                var selectedInternalNoteDetails = internalNoteDetails.Where(element => selectedInternalNoteItemIds.Contains(element.GarmentItemINId)).ToList();
                var selectedDOIds = selectedInternalNoteDetails.Select(element => element.DOId).ToList();
                var selectedCorrections = corrections.Where(element => selectedDOIds.Contains(element.DOId)).ToList();

                var amountDPP = invoices.Where(element => selectedInvoiceIds.Contains(element.Id)).Sum(element => element.TotalAmount);

                var correctionAmount = selectedCorrections.Sum(element =>
                {
                    var selectedCorrectionItems = correctionItems.Where(item => item.GCorrectionId == element.Id);

                    var total = 0.0;
                    if (element.CorrectionType.ToUpper() == "RETUR")
                        total = (double)selectedCorrectionItems.Sum(item => item.PricePerDealUnitAfter * item.Quantity);
                    else
                        total = (double)element.TotalCorrection;

                    return total;
                });

                var totalAmount = invoices.Where(element => selectedInvoiceIds.Contains(element.Id)).Sum(element =>
                {
                    var total = element.TotalAmount;

                    if (element.UseVat)
                        total += element.TotalAmount * 0.1;

                    if (element.UseIncomeTax && element.IsPayTax)
                        total -= element.TotalAmount * (element.IncomeTaxRate / 100);

                    return total;
                });
                totalAmount += correctionAmount;

                var vatTotal = invoices.Where(element => selectedInvoiceIds.Contains(element.Id)).Sum(element =>
                {
                    var vat = 0.0;

                    if (element.UseVat)
                        vat += element.TotalAmount * 0.1;

                    return vat;
                });

                var incomeTaxTotal = invoices.Where(element => selectedInvoiceIds.Contains(element.Id)).Sum(element =>
                {
                    var incomeTax = 0.0;

                    if (element.UseIncomeTax && element.IsPayTax)
                        incomeTax += element.TotalAmount * (element.IncomeTaxRate / 100);

                    return incomeTax;
                });

                return new GarmentInternalNoteDto((int)internalNote.Id, internalNote.INNo, internalNote.INDate, internalNoteDetail.PaymentDueDate, (int)internalNote.SupplierId, internalNote.SupplierName, vatTotal, incomeTaxTotal, totalAmount, (int)internalNote.CurrencyId, internalNote.CurrencyCode, amountDPP);
            }).ToList();

            return result;
        }

        public int UpdateInternNotePosition(UpdatePositionFormDto form)
        {
            var models = _dbContext.GarmentInternNotes.Where(entity => form.Ids.Contains((int)entity.Id)).ToList();

            models = models.Select(model =>
            {
                model.Position = form.Position;
                EntityExtension.FlagForUpdate(model, _identityService.Username, UserAgent);

                return model;
            }).ToList();

            _dbContext.GarmentInternNotes.UpdateRange(models);
            return _dbContext.SaveChanges();
        }
    }
}
