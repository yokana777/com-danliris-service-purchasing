using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public interface IGarmentPurchasingBookReportService
    {
        ReportDto GetReport(string billNo, string paymentBill, string garmentCategory, DateTimeOffset startDate, DateTimeOffset endDate, bool isForeignCurrency, bool isImportSupplier);
        List<AutoCompleteDto> GetBillNos(string keyword);
        List<AutoCompleteDto> GetPaymentBills(string keyword);
        List<AutoCompleteDto> GetAccountingCategories(string keyword);
    }
}
