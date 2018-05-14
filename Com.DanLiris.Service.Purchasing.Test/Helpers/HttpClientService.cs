using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.Helpers
{
    public class HttpClientTestService : HttpClient
    {
        public static string Token;

        public HttpClientTestService(IServiceProvider serviceProvider)
        {
            var User = new { username = "dev2", password = "Standar123" };

            var response = this.PostAsync("http://localhost:5000/v1/authenticate", new StringContent(JsonConvert.SerializeObject(User).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
            response.EnsureSuccessStatusCode();

            var data = response.Content.ReadAsStringAsync();
            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.Result.ToString());
            var token = result["data"].ToString();
            Token = token;

            this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            IdentityService identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            identityService.Token = Token;
            identityService.Username = "Unit Test";
        }
    }
}
