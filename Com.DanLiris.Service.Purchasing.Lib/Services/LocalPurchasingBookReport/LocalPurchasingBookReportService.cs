using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Services.LocalPurchasingBookReport
{
    public class LocalPurchasingBookReportService
    {

        public Tuple<List<object>, int> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            IMongoCollection<BsonDocument> collection = new MongoDbContext().UnitReceiptNote;
            IMongoCollection<BsonDocument> collectionUnitPaymentOrder = new MongoDbContext().UnitPaymentOrder;

            FilterDefinitionBuilder<BsonDocument> filterBuilder = Builders<BsonDocument>.Filter;
            List<FilterDefinition<BsonDocument>> filter = new List<FilterDefinition<BsonDocument>>
            {
                filterBuilder.Eq("_deleted", false),
                filterBuilder.Eq("supplier.import", false)
            };

            if (no != null)
                filter.Add(filterBuilder.Eq("no", no));
            if (unit != null)
                filter.Add(filterBuilder.Eq("unit.code", unit));
            if (category != null)
                filter.Add(filterBuilder.Eq("items.purchaseOrder.category.code", category));
            if (dateFrom != null && dateTo != null)
                filter.Add(filterBuilder.And(filterBuilder.Gte("date", dateFrom), filterBuilder.Lte("date", dateTo)));

            List<BsonDocument> ListData = collection.Aggregate()
                .Match(filterBuilder.And(filter))
                .ToList();

            List<object> Data = new List<object>();

            foreach (var data in ListData)
            {
                List<object> Items = new List<object>();
                foreach (var item in data.GetValue("items").AsBsonArray)
                {
                    var itemDocument = item.AsBsonDocument;
                    Items.Add(new
                    {
                        DeliveredQuantity = itemDocument.GetValue("deliveredQuantity").AsBsonValue,
                        PricePerDealUnit = itemDocument.GetValue("pricePerDealUnit").AsBsonValue,
                        
                        Product = new
                        {
                            Name = itemDocument.GetValue("product").AsBsonDocument.GetValue("name").AsBsonValue
                        },
                        PurchaseOrder = new
                        {
                            Category = new
                            {
                                Name = itemDocument.GetValue("purchaseOrder").AsBsonDocument.GetValue("category").AsBsonDocument.GetValue("code").AsBsonValue
                            }
                        },
                    });
                }
                var UnitReceiptNoteNo = data.GetValue("no").AsBsonValue;
                var dataUnitPaymentOrder = collectionUnitPaymentOrder.Find(filterBuilder.Eq("items.unitReceiptNote.no", UnitReceiptNoteNo.ToString())).FirstOrDefault();
                Data.Add(new
                {
                    No = data.GetValue("no").AsBsonValue,
                    Date = data.GetValue("date").AsBsonValue,
                    Unit = new
                    {
                        Name = data.GetValue("unit").AsBsonDocument.GetValue("name").AsBsonValue
                    },
                    SPB = dataUnitPaymentOrder != null ? dataUnitPaymentOrder.GetValue("incomeTaxNo", new BsonString("-")).AsBsonValue : "-",
                    UnitReceiptNoteItems = Items,
                });
            }

            return Tuple.Create(Data, Data.Count);
        }

        public Tuple<List<BsonDocument>, int> GetReport()
        {
            IMongoCollection<BsonDocument> collection = new MongoDbContext().UnitReceiptNote;
            List<BsonDocument> ListData = collection.Aggregate().ToList();

            return Tuple.Create(ListData, ListData.Count);
        }
    }
}
