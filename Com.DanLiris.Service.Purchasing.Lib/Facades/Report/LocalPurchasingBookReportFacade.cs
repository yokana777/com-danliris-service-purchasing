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
using OfficeOpenXml;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Report
{
    public class LocalPurchasingBookReportFacade
    {
        IMongoCollection<BsonDocument> collection;
        IMongoCollection<BsonDocument> collectionUnitPaymentOrder;

        FilterDefinitionBuilder<BsonDocument> filterBuilder;

        public LocalPurchasingBookReportFacade()
        {
            MongoDbContext mongoDbContext = new MongoDbContext();
            collection = mongoDbContext.UnitReceiptNote;
            collectionUnitPaymentOrder = mongoDbContext.UnitPaymentOrder;

            filterBuilder = Builders<BsonDocument>.Filter;
        }

        public Tuple<List<UnitReceiptNoteViewModel>, int> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            FilterDefinitionBuilder<UnitReceiptNoteViewModel> filterBuilderUnitReceiptNote = Builders<UnitReceiptNoteViewModel>.Filter;
            List<FilterDefinition<UnitReceiptNoteViewModel>> filter = new List<FilterDefinition<UnitReceiptNoteViewModel>>
            {
                filterBuilderUnitReceiptNote.Eq("_deleted", false),
                filterBuilderUnitReceiptNote.Eq("supplier.import", false)
            };

            if (no != null)
                filter.Add(filterBuilderUnitReceiptNote.Eq("no", no));
            if (unit != null)
                filter.Add(filterBuilderUnitReceiptNote.Eq("unit.code", unit));
            if (category != null)
                filter.Add(filterBuilderUnitReceiptNote.Eq("items.purchaseOrder.category.code", category));
            if (dateFrom != null && dateTo != null)
                filter.Add(filterBuilderUnitReceiptNote.And(filterBuilderUnitReceiptNote.Gte("date", dateFrom), filterBuilderUnitReceiptNote.Lte("date", dateTo)));

            IMongoCollection<UnitReceiptNoteViewModel> collection = new MongoDbContext().UnitReceiptNoteViewModel;
            List<UnitReceiptNoteViewModel> ListData = collection.Find(filterBuilderUnitReceiptNote.And(filter)).ToList();
            //List<UnitReceiptNoteViewModel> ListData = collection.Aggregate()
            //    .Match(filterBuilder.And(filter))
            //    .ToList();

            foreach (var data in ListData)
            {
                var dataUnitPaymentOrder = collectionUnitPaymentOrder.Find(filterBuilder.Eq("items.unitReceiptNote.no", data.no)).FirstOrDefault();
                data.incomeTaxNo = dataUnitPaymentOrder != null ? GetBsonValue.ToString(dataUnitPaymentOrder, "incomeTaxNo", new BsonString("-")) : "-";
            }

            return Tuple.Create(ListData, ListData.Count);
        }

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
            result.Columns.Add(new DataColumn() { ColumnName = "NO FAKTUR PAJAK", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TIPE", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "UNIT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PEMBELIAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = " ", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TOTAL", DataType = typeof(double) });

            result.Rows.Add("", "", "", "", "", "", "DPP", "PPN", 0);

            ExcelPackage package = new ExcelPackage();
            foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Report") })
            {
                var sheet = package.Workbook.Worksheets.Add(item.Value);
                sheet.Cells["A1:A2"].Merge = true;
                sheet.Cells["B1:B2"].Merge = true;
                sheet.Cells["C1:C2"].Merge = true;
                sheet.Cells["D1:D2"].Merge = true;
                sheet.Cells["E1:E2"].Merge = true;
                sheet.Cells["F1:F2"].Merge = true;
                sheet.Cells["G1:H1"].Merge = true;
                sheet.Cells["I1:I2"].Merge = true;
            }

            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            if (Data.Item2 == 0)
            {
                result.Rows.Add("", "", "", "", "", "", "", "", 0); // to allow column name to be generated properly for empty data as template
            }
            else
            {
                Dictionary<string, List<UnitReceiptNoteViewModel>> dataByCategory = new Dictionary<string, List<UnitReceiptNoteViewModel>>();
                Dictionary<string, double> subTotalDPPCategory = new Dictionary<string, double>();
                Dictionary<string, double> subTotalPPNCategory = new Dictionary<string, double>();
                Dictionary<string, double> subTotalCategory = new Dictionary<string, double>();
                Dictionary<string, double> checkIncomeTax = new Dictionary<string, double>();

                foreach (UnitReceiptNoteViewModel data in Data.Item1)
                {
                    foreach (UnitReceiptNoteItemViewModel item in data.items)
                    {
                        string categoryCode = item.purchaseOrder.category.code;

                        if (!dataByCategory.ContainsKey(categoryCode)) dataByCategory.Add(categoryCode, new List<UnitReceiptNoteViewModel> { });
                        dataByCategory[categoryCode].Add(new UnitReceiptNoteViewModel
                        {
                            no = data.no,
                            date = data.date,
                            incomeTaxNo = data.incomeTaxNo,
                            unit = data.unit,
                            items = new List<UnitReceiptNoteItemViewModel>() { item }
                        });

                        if (!subTotalCategory.ContainsKey(categoryCode))
                        {
                            subTotalCategory.Add(categoryCode, 0);
                            subTotalDPPCategory.Add(categoryCode, 0);
                            subTotalPPNCategory.Add(categoryCode, 0);
                        }
                        
                        if (item.purchaseOrder.useIncomeTax == true)
                        {
                            checkIncomeTax[categoryCode] = 1;
                            subTotalCategory[categoryCode] += (item.pricePerDealUnit * item.deliveredQuantity) + (item.pricePerDealUnit * item.deliveredQuantity * 10 / 100);
                        }
                        else
                        {
                            checkIncomeTax[categoryCode] = 0;
                            subTotalCategory[categoryCode] += (item.pricePerDealUnit * item.deliveredQuantity);
                        }
                        subTotalDPPCategory[categoryCode] += item.pricePerDealUnit * item.deliveredQuantity;
                        subTotalPPNCategory[categoryCode] += item.pricePerDealUnit * item.deliveredQuantity * 10/100 * checkIncomeTax[categoryCode];
                        
                    }
                }

                double totalPPN = 0;
                double totalDPP = 0;
                double total = 0;
                int rowPosition = 2;
                          

                foreach (KeyValuePair<string, List<UnitReceiptNoteViewModel>> categoryCode in dataByCategory)
                {
                    string catCode="";
                    foreach (UnitReceiptNoteViewModel data in categoryCode.Value)
                    {
                        UnitReceiptNoteItemViewModel item = data.items[0];
                        if (item.purchaseOrder.useIncomeTax == true)
                        {
                            result.Rows.Add(data.date.ToString("dd MMM yyyy", new CultureInfo("id-ID")), data.no, item.product.name, data.incomeTaxNo, item.purchaseOrder.category.code, data.unit.name, String.Format("{0:0.00}", (item.pricePerDealUnit * item.deliveredQuantity)), String.Format("{0:0.00}", (item.pricePerDealUnit * item.deliveredQuantity * 0.1)), Math.Round((double)((item.pricePerDealUnit * item.deliveredQuantity) + ((item.pricePerDealUnit * item.deliveredQuantity) * 0.1)),2));
                        }
                        else
                        {
                            result.Rows.Add(data.date.ToString("dd MMM yyyy", new CultureInfo("id-ID")), data.no, item.product.name, data.incomeTaxNo, item.purchaseOrder.category.code, data.unit.name, String.Format("{0:0.00}", (item.pricePerDealUnit * item.deliveredQuantity)), "-", Math.Round((double)(item.pricePerDealUnit * item.deliveredQuantity),2));
                        }                        
                        rowPosition += 1;
                        catCode = item.purchaseOrder.category.code;
                    }
                    if (subTotalPPNCategory[categoryCode.Key] == 0)
                    {
                        result.Rows.Add("SUB TOTAL", "", "", "", catCode, "", String.Format("{0:0.00}", subTotalDPPCategory[categoryCode.Key]), "-", Math.Round((double)subTotalCategory[categoryCode.Key],2));
                    } else
                    {
                        result.Rows.Add("SUB TOTAL", "", "", "", catCode, "", String.Format("{0:0.00}", subTotalDPPCategory[categoryCode.Key]), String.Format("{0:0.00}", subTotalPPNCategory[categoryCode.Key]), Math.Round((double)subTotalCategory[categoryCode.Key],2));
                    }
                    rowPosition += 1;
                    
                    mergeCells.Add(($"A{rowPosition}:D{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

                    totalDPP += subTotalDPPCategory[categoryCode.Key];
                    totalPPN += subTotalPPNCategory[categoryCode.Key];
                    total += subTotalCategory[categoryCode.Key];
                }
                if (totalPPN == 0)
                {
                    result.Rows.Add("TOTAL", "", "", "", "", "", String.Format("{0:0.00}", totalDPP), "-", Math.Round((double)total, 2));
                }
                else
                {
                    result.Rows.Add("TOTAL", "", "", "", "", "", String.Format("{0:0.00}", totalDPP), String.Format("{0:0.00}", totalPPN), Math.Round((double)total,2));
                }
                rowPosition += 1;
                mergeCells.Add(($"A{rowPosition}:D{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));
            }
            mergeCells.Add(($"A{1}:A{2}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
            mergeCells.Add(($"B{1}:B{2}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
            mergeCells.Add(($"C{1}:C{2}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
            mergeCells.Add(($"D{1}:D{2}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
            mergeCells.Add(($"E{1}:E{2}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
            mergeCells.Add(($"F{1}:F{2}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
            mergeCells.Add(($"G{1}:H{1}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));
            mergeCells.Add(($"I{1}:I{2}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Center, OfficeOpenXml.Style.ExcelVerticalAlignment.Center));

            return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
        }

        public void InsertToMongo(BsonDocument document)
        {
            collection.InsertOne(document);
        }
        public void DeleteDataMongoByNo(string no)
        {
            collection.DeleteOne(filterBuilder.Eq("no", no));
        }
    }
}