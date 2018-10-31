using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Report
{
    public class ImportPurchasingBookReportFacade
    {
		private readonly PurchasingDbContext dbContext;
		public readonly IServiceProvider serviceProvider;
		private readonly DbSet<UnitReceiptNote> dbSet;


		public ImportPurchasingBookReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
		{
			//MongoDbContext mongoDbContext = new MongoDbContext();
			//collection = mongoDbContext.UnitReceiptNote;
			//collectionUnitPaymentOrder = mongoDbContext.UnitPaymentOrder;

			//filterBuilder = Builders<BsonDocument>.Filter;
			this.serviceProvider = serviceProvider;
			this.dbContext = dbContext;
			this.dbSet = dbContext.Set<UnitReceiptNote>();
		}

        public IEnumerable<ImportPurchasingBookViewModel> GetReportQuery(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Data = (from a in dbContext.InternalPurchaseOrders
                        join b in dbContext.ExternalPurchaseOrderItems on a.Id equals b.POId
                        join c in dbContext.ExternalPurchaseOrders on b.EPOId equals c.Id
                        join d in dbContext.ExternalPurchaseOrderDetails on b.Id equals d.EPOItemId
                        join e in dbContext.DeliveryOrderItems on c.Id equals e.EPOId
                        join f in dbContext.DeliveryOrders on e.DOId equals f.Id
                        join g in dbContext.UnitReceiptNotes on f.Id equals g.DOId
                        join h in dbContext.UnitReceiptNoteItems on new { gId = g.Id, dId = d.Id } equals new { gId = h.URNId, dId = h.EPODetailId }
                        where g.IsDeleted == false && g.URNNo.Contains("BPI")
                            && ((d1 != new DateTime(1970, 1, 1)) ? (g.ReceiptDate.Date >= d1 && g.ReceiptDate.Date <= d2) : true)
                            && ((category != null) ? (a.CategoryCode == category) : true)
                            && ((unit != null) ? (g.UnitCode == unit) : true)
                            && ((no != null) ? (g.URNNo == no) : true)
                        select new
                        {
                            //g.Id,
                            g.URNNo,
                            g.ReceiptDate,
                            h.ProductName,
                            g.UnitName,
                            a.CategoryName,
                            PIBNo = g.IsPaid == false ? "-" : (
                                    from o in dbContext.UnitPaymentOrders
                                    join uo in dbContext.UnitPaymentOrderItems on o.Id equals uo.UPOId
                                    where uo.URNId == g.Id
                                    select o.PibNo
                                )
                                .FirstOrDefault() ?? "-",
                            amount = h.PricePerDealUnit * h.ReceiptQuantity,
                            amountIDR = h.PricePerDealUnit * h.ReceiptQuantity * c.CurrencyRate,
                            c.CurrencyRate
                        })
                        .Distinct()
                        .ToList();

            var Query = from data in Data
                        group data by new { data.URNNo, data.ReceiptDate, data.ProductName, data.UnitName, data.CategoryName, data.PIBNo, data.CurrencyRate } into groupData
                        select new ImportPurchasingBookViewModel
                        {
                            urnNo = groupData.Key.URNNo,
                            receiptDate = groupData.Key.ReceiptDate.DateTime,
                            productName = groupData.Key.ProductName,
                            unitName = groupData.Key.UnitName,
                            categoryName = groupData.Key.CategoryName,
                            PIBNo = groupData.Key.PIBNo,
                            amount = (decimal)groupData.Sum(s => s.amount),
                            amountIDR = (decimal)groupData.Sum(s => s.amountIDR),
                            rate = (decimal)groupData.Key.CurrencyRate,
                        };

            return Query;
        }

        public Tuple<List<ImportPurchasingBookViewModel>, int> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            List<ImportPurchasingBookViewModel> reportData = GetReportQuery(no, unit, category, dateFrom, dateTo).ToList();

            return Tuple.Create(reportData, reportData.Count);
		}



		#region Ra sido dinggo

		//public Tuple<List<UnitReceiptNoteViewModel>, int> GetReports(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
		//{
		//    List<FilterDefinition<BsonDocument>> filter = new List<FilterDefinition<BsonDocument>>
		//    {
		//        filterBuilder.Eq("_deleted", false),
		//        filterBuilder.Eq("supplier.import", true)
		//    };

		//    if (no != null)
		//        filter.Add(filterBuilder.Eq("no", no));
		//    if (unit != null)
		//        filter.Add(filterBuilder.Eq("unit.code", unit));
		//    if (category != null)
		//        filter.Add(filterBuilder.Eq("items.purchaseOrder.category.code", category));
		//    if (dateFrom != null && dateTo != null)
		//        filter.Add(filterBuilder.And(filterBuilder.Gte("date", dateFrom), filterBuilder.Lte("date", dateTo)));

		//    List<BsonDocument> ListData = collection.Find(filterBuilder.And(filter)).ToList();
		//    //List<BsonDocument> ListData = collection.Aggregate()
		//    //    .Match(filterBuilder.And(filter))
		//    //    .ToList();

		//    List<UnitReceiptNoteViewModel> Data = new List<UnitReceiptNoteViewModel>();

		//    foreach (var data in ListData)
		//    {
		//        List<UnitReceiptNoteItemViewModel> Items = new List<UnitReceiptNoteItemViewModel>();
		//        foreach (var item in data.GetValue("items").AsBsonArray)
		//        {
		//            var itemDocument = item.AsBsonDocument;
		//            Items.Add(new UnitReceiptNoteItemViewModel
		//            {
		//                deliveredQuantity = GetBsonValue.ToDouble(itemDocument, "deliveredQuantity"),
		//                pricePerDealUnit = GetBsonValue.ToDouble(itemDocument, "pricePerDealUnit"),
		//                currencyRate = GetBsonValue.ToDouble(itemDocument, "currencyRate"),
		//                product = new ProductViewModel
		//                {
		//                    name = GetBsonValue.ToString(itemDocument, "product.name")
		//                },
		//                purchaseOrder = new PurchaseOrderViewModel
		//                {
		//                    category = new CategoryViewModel
		//                    {
		//                        name = GetBsonValue.ToString(itemDocument, "purchaseOrder.category.code")
		//                    }
		//                },
		//            });
		//        }
		//        var UnitReceiptNoteNo = GetBsonValue.ToString(data, "no");
		//        var dataUnitPaymentOrder = collectionUnitPaymentOrder.Find(filterBuilder.Eq("items.unitReceiptNote.no", UnitReceiptNoteNo)).FirstOrDefault();
		//        Data.Add(new UnitReceiptNoteViewModel
		//        {
		//            no = UnitReceiptNoteNo,
		//            date = data.GetValue("date").ToUniversalTime(),
		//            unit = new UnitViewModel
		//            {
		//                name = GetBsonValue.ToString(data, "unit.name")
		//            },
		//            pibNo = dataUnitPaymentOrder != null ? GetBsonValue.ToString(dataUnitPaymentOrder, "pibNo", new BsonString("-")) : "-",
		//            items = Items,
		//        });
		//    }

		//    return Tuple.Create(Data, Data.Count);
		//}

		//// JSON ora iso nge-cast
		//public Tuple<List<BsonDocument>, int> GetReport()
		//{
		//    IMongoCollection<BsonDocument> collection = new MongoDbContext().UnitReceiptNote;
		//    List<BsonDocument> ListData = collection.Aggregate().ToList();

		//    return Tuple.Create(ListData, ListData.Count);
		//}

		#endregion

		public MemoryStream GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
		{
			Tuple<List<ImportPurchasingBookViewModel>, int> Data = this.GetReport(no, unit, category, dateFrom, dateTo);

			DataTable result = new DataTable();
			result.Columns.Add(new DataColumn() { ColumnName = "TGL", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NOMOR NOTA", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "TIPE", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO PIB", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NILAI", DataType = typeof(decimal) });
			result.Columns.Add(new DataColumn() { ColumnName = "RATE", DataType = typeof(decimal) });
			result.Columns.Add(new DataColumn() { ColumnName = "TOTAL", DataType = typeof(decimal ) });

			List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

			if (Data.Item2 == 0)
			{
				result.Rows.Add("", "", "", "", "", "", 0, 0, 0); // to allow column name to be generated properly for empty data as template
			}
			else
			{
				Dictionary<string, List<ImportPurchasingBookViewModel>> dataByCategory = new Dictionary<string, List<ImportPurchasingBookViewModel>>();
				Dictionary<string, decimal> subTotalCategory = new Dictionary<string, decimal>();

				foreach (ImportPurchasingBookViewModel data in Data.Item1)
				{
					//foreach (UnitReceiptNoteItemViewModel item in data.items)
					//{
						string categoryName = data.categoryName;

						if (!dataByCategory.ContainsKey(categoryName)) dataByCategory.Add(categoryName, new List<ImportPurchasingBookViewModel> { });
						dataByCategory[categoryName].Add(new ImportPurchasingBookViewModel
						{
							urnNo = data.urnNo,
							receiptDate = data.receiptDate,
							PIBNo = data.PIBNo,
							unitName = data.unitName,
							amount=data.amount,
							amountIDR=data.amountIDR,
							rate=data.rate,
							productName=data.productName,
							categoryName=data.categoryName
							//items = new List<ImportPurchasingBookViewModel>() { item }
						});

						if (!subTotalCategory.ContainsKey(categoryName)) subTotalCategory.Add(categoryName, 0);
						subTotalCategory[categoryName] += (data.amountIDR);
					//}
				}

				decimal total = 0;
				int rowPosition = 1;

				foreach (KeyValuePair<string, List<ImportPurchasingBookViewModel>> categoryName in dataByCategory)
				{
					foreach (ImportPurchasingBookViewModel data in categoryName.Value)
					{
						
						result.Rows.Add(Convert.ToDateTime( data.receiptDate).ToShortDateString(), data.urnNo, data.productName, data.categoryName, data.unitName, data.PIBNo, Math.Round(data.amount, 2), Math.Round(data.rate, 2), Math.Round(data.amountIDR, 2));
						rowPosition += 1;
					}
					result.Rows.Add("SUB TOTAL", "", "", "", "", "", 0, 0, Math.Round(subTotalCategory[categoryName.Key], 2));
					rowPosition += 1;

					mergeCells.Add(($"A{rowPosition}:H{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

					total += subTotalCategory[categoryName.Key];
				}
				result.Rows.Add("TOTAL", "", "", "", "", "", 0, 0, Math.Round(total, 2));
				rowPosition += 1;

				mergeCells.Add(($"A{rowPosition}:H{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));
			}

			return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
		}

	}
}