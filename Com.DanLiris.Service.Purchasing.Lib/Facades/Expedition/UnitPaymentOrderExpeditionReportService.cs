using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class UnitPaymentOrderExpeditionReportService : IUnitPaymentOrderExpeditionReportService
    {
        private readonly PurchasingDbContext _dbContext;

        public UnitPaymentOrderExpeditionReportService(PurchasingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UnitPaymentOrderExpeditionReportWrapper> GetReport(string no, string supplierCode, string divisionCode, int status, DateTimeOffset dateFrom, DateTimeOffset dateTo, string order, int page, int size)
        {
            var expeditionDocumentQuery = _dbContext.Set<PurchasingDocumentExpedition>().AsQueryable();
            var query = _dbContext.Set<UnitPaymentOrder>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(no))
            {
                query = query.Where(document => document.UPONo.Equals(no));
            }

            if (!string.IsNullOrWhiteSpace(supplierCode))
            {
                query = query.Where(document => document.SupplierCode.Equals(supplierCode));
            }

            if (!string.IsNullOrWhiteSpace(divisionCode))
            {
                query = query.Where(document => document.DivisionCode.Equals(divisionCode));
            }

            if (status != 0)
            {
                query = query.Where(document => document.Position.Equals(status));
            }

            query = query.Where(document => document.Date >= dateFrom && document.Date <= dateTo);

            var joinedQuery = from unitPaymentOrder in query
                              join expeditionDocument in expeditionDocumentQuery on unitPaymentOrder.UPONo equals expeditionDocument.UnitPaymentOrderNo into upoExpeditions
                              from upoExpedition in upoExpeditions.DefaultIfEmpty()
                              select new UnitPaymentOrderExpeditionReportViewModel()
                              {
                                  SendToVerificationDivisionDate = upoExpedition.SendToVerificationDivisionDate,
                                  VerificationDivisionDate = upoExpedition.VerificationDivisionDate,
                                  VerifyDate = upoExpedition.VerifyDate,
                                  SendDate = (upoExpedition.Position == ExpeditionPosition.CASHIER_DIVISION || upoExpedition.Position == ExpeditionPosition.SEND_TO_CASHIER_DIVISION) ? upoExpedition.SendToCashierDivisionDate : (upoExpedition.Position == ExpeditionPosition.FINANCE_DIVISION || upoExpedition.Position == ExpeditionPosition.SEND_TO_ACCOUNTING_DIVISION) ? upoExpedition.SendToAccountingDivisionDate : (upoExpedition.Position == ExpeditionPosition.SEND_TO_PURCHASING_DIVISION) ? upoExpedition.SendToPurchasingDivisionDate : null,
                                  CashierDivisionDate = upoExpedition.CashierDivisionDate,
                                  BankExpenditureNoteNo = upoExpedition.BankExpenditureNoteNo,
                                  Date = upoExpedition.UPODate,
                                  Division = new DivisionViewModel()
                                  {
                                      Code = upoExpedition.DivisionCode,
                                      Name = upoExpedition.DivisionName
                                  },
                                  DueDate = upoExpedition.DueDate,
                                  InvoiceNo = upoExpedition.InvoiceNo,
                                  No = upoExpedition.UnitPaymentOrderNo,
                                  Position = upoExpedition.Position,
                                  Supplier = new NewSupplierViewModel()
                                  {
                                      code = upoExpedition.SupplierCode,
                                      name = upoExpedition.SupplierName
                                  },
                                  LastModifiedUtc = upoExpedition.LastModifiedUtc
                              };

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            joinedQuery = QueryHelper<UnitPaymentOrderExpeditionReportViewModel>.ConfigureOrder(joinedQuery, OrderDictionary);


            return new UnitPaymentOrderExpeditionReportWrapper()
            {
                Total = await joinedQuery.CountAsync(),
                Data = await joinedQuery
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync()
            };
        }
    }
}
