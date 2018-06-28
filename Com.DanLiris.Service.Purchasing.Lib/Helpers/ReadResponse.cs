using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse
{
    public class ReadResponse
    {
        public List<object> Data { get; set; }
        public int TotalData { get; set; }
        public Dictionary<string, string> Order { get; set; }

        public ReadResponse(List<object> Data, int TotalData, Dictionary<string, string> Order)
        {
            this.Data = Data;
            this.TotalData = TotalData;
            this.Order = Order;
        }
    }
}
