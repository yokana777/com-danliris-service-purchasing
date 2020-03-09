using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class GarmentInternNotePaymentStatusFacade : IGarmenInternNotePaymentStatusFacade
    {
        private readonly PurchasingDbContext dbContext;
        ILocalDbCashFlowDbContext dbContextLocal;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentInternNote> dbSet;

        public GarmentInternNotePaymentStatusFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext, ILocalDbCashFlowDbContext dbContextLocal)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbContextLocal = dbContextLocal;
            this.dbSet = dbContext.Set<GarmentInternNote>();
        }

        public IQueryable<GarmentInternNotePaymentStatusViewModel> GetQuery(string inno, string invono, string dono, string npn, string nph, string corrno, string supplier, DateTime? dateNIFrom, DateTime? dateNITo, DateTime? dueDateFrom, DateTime? dueDateTo, string status, int offset)
        {
            DateTime DateNIFrom = dateNIFrom == null ? new DateTime(1070, 1, 1) : (DateTime)dateNIFrom;
            DateTime DateNITo = dateNITo == null ? DateTime.Now : (DateTime)dateNITo;
            DateTime DueDateFrom = dueDateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dueDateFrom;
            DateTime DueDateTo = dueDateTo == null ? DateTime.Now : (DateTime)dueDateTo;
            List<GarmentInternNotePaymentStatusViewModel> paymentStatus = new List<GarmentInternNotePaymentStatusViewModel>();
            var Query = (from a in dbContext.GarmentInternNotes
                        join b in dbContext.GarmentInternNoteItems on a.Id equals b.GarmentINId
                        join c in dbContext.GarmentInternNoteDetails on b.Id equals c.GarmentItemINId
                        join e in dbContext.GarmentInvoices on b.InvoiceId equals e.Id
                        join d in dbContext.GarmentDeliveryOrders on c.DOId equals d.Id
                        join f in dbContext.GarmentCorrectionNotes on d.Id equals f.DOId into correction
                        from t in correction.DefaultIfEmpty()
                        where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false //&& t.IsDeleted == false
                        && a.INNo == (String.IsNullOrWhiteSpace(inno) ? a.INNo : inno)
                        && b.InvoiceNo == (String.IsNullOrWhiteSpace(invono) ? b.InvoiceNo : invono)
                        && c.DONo == (String.IsNullOrWhiteSpace(dono) ? c.DONo : dono)
                        && (e.NPN != null ? e.NPN == (String.IsNullOrWhiteSpace(npn) ? e.NPN : npn) : true )
                        && t.CorrectionNo == (String.IsNullOrWhiteSpace(corrno) ? t.CorrectionNo : corrno)
                        && (e.NPH != null ? e.NPH == (String.IsNullOrWhiteSpace(nph) ? e.NPH : nph) : true ) 
                        && a.SupplierCode == (String.IsNullOrWhiteSpace(supplier) ? a.SupplierCode : supplier)
                        && a.INDate.AddHours(offset).Date >= DateNIFrom.Date
                        && a.INDate.AddHours(offset).Date <= DateNITo.Date
                        && c.PaymentDueDate.AddHours(offset).Date >= DueDateFrom.Date
                        && c.PaymentDueDate.AddHours(offset).Date <= DueDateTo.Date
                        select new GarmentInternNotePaymentStatusViewModel
                        {
                            INNo = a.INNo,
                            INDate = a.INDate,
                            SuppCode = a.SupplierCode,
                            SuppName = a.SupplierName,
                            PaymentMethod = c.PaymentMethod,
                            CurrCode = a.CurrencyCode,
                            PaymentDueDate = c.PaymentDueDate,
                            InvoiceNo = b.InvoiceNo,
                            InvoDate = b.InvoiceDate,
                            DoNo = c.DONo,
                            DoDate = c.DODate,
                            PriceTot = c.PriceTotal,
                            BillNo = d.BillNo,
                            PayBill = d.PaymentBill,
                            NPN = e.NPN == null ? "" : e.NPN,
                            VatDate = e.VatDate,
                            NPH = e.NPH == null ? "" : e.NPH,
                            IncomeTaxDate = e.IncomeTaxDate,
                            CorrNo = t.CorrectionNo ?? "",
                            CorrType = t.CorrectionType ?? "",
                            CorDate = t.CorrectionDate == null ? new DateTime(1970, 1, 1) : t.CorrectionDate,
                            Nomor = "",
                            Tgl = new DateTime(1970, 1, 1),
                            Jumlah = 0

                        });
            var Data = from a in Query
                       group a by new { a.BillNo, a.DoNo } into datagroup
                       select new GarmentInternNotePaymentStatusViewModel
                       {
                           INNo = datagroup.FirstOrDefault().INNo,
                           INDate = datagroup.FirstOrDefault().INDate,
                           SuppCode = datagroup.FirstOrDefault().SuppCode,
                           SuppName = datagroup.FirstOrDefault().SuppName,
                           PaymentMethod = datagroup.FirstOrDefault().PaymentMethod,
                           CurrCode = datagroup.FirstOrDefault().CurrCode,
                           PaymentDueDate = datagroup.FirstOrDefault().PaymentDueDate,
                           InvoiceNo = datagroup.FirstOrDefault().InvoiceNo,
                           InvoDate = datagroup.FirstOrDefault().InvoDate,
                           DoNo = datagroup.FirstOrDefault().DoNo,
                           DoDate = datagroup.FirstOrDefault().DoDate,
                           PriceTot = datagroup.Sum(x => x.PriceTot),
                           BillNo = datagroup.FirstOrDefault().BillNo,
                           PayBill = datagroup.FirstOrDefault().PayBill,
                           NPN = datagroup.FirstOrDefault().NPN,
                           VatDate = datagroup.FirstOrDefault().VatDate,
                           NPH = datagroup.FirstOrDefault().NPH,
                           IncomeTaxDate = datagroup.FirstOrDefault().IncomeTaxDate,
                           CorrNo = datagroup.FirstOrDefault().CorrNo,
                           CorrType = datagroup.FirstOrDefault().CorrType,
                           CorDate = datagroup.FirstOrDefault().CorDate,
                           Nomor = datagroup.FirstOrDefault().Nomor,
                           Tgl = datagroup.FirstOrDefault().Tgl,
                           Jumlah = datagroup.FirstOrDefault().Jumlah
                       };

            foreach (GarmentInternNotePaymentStatusViewModel i in Data)
            {
                string cmddetail = "Select nomor,tgl,jumlah from RincianDetil where no_bon = '" + i.BillNo + "' and no_sj = '" + i.DoNo + "'";
                string cmdmemo = "Select nomor,tgl,jumlah from RincianMemo where no_bon = @bon";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("bon", i.BillNo));
                parameters.Add(new SqlParameter("do", i.DoNo));
                List<SqlParameter> parameters2 = new List<SqlParameter>();
                parameters2.Add(new SqlParameter("bon", i.BillNo));

                var data = dbContextLocal.ExecuteReaderOnlyQuery(cmddetail);
                
                while (data.Read())
                {
                    i.Nomor = data["nomor"].ToString();
                    i.Tgl = data.GetDateTime(1);
                    i.Jumlah = (decimal)data["jumlah"];
                };
                //data.Close();
                paymentStatus.Add(i);
              

                var data2 = dbContextLocal.ExecuteReader(cmdmemo,parameters2);
                    while (data2.Read())
                    {
                        paymentStatus.Add(new GarmentInternNotePaymentStatusViewModel
                        {
                            BillNo = i.BillNo,
                            CorDate = i.CorDate,
                            CorrNo = i.CorrNo,
                            CorrType = i.CorrType,
                            CurrCode = i.CurrCode,
                            DoDate = i.DoDate,
                            DoNo = i.DoNo,
                            IncomeTaxDate = i.IncomeTaxDate,
                            INDate = i.INDate,
                            INNo = i.INNo,
                            InvoDate = i.InvoDate,
                            InvoiceNo = i.InvoiceNo,
                            NPH = i.NPH,
                            NPN = i.NPN,
                            PayBill = i.PayBill,
                            PaymentDueDate = i.PaymentDueDate,
                            PaymentMethod = i.PaymentMethod,
                            PriceTot = i.PriceTot,
                            SuppCode = i.SuppCode,
                            SuppName = i.SuppName,
                            VatDate = i.VatDate,
                            Nomor = data2["nomor"].ToString(),
                            Tgl = data2.GetDateTime(1),
                            Jumlah = (decimal)data2["jumlah"]
                        });
                    }
                
                //data2.Close();
            };
            var datastatus = status == "BB" ? paymentStatus.Where(x => Convert.ToDouble(x.Jumlah) - x.PriceTot < 0) : status == "SB" ? paymentStatus.Where(x => Convert.ToDouble(x.Jumlah) - x.PriceTot >= 0) : paymentStatus;
            //return paymentStatus.Distinct().AsQueryable();

            return datastatus.AsQueryable();
           
        }
        public Tuple<List<GarmentInternNotePaymentStatusViewModel>, int> GetReport(string inno, string invono, string dono, string npn, string nph, string corrno, string supplier, DateTime? dateNIFrom, DateTime? dateNITo, DateTime? dueDateFrom, DateTime? dueDateTo, string status, int page, int size, string Order, int offset)
        {
            var Query = GetQuery(inno, invono, dono, npn, nph, corrno, supplier, dateNIFrom, dateNITo, dueDateFrom, dueDateTo, status, offset);
            Query.OrderBy(x => x.BillNo).ThenBy(x => x.CorDate).ThenBy(x => x.CorrNo).ThenBy(x => x.CorrType).ThenBy(x => x.CurrCode).ThenBy(x => x.DoDate).ThenBy(x => x.DoNo).ThenBy(x => x.IncomeTaxDate).ThenBy(x => x.INDate).ThenBy(x => x.INNo).ThenBy(x => x.InvoDate).ThenBy(x => x.InvoiceNo).ThenBy(x => x.NPH).ThenBy(x => x.NPN).ThenBy(x => x.PayBill).ThenBy(x => x.PaymentDueDate).ThenBy(x => x.PaymentMethod).ThenBy(x => x.PriceTot).ThenBy(x => x.SuppCode).ThenBy(x => x.SuppName).ThenBy(x => x.VatDate);
            //Console.WriteLine(Query);
            var b = Query.ToArray();
            var index = 0;
            foreach (GarmentInternNotePaymentStatusViewModel a in Query)
            {
                GarmentInternNotePaymentStatusViewModel dup = Array.Find(b, o => o.BillNo == a.BillNo && o.CorDate == a.CorDate && o.CorrNo == a.CorrNo && o.CorrType == a.CorrType && o.CurrCode == a.CurrCode && o.DoDate == a.DoDate && o.DoNo == a.DoNo && o.IncomeTaxDate == a.IncomeTaxDate && o.INDate == a.INDate && o.INNo == a.INNo && o.InvoDate == a.InvoDate && o.InvoiceNo == a.InvoiceNo && o.NPH == a.NPH && o.NPN == a.NPN && o.PayBill == a.PayBill && o.PaymentDueDate == a.PaymentDueDate && o.PaymentMethod == a.PaymentMethod && o.PriceTot == a.PriceTot && o.SuppCode == a.SuppCode && o.SuppName == a.SuppName && o.VatDate == a.VatDate);
                if (dup != null)
                {
                    if (dup.count == 0)
                    {
                        index++;
                        dup.count = index;
                    }
                }
                a.count = dup.count;
            }
            Pageable<GarmentInternNotePaymentStatusViewModel> pageable = new Pageable<GarmentInternNotePaymentStatusViewModel>(Query, page - 1, size);
            List<GarmentInternNotePaymentStatusViewModel> Data = pageable.Data.ToList<GarmentInternNotePaymentStatusViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GetXLs(string inno, string invono, string dono, string npn, string nph, string corrno, string supplier, DateTime? dateNIFrom, DateTime? dateNITo, DateTime? dueDateFrom, DateTime? dueDateTo, string status, int offset)
        {
            var Data = GetQuery(inno, invono, dono, npn, nph, corrno, supplier, dateNIFrom, dateNITo, dueDateFrom, dueDateTo, status, offset);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Suppler", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Term Pembayaran", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Jatuh Tempo", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nominal SJ", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "BP Besar", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "BP Kecil", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nota Pajak PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Pajak PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nota Pajak PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Pajak PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nota Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Koreksi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Kasbon", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Kasbon", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Bayar", DataType = typeof(Decimal) });


            if (Data.ToArray().Count() == 0)
                // result.Rows.Add("", "", "", "", "", "", "", "", "", "", 0, 0, 0, ""); // to allow column name to be generated properly for empty data as template
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0);
            else
            {
                int index = 0;
                foreach (var item in Data)
                {
                    index++;
                    string dateintern = item.INDate == new DateTime(1970, 1, 1) || item.INDate.Value.ToString("dd MMM yyyy") == "01 Jan 0001" ? "-" : item.INDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string dueDate = item.PaymentDueDate == new DateTime(1970, 1, 1) || item.PaymentDueDate.Value.ToString("dd MMM yyyy") == "01 Jan 0001" ? "-" : item.PaymentDueDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string invodate = item.InvoDate == new DateTime(1970, 1, 1) || item.InvoDate.Value.ToString("dd MMM yyyy") == "01 Jan 0001" ? "-" : item.InvoDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string doDate = item.DoDate == new DateTime(1970, 1, 1) || item.DoDate.Value.ToString("dd MMM yyyy") == "01 Jan 0001" ? "-" : item.DoDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string incometaxDate = item.IncomeTaxDate == new DateTime(1970, 1, 1) || item.IncomeTaxDate.Value.ToString("dd MMM yyyy") == "01 Jan 0001" ? "-" : item.IncomeTaxDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string VatDate = item.VatDate == new DateTime(1970, 1, 1) || item.VatDate.Value.ToString("dd MMM yyyy") == "01 Jan 0001" ? "-" : item.VatDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string corrDate = item.CorDate == new DateTime(1970, 1, 1) || item.CorDate.Value.ToString("dd MMM yyyy") == "01 Jan 0001" ? "-" : item.CorDate.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string kasbon = item.Tgl == new DateTime(1970, 1, 1) ? "-" : item.Tgl.Value.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string nonpn = item.NPN == "" ? "-" : item.NPN;
                    string nonph = item.NPH == "" ? "-" : item.NPH;
                    string correction = item.CorrNo == "" ? "-" : item.CorrNo;
                    string typecorrection = item.CorrType == "" ? "-" : item.CorrType;
                    string NoKasbon = item.Nomor == "" ? "-" : item.Nomor;

                    // result.Rows.Add(index, item.supplierCode, item.supplierName, item.no, supplierDoDate, date, item.ePONo, item.productCode, item.productName, item.productRemark, item.dealQuantity, item.dOQuantity, item.remainingQuantity, item.uomUnit);
                    result.Rows.Add(index, item.INNo, dateintern, item.SuppCode, item.SuppName, item.PaymentMethod, item.CurrCode, dueDate, item.InvoiceNo, invodate, item.DoNo, doDate, item.PriceTot, item.BillNo, item.PayBill, nonpn, VatDate, nonph, incometaxDate, correction, corrDate, typecorrection, NoKasbon, kasbon, item.Jumlah);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

    }
}
