using Dapper;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _config;

        public CurrencyProvider(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public async Task<Currency> GetCurrencyByCurrencyCode(string currencyCode)
        {
            using (var conn = Connection)
            {
                string query = "SELECT * FROM kurs WHERE code = @code ORDER BY date DESC";
                conn.Open();
                var result = await conn.QueryAsync<Currency>(query, new { code = currencyCode });
                return result.FirstOrDefault();
            }
        }
    }
}
