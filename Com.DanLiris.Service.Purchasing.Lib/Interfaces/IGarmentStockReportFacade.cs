using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentStockReportFacade
    {
        Tuple<List<GarmentStockReportViewModel>, int> GetStockReport(int offset, string unitcode, string tipebarang, int page, int size, string Order, DateTime? dateFrom, DateTime? dateTo);
        MemoryStream GenerateExcelStockReport(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset);
    }
}
