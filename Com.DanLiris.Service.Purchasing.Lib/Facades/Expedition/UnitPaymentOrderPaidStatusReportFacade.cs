using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class UnitPaymentOrderPaidStatusReportFacade : IUnitPaymentOrderPaidStatusReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<PurchasingDocumentExpedition> dbSet;
        public UnitPaymentOrderPaidStatusReportFacade(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = this.dbContext.Set<PurchasingDocumentExpedition>();
        }

        public ReadResponse GetReport(int Size, int Page, string Order, string UnitPaymentOrderNo, string SupplierCode, string DivisionCode, string Status, DateTimeOffset? DateFrom, DateTimeOffset? DateTo, int Offset)
        {
            IQueryable<PurchasingDocumentExpedition> Query = this.dbContext.PurchasingDocumentExpeditions;

            if (DateFrom == null || DateTo == null)
            {
                Query = Query
                  .Where(p => p.IsDeleted == false &&
                      p.UnitPaymentOrderNo == (UnitPaymentOrderNo == null ? p.UnitPaymentOrderNo : UnitPaymentOrderNo) &&
                      p.SupplierCode == (SupplierCode == null ? p.SupplierCode : SupplierCode) &&
                      p.DivisionCode == (DivisionCode == null ? p.DivisionCode : DivisionCode)
                  );
            }
            else
            {
                Query = Query
                   .Where(p => p.IsDeleted == false &&
                          p.UnitPaymentOrderNo == (UnitPaymentOrderNo == null ? p.UnitPaymentOrderNo : UnitPaymentOrderNo) &&
                          p.SupplierCode == (SupplierCode == null ? p.SupplierCode : SupplierCode) &&
                          p.DivisionCode == (DivisionCode == null ? p.DivisionCode : DivisionCode) &&
                          p.DueDate.Date >= DateFrom.Value.Date &&
                          p.DueDate.Date <= DateTo.Value.Date
                   );
            }

            Query = Query
                .Select(s => new PurchasingDocumentExpedition
                {
                    Id = s.Id,
                    UnitPaymentOrderNo = s.UnitPaymentOrderNo,
                    UPODate = s.UPODate,
                    DueDate = s.DueDate,
                    InvoiceNo = s.InvoiceNo,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    DivisionCode = s.DivisionCode,
                    DivisionName = s.DivisionName,
                    PaymentMethod = s.PaymentMethod,
                    IsPaid = s.IsPaid,
                    IsPaidPPH = s.IsPaidPPH,
                    TotalPaid = s.TotalPaid,
                    IncomeTax = s.IncomeTax,
                    Vat = s.Vat,
                    Currency = s.Currency,
                    BankExpenditureNoteDate = s.BankExpenditureNoteDate,
                    BankExpenditureNoteNo = s.BankExpenditureNoteNo,
                    BankExpenditureNotePPHDate = s.BankExpenditureNotePPHDate,
                    BankExpenditureNotePPHNo = s.BankExpenditureNotePPHNo,
                    LastModifiedUtc = s.LastModifiedUtc
                });

            if(!string.IsNullOrWhiteSpace(Status))
            {
                if (Status.Equals("LUNAS"))
                {
                    Query = Query.Where(p => p.IsPaid == true && p.IsPaidPPH == true);
                }
                else if (Status.Equals("SUDAH BAYAR DPP+PPN"))
                {
                    Query = Query.Where(p => p.IsPaid == true && p.IsPaidPPH == false);
                }
                else if (Status.Equals("SUDAH BAYAR PPH"))
                {
                    Query = Query.Where(p => p.IsPaidPPH == true && p.IsPaid == false);
                }
                else if (Status.Equals("BELUM BAYAR"))
                {
                    Query = Query.Where(p => p.IsPaid == false && p.IsPaidPPH == false);
                }
            }

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<PurchasingDocumentExpedition>.ConfigureOrder(Query, OrderDictionary);

            Pageable<PurchasingDocumentExpedition> pageable = new Pageable<PurchasingDocumentExpedition>(Query, Page - 1, Size);
            List<PurchasingDocumentExpedition> Data = pageable.Data.ToList<PurchasingDocumentExpedition>();
            int TotalData = pageable.TotalCount;

            string[] BENNo = Data.Where(p => p.BankExpenditureNoteNo != null).Select(p => p.BankExpenditureNoteNo).ToArray();
            string[] BENPPHNo = Data.Where(p => p.BankExpenditureNotePPHNo != null).Select(p => p.BankExpenditureNotePPHNo).ToArray();

            var BEN = this.dbContext.BankExpenditureNotes.Where(p => BENNo.Contains(p.DocumentNo)).ToList();
            var BENPPH = this.dbContext.PPHBankExpenditureNotes.Where(p => BENPPHNo.Contains(p.No)).ToList();

            List<object> list = new List<object>();

            foreach (var datum in Data)
            {
                var Bank = BEN.SingleOrDefault(p => p.DocumentNo == datum.BankExpenditureNoteNo);
                var BankPPH = BENPPH.SingleOrDefault(p => p.No == datum.BankExpenditureNotePPHNo);

                list.Add(new UnitPaymentOrderPaidStatusViewModel
                {
                    UnitPaymentOrderNo = datum.UnitPaymentOrderNo,
                    UPODate = datum.UPODate,
                    DueDate = datum.DueDate,
                    InvoiceNo = datum.InvoiceNo,
                    SupplierName = datum.SupplierName,
                    DivisionName = datum.DivisionName,
                    PaymentMethod = datum.PaymentMethod,
                    Status = (datum.IsPaid == true && datum.IsPaidPPH == true) ? "LUNAS" : (datum.IsPaid == true && datum.IsPaidPPH == false) ? "SUDAH BAYAR DPP+PPB" : (datum.IsPaidPPH == true && datum.IsPaid == false) ? "SUDAH BAYAR PPH" : "BELUM BAYAR",
                    DPP = datum.TotalPaid - datum.Vat,
                    IncomeTax = datum.IncomeTax,
                    Vat = datum.Vat,
                    TotalPaid = datum.TotalPaid,
                    Currency = datum.Currency,
                    BankExpenditureNotePPHDate = datum.BankExpenditureNotePPHDate,
                    PPHBank = BankPPH != null ? string.Concat(BankPPH.BankAccountName, " - ", BankPPH.BankName, " - ", BankPPH.BankAccountNumber, " - ", BankPPH.Currency) : null,
                    BankExpenditureNoteDate = datum.BankExpenditureNoteDate,
                    Bank = Bank != null ? string.Concat(Bank.BankAccountName, " - ", Bank.BankName, " - ", Bank.BankAccountNumber, " - ", Bank.BankCurrencyCode) : null,
                });
            }

            return new ReadResponse(list, TotalData, OrderDictionary);
        }
    }
}
