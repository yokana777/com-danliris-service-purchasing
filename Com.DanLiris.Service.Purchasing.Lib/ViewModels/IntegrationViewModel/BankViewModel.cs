using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel
{
    public class BankViewModel
    {
        public string _id { get; set; }
        public string code { get; set; }
        public string bankName { get; set; }
        public string bankCode { get; set; }
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public CurrencyViewModel currency { get; set; }
    }
}
