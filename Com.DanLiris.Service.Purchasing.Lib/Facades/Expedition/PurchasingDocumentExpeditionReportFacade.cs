using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Services.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class PurchasingDocumentExpeditionReportFacade
    {
        public readonly PurchasingDocumentExpeditionService purchasingDocumentExpeditionService;

        public PurchasingDocumentExpeditionReportFacade(PurchasingDocumentExpeditionService purchasingDocumentExpeditionService)
        {
            this.purchasingDocumentExpeditionService = purchasingDocumentExpeditionService;
        }

        public List<PurchasingDocumentExpeditionReportViewModel> GetReport(List<string> unitPaymentOrders)
        {
            var data = this.purchasingDocumentExpeditionService.DbSet
                .Select(s => new PurchasingDocumentExpedition
                {
                    UnitPaymentOrderNo = s.UnitPaymentOrderNo,
                    SendToVerificationDivisionDate = s.SendToVerificationDivisionDate,
                    VerificationDivisionDate = s.VerificationDivisionDate,
                    VerifyDate = s.VerifyDate,
                    SendToCashierDivisionDate = s.SendToCashierDivisionDate,
                    SendToFinanceDivisionDate = s.SendToFinanceDivisionDate,
                    SendToPurchasingDivisionDate = s.SendToPurchasingDivisionDate,
                    CashierDivisionDate = s.CashierDivisionDate,
                    Position = s.Position,
                })
                .Where(p => unitPaymentOrders.Contains(p.UnitPaymentOrderNo));

            List<PurchasingDocumentExpeditionReportViewModel> list = new List<PurchasingDocumentExpeditionReportViewModel>();

            foreach(PurchasingDocumentExpedition d in data)
            {
                PurchasingDocumentExpeditionReportViewModel item = new PurchasingDocumentExpeditionReportViewModel()
                {
                    SendToVerificationDivisionDate = d.SendToVerificationDivisionDate,
                    VerificationDivisionDate = d.VerificationDivisionDate,
                    VerifyDate = d.VerifyDate,
                    SendDate = (d.Position == ExpeditionPosition.CASHIER_DIVISION || d.Position == ExpeditionPosition.SEND_TO_CASHIER_DIVISION) ? d.SendToCashierDivisionDate :
                    (d.Position == ExpeditionPosition.FINANCE_DIVISION || d.Position == ExpeditionPosition.SEND_TO_FINANCE_DIVISION) ? d.SendToFinanceDivisionDate :
                    (d.Position == ExpeditionPosition.SEND_TO_PURCHASING_DIVISION) ? d.SendToPurchasingDivisionDate : null,
                    CashierDivisionDate = d.CashierDivisionDate,
                    UnitPaymentOrderNo = d.UnitPaymentOrderNo
                };

                list.Add(item);
            }

            return list;
        }
    }
}
