using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager.CacheData;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies
{
    public class CurrencyProvider : ICurrencyProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Currency> GetCurrencyByCurrencyCode(string currencyCode)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));

            var currencyUri = APIEndpoint.Core + $"master/garment-currencies/single-by-code/{currencyCode}";
            var currencyResponse = await httpClient.GetAsync(currencyUri);

            var currencyResult = new BaseResponse<Currency>()
            {
                data = new Currency()
            };

            if (currencyResponse.IsSuccessStatusCode)
            {
                currencyResult = JsonConvert.DeserializeObject<BaseResponse<Currency>>(currencyResponse.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }

            return currencyResult.data;
        }
    }
}
