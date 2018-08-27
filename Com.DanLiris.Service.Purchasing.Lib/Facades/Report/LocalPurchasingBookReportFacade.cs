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
    public class LocalPurchasingBookReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitReceiptNote> dbSet;


        public LocalPurchasingBookReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            //MongoDbContext mongoDbContext = new MongoDbContext();
            //collection = mongoDbContext.UnitReceiptNote;
            //collectionUnitPaymentOrder = mongoDbContext.UnitPaymentOrder;

            //filterBuilder = Builders<BsonDocument>.Filter;
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitReceiptNote>();
        }

        public Tuple<List<LocalPurchasingBookReportViewModel>, int> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            string DateFrom = d1.ToString("yyyy-MM-dd");
            string DateTo = d2.ToString("yyyy-MM-dd");
            string _cat, _no, _unit = "";
            if (category != null)
                _cat = " and a.categorycode= '" + category + "'";
            else
                _cat = "";
            if (unit != null)
                _unit = " and g.unitcode= '" + unit + "'";
            else
                _unit = "";
            if (no != null)
                _no = " and URNNo= '" + no + "'";
            else
                _no = "";
            List<LocalPurchasingBookReportViewModel> reportData = new List<LocalPurchasingBookReportViewModel>();
            string connectionString = APIEndpoint.ConnectionString;
            using (SqlConnection conn =
                new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "declare @EndDate datetime = '" + DateTo + "' " +
                    "declare @StartDate datetime = '" + DateFrom + "' " +
                    "select URNNo,ReceiptDate,ProductName,UnitName,CategoryName,useVat,VatNo,sum(dpp)Dpp,sum(ppn) Ppn  from(select  g.URNNo,g.ReceiptDate,h.productname,g.unitname,a.CategoryName,c.UseVat, " +
                    " case when ispaid = 0 then '-' else (select top(1)VatNo from UnitPaymentOrders o join unitpaymentorderItems uo on o.id = uo.UPOId where uo.urnid = g.Id) end as VatNo," +
                    " h.priceperdealunit* h.receiptquantity as dpp,h.priceperdealunit * h.receiptquantity* 10/100 as ppn" +
                    " from " +
                    " internalpurchaseorders a " +
                    " join ExternalPurchaseOrderItems b on a.id = b.POId " +
                    " join ExternalPurchaseOrders c on c.Id = b.EPOId " +
                    " join Externalpurchaseorderdetails d on  b.id = d.epoitemid " +
                    " join deliveryorderitems e on c.id = e.epoid " +
                    " join deliveryorders f on f.id = e.DOId " +
                    " join UnitReceiptNotes g on  f.id = g.DOId " +
                    " join UnitReceiptNoteItems h on g.id = h.urnid " +
                    " join Suppliers i on i.Id = g.SupplierId " +
                    " where g.IsDeleted = 0 and i.Import = 0  and  receiptdate between @StartDate and @EndDate " + _cat + _no + _unit +
                    " ) as data " +

                    " group by URNNo, ReceiptDate, ProductName, UnitName, CategoryName, useVat, VatNo, DPP, PPN", conn))
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                    DataSet dSet = new DataSet();
                    dataAdapter.Fill(dSet);
                    foreach (DataRow data in dSet.Tables[0].Rows)
                    {

                        LocalPurchasingBookReportViewModel view = new LocalPurchasingBookReportViewModel
                        {
                            uRNNo = data["URNNo"].ToString(),
                            receiptDate = (DateTimeOffset)data["ReceiptDate"],
                            productName = data["ProductName"].ToString(),
                            unitName = data["UnitName"].ToString(),
                            categoryName = data["CategoryName"].ToString(),
                            invoiceNo = data["VatNo"].ToString(),
                            dpp = Convert.ToDecimal(data["DPP"].ToString()),
                            ppn = Convert.ToDecimal(data["PPN"].ToString()),
                            useVat = (bool)data["useVat"],
                        };
                        reportData.Add(view);
                    }
                }
                conn.Close();
            }
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
            Tuple<List<LocalPurchasingBookReportViewModel>, int> Data = this.GetReport(no, unit, category, dateFrom, dateTo);

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "TGL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR NOTA", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO FAKTUR PAJAK", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TIPE", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "UNIT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
            result.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
            result.Columns.Add(new DataColumn() { ColumnName = "TOTAL", DataType = typeof(decimal) });

            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            if (Data.Item2 == 0)
            {
                result.Rows.Add("", "", "", "", "", "", 0, 0, 0); // to allow column name to be generated properly for empty data as template
            }
            else
            {
                Dictionary<string, List<LocalPurchasingBookReportViewModel>> dataByCategory = new Dictionary<string, List<LocalPurchasingBookReportViewModel>>();
                Dictionary<string, decimal> subTotalDPPCategory = new Dictionary<string, decimal>();
                Dictionary<string, decimal> subTotalPPNCategory = new Dictionary<string, decimal>();
                Dictionary<string, decimal> subTotalCategory = new Dictionary<string, decimal>();
                Dictionary<string, decimal> checkIncomeTax = new Dictionary<string, decimal>();

                foreach (LocalPurchasingBookReportViewModel data in Data.Item1)
                {
                    //foreach (UnitReceiptNoteItemViewModel item in data.items)
                    //{
                    string categoryName = data.categoryName;

                    if (!dataByCategory.ContainsKey(categoryName)) dataByCategory.Add(categoryName, new List<LocalPurchasingBookReportViewModel> { });
                    dataByCategory[categoryName].Add(new LocalPurchasingBookReportViewModel
                    {
                        uRNNo = data.uRNNo,
                        receiptDate = data.receiptDate,
                        invoiceNo = data.invoiceNo,
                        unitName = data.unitName,
                        dpp = data.dpp,
                        ppn = data.ppn,
                        productName = data.productName,
                        categoryName = data.categoryName,
                        useVat = data.useVat,
                        //items = new List<ImportPurchasingBookViewModel>() { item }
                    });

                    if (!subTotalCategory.ContainsKey(categoryName))
                    {
                        subTotalCategory.Add(categoryName, 0);
                        subTotalDPPCategory.Add(categoryName, 0);
                        subTotalPPNCategory.Add(categoryName, 0);
                    }
                    //subTotalCategory[categoryName] += (data.amountIDR);

                    if (data.useVat == true)
                    {
                        checkIncomeTax[categoryName] = 1;
                        subTotalCategory[categoryName] += data.dpp + data.ppn;
                    }
                    else
                    {
                        checkIncomeTax[categoryName] = 0;
                        subTotalCategory[categoryName] += data.dpp;
                    }
                    subTotalDPPCategory[categoryName] += data.dpp;
                    subTotalPPNCategory[categoryName] += data.ppn * checkIncomeTax[categoryName];

                    //}
                }

                decimal totalPPN = 0;
                decimal totalDPP = 0;
                decimal total = 0;
                int rowPosition = 1;

                foreach (KeyValuePair<string, List<LocalPurchasingBookReportViewModel>> categoryName in dataByCategory)
                {
                    string catCode = "";
                    foreach (LocalPurchasingBookReportViewModel data in categoryName.Value)
                    {
                        if (data.useVat == true)
                        {
                            result.Rows.Add(data.receiptDate.ToString("dd MMM yyyy", new CultureInfo("id-ID")), data.uRNNo, data.productName, data.invoiceNo, data.categoryName, data.unitName, Math.Round(data.dpp,2), Math.Round(data.ppn,2), Math.Round(data.dpp+data.ppn,2));
                        }
                        else
                        {
                            result.Rows.Add(data.receiptDate.ToString("dd MMM yyyy", new CultureInfo("id-ID")), data.uRNNo, data.productName, data.invoiceNo, data.categoryName, data.unitName, data.dpp, 0, data.dpp);
                        }
                        rowPosition += 1;
                        catCode = data.categoryName;
                        //result.Rows.Add(Convert.ToDateTime(data.receiptDate).ToShortDateString(), data.uRNNo, data.productName, data.categoryName, data.unitName, data.PIBNo, Math.Round(data.amount, 2), Math.Round(data.rate, 2), Math.Round(data.amountIDR, 2));
                        //rowPosition += 1;
                    }
                    if (subTotalPPNCategory[categoryName.Key] == 0)
                    {
                        result.Rows.Add("SUB TOTAL", "", "", "", catCode, "", Math.Round(subTotalDPPCategory[categoryName.Key],2), 0, Math.Round(subTotalCategory[categoryName.Key],2));
                    }
                    else
                    {
                        result.Rows.Add("SUB TOTAL", "", "", "", catCode, "", Math.Round(subTotalDPPCategory[categoryName.Key],2), Math.Round(subTotalPPNCategory[categoryName.Key],2), Math.Round(subTotalCategory[categoryName.Key],2));
                    }
                    rowPosition += 1;
                    //result.Rows.Add("SUB TOTAL", "", "", "", "", "", 0, 0, Math.Round(subTotalCategory[categoryName.Key], 2));
                    //rowPosition += 1;

                    //mergeCells.Add(($"A{rowPosition}:H{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

                    //total += subTotalCategory[categoryName.Key];

                    mergeCells.Add(($"A{rowPosition}:D{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

                    totalDPP += subTotalDPPCategory[categoryName.Key];
                    totalPPN += subTotalPPNCategory[categoryName.Key];
                    total += subTotalCategory[categoryName.Key];
                }

                if (totalPPN == 0)
                {
                    result.Rows.Add("TOTAL", "", "", "", "", "", Math.Round(totalDPP,2), 0, Math.Round(total,2));
                }
                else
                {
                    result.Rows.Add("TOTAL", "", "", "", "", "", Math.Round(totalDPP,2), Math.Round(totalPPN,2), Math.Round(total,2));
                }
                rowPosition += 1;
                mergeCells.Add(($"A{rowPosition}:D{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

                //result.Rows.Add("TOTAL", "", "", "", "", "", 0, 0, Math.Round(total, 2));
                //rowPosition += 1;

                //mergeCells.Add(($"A{rowPosition}:H{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));
            }

            return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
        }

    }
}