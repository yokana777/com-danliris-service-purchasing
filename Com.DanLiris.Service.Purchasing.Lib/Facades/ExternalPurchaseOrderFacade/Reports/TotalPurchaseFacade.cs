
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel.Reports;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade.Reports
{
	public class TotalPurchaseFacade
	{
		private readonly PurchasingDbContext dbContext;
		public readonly IServiceProvider serviceProvider;
		private readonly DbSet<ExternalPurchaseOrder> dbSet;

		public TotalPurchaseFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
		{
			this.serviceProvider = serviceProvider;
			this.dbContext = dbContext;
			this.dbSet = dbContext.Set<ExternalPurchaseOrder>();
		}
		public Tuple<List<ExternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
		{
			IQueryable<ExternalPurchaseOrder> Query = this.dbSet;

			List<string> searchAttributes = new List<string>()
			{
				"EPONo", "SupplierName", "DivisionName","UnitName","Items.PRNo"
			};

			Query = QueryHelper<ExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

			Query = Query
				.Select(s => new ExternalPurchaseOrder
				{
					Id = s.Id,
					EPONo = s.EPONo,
					CurrencyCode = s.CurrencyCode,
					CurrencyRate = s.CurrencyRate,
					OrderDate = s.OrderDate,
					DeliveryDate = s.DeliveryDate,
					SupplierCode = s.SupplierCode,
					SupplierName = s.SupplierName,
					DivisionCode = s.DivisionCode,
					DivisionName = s.DivisionName,
					LastModifiedUtc = s.LastModifiedUtc,
					UnitName = s.UnitName,
					UnitCode = s.UnitCode,
					CreatedBy = s.CreatedBy,
					IsPosted = s.IsPosted,
					Items = s.Items.Select(
						q => new ExternalPurchaseOrderItem
						{
							Id = q.Id,
							POId = q.POId,
							PRNo = q.PRNo
						}
					)
					.ToList()
				});



			Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
			Query = QueryHelper<ExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			Query = QueryHelper<ExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

			Pageable<ExternalPurchaseOrder> pageable = new Pageable<ExternalPurchaseOrder>(Query, Page - 1, Size);
			List<ExternalPurchaseOrder> Data = pageable.Data.ToList<ExternalPurchaseOrder>();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData, OrderDictionary);
		}

		public ExternalPurchaseOrder ReadModelById(int id)
		{
			var a = this.dbSet.Where(d => d.Id.Equals(id) && d.IsDeleted.Equals(false))
				.Include(p => p.Items)
				.ThenInclude(p => p.Details)
				.FirstOrDefault();
			return a;
		}
		public IQueryable<TotalPurchaseBySupplierViewModel> GetTotalPurchaseBySupplierReportQuery(string unit, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
		{
			DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
			DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
			var Total = (from a in dbContext.ExternalPurchaseOrders
						 join b in dbContext.ExternalPurchaseOrderItems on a.Id equals b.EPOId
						 join c in dbContext.ExternalPurchaseOrderDetails on b.Id equals c.EPOItemId
						 join d in dbContext.InternalPurchaseOrders on b.POId equals d.Id
						 //Conditions
						 where  a.IsDeleted==false && a.IsCanceled ==false &&
						  a.UnitId == (string.IsNullOrWhiteSpace(unit) ? a.UnitId : unit) && d.CategoryId == (string.IsNullOrWhiteSpace(category) ? d.CategoryId : category)
						 && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
						 && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
						 select c.DealQuantity * c.PricePerDealUnit).Sum();
			var Query = (from a in dbContext.ExternalPurchaseOrders
						 join b in dbContext.ExternalPurchaseOrderItems on a.Id equals b.EPOId
						 join c in dbContext.ExternalPurchaseOrderDetails on b.Id equals c.EPOItemId
						 join d in dbContext.InternalPurchaseOrders on b.POId equals d.Id
						 //Conditions
						 where  a.IsDeleted==false && a.IsCanceled == false &&
						 c.DealQuantity !=0 && a.UnitId == (string.IsNullOrWhiteSpace(unit) ? a.UnitId : unit) && d.CategoryId == (string.IsNullOrWhiteSpace(category) ? d.CategoryId : category)
						 && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
						 && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
						 group new { DealQuantity = c.DealQuantity , PricePerDealUnit = c.PricePerDealUnit} by new { a.SupplierName, a.UnitName, d.CategoryName } into G
						 select new TotalPurchaseBySupplierViewModel
						 {
							 supplierName =G.Key.SupplierName,
							 unitName = G.Key.UnitName,
							 categoryName = G.Key.CategoryName,
							 amount = (Decimal)Math.Round(G.Sum(c => c.DealQuantity * c.PricePerDealUnit), 2)*100,
							 total =  (Decimal)Math.Round(Total,2)
						 });
			return Query;
		}

		public   IQueryable<TotalPurchaseBySupplierViewModel> GetTotalPurchaseBySupplierReport(string unit, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
		{
			var Query = GetTotalPurchaseBySupplierReportQuery(unit, category , dateFrom, dateTo, offset);
			Query = Query.OrderBy(b => b.supplierName).ThenBy(b=>b.unitName).ThenBy(b=>b.categoryName);
			return Query;
		}

		public MemoryStream GenerateExcelTotalPurchaseBySupplier(string unit, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
		{
			var Query = GetTotalPurchaseBySupplierReportQuery(unit, category , dateFrom, dateTo, offset);
			DataTable result = new DataTable();
		
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Jumlah(Rp)", DataType = typeof(Decimal) });
			result.Columns.Add(new DataColumn() { ColumnName = "%", DataType = typeof(Decimal) });

			decimal Total = 0;
			if (Query.ToArray().Count() == 0)
				result.Rows.Add("", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
			else
			{
				int index = 0;
				foreach (var item in Query)
				{
					index++;
					Total = item.total;
						result.Rows.Add(index, item.supplierName, item.unitName,item.categoryName, (Decimal)Math.Round((item.amount), 2), (Decimal)Math.Round((item.amount / item.total),2));
				}
				result.Rows.Add("", "Total Pembelian", "", "", Total, 100);
			}

			return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
		}
	}
}
