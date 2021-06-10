using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentClosingDateFacades;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentClosingDateFacade
    {
        Task<int> Update(int id, GarmentClosingDateFacade m, string user, int clientTimeZoneOffset = 7);
    }
}
