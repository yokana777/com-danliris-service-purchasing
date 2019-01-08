using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class VatMongo : MongoBaseModel
    {
        public string name { get; set; }

        public double rate { get; set; }

        public string description { get; set; }
    }
}
