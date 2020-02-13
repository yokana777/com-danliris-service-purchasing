using Com.DanLiris.Service.Purchasing.Lib.ViewModels.MonitoringCentralBillReceptionViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IMonitoringCentralBillReceptionFacade
    {
        Tuple<List<MonitoringCentralBillReceptionViewModel>, int> GetMonitoringTerimaBonPusatReport(DateTime? dateFrom, DateTime? dateTo, string jnsBC, int page, int size, string Order, int offset);
        MemoryStream GenerateExcelMonitoringTerimaBonPusat(DateTime? dateFrom, DateTime? dateTo, string jnsBC, int page, int size, string Order, int offset);

        Tuple<List<MonitoringCentralBillReceptionViewModel>, int> GetMonitoringTerimaBonPusatByUserReport(DateTime? dateFrom, DateTime? dateTo, string jnsBC, int page, int size, string Order, int offset);
        MemoryStream GenerateExcelMonitoringTerimaBonPusatByUser(DateTime? dateFrom, DateTime? dateTo, string jnsBC, int page, int size, string Order, int offset);
    }
}
