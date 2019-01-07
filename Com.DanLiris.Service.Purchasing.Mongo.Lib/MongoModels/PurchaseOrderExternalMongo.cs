using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class PurchaseOrderExternalMongo : MongoBaseModel
    {
        public string no { get; set; }

        public string refNo { get; set; }

        public ObjectId supplierId { get; set; }

        public SupplierMongo supplier { get; set; }

        public string freightCostBy { get; set; }

        public CurrencyMongo currency { get; set; }

        public int currencyRate { get; set; }

        public string paymentMethod { get; set; }

        public int paymentDueDays { get; set; }

        public object vat { get; set; }

        public bool useVat { get; set; }

        public int vatRate { get; set; }

        public bool useIncomeTax { get; set; }

        public DateTime date { get; set; }

        public DateTime expectedDeliveryDate { get; set; }

        public DateTime actualDeliveryDate { get; set; }

        public bool isPosted { get; set; }

        public bool isClosed { get; set; }

        public string remark { get; set; }

        public List<PurchaseOrderExternalItemMongo> items { get; set; }

        public Status status { get; set; }
    }
}
