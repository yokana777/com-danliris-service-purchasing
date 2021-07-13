using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface ITraceableBeacukaiFacade
    {
        Tuple<List<TraceableInBeacukaiViewModel>, int> GetReportTraceableIN(string filter, string tipe, string tipebc);
        MemoryStream GetTraceableInExcel(string filter, string tipe, string tipebc);
        //List<TraceableOutBeacukaiDetailViewModel> getQueryDetail(string RO);
        List<TraceableOutBeacukaiViewModel> getQueryTraceableOut(string bcno);
        MemoryStream GetExceltraceOut(string bcno);
    }
}
