using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using System;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IUnitPaymentOrderUnpaidReportFacade
    {
        ReadResponse GetReport(int Size, int Page, string Order, string UnitPaymentOrderNo, string SupplierCode, DateTimeOffset? DateFrom, DateTimeOffset? DateTo, int Offset);
    }
}
