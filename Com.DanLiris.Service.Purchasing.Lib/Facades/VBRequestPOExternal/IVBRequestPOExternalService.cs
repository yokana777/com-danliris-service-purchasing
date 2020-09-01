using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public interface IVBRequestPOExternalService
    {
        List<POExternalDto> ReadPOExternal(string keyword, string division, string currencyCode);
        List<SPBDto> ReadSPB(string keyword, string division, List<int> epoIds, string currencyCode);
        int UpdateSPB(string division, int spbId);
    }
}
