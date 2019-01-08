using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class PurchaseOrderExternalItemMongo : MongoBaseModel
    {
        public string no { get; set; }

        public string refNo { get; set; }

        public ObjectId purchaseRequestId { get; set; }

        public PurchaseRequestMongo purchaseRequest { get; set; }

        public dynamic purchaseOrderExternalId { get; set; }

        public PurchaseOrderExternalMongo purchaseOrderExternal { get; set; }

        public dynamic supplierId { get; set; }

        public SupplierMongo supplier { get; set; }

        public ObjectId unitId { get; set; }

        public UnitMongo unit { get; set; }

        public ObjectId categoryId { get; set; }

        public CategoryMongo category { get; set; }

        public string freightCostBy { get; set; }

        public CurrencyMongo currency { get; set; }

        public double currencyRate { get; set; }

        public string paymentMethod { get; set; }

        public int paymentDueDays { get; set; }

        public VatMongo vat { get; set; }

        public bool useVat { get; set; }

        public double vatRate { get; set; }

        public bool useIncomeTax { get; set; }

        public DateTime date { get; set; }

        public DateTime expectedDeliveryDate { get; set; }

        public DateTime actualDeliveryDate { get; set; }

        public bool isPosted { get; set; }

        public bool isClosed { get; set; }

        public string remark { get; set; }

        public List<PurchaseOrderExternalDetailMongo> items { get; set; }

        public Status status { get; set; }
        public string iso { get; set; }
        public dynamic realizationOrderId { get; set; }
        public dynamic realizationOrder { get; set; }
        public dynamic buyerId { get; set; }
        public dynamic buyer { get; set; }
        public dynamic sourcePurchaseOrderId { get; set; }
        public dynamic sourcePurchaseOrder { get; set; }
    }
    
}
