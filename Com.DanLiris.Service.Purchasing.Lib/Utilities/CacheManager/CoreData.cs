using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager.CacheData;
//using Com.DanLiris.Service.Purchasing.WebApi.Helpers
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager
{
    public class CoreData : ICoreData
    {
        private readonly ICoreHttpClientService _http;
        private readonly IMemoryCacheManager _cacheManager;

        public CoreData(ICoreHttpClientService http, IMemoryCacheManager cacheManager)
        {
            _http = http;
            _cacheManager = cacheManager;
        }

        public void SetCategoryCOA()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var token = GetTokenAsync().Result;

            var categoryUri = APIEndpoint.Core + $"master/categories?size={int.MaxValue}";
            //var masterUnitUri = $"https://com-danliris-service-core-dev.azurewebsites.net/v1/master/units/simple";
            var categoryResponse = _http.GetAsync(categoryUri, token).Result;

            var categoryResult = new BaseResponse<List<CategoryCOAResult>>()
            {
                data = new List<CategoryCOAResult>()
            };
            if (categoryResponse.IsSuccessStatusCode)
            {
                categoryResult = JsonConvert.DeserializeObject<BaseResponse<List<CategoryCOAResult>>>(categoryResponse.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }
            //else
            //{
            //    SetCategoryCOA();
            //}

            //if (categoryResult.data.Count > 0)
            _cacheManager.Set(MemoryCacheConstant.Categories, categoryResult.data);
        }

        public void SetDivisionCOA()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var token = GetTokenAsync().Result;

            var categoryUri = APIEndpoint.Core + $"master/divisions?size={int.MaxValue}";
            //var masterUnitUri = $"https://com-danliris-service-core-dev.azurewebsites.net/v1/master/units/simple";
            var categoryResponse = _http.GetAsync(categoryUri, token).Result;

            var categoryResult = new BaseResponse<List<IdCOAResult>>()
            {
                data = new List<IdCOAResult>()
            };
            if (categoryResponse.IsSuccessStatusCode)
            {
                categoryResult = JsonConvert.DeserializeObject<BaseResponse<List<IdCOAResult>>>(categoryResponse.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }
            //else
            //{
            //    SetDivisionCOA();
            //}

            //if (categoryResult.data.Count > 0)
            _cacheManager.Set(MemoryCacheConstant.Divisions, categoryResult.data);
        }

        public void SetPPhCOA()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var token = GetTokenAsync().Result;

            var incomeTaxUri = APIEndpoint.Core + $"master/income-taxes?size={int.MaxValue}";
            //var masterUnitUri = $"https://com-danliris-service-core-dev.azurewebsites.net/v1/master/units/simple";
            var incomeTaxResponse = _http.GetAsync(incomeTaxUri, token).Result;

            var incomeTaxResult = new BaseResponse<List<IncomeTaxCOAResult>>()
            {
                data = new List<IncomeTaxCOAResult>()
            };
            if (incomeTaxResponse.IsSuccessStatusCode)
            {
                incomeTaxResult = JsonConvert.DeserializeObject<BaseResponse<List<IncomeTaxCOAResult>>>(incomeTaxResponse.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }
            //else
            //{
            //    SetUnitCOA();
            //}

            //if (categoryResult.data.Count > 0)
            _cacheManager.Set(MemoryCacheConstant.IncomeTaxes, incomeTaxResult.data);
        }

        public void SetUnitCOA()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var token = GetTokenAsync().Result;

            var categoryUri = APIEndpoint.Core + $"master/units?size={int.MaxValue}";
            //var masterUnitUri = $"https://com-danliris-service-core-dev.azurewebsites.net/v1/master/units/simple";
            var categoryResponse = _http.GetAsync(categoryUri, token).Result;

            var categoryResult = new BaseResponse<List<IdCOAResult>>()
            {
                data = new List<IdCOAResult>()
            };
            if (categoryResponse.IsSuccessStatusCode)
            {
                categoryResult = JsonConvert.DeserializeObject<BaseResponse<List<IdCOAResult>>>(categoryResponse.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
            }
            //else
            //{
            //    SetUnitCOA();
            //}

            //if (categoryResult.data.Count > 0)
            _cacheManager.Set(MemoryCacheConstant.Units, categoryResult.data);
        }

        protected async Task<string> GetTokenAsync()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var response = await _http.PostAsync(APIEndpoint.Auth + "authenticate",
                new StringContent(JsonConvert.SerializeObject(new { username = AuthCredential.Username, password = AuthCredential.Password }), Encoding.UTF8, "application/json"));
            var tokenResult = new BaseResponse<string>();
            if (response.IsSuccessStatusCode)
            {
                tokenResult = JsonConvert.DeserializeObject<BaseResponse<string>>(await response.Content.ReadAsStringAsync(), jsonSerializerSettings);
            }
            //else
            //{
            //    await GetTokenAsync();
            //}
            return tokenResult.data;
        }
    }

    public interface ICoreData
    {
        void SetCategoryCOA();
        void SetDivisionCOA();
        void SetUnitCOA();
        void SetPPhCOA();
    }
}
