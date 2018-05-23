using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Report
{
    public class ImportPurchasingBookReportFacade
    {
        IMongoCollection<BsonDocument> collection;
        IMongoCollection<BsonDocument> collectionUnitPaymentOrder;

        FilterDefinitionBuilder<BsonDocument> filterBuilder;

        public ImportPurchasingBookReportFacade()
        {
            MongoDbContext mongoDbContext = new MongoDbContext();
            collection = mongoDbContext.UnitReceiptNote;
            collectionUnitPaymentOrder = mongoDbContext.UnitPaymentOrder;

            filterBuilder = Builders<BsonDocument>.Filter;
        }

        public Tuple<List<UnitReceiptNoteViewModel>, int> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            List<FilterDefinition<BsonDocument>> filter = new List<FilterDefinition<BsonDocument>>
            {
                filterBuilder.Eq("_deleted", false),
                filterBuilder.Eq("supplier.import", true)
            };

            if (no != null)
                filter.Add(filterBuilder.Eq("no", no));
            if (unit != null)
                filter.Add(filterBuilder.Eq("unit.code", unit));
            if (category != null)
                filter.Add(filterBuilder.Eq("items.purchaseOrder.category.code", category));
            if (dateFrom != null && dateTo != null)
                filter.Add(filterBuilder.And(filterBuilder.Gte("date", dateFrom), filterBuilder.Lte("date", dateTo)));

            List<BsonDocument> ListData = collection.Find(filterBuilder.And(filter)).ToList();
            //List<BsonDocument> ListData = collection.Aggregate()
            //    .Match(filterBuilder.And(filter))
            //    .ToList();

            List<UnitReceiptNoteViewModel> Data = new List<UnitReceiptNoteViewModel>();

            foreach (var data in ListData)
            {
                List<UnitReceiptNoteItemViewModel> Items = new List<UnitReceiptNoteItemViewModel>();
                foreach (var item in data.GetValue("items").AsBsonArray)
                {
                    var itemDocument = item.AsBsonDocument;
                    Items.Add(new UnitReceiptNoteItemViewModel
                    {
                        deliveredQuantity = GetBsonValue.ToDouble(itemDocument, "deliveredQuantity"),
                        pricePerDealUnit = GetBsonValue.ToDouble(itemDocument, "pricePerDealUnit"),
                        currencyRate = GetBsonValue.ToDouble(itemDocument, "currencyRate"),
                        product = new ProductViewModel
                        {
                            name = GetBsonValue.ToString(itemDocument, "product.name")
                        },
                        purchaseOrder = new PurchaseOrderViewModel
                        {
                            category = new CategoryViewModel
                            {
                                name = GetBsonValue.ToString(itemDocument, "purchaseOrder.category.code")
                            }
                        },
                    });
                }
                var UnitReceiptNoteNo = GetBsonValue.ToString(data, "no");
                var dataUnitPaymentOrder = collectionUnitPaymentOrder.Find(filterBuilder.Eq("items.unitReceiptNote.no", UnitReceiptNoteNo)).FirstOrDefault();
                Data.Add(new UnitReceiptNoteViewModel
                {
                    no = UnitReceiptNoteNo,
                    date = data.GetValue("date").ToUniversalTime(),
                    unit = new UnitViewModel
                    {
                        name = GetBsonValue.ToString(data, "unit.name")
                    },
                    pibNo = dataUnitPaymentOrder != null ? GetBsonValue.ToString(dataUnitPaymentOrder, "pibNo", new BsonString("-")) : "-",
                    items = Items,
                });
            }

            return Tuple.Create(Data, Data.Count);
        }

        // JSON ora iso nge-cast
        public Tuple<List<BsonDocument>, int> GetReport()
        {
            IMongoCollection<BsonDocument> collection = new MongoDbContext().UnitReceiptNote;
            List<BsonDocument> ListData = collection.Aggregate().ToList();

            return Tuple.Create(ListData, ListData.Count);
        }

        public MemoryStream GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            Tuple<List<UnitReceiptNoteViewModel>, int> Data = this.GetReport(no, unit, category, dateFrom, dateTo);

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "TGL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR NOTA", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TIPE", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "UNIT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO PIB", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NILAI", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "RATE", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "TOTAL", DataType = typeof(double) });

            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            if (Data.Item2 == 0)
            {
                result.Rows.Add("", "", "", "", "", "", 0, 0, 0); // to allow column name to be generated properly for empty data as template
            }
            else
            {
                Dictionary<string, List<UnitReceiptNoteViewModel>> dataByCategory = new Dictionary<string, List<UnitReceiptNoteViewModel>>();
                Dictionary<string, double> subTotalCategory = new Dictionary<string, double>();

                foreach (UnitReceiptNoteViewModel data in Data.Item1)
                {
                    foreach (UnitReceiptNoteItemViewModel item in data.items)
                    {
                        string categoryName = item.purchaseOrder.category.name;

                        if (!dataByCategory.ContainsKey(categoryName)) dataByCategory.Add(categoryName, new List<UnitReceiptNoteViewModel> { });
                        dataByCategory[categoryName].Add(new UnitReceiptNoteViewModel
                        {
                            no = data.no,
                            date = data.date,
                            pibNo = data.pibNo,
                            unit = data.unit,
                            items = new List<UnitReceiptNoteItemViewModel>() { item }
                        });

                        if (!subTotalCategory.ContainsKey(categoryName)) subTotalCategory.Add(categoryName, 0);
                        subTotalCategory[categoryName] += item.deliveredQuantity;
                    }
                }

                double total = 0;
                int rowPosition = 1;

                foreach (KeyValuePair<string, List<UnitReceiptNoteViewModel>> categoryName in dataByCategory)
                {
                    foreach (UnitReceiptNoteViewModel data in categoryName.Value)
                    {
                        UnitReceiptNoteItemViewModel item = data.items[0];
                        result.Rows.Add(data.date.ToString("dd MMM yyyy", new CultureInfo("id-ID")), data.no, item.product.name, item.purchaseOrder.category.name, data.unit.name, data.pibNo, item.pricePerDealUnit * item.deliveredQuantity, item.currencyRate, item.pricePerDealUnit * item.deliveredQuantity * item.currencyRate);
                        rowPosition += 1;
                    }
                    result.Rows.Add("SUB TOTAL", "", "", "", "", "", 0, 0, subTotalCategory[categoryName.Key]);
                    rowPosition += 1;

                    mergeCells.Add(($"A{rowPosition}:H{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

                    total += subTotalCategory[categoryName.Key];
                }
                result.Rows.Add("TOTAL", "", "", "", "", "", 0, 0, total);
                rowPosition += 1;

                mergeCells.Add(($"A{rowPosition}:H{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));
            }

            return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
        }

        public void InsertToMongo(BsonDocument document)
        {
            collection.InsertOne(document);
        }
        public void DeleteDataMongo(BsonObjectId id)
        {
            collection.DeleteOne(filterBuilder.Eq("_id" , id));
        }
    }
}
