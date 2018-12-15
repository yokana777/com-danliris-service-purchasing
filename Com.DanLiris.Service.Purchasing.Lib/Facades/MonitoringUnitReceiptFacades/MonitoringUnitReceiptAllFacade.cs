using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.MonitoringUnitReceiptAllViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.MonitoringUnitReceiptFacades
{
	public class MonitoringUnitReceiptAllFacade
	{
		private readonly PurchasingDbContext dbContext;
		public readonly IServiceProvider serviceProvider;
		private readonly DbSet<GarmentUnitReceiptNote> dbSet;
		public MonitoringUnitReceiptAllFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
		{
			this.dbSet = dbContext.Set<GarmentUnitReceiptNote>();
			this.serviceProvider = serviceProvider;
			this.dbContext = dbContext;
		 
		}
		public IEnumerable<MonitoringUnitReceiptAll> GetReportQuery(string no, string refNo, string roNo,string doNo, string unit,string supplier, DateTime? dateFrom, DateTime? dateTo)
		{
			DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
			DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;

			var Data = (from a in dbContext.GarmentUnitReceiptNotes
						join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
						join c in dbContext.GarmentDeliveryOrders on a.DOId equals c.Id
						join d in dbContext.GarmentExternalPurchaseOrderItems on b.EPOItemId equals d.Id
						join e in dbContext.GarmentExternalPurchaseOrders on d.GarmentEPOId equals e.Id
						where a.IsDeleted == false  
						   && ((d1 != new DateTime(1970, 1, 1)) ? (a.ReceiptDate.Date >= d1 && a.ReceiptDate.Date <= d2) : true)
						   && ((supplier != null) ? (a.SupplierCode == supplier) : true)
						   && ((unit != null) ? (a.UnitCode == unit) : true)
						   && ((no != null) ? (a.URNNo == no) : true)
						   && ((doNo  != null) ? (a.DONo == doNo) : true)
							&& ((roNo  != null) ? (b.RONo == roNo) : true)
							&& ((refNo != null) ? (b.POSerialNumber == refNo ) : true)
						select  new {	id= a.Id, no=a.URNNo, dateBon= a.ReceiptDate, unit=a.UnitName, supplier= a.SupplierName, doNo= a.DONo,poEksternalNo=e.EPONo,poRefPR=b.POSerialNumber,
										roNo = b.RONo,article=d.Article,productCode=b.ProductCode,productName=b.ProductName, qty= b.ReceiptQuantity,uom=b.UomUnit,remark= b.ProductRemark, user= a.CreatedBy, internNo=c.InternNo}
						)
						.Distinct()
						.ToList();

			var Query = from data in Data
						 select new MonitoringUnitReceiptAll
						{
							id=data.id,
							no=data.no,
							dateBon=data.dateBon.AddHours(-7),
							unit=data.unit,
							supplier=data.supplier,
							doNo=data.doNo,
							poEksternalNo=data.poEksternalNo,
							poRefPR=data.poRefPR,
							roNo=data.roNo,
							article=data.article,
							productCode=data.productCode,
							productName=data.productName,
							qty=data.qty,
							uom=data.uom,
							remark=data.remark,
							user=data.user,
							internNote=data.internNo

						};

			return Query;
		}

		public Tuple<List<MonitoringUnitReceiptAll>, int> GetReport(string no, string refNo, string roNo, string doNo, string unit, string supplier, DateTime? dateFrom, DateTime? dateTo)
		{
			List<MonitoringUnitReceiptAll> reportData = GetReportQuery( no,  refNo,  roNo,  doNo,  unit,  supplier, dateFrom, dateTo).ToList();
			return Tuple.Create(reportData, reportData.Count);
		}
		public MemoryStream GenerateExcel(string no, string refNo, string roNo, string doNo, string unit, string supplier, DateTime? dateFrom, DateTime? dateTo)
		{
			Tuple<List<MonitoringUnitReceiptAll>, int> Data = this.GetReport(no, refNo, roNo, doNo, unit, supplier, dateFrom, dateTo);

			DataTable result = new DataTable();
			result.Columns.Add(new DataColumn() { ColumnName = "NOMOR BON TERIMA UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL BON TERIMA UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "SURAT JALAN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO PO EKSTERNAL", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "RO REFERENSI PR", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO RO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH", DataType = typeof(decimal) });
			result.Columns.Add(new DataColumn() { ColumnName = "SATUAN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "USER", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NOTA INTERN", DataType = typeof(String) });

			List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

			if (Data.Item2 == 0)
			{
				result.Rows.Add("", "", "", "", "", "","","","","","",0,"","","",""); // to allow column name to be generated properly for empty data as template
			}
			else
			{ 
				foreach (MonitoringUnitReceiptAll data in Data.Item1)
				{
					var dates =  data.dateBon.AddHours(-7) ;

					result.Rows.Add(data.no, data.dateBon.ToString("dd MMM yyyy", new CultureInfo("id-ID")), data.unit,data.supplier,data.doNo,data.poEksternalNo,data.poRefPR,data.roNo,data.productCode,data.productName,data.qty,data.uom,data.remark,data.user,data.internNote);

				}
			
			}

			return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
		}
	}
}
