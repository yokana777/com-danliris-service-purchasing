using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary
{
    public class DebtAndDispositionSummaryService : IDebtAndDispositionSummaryService
    {
        private readonly PurchasingDbContext _dbContext;

        public DebtAndDispositionSummaryService(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
        }

        private IQueryable<DebtAndDispositionSummaryDto> GetDebtQuery(int categoryId, int unitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
        {
            var unitReceiptNoteItems = _dbContext.UnitReceiptNoteItems.AsQueryable();
            var unitReceiptNotes = _dbContext.UnitReceiptNotes.AsQueryable();
            var unitPaymentOrderItems = _dbContext.UnitPaymentOrderItems.AsQueryable();
            var unitPaymentOrders = _dbContext.UnitPaymentOrders.AsQueryable();
            var externalPurchaseOrders = _dbContext.ExternalPurchaseOrders.AsQueryable();
            var purchaseRequests = _dbContext.PurchaseRequests.AsQueryable();

            var query = from unitReceiptNoteItem in unitReceiptNoteItems

                        join unitReceiptNote in unitReceiptNotes on unitReceiptNoteItem.URNId equals unitReceiptNote.Id into urnWithItems
                        from urnWithItem in urnWithItems.DefaultIfEmpty()

                        join unitPaymentOrderItem in unitPaymentOrderItems on urnWithItem.Id equals unitPaymentOrderItem.URNId into urnUPOItems
                        from urnUPOItem in urnUPOItems.DefaultIfEmpty()

                        join unitPaymentOrder in unitPaymentOrders on urnUPOItem.UPOId equals unitPaymentOrder.Id into upoWithItems
                        from upoWithItem in upoWithItems.DefaultIfEmpty()

                        join externalPurchaseOrder in externalPurchaseOrders on unitReceiptNoteItem.EPOId equals externalPurchaseOrder.Id into urnEPOs
                        from urnEPO in urnEPOs.DefaultIfEmpty()

                        join purchaseRequest in purchaseRequests on unitReceiptNoteItem.PRId equals purchaseRequest.Id into urnPRs
                        from urnPR in urnPRs.DefaultIfEmpty()

                        select new DebtAndDispositionSummaryDto
                        {
                            CurrencyId = urnEPO.CurrencyId,
                            CurrencyCode = urnEPO.CurrencyCode,
                            CurrencyRate = urnEPO.CurrencyRate,
                            CategoryId = urnPR.CategoryId,
                            CategoryCode = urnPR.CategoryCode,
                            CategoryName = urnPR.CategoryName,
                            UnitId = urnPR.UnitId,
                            UnitCode = urnPR.UnitCode,
                            UnitName = urnPR.UnitName,
                            DivisionId = urnPR.DivisionId,
                            DivisionCode = urnPR.DivisionCode,
                            DivisionName = urnPR.DivisionName,
                            IsImport = urnWithItem.SupplierIsImport,
                            IsPaid = upoWithItem != null && upoWithItem.IsPaid,
                            DebtPrice = unitReceiptNoteItem.PricePerDealUnit,
                            DebtQuantity = unitReceiptNoteItem.ReceiptQuantity,
                            DebtTotal = unitReceiptNoteItem.PricePerDealUnit * unitReceiptNoteItem.ReceiptQuantity,
                            DueDate = urnWithItem.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays))
                        };

            query = query.Where(entity => !entity.IsPaid && (entity.IsImport == isImport) && entity.DueDate <= dueDate);

            if (categoryId > 0)
                query = query.Where(entity => entity.CategoryId == categoryId.ToString());

            if (unitId > 0)
                query = query.Where(entity => entity.UnitId == unitId.ToString());

            if (divisionId > 0)
                query = query.Where(entity => entity.DivisionId == divisionId.ToString());

            if (!isForeignCurrency && !isImport)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() == "IDR");
            else if (isForeignCurrency)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() != "IDR");

            return query;
        }

        private IQueryable<DebtAndDispositionSummaryDto> GetDispositionQuery(int categoryId, int unitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
        {
            var externalPurchaseOrders = _dbContext.ExternalPurchaseOrders.AsQueryable();
            var purchasingDispositionDetails = _dbContext.PurchasingDispositionDetails.AsQueryable();
            var purchasingDispositionItems = _dbContext.PurchasingDispositionItems.AsQueryable();
            var purchasingDispositions = _dbContext.PurchasingDispositions.AsQueryable();

            var query = from purchasingDispositionDetail in purchasingDispositionDetails

                        join purchasingDispositionItem in purchasingDispositionItems on purchasingDispositionDetail.PurchasingDispositionItemId equals purchasingDispositionItem.Id into pdDetailItems
                        from pdDetailItem in pdDetailItems.DefaultIfEmpty()

                        join purchasingDisposition in purchasingDispositions on pdDetailItem.PurchasingDispositionId equals purchasingDisposition.Id into pdWithItems
                        from pdWithItem in pdWithItems.DefaultIfEmpty()

                        join externalPurchaseOrder in externalPurchaseOrders on pdDetailItem.EPOId equals externalPurchaseOrder.Id.ToString() into pdItemEPOs
                        from pdItemEPO in pdItemEPOs.DefaultIfEmpty()

                        select new DebtAndDispositionSummaryDto
                        {
                            CurrencyId = pdWithItem.CurrencyId,
                            CurrencyCode = pdWithItem.CurrencyCode,
                            CurrencyRate = pdWithItem.CurrencyRate,
                            CategoryId = pdWithItem.CategoryId,
                            CategoryCode = pdWithItem.CategoryCode,
                            CategoryName = pdWithItem.CategoryName,
                            UnitId = purchasingDispositionDetail.UnitId,
                            UnitCode = purchasingDispositionDetail.UnitCode,
                            UnitName = purchasingDispositionDetail.UnitName,
                            DivisionId = pdWithItem.DivisionId,
                            DivisionCode = pdWithItem.DivisionCode,
                            DivisionName = pdWithItem.DivisionName,
                            IsImport = pdItemEPO.SupplierIsImport,
                            IsPaid = pdWithItem.IsPaid,
                            DispositionPrice = purchasingDispositionDetail.PricePerDealUnit,
                            DispositionQuantity = purchasingDispositionDetail.DealQuantity,
                            DispositionTotal = purchasingDispositionDetail.PriceTotal,
                            DueDate = pdWithItem.PaymentDueDate
                        };

            query = query.Where(entity => !entity.IsPaid && (entity.IsImport == isImport) && entity.DueDate <= dueDate);

            if (categoryId > 0)
                query = query.Where(entity => entity.CategoryId == categoryId.ToString());

            if (unitId > 0)
                query = query.Where(entity => entity.UnitId == unitId.ToString());

            if (divisionId > 0)
                query = query.Where(entity => entity.DivisionId == divisionId.ToString());

            if (!isForeignCurrency && !isImport)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() == "IDR");
            else if (isForeignCurrency)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() != "IDR");

            return query;
        }

        public ReadResponse<DebtAndDispositionSummaryDto> GetReport(int categoryId, int unitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
        {
            var debtQuery = GetDebtQuery(categoryId, unitId, divisionId, dueDate, isImport, isForeignCurrency);
            var dispositionQuery = GetDispositionQuery(categoryId, unitId, divisionId, dueDate, isImport, isForeignCurrency);

            var debts = debtQuery.ToList();
            var dispositions = dispositionQuery.ToList();

            var result = new List<DebtAndDispositionSummaryDto>();
            result.AddRange(debts);
            result.AddRange(dispositions);

            if (!isImport && !isForeignCurrency)
            {
                result = result
                    .GroupBy(element => element.CategoryCode)
                    .Select(element => new DebtAndDispositionSummaryDto()
                    {
                        CategoryCode = element.Key,
                        CategoryName = element.FirstOrDefault().CategoryName,
                        CurrencyCode = element.FirstOrDefault().CurrencyCode,
                        DebtTotal = element.Sum(sum => sum.DebtTotal),
                        DispositionTotal = element.Sum(sum => sum.DispositionTotal),
                        Total = element.Sum(sum => sum.DebtTotal) + element.Sum(sum => sum.DispositionTotal)
                    })
                    .ToList();
            }
            else
            {
                result = result
                    .GroupBy(element => new { element.CategoryCode, element.CurrencyCode })
                    .Select(element => new DebtAndDispositionSummaryDto()
                    {
                        CategoryCode = element.Key.CategoryCode,
                        CategoryName = element.FirstOrDefault().CategoryName,
                        CurrencyCode = element.Key.CurrencyCode,
                        DebtTotal = element.Sum(sum => sum.DebtTotal),
                        DispositionTotal = element.Sum(sum => sum.DispositionTotal),
                        Total = element.Sum(sum => sum.DebtTotal) + element.Sum(sum => sum.DispositionTotal)
                    })
                    .ToList();
            }

            return new ReadResponse<DebtAndDispositionSummaryDto>(result, result.Count, new Dictionary<string, string>());
        }
    }
}
