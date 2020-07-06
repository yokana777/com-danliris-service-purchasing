using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class GarmentStockReportFacade : IGarmentStockReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSet;

        public GarmentStockReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentDeliveryOrder>();
        }

        public IEnumerable<GarmentStockReportViewModel> GetStockQuery(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;

            var PPAwal = (from a in dbContext.GarmentUnitReceiptNotes
                          join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                          join d in dbContext.GarmentDeliveryOrderDetails on b.POId equals d.POId
                          join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
                          join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
                          from ty in RC.DefaultIfEmpty()
                          join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
                          from ww in UE.DefaultIfEmpty()
                          join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                          from dd in UEN.DefaultIfEmpty()
                          join epoItem in dbContext.GarmentExternalPurchaseOrderItems on b.POId equals epoItem.POId into EP
                          from epoItem in EP.DefaultIfEmpty()
                          join epo in dbContext.GarmentExternalPurchaseOrders on epoItem.GarmentEPOId equals epo.Id into EPO
                          from epo in EPO.DefaultIfEmpty()
                          where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
                          && a.IsDeleted == false && b.IsDeleted == false
                          //String.IsNullOrEmpty(a.UENNo) ? false : a.UENNo.Contains(unitcode)
                          //|| a.UnitCode == unitcode
                          //a.UENNo.Contains(unitcode) || a.UnitCode == unitcode
                          //a.UnitCode == unitcode || a.UENNo.Contains(unitcode)

                          //&& a.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                          && a.CreatedUtc.AddHours(offset).Date < DateFrom.Date
                          select new
                          {
                              ReceiptDate = a.ReceiptDate,
                              CodeRequirment = d.CodeRequirment,
                              ProductCode = b.ProductCode,
                              ProductName = b.ProductName,
                              ProductRemark = b.ProductRemark,
                              RO = b.RONo,
                              Uom = b.UomUnit,
                              Buyer = f.BuyerCode,
                              PlanPo = b.POSerialNumber,
                              NoArticle = f.Article,
                              QtyReceipt = b.ReceiptQuantity,
                              QtyCorrection = ty.POSerialNumber == null ? 0 : ty.Quantity,
                              QtyExpend = ww.POSerialNumber == null ? 0 : ww.Quantity,
                              PriceReceipt = b.PricePerDealUnit,
                              PriceCorrection = ty.POSerialNumber == null ? 0 : ty.PricePerDealUnit,
                              PriceExpend = ww.POSerialNumber == null ? 0 : ww.PricePerDealUnit,
                              POId = b.POId,
                              URNType = a.URNType,
                              UnitCode = a.UnitCode,
                              UENNo = a.UENNo,
                              UnitSenderCode = dd.UnitSenderCode == null ? "-" : dd.UnitSenderCode,
                              UnitRequestName = dd.UnitRequestName == null ? "-" : dd.UnitRequestName,
                              ExpenditureTo = dd.ExpenditureTo == null ? "-" : dd.ExpenditureTo,
                              PaymentMethod = epo.PaymentMethod == null ? "-": epo.PaymentMethod,
                              a.IsDeleted
                          });
            var CobaPP = from a in PPAwal
                             //where a.ReceiptDate.AddHours(offset).Date < DateFrom.Date && a.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? a.CodeRequirment : ctg) && a.IsDeleted == false
                         where a.UENNo.Contains((String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)) || a.UnitCode == (String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)
                         select a;
            var PPAkhir = from a in dbContext.GarmentUnitReceiptNotes
                          join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                          join d in dbContext.GarmentDeliveryOrderDetails on b.POId equals d.POId
                          join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
                          //join f in SaldoAwal on b.POId equals f.POID
                          join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
                          from ty in RC.DefaultIfEmpty()
                          join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
                          from ww in UE.DefaultIfEmpty()
                          join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                          from dd in UEN.DefaultIfEmpty()
                          join epoItem in dbContext.GarmentExternalPurchaseOrderItems on b.POId equals epoItem.POId into EP
                          from epoItem in EP.DefaultIfEmpty()
                          join epo in dbContext.GarmentExternalPurchaseOrders on  epoItem.GarmentEPOId equals epo.Id into EPO
                          from epo in EPO.DefaultIfEmpty()

                          where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
                          && a.IsDeleted == false && b.IsDeleted == false
                          //String.IsNullOrEmpty(a.UENNo) ? false : a.UENNo.Contains(unitcode)
                          //|| a.UnitCode == unitcode
                          //a.UnitCode == unitcode || a.UENNo.Contains(unitcode)
                          // a.UENNo.Contains(unitcode) || a.UnitCode == unitcode     /*String.IsNullOrEmpty(a.UENNo) ? true :*/ 
                         && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                          && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date

                          select new
                          {
                              ReceiptDate = a.ReceiptDate,
                              CodeRequirment = d.CodeRequirment,
                              ProductCode = b.ProductCode,
                              ProductName = b.ProductName,
                              ProductRemark = b.ProductRemark,
                              RO = b.RONo,
                              Uom = b.UomUnit,
                              Buyer = f.BuyerCode,
                              PlanPo = b.POSerialNumber,
                              NoArticle = f.Article,
                              QtyReceipt = b.ReceiptQuantity,
                              QtyCorrection = ty.POSerialNumber == null ? 0 : ty.Quantity,
                              QtyExpend = ww.POSerialNumber == null ? 0 : ww.Quantity,
                              PriceReceipt = b.PricePerDealUnit,
                              PriceCorrection = ty.POSerialNumber == null ? 0 : ty.PricePerDealUnit,
                              PriceExpend = ww.POSerialNumber == null ? 0 : ww.PricePerDealUnit,
                              POId = b.POId,
                              URNType = a.URNType,
                              UnitCode = a.UnitCode,
                              UENNo = a.UENNo,
                              UnitSenderCode = dd.UnitSenderCode == null ? "-" : dd.UnitSenderCode,
                              UnitRequestName = dd.UnitRequestName == null ? "-" : dd.UnitRequestName,
                              ExpenditureTo = dd.ExpenditureTo == null ? "-" : dd.ExpenditureTo,
                              PaymentMethod = epo.PaymentMethod == null ? "-" : epo.PaymentMethod,
                              a.IsDeleted
                          };
            var CobaPPAkhir = from a in PPAkhir
                              where a.UENNo.Contains((String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)) || a.UnitCode == (String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)
                              //where a.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                              //      && a.ReceiptDate.AddHours(offset).Date <= DateTo.Date
                              //      && a.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? a.CodeRequirment : ctg)
                              //      && a.IsDeleted == false 
                              select a;
            var SaldoAwal = from query in CobaPP
                            group query by new { query.ProductCode, query.ProductName, query.RO, query.PlanPo, query.POId, query.UnitCode, query.UnitSenderCode, query.UnitRequestName } into data
                            select new GarmentStockReportViewModel
                            {
                                ProductCode = data.Key.ProductCode,
                                RO = data.Key.RO,
                                PlanPo = data.FirstOrDefault().PlanPo,
                                NoArticle = data.FirstOrDefault().NoArticle,
                                ProductName = data.FirstOrDefault().ProductName,
                                ProductRemark= data.FirstOrDefault().ProductRemark,
                                Buyer = data.FirstOrDefault().Buyer,
                                BeginningBalanceQty = data.Sum(x => x.QtyReceipt) + Convert.ToDecimal(data.Sum(x => x.QtyCorrection)) - Convert.ToDecimal(data.Sum(x => x.QtyExpend)),
                                BeginningBalanceUom = data.FirstOrDefault().Uom,
                                ReceiptCorrectionQty = 0,
                                ReceiptQty =0,
                                ReceiptUom =data.FirstOrDefault().Uom,
                                ExpendQty =0,
                                ExpandUom = data.FirstOrDefault().Uom,
                                EndingBalanceQty = 0,
                                EndingUom= data.FirstOrDefault().Uom,
                                POId = data.FirstOrDefault().POId,
                                PaymentMethod = data.FirstOrDefault().PaymentMethod

                            };
            var SaldoAkhir = from query in CobaPPAkhir
                             group query by new { query.ProductCode, query.ProductName, query.RO, query.PlanPo, query.POId, query.UnitCode, query.UnitSenderCode, query.UnitRequestName } into data
                             select new GarmentStockReportViewModel
                             {
                                 ProductCode = data.Key.ProductCode,
                                 RO = data.Key.RO,
                                 PlanPo = data.FirstOrDefault().PlanPo,
                                 NoArticle = data.FirstOrDefault().NoArticle,
                                 ProductName = data.FirstOrDefault().ProductName,
                                 ProductRemark = data.FirstOrDefault().ProductRemark,
                                 Buyer = data.FirstOrDefault().Buyer,
                                 BeginningBalanceQty =0,
                                 BeginningBalanceUom = data.FirstOrDefault().Uom,
                                 ReceiptCorrectionQty = 0,
                                 ReceiptQty = data.Sum(x => x.QtyReceipt),
                                 ReceiptUom = data.FirstOrDefault().Uom,
                                 ExpendQty = data.Sum(x => x.QtyExpend),
                                 ExpandUom = data.FirstOrDefault().Uom,
                                 EndingBalanceQty = Convert.ToDecimal(Convert.ToDouble(data.Sum(x => x.QtyReceipt)) - data.Sum(x => x.QtyExpend)),
                                 EndingUom = data.FirstOrDefault().Uom,
                                 POId = data.FirstOrDefault().POId,
                                 PaymentMethod = data.FirstOrDefault().PaymentMethod
                             };
            List<GarmentStockReportViewModel> Data1 = SaldoAwal.Concat(SaldoAkhir).ToList();
            var Data = (from query in Data1
                        group query by new { query.POId, query.ProductCode, query.RO } into groupdata
                        select new GarmentStockReportViewModel
                        {
                            ProductCode = groupdata.FirstOrDefault().ProductCode == null ? "-" : groupdata.FirstOrDefault().ProductCode,
                            RO = groupdata.FirstOrDefault().RO == null ? "-" : groupdata.FirstOrDefault().RO,
                            PlanPo = groupdata.FirstOrDefault().PlanPo == null ? "-" : groupdata.FirstOrDefault().PlanPo,
                            NoArticle = groupdata.FirstOrDefault().NoArticle == null ? "-" : groupdata.FirstOrDefault().NoArticle,
                            ProductName = groupdata.FirstOrDefault().ProductName == null ? "-" : groupdata.FirstOrDefault().ProductName,
                            ProductRemark = groupdata.FirstOrDefault().ProductRemark,
                            Buyer = groupdata.FirstOrDefault().Buyer == null ? "-" : groupdata.FirstOrDefault().Buyer,
                            BeginningBalanceQty = groupdata.Sum(x => x.BeginningBalanceQty),
                            BeginningBalanceUom = groupdata.FirstOrDefault().BeginningBalanceUom,
                            ReceiptCorrectionQty = 0,
                            ReceiptQty = groupdata.Sum(x => x.ReceiptQty),
                            ReceiptUom = groupdata.FirstOrDefault().ReceiptUom,
                            ExpendQty = groupdata.Sum(x => x.ExpendQty),
                            ExpandUom = groupdata.FirstOrDefault().ExpandUom,
                            EndingBalanceQty = Convert.ToDecimal((groupdata.Sum(x => x.BeginningBalanceQty) + groupdata.Sum(x => x.ReceiptQty) + 0) - (Convert.ToDecimal(groupdata.Sum(x => x.ExpendQty)) )),
                            EndingUom = groupdata.FirstOrDefault().EndingUom,
                            PaymentMethod = groupdata.FirstOrDefault().PaymentMethod
                        });

            return Data.AsQueryable();
        }

        public Tuple<List<GarmentStockReportViewModel>, int> GetStockReport(int offset, string unitcode, string tipebarang, int page, int size, string Order, DateTime? dateFrom, DateTime? dateTo)
        {
            //var Query = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset);
            //Query = Query.OrderByDescending(x => x.SupplierName).ThenBy(x => x.Dono);
            List<GarmentStockReportViewModel> Data = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset).ToList();
            Data = Data.OrderByDescending(x => x.ProductCode).ThenBy(x => x.ProductName).ToList();
            //int TotalData = Data.Count();
            return Tuple.Create(Data, Data.Count());
        }

        public MemoryStream GenerateExcelStockReport(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            var data = GetStockQuery(ctg, unitcode, datefrom, dateto, offset);
            var Query = data.OrderByDescending(x => x.ProductCode).ThenBy(x => x.ProductName).ToList();
            DataTable result = new DataTable();
            var headers = new string[] { "No","Kode Barang", "No RO", "Plan PO", "Artikel", "Nama Barang","Keterangan Barang", "Buyer","Saldo Awal","Saldo Awal2", "Penerimaan", "Penerimaan1", "Penerimaan2","Pengeluaran","Pengeluaran1", "Saldo Akhir", "Saldo Akhir1", "Asal" }; 
            var subheaders = new string[] { "Jumlah", "Sat", "Jumlah", "Koreksi", "Sat", "Jumlah", "Sat", "Jumlah", "Sat" };
            for (int i = 0; i < headers.Length; i++)
            {
                result.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(string) });
            }
            var index = 1;
            foreach (var item in Query)
            {

                //result.Rows.Add(index++, item.ProductCode, item.RO, item.PlanPo, item.NoArticle, item.ProductName, item.Information, item.Buyer,

                //    item.BeginningBalanceQty, item.BeginningBalanceUom, item.ReceiptQty, item.ReceiptCorrectionQty, item.ReceiptUom,
                //    NumberFormat(item.ExpendQty),
                //    item.ExpandUom, item.EndingBalanceQty, item.EndingUom, item.From);


                result.Rows.Add(index++, item.ProductCode, item.RO, item.PlanPo, item.NoArticle, item.ProductName, item.ProductRemark, item.Buyer,

                    Convert.ToDouble(item.BeginningBalanceQty), item.BeginningBalanceUom, Convert.ToDouble(item.ReceiptQty), Convert.ToDouble(item.ReceiptCorrectionQty), item.ReceiptUom,
                    item.ExpendQty,
                    item.ExpandUom, Convert.ToDouble(item.EndingBalanceQty), item.EndingUom,
                    item.PaymentMethod == "FREE FROM BUYER" || item.PaymentMethod == "CMT" || item.PaymentMethod == "CMT/IMPORT" ? "BY" : "BL");

            }

            ExcelPackage package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Data");

            sheet.Cells["A3"].LoadFromDataTable(result, false, OfficeOpenXml.Table.TableStyles.Light16);
            sheet.Cells["I1"].Value = headers[8];
            sheet.Cells["I1:J1"].Merge = true;

            sheet.Cells["K1"].Value = headers[10];
            sheet.Cells["K1:M1"].Merge = true;
            sheet.Cells["N1"].Value = headers[13];
            sheet.Cells["N1:O1"].Merge = true;
            sheet.Cells["P1"].Value = headers[15];
            sheet.Cells["P1:Q1"].Merge = true;

            foreach (var i in Enumerable.Range(0, 8))
            {
                var col = (char)('A' + i);
                sheet.Cells[$"{col}1"].Value = headers[i];
                sheet.Cells[$"{col}1:{col}2"].Merge = true;
            }

            for (var i = 0; i < 9; i++)
            {
                var col = (char)('I' + i);
                sheet.Cells[$"{col}2"].Value = subheaders[i];

            }

            foreach (var i in Enumerable.Range(0, 1))
            {
                var col = (char)('R' + i);
                sheet.Cells[$"{col}1"].Value = headers[i + 17];
                sheet.Cells[$"{col}1:{col}2"].Merge = true;
            }

            sheet.Cells["A1:R2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1:R2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A1:R2"].Style.Font.Bold = true;
            var widths = new int[] {10, 15, 15, 20, 20, 15, 20, 15, 10, 10, 10, 10, 10, 10, 10, 10, 10,15 };
            foreach (var i in Enumerable.Range(0, headers.Length))
            {
                sheet.Column(i + 1).Width = widths[i];
            }

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;


        }

        String NumberFormat(double? numb)
        {

            var number = string.Format("{0:0,0.00}", numb);

            return number;
        }


    }
}
