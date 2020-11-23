using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary
{
    public interface IDebtAndDispositionSummaryService
    {
        ReadResponse<DebtAndDispositionSummaryDto> GetReport(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency);
        ReadResponse<DebtAndDispositionSummaryDto> GetReportDisposition(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency);
        List<DebtAndDispositionSummaryDto> GetSummary(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency);
        List<DebtAndDispositionSummaryDto> GetDispositionSummary(int categoryId, int accountingUnitId, int divisionId, DateTimeOffset dueDate, bool isImport, bool isForeignCurrency);
    }
}
