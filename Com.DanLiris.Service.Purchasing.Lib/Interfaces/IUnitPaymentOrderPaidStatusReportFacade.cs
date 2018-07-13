using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IUnitPaymentOrderPaidStatusReportFacade
    {
        ReadResponse GetReport(int Size, int Page, string Order, string UnitPaymentOrderNo, string SupplierCode, string DivisionCode, string Status, DateTimeOffset? DateFrom, DateTimeOffset? DateTo, int Offset);
    }
}
