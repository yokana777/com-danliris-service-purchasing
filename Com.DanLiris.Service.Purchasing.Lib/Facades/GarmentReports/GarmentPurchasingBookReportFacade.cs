using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentPurchasingBookReportViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class GarmentPurchasingBookReportFacade : IGarmentPurchasingBookReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSet;

        public GarmentPurchasingBookReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentDeliveryOrder>();
        }
        public Tuple<List<GarmentPurchasingBookReportViewModel>, int> GetBookReport(int offset, bool? suppliertype, string suppliercode, string tipebarang, int page, int size, string Order, DateTime? dateFrom, DateTime? dateTo)
        {
            var Query = GetBookQuery(tipebarang, suppliertype, suppliercode, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(x => x.SupplierName).ThenBy(x => x.Dono);
            List<GarmentPurchasingBookReportViewModel> Data = Query.ToList<GarmentPurchasingBookReportViewModel>();
            int TotalData = Data.Count();
            return Tuple.Create(Data, TotalData);
        }
        public IQueryable<GarmentPurchasingBookReportViewModel> GetBookQuery(string ctg, bool? suppliertype, string suppliercode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            //DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;

            var Query1 = (from c in dbContext.GarmentDeliveryOrderDetails
                          join b in dbContext.GarmentDeliveryOrderItems on c.GarmentDOItemId equals b.Id
                          join a in dbContext.GarmentDeliveryOrders on b.GarmentDOId equals a.Id
                          join h in dbContext.GarmentExternalPurchaseOrders on b.EPOId equals h.Id
                          join d in dbContext.GarmentBeacukaiItems on a.Id equals d.GarmentDOId
                          join e in dbContext.GarmentBeacukais on d.BeacukaiId equals e.Id
                          join f in dbContext.GarmentInternNoteDetails on a.Id equals f.DOId into indet
                          from t in indet.DefaultIfEmpty()
                          join re in dbContext.GarmentInternNoteItems on t.GarmentItemINId equals re.Id into internitem
                          from x in internitem.DefaultIfEmpty()
                          join j in dbContext.GarmentInvoices on x.InvoiceId equals j.Id into invo
                          from kp in invo.DefaultIfEmpty()
                          join gh in dbContext.GarmentInvoiceItems on kp.Id equals gh.InvoiceId into invoitems
                          from vb in invoitems.DefaultIfEmpty()
                          where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                          && c.CodeRequirment == (string.IsNullOrWhiteSpace(ctg) ? c.CodeRequirment : ctg)
                          && a.SupplierCode == (string.IsNullOrWhiteSpace(suppliercode) ? a.SupplierCode : suppliercode)
                          && (d.ArrivalDate != null ? d.ArrivalDate.AddHours(offset).Date >= DateFrom.Date : e.BeacukaiDate.AddHours(offset).Date >= DateFrom.Date)
                          && (d.ArrivalDate != null ? d.ArrivalDate.AddHours(offset).Date <= DateTo.Date : e.BeacukaiDate.AddHours(offset).Date <= DateTo.Date)
                          && h.SupplierImport == (suppliertype.HasValue ? suppliertype : h.SupplierImport)
                          //&& a.BillNo == "BP190904171636000028"
                          && !e.BeacukaiNo.Contains("BCDL")
                          select new
                          {
                              a.SupplierName,
                              a.BillNo,
                              a.IsInvoice,
                              a.PaymentBill,
                              a.DONo,
                              InternNo = a.InternNo == null ? "" : a.InternNo,
                              DoId = a.Id,
                              e.BeacukaiDate,
                              d.ArrivalDate,
                              c.CodeRequirment,
                              a.TotalAmount,
                              a.UseVat,
                              a.UseIncomeTax,
                              a.IsCorrection,
                              a.IncomeTaxRate,
                              InvoiceNo = kp.InvoiceNo == null ? "" : kp.InvoiceNo,
                              PaymentDueDate = t.PaymentDueDate == null ? new DateTime(1970, 1, 1) : t.PaymentDueDate,
                              ppn = a.UseVat == true && kp.IsPayTax == true ? (kp.TotalAmount * 10) / 100 : 0,
                              ppn2 = a.UseIncomeTax == true && kp.IsPayTax == true && a.IncomeTaxRate != 0 ? (kp.IncomeTaxRate * kp.TotalAmount) / 100 : 0,
                              NPN = kp.NPN == null ? "" : kp.NPN,
                              NPH = kp.NPH == null ? "" : kp.NPH,
                              a.DOCurrencyCode,
                              a.DOCurrencyRate
                          }).ToList();

            var Data = (from query in Query1
                        group query by new { query.DONo, query.SupplierName } into groupdata
                        select new
                        {
                            SupplierName = groupdata.First().SupplierName,
                            BillNo = groupdata.First().BillNo,
                            PaymentBill = groupdata.First().PaymentBill,
                            Dono = groupdata.First().DONo,
                            InvoiceNo = groupdata.First().InvoiceNo == "" ? "-" : groupdata.First().InvoiceNo,
                            InternNo = groupdata.First().InternNo == "" ? "-" : groupdata.First().InternNo,
                            Tipe = groupdata.First().CodeRequirment,
                            internDate = groupdata.First().ArrivalDate != null ? groupdata.First().ArrivalDate : groupdata.First().BeacukaiDate,
                            paymentduedate = groupdata.First().PaymentDueDate,
                            priceTotal = groupdata.First().TotalAmount,
                            ppn = groupdata.First().ppn,
                            ppn2 = groupdata.First().ppn2,
                            total = groupdata.First().TotalAmount,
                            totalppn = groupdata.First().ppn,
                            totalppn2 = groupdata.First().ppn2,
                            npn = groupdata.First().NPN,
                            nph = groupdata.First().NPH,
                            vat = groupdata.First().UseVat,
                            incomtax = groupdata.First().UseIncomeTax,
                            correction = groupdata.First().IsCorrection,
                            doid = groupdata.First().DoId,
                            ArrivalDate = groupdata.First().ArrivalDate,
                            BeacukaiDate = groupdata.First().BeacukaiDate,
                            MataUang = groupdata.First().DOCurrencyCode,
                            nilaiMataUang = groupdata.First().DOCurrencyRate
                        }).ToList().OrderBy(a => a.SupplierName);

            List<GarmentPurchasingBookReportViewModel> Datacoba = new List<GarmentPurchasingBookReportViewModel>();


            foreach (var item in Data)
            {
                //BP
                if (!string.IsNullOrEmpty(item.BillNo))
                {
                    Datacoba.Add(new
                        GarmentPurchasingBookReportViewModel
                    {
                        SupplierName = item.SupplierName,
                        BillNo = item.BillNo,
                        PaymentBill = item.PaymentBill,
                        Dono = item.Dono,
                        InvoiceNo = item.InvoiceNo,
                        InternNo = item.InternNo,
                        Tipe = item.Tipe,
                        internDate = item.ArrivalDate != null ? item.ArrivalDate : item.BeacukaiDate,
                        paymentduedate = item.paymentduedate,
                        priceTotal = item.priceTotal,
                        dpp = item.total,
                        ppn = 0,
                        total = item.total * item.nilaiMataUang,
                        totalppn = 0,
                        CurrencyCode = item.MataUang,
                        CurrencyRate = item.nilaiMataUang
                    });
                }
                //NPN
                if (!string.IsNullOrEmpty(item.npn))
                {
                    Datacoba.Add(new GarmentPurchasingBookReportViewModel
                    {
                        SupplierName = item.SupplierName,
                        BillNo = item.npn,
                        PaymentBill = item.PaymentBill,
                        Dono = item.Dono,
                        InvoiceNo = item.InvoiceNo,
                        InternNo = item.InternNo,
                        Tipe = item.Tipe,
                        internDate = item.ArrivalDate != null ? item.ArrivalDate : item.BeacukaiDate,
                        paymentduedate = item.paymentduedate,
                        priceTotal = item.ppn,
                        ppn = item.ppn,
                        dpp = 0,
                        total = item.totalppn * item.nilaiMataUang,
                        totalppn = item.totalppn,
                        CurrencyCode = item.MataUang,
                        CurrencyRate = item.nilaiMataUang
                    });
                }
                //NPH
                if (!string.IsNullOrEmpty(item.nph))
                {
                    Datacoba.Add(new
                        GarmentPurchasingBookReportViewModel
                    {
                        SupplierName = item.SupplierName,
                        BillNo = item.nph,
                        PaymentBill = item.PaymentBill,
                        Dono = item.Dono,
                        InvoiceNo = item.InvoiceNo,
                        InternNo = item.InternNo,
                        Tipe = item.Tipe,
                        internDate = item.ArrivalDate != null ? item.ArrivalDate : item.BeacukaiDate,
                        paymentduedate = item.paymentduedate,
                        priceTotal = item.priceTotal,
                        ppn = item.ppn != 0 ? item.ppn : 0,
                        totalppn = item.totalppn2,
                        total = item.totalppn2 != 0 ? item.totalppn2 * item.nilaiMataUang : 0,
                        dpp = 0,
                        CurrencyCode = item.MataUang,
                        CurrencyRate = item.nilaiMataUang
                    });
                }
                if (item.correction == true)
                {
                    var c = (from a in dbContext.GarmentCorrectionNotes
                             where a.DOId == item.doid
                             select a);
                    var b = from a in c
                            group a by new { a.DONo } into vb
                            select new { nk = vb.First().CorrectionNo, totalamo = vb.Sum(x => x.TotalCorrection) };
                    foreach (var i in b)
                    {

                        Datacoba.Add(new
                            GarmentPurchasingBookReportViewModel
                        {
                            SupplierName = item.SupplierName,
                            BillNo = i.nk,
                            PaymentBill = item.PaymentBill,
                            Dono = item.Dono,
                            InvoiceNo = item.InvoiceNo,
                            InternNo = item.InternNo,
                            Tipe = item.Tipe,
                            internDate = item.ArrivalDate != null ? item.ArrivalDate : item.BeacukaiDate,
                            paymentduedate = item.paymentduedate,
                            priceTotal = item.priceTotal,
                            ppn = 0,
                            total = Convert.ToDouble(i.totalamo) * item.nilaiMataUang,
                            totalppn = 0,
                            dpp = Convert.ToDouble(i.totalamo),
                            CurrencyCode = item.MataUang,
                            CurrencyRate = item.nilaiMataUang
                        });
                    }


                }

            }

            return Datacoba.AsQueryable();



        }
        public MemoryStream GenerateExcelBookReport(string ctg, bool? suppliertype, string suppliercode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            var Query = GetBookQuery(ctg, suppliertype, suppliercode, datefrom, dateto, offset);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "Nomor", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Nota", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Bon Kecil", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tipe", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Nota", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Jatuh Tempo", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(Decimal) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Valas", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Rate", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(Decimal) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total(IDR)", DataType = typeof(Decimal) });

            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", 0, "", 0, 0, 0);
            else
            {
                Dictionary<string, List<GarmentPurchasingBookReportViewModel>> bySupplier = new Dictionary<string, List<GarmentPurchasingBookReportViewModel>>();
                Dictionary<string, double> subTotalDPP = new Dictionary<string, double>();
                Dictionary<string, double> subTotalPPN = new Dictionary<string, double>();
                Dictionary<string, double?> subTotal = new Dictionary<string, double?>();
                foreach (GarmentPurchasingBookReportViewModel data in Query)
                {
                    string supplierName = data.SupplierName;
                    if (!bySupplier.ContainsKey(supplierName))
                        bySupplier.Add(supplierName, new List<GarmentPurchasingBookReportViewModel> { });
                    bySupplier[supplierName].Add(new GarmentPurchasingBookReportViewModel
                    {
                        BillNo = data.BillNo,
                        CurrencyCode = data.CurrencyCode,
                        CurrencyRate = data.CurrencyRate,
                        Dono = data.Dono,
                        dpp = data.dpp,
                        internDate = data.internDate,
                        InvoiceNo = data.InvoiceNo,
                        PaymentBill = data.PaymentBill,
                        paymentduedate = data.paymentduedate,
                        ppn = data.ppn,
                        priceTotal = data.priceTotal,
                        SupplierName = data.SupplierName,
                        Tipe = data.Tipe,
                        total = data.total,
                        totalppn = data.totalppn
                    });
                    if (!subTotalDPP.ContainsKey(supplierName))
                    {
                        subTotalDPP.Add(supplierName, 0);
                    }
                    if (!subTotalPPN.ContainsKey(supplierName))
                    {
                        subTotalPPN.Add(supplierName, 0);
                    }
                    if (!subTotal.ContainsKey(supplierName))
                    {
                        subTotal.Add(supplierName, 0);
                    }

                    subTotalDPP[supplierName] += data.dpp;
                    subTotalPPN[supplierName] += data.ppn;
                    subTotal[supplierName] += data.total;
                }

                double TotalDpp = 0;
                double TotalPPn = 0;
                double? jmlTotal = 0;

                int rowPosition = 1;

                foreach (KeyValuePair<string, List<GarmentPurchasingBookReportViewModel>> supplName in bySupplier)
                {
                    string supplierName = "";
                    int index = 0;
                    foreach (GarmentPurchasingBookReportViewModel item in supplName.Value)
                    {
                        index++;
                        result.Rows.Add(index, item.SupplierName, item.BillNo, item.PaymentBill, item.Dono, item.InvoiceNo, item.InternNo, item.Tipe, item.internDate == null ? "-" : item.internDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID")), item.paymentduedate == new DateTime(1970, 1, 1) ? "-" : item.paymentduedate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID")), (Decimal)Math.Round((item.dpp), 2), item.CurrencyCode, item.CurrencyRate, (Decimal)Math.Round(Convert.ToDecimal(item.totalppn), 2), (Decimal)Math.Round(Convert.ToDecimal(item.total), 2));
                        rowPosition += 1;
                        supplierName = item.SupplierName;
                    }
                    result.Rows.Add("SUB TOTAL", "", "", "", "", "", "", "", "", supplierName, Math.Round(subTotalDPP[supplName.Key], 2), "", 0, Math.Round(subTotalPPN[supplName.Key], 2), Math.Round(Convert.ToDouble(subTotal[supplName.Key]), 2));

                    rowPosition += 1;
                    mergeCells.Add(($"A{rowPosition}:D{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

                    TotalDpp += subTotalDPP[supplName.Key];
                    TotalPPn += subTotalPPN[supplName.Key];
                    jmlTotal += subTotal[supplName.Key];
                }
                result.Rows.Add("SUB TOTAL", "", "", "", "", "", "", "", "", "", Math.Round(TotalDpp, 2), "", 0, Math.Round(TotalPPn, 2), Math.Round(Convert.ToDouble(jmlTotal), 2));

                rowPosition += 1;
                mergeCells.Add(($"A{rowPosition}:D{rowPosition}", OfficeOpenXml.Style.ExcelHorizontalAlignment.Right, OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom));

            }
            return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "BookPurchasingReport", mergeCells) }, true);
        }
    }
}
