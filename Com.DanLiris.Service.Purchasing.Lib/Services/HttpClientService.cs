using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Com.DanLiris.Service.Purchasing.Lib.Services
{
    public class HttpClientService : HttpClient
    {
        public HttpClientService(IdentityService identityService)
        {
            this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, identityService.Token);
        }
    }
}
