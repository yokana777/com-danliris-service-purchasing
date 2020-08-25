using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public interface IVBRequestPOExternalService
    {
        List<POExternalDto> ReadPOExternal(string keyword, string division, string currencyCode);
    }
}
