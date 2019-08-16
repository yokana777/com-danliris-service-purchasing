using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IUnitPaymentOrderExpeditionReportService
    {
        Task<List<UnitPaymentOrderExpeditionReportViewModel>> GetReport(string no, string supplierCode, string divisionCode, int status, DateTimeOffset dateFrom, DateTimeOffset dateTo, string order, int page, int size);
    }
}
