using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class BuyerMongo : MongoBaseModel
    {
        public string code { get; set; }

        public string name { get; set; }

        public string address { get; set; }

        public string city { get; set; }

        public string country { get; set; }

        public string contact { get; set; }

        public string tempo { get; set; }

        public string type { get; set; }

        public string NPWP { get; set; }
    }
}
