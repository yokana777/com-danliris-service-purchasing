using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnpaidDispositionReport;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.UnpaidDispositionReportFacades
{
    public interface IUnpaidDispositionReportDetailFacade
    {
        Task<UnpaidDispositionReportDetailViewModel> GetReport(int accountingUnitId, int accountingCategoryId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency);
        Task<MemoryStream> GenerateExcel(int accountingUnitId, int accountingCategoryId, int divisionId, DateTimeOffset? dateTo, bool isImport, bool isForeignCurrency);
    }
}
