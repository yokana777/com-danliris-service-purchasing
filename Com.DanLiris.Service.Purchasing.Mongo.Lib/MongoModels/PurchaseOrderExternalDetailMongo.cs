using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class PurchaseOrderExternalDetailMongo : MongoBaseModel
    {
        public ProductMongo product { get; set; }

        public double defaultQuantity { get; set; }

        public UomMongo defaultUom { get; set; }

        public double dealQuantity { get; set; }

        public UomMongo dealUom { get; set; }

        public double realizationQuantity { get; set; }

        public double pricePerDealUnit { get; set; }

        public double priceBeforeTax { get; set; }

        public CurrencyMongo currency { get; set; }

        public double currencyRate { get; set; }

        public double conversion { get; set; }

        public bool isClosed { get; set; }

        public bool useIncomeTax { get; set; }

        public string remark { get; set; }

        public List<object> fulfillments { get; set; }
    }
}
