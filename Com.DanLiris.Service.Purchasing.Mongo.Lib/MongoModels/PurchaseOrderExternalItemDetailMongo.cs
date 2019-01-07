using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class PurchaseOrderExternalItemDetailMongo : MongoBaseModel
    {
        public ProductMongo product { get; set; }

        public int defaultQuantity { get; set; }

        public UomMongo defaultUom { get; set; }

        public int dealQuantity { get; set; }

        public UomMongo dealUom { get; set; }

        public int realizationQuantity { get; set; }

        public int pricePerDealUnit { get; set; }

        public int priceBeforeTax { get; set; }

        public CurrencyMongo currency { get; set; }

        public int currencyRate { get; set; }

        public int conversion { get; set; }

        public bool isClosed { get; set; }

        public bool useIncomeTax { get; set; }

        public string remark { get; set; }

        public List<object> fulfillments { get; set; }
    }
}
