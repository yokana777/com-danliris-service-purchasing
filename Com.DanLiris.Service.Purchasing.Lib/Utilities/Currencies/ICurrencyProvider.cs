using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies
{
    public interface ICurrencyProvider
    {
        Task<Currency> GetCurrencyByCurrencyCode(string currencyCode);
        Task<List<Currency>> GetCurrencyByCurrencyCodeList(List<string> currencyCodeList);
    }
}
