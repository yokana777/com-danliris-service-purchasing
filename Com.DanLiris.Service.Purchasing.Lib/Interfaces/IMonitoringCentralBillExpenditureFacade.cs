using Com.DanLiris.Service.Purchasing.Lib.ViewModels.MonitoringCentralBillExpenditureViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
	public interface IMonitoringCentralBillExpenditureFacade
	{
        Tuple<List<MonitoringCentralBillExpenditureViewModel>, int> GetMonitoringKeluarBonPusatReport(DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset);
        MemoryStream GenerateExcelMonitoringKeluarBonPusat(DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset);

        Tuple<List<MonitoringCentralBillExpenditureViewModel>, int> GetMonitoringKeluarBonPusatByUserReport(DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset);
        MemoryStream GenerateExcelMonitoringKeluarBonPusatByUser(DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset);
    }
}
