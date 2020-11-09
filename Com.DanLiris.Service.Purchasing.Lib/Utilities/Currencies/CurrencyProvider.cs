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

        public async Task<Currency> GetCurrencyByCurrencyCodeDate(string currencyCode, DateTimeOffset date)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));

            var currencyUri = APIEndpoint.Core + $"master/garment-currencies/single-by-code-date?code={currencyCode}&stringDate={date.DateTime.ToString("yyyy-MM-dd")}";
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

        public async Task<List<Currency>> GetCurrencyByCurrencyCodeDateList(IEnumerable<Tuple<string, DateTimeOffset>> currencyTuples)
        {
            var tasks = currencyTuples.Select(s => GetCurrencyByCurrencyCodeDate(s.Item1, s.Item2));

            var result = await Task.WhenAll(tasks);

            return result.ToList();
        }

        public async Task<Unit> GetUnitById(int id)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));

            var uri = APIEndpoint.Core + $"master/units/{id}";
            var response = await httpClient.GetAsync(uri);

            var result = new BaseResponse<Unit>()
            {
                data = new Unit()
            };

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<BaseResponse<Unit>>(response.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }

            return result.data;
        }

        public async Task<AccountingUnit> GetAccountingUnitById(int id)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));

            var uri = APIEndpoint.Core + $"master/accounting-units/{id}";
            var response = await httpClient.GetAsync(uri);

            var result = new BaseResponse<AccountingUnit>()
            {
                data = new AccountingUnit()
            };

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<BaseResponse<AccountingUnit>>(response.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }

            return result.data;
        }

        public async Task<List<AccountingUnit>> GetAccountingUnitsByUnitIds(List<int> unitIds)
        {
            var unitTasks = unitIds.Select(unitId => GetUnitById(unitId));
            var unitTaskResult = await Task.WhenAll(unitTasks);
            var units = unitTaskResult.ToList();

            var accountingUnitTasks = units.Select(unit => GetAccountingUnitById(unit.AccountingUnitId));
            var accountingUnitTaskResult = await Task.WhenAll(accountingUnitTasks);

            return accountingUnitTaskResult.ToList();
        }

        public async Task<List<Unit>> GetUnitsByUnitIds(List<int> unitIds)
        {
            var unitTasks = unitIds.Select(unitId => GetUnitById(unitId));
            var unitTaskResult = await Task.WhenAll(unitTasks);

            return unitTaskResult.ToList();
        }

        public async Task<Category> GetCategoryById(int id)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));

            var uri = APIEndpoint.Core + $"master/categories/{id}";
            var response = await httpClient.GetAsync(uri);

            var result = new BaseResponse<Category>()
            {
                data = new Category()
            };

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<BaseResponse<Category>>(response.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }

            return result.data;
        }

        public async Task<AccountingCategory> GetAccountingCategoryById(int id)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));

            var uri = APIEndpoint.Core + $"master/accounting-categories/{id}";
            var response = await httpClient.GetAsync(uri);

            var result = new BaseResponse<AccountingCategory>()
            {
                data = new AccountingCategory()
            };

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<BaseResponse<AccountingCategory>>(response.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }

            return result.data;
        }

        public async Task<List<AccountingCategory>> GetAccountingCategoriesByCategoryIds(List<int> categoryIds)
        {
            var categoryTasks = categoryIds.Select(unitId => GetCategoryById(unitId));
            var categoryTaskResult = await Task.WhenAll(categoryTasks);
            var categories = categoryTaskResult.ToList();

            var accountingCategoryTasks = categories.Select(unit => GetAccountingCategoryById(unit.AccountingCategoryId));
            var accountingCategoryTaskResult = await Task.WhenAll(accountingCategoryTasks);

            return accountingCategoryTaskResult.ToList();
        }

        public async Task<List<Category>> GetCategoriesByCategoryIds(List<int> categoryIds)
        {
            var categoryTasks = categoryIds.Select(unitId => GetCategoryById(unitId));
            var categoryTaskResult = await Task.WhenAll(categoryTasks);

            return categoryTaskResult.ToList();
        }
    }
}
