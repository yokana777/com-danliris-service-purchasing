using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Services.GarmentDebtBalance
{
    public interface IGarmentDebtBalanceService
    {
        Task<int> CreateFromCustoms(CustomsFormDto form);
    }
}
