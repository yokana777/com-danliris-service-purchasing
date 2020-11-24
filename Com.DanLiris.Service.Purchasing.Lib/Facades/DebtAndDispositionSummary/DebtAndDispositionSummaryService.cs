﻿using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary
{
    public class DebtAndDispositionSummaryService : IDebtAndDispositionSummaryService
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly List<UnitDto> _units;

        //private readonly IDistributedCache _cache;

        public DebtAndDispositionSummaryService(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
            var cache = serviceProvider.GetService<IDistributedCache>();
            var jsonUnits = cache.GetString(MemoryCacheConstant.Units);
            _units = JsonConvert.DeserializeObject<List<UnitDto>>(jsonUnits, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }

        private IQueryable<DebtAndDispositionSummaryDto> GetDebtQuery(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
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
                            DueDate = urnWithItem.ReceiptDate.AddDays(Convert.ToInt32(urnEPO.PaymentDueDays)),
                            IncomeTaxBy = urnEPO.IncomeTaxBy,
                            UseIncomeTax = urnEPO.UseIncomeTax,
                            IncomeTaxRate = urnEPO.IncomeTaxRate
                        };

            query = query.Where(entity => !entity.IsPaid && (entity.IsImport == isImport) && entity.DueDate <= dueDate);

            if (categoryId > 0)
                query = query.Where(entity => entity.CategoryId == categoryId.ToString());

            if (accountingUnitId > 0)
            {
                var unitIds = _units.Where(unit => unit.AccountingUnitId == accountingUnitId).Select(unit => unit.Id.ToString()).ToList();
                if (unitIds.Count == 0)
                    // intentionally added to make the query returns empty data
                    unitIds.Add("0");
                query = query.Where(entity => unitIds.Contains(entity.UnitId));

            }

            if (divisionId > 0)
                query = query.Where(entity => entity.DivisionId == divisionId.ToString());

            if (!isForeignCurrency && !isImport)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() == "IDR");
            else if (isForeignCurrency)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() != "IDR");

            return query;
        }

        private IQueryable<DebtAndDispositionSummaryDto> GetDispositionQuery(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
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
                            DueDate = pdWithItem.PaymentDueDate,
                            IncomeTaxBy = pdItemEPO.IncomeTaxBy,
                            UseIncomeTax = pdItemEPO.UseIncomeTax,
                            IncomeTaxRate = pdItemEPO.IncomeTaxRate
                        };

            query = query.Where(entity => !entity.IsPaid && (entity.IsImport == isImport) && entity.DueDate <= dueDate);

            if (categoryId > 0)
                query = query.Where(entity => entity.CategoryId == categoryId.ToString());

            if (accountingUnitId > 0)
            {
                var unitIds = _units.Where(unit => unit.AccountingUnitId == accountingUnitId).Select(unit => unit.Id.ToString()).ToList();
                if (unitIds.Count == 0)
                    // intentionally added to make the query returns empty data
                    unitIds.Add("0");
                query = query.Where(entity => unitIds.Contains(entity.UnitId));

            }

            if (divisionId > 0)
                query = query.Where(entity => entity.DivisionId == divisionId.ToString());

            if (!isForeignCurrency && !isImport)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() == "IDR");
            else if (isForeignCurrency)
                query = query.Where(entity => entity.CurrencyCode.ToUpper() != "IDR");

            return query;
        }

        public ReadResponse<DebtAndDispositionSummaryDto> GetReport(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
        {
            var debtQuery = GetDebtQuery(categoryId, accountingUnitId, divisionId, dueDate, isImport, isForeignCurrency);
            var dispositionQuery = GetDispositionQuery(categoryId, accountingUnitId, divisionId, dueDate, isImport, isForeignCurrency);

            var debts = debtQuery.ToList();
            var dispositions = dispositionQuery.ToList();

            var result = new List<DebtAndDispositionSummaryDto>();
            result.AddRange(debts);
            result.AddRange(dispositions);


            result = result
                .Select(element =>
                {
                    double.TryParse(element.IncomeTaxRate, out var incomeTaxRate);
                    var debtTotal = element.DebtTotal;
                    var dispositionTotal = element.DispositionTotal;

                    if (element.UseIncomeTax && element.IncomeTaxBy.ToUpper() == "SUPPLIER")
                    {
                        debtTotal += debtTotal * (incomeTaxRate / 100);
                        dispositionTotal += dispositionTotal * (incomeTaxRate / 100);
                    }

                    return new DebtAndDispositionSummaryDto()
                    {
                        CategoryCode = element.CategoryCode,
                        CategoryName = element.CategoryName,
                        CurrencyCode = element.CurrencyCode,
                        DebtTotal = debtTotal,
                        DispositionTotal = dispositionTotal,
                        Total = debtTotal + dispositionTotal
                    };
                })
                .GroupBy(element => new { element.CategoryCode, element.CurrencyCode })
                .Select(element =>
                {
                    var debtTotal = element.Sum(sum => sum.DebtTotal);
                    var dispositionTotal = element.Sum(sum => sum.DispositionTotal);

                    return new DebtAndDispositionSummaryDto()
                    {
                        CategoryCode = element.Key.CategoryCode,
                        CategoryName = element.FirstOrDefault().CategoryName,
                        CurrencyCode = element.Key.CurrencyCode,
                        DebtTotal = debtTotal,
                        DispositionTotal = dispositionTotal,
                        Total = debtTotal + dispositionTotal
                    };
                })
                .ToList();

            return new ReadResponse<DebtAndDispositionSummaryDto>(result, result.Count, new Dictionary<string, string>());
        }

        public List<DebtAndDispositionSummaryDto> GetSummary(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
        {
            var debtQuery = GetDebtQuery(categoryId, accountingUnitId, divisionId, dueDate, isImport, isForeignCurrency);
            var dispositionQuery = GetDispositionQuery(categoryId, accountingUnitId, divisionId, dueDate, isImport, isForeignCurrency);

            var debts = debtQuery.ToList();
            var dispositions = dispositionQuery.ToList();

            var result = new List<DebtAndDispositionSummaryDto>();
            result.AddRange(debts);
            result.AddRange(dispositions);

            result = result.Select(element =>
            {
                double.TryParse(element.IncomeTaxRate, out var incomeTaxRate);
                var debtTotal = element.DebtTotal;
                var dispositionTotal = element.DispositionTotal;

                if (element.UseIncomeTax && element.IncomeTaxBy.ToUpper() == "SUPPLIER")
                {
                    debtTotal += debtTotal * (incomeTaxRate / 100);
                    dispositionTotal += dispositionTotal * (incomeTaxRate / 100);
                }

                return new DebtAndDispositionSummaryDto()
                {
                    CategoryCode = element.CategoryCode,
                    CategoryId = element.CategoryId,
                    CategoryName = element.CategoryName,
                    CurrencyCode = element.CurrencyCode,
                    CurrencyId = element.CurrencyId,
                    CurrencyRate = element.CurrencyRate,
                    DebtPrice = element.DebtPrice,
                    DebtQuantity = element.DebtQuantity,
                    DebtTotal = debtTotal,
                    DispositionPrice = element.DispositionPrice,
                    DispositionQuantity = element.DispositionQuantity,
                    DispositionTotal = dispositionTotal,
                    DivisionCode = element.DivisionCode,
                    DivisionId = element.DivisionId,
                    DivisionName = element.DivisionName,
                    DueDate = element.DueDate,
                    IncomeTaxBy = element.IncomeTaxBy,
                    IncomeTaxRate = element.IncomeTaxRate,
                    IsImport = element.IsImport,
                    IsPaid = element.IsPaid,
                    Total = debtTotal + dispositionTotal,
                    UnitCode = element.UnitCode,
                    UnitId = element.UnitId,
                    UnitName = element.UnitName,
                    UseIncomeTax = element.UseIncomeTax
                };
            }).ToList();

            return result;
        }

        public ReadResponse<DebtAndDispositionSummaryDto> GetReportDisposition(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
        {
            var dispositionQuery = GetDispositionQuery(categoryId, accountingUnitId, divisionId, dueDate, isImport, isForeignCurrency);

            var dispositions = dispositionQuery.ToList();

            var result = new List<DebtAndDispositionSummaryDto>();
            result.AddRange(dispositions);


            result = result
                .Select(element =>
                {
                    double.TryParse(element.IncomeTaxRate, out var incomeTaxRate);
                    var dispositionTotal = element.DispositionTotal;

                    if (element.UseIncomeTax && element.IncomeTaxBy.ToUpper() == "SUPPLIER")
                    {
                        dispositionTotal += dispositionTotal * (incomeTaxRate / 100);
                    }

                    return new DebtAndDispositionSummaryDto()
                    {
                        CategoryCode = element.CategoryCode,
                        CategoryName = element.CategoryName,
                        CurrencyCode = element.CurrencyCode,
                        DispositionTotal = dispositionTotal,
                        Total = dispositionTotal
                    };
                })
                .GroupBy(element => new { element.CategoryCode, element.CurrencyCode })
                .Select(element =>
                {
                    var dispositionTotal = element.Sum(sum => sum.DispositionTotal);

                    return new DebtAndDispositionSummaryDto()
                    {
                        CategoryCode = element.Key.CategoryCode,
                        CategoryName = element.FirstOrDefault().CategoryName,
                        CurrencyCode = element.Key.CurrencyCode,
                        DispositionTotal = dispositionTotal,
                        Total = dispositionTotal
                    };
                })
                .ToList();

            return new ReadResponse<DebtAndDispositionSummaryDto>(result, result.Count, new Dictionary<string, string>());
        }

        public List<DebtAndDispositionSummaryDto> GetDispositionSummary(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency)
        {
            var dispositionQuery = GetDispositionQuery(categoryId, accountingUnitId, divisionId, dueDate, isImport, isForeignCurrency);

            var dispositions = dispositionQuery.ToList();

            var result = new List<DebtAndDispositionSummaryDto>();
            result.AddRange(dispositions);

            result = result.Select(element =>
            {
                double.TryParse(element.IncomeTaxRate, out var incomeTaxRate);
                var dispositionTotal = element.DispositionTotal;

                if (element.UseIncomeTax && element.IncomeTaxBy.ToUpper() == "SUPPLIER")
                {
                    dispositionTotal += dispositionTotal * incomeTaxRate;
                }

                return new DebtAndDispositionSummaryDto()
                {
                    CategoryCode = element.CategoryCode,
                    CategoryId = element.CategoryId,
                    CategoryName = element.CategoryName,
                    CurrencyCode = element.CurrencyCode,
                    CurrencyId = element.CurrencyId,
                    CurrencyRate = element.CurrencyRate,
                    DispositionPrice = element.DispositionPrice,
                    DispositionQuantity = element.DispositionQuantity,
                    DispositionTotal = dispositionTotal,
                    DivisionCode = element.DivisionCode,
                    DivisionId = element.DivisionId,
                    DivisionName = element.DivisionName,
                    DueDate = element.DueDate,
                    IncomeTaxBy = element.IncomeTaxBy,
                    IncomeTaxRate = element.IncomeTaxRate,
                    IsImport = element.IsImport,
                    IsPaid = element.IsPaid,
                    Total = dispositionTotal,
                    UnitCode = element.UnitCode,
                    UnitId = element.UnitId,
                    UnitName = element.UnitName,
                    UseIncomeTax = element.UseIncomeTax
                };
            }).ToList();

            return result;
        }
    }
}