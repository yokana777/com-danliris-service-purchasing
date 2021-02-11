using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public interface IGarmentPurchasingBookReportService
    {
        ReportDto GetReport(string billNo, string paymentBill, string garmentCategory, DateTimeOffset startDate, DateTimeOffset endDate, bool isForeignCurrency, bool isImportSupplier);
        List<BillNoPaymentBillAutoCompleteDto> GetBillNos(string keyword);
        List<BillNoPaymentBillAutoCompleteDto> GetPaymentBills(string keyword);
    }
}
