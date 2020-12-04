using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class AccountingStockReportFacade : IAccountingStockReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSet;

        public AccountingStockReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentDeliveryOrder>();
        }

        public Tuple<List<AccountingStockReportViewModel>, int> GetStockReport(int offset, string unitcode, string tipebarang, int page, int size, string Order, DateTime? dateFrom, DateTime? dateTo)
        {
            //var Query = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset);
            //Query = Query.OrderByDescending(x => x.SupplierName).ThenBy(x => x.Dono);
            List<AccountingStockReportViewModel> Data = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset).ToList();
            Data = Data.OrderByDescending(x => x.ProductCode).ThenBy(x => x.ProductName).ToList();
            //int TotalData = Data.Count();
            return Tuple.Create(Data, Data.Count());
        }
        public List<AccountingStockReportViewModel> GetStockQuery(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;

            var lastdate = dbContext.BalanceStocks.OrderByDescending(x => x.CreateDate).Select(x => x.CreateDate).FirstOrDefault();

            var BalaceStock = (from a in dbContext.BalanceStocks
                               where a.CreateDate == lastdate
                               group a by new { a.ArticleNo, a.EPOID, a.EPOItemId } into data
                               select new
                               {
                                   BalanceStockId = data.FirstOrDefault().BalanceStockId,
                                   ArticleNo = data.FirstOrDefault().ArticleNo,
                                   EPOID = data.FirstOrDefault().EPOID,
                                   EPOItemId = data.FirstOrDefault().EPOItemId,
                                   CloseStock = (double)data.Sum(x => x.CloseStock),
                                   ClosePrice = (decimal)data.Sum(x => x.ClosePrice)
                               }).ToList();
           
            var SAkhir  = (from a in dbContext.GarmentUnitReceiptNotes
                           join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                           join d in dbContext.GarmentDeliveryOrderDetails on b.POId equals d.POId
                           join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
                           join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
                           from ty in RC.DefaultIfEmpty()
                           join c in dbContext.GarmentUnitExpenditureNoteItems on b.Id equals c.URNItemId into UE
                           from ww in UE.DefaultIfEmpty()
                           join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                           from dd in UEN.DefaultIfEmpty()
                           join g in dbContext.GarmentExternalPurchaseOrderItems on b.EPOItemId equals g.Id into EPO
                           from gg in EPO.DefaultIfEmpty()
                           where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
                           && a.IsDeleted == false && b.IsDeleted == false
                           && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                           && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                           && a.UnitCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)
                           select new
                           {
                               UrnId = a.Id,
                               UrnItemId = b.Id,
                               DoDetailId = d.Id,
                               POID = f.Id,
                               RCorId = ty == null ? 0 : ty.Id,
                               UENItemsId = ww == null ? 0 : ww.Id,
                               UENId = dd == null ? 0 : dd.Id,
                               EPOItemId = gg == null ? 0 : gg.Id,
                               UENNo = dd == null ? "-" : dd.UENNo,
                               a.UnitCode
                           }).ToList();
            List<AccountingStockTempViewModel> saldoakhirs = new List<AccountingStockTempViewModel>();
            //SAkhir = SAkhir.Where(x => x.UENNo.Contains((String.IsNullOrWhiteSpace(unitcode) ? x.UnitCode : unitcode)) || x.UnitCode == (String.IsNullOrWhiteSpace(unitcode) ? x.UnitCode : unitcode)).ToList();
            var SAkhirs = SAkhir.Distinct().ToList();
            var saldoakhirunitreceiptnoteIds = SAkhirs.Select(x => x.UrnId).ToList();
            var saldoakhirunitreceiptnotes = dbContext.GarmentUnitReceiptNotes.Where(x => saldoakhirunitreceiptnoteIds.Contains(x.Id)).Select(s => new { s.ReceiptDate, s.URNType, s.UnitCode, s.UENNo, s.Id }).ToList();
            var saldoakhirunitreceiptnoteItemIds = SAkhirs.Select(x => x.UrnItemId).ToList();
            var saldoakhiruntreceiptnoteItems = dbContext.GarmentUnitReceiptNoteItems.Where(x => saldoakhirunitreceiptnoteItemIds.Contains(x.Id)).Select(s => new { s.ProductCode, s.ProductName, s.RONo, s.SmallUomUnit, s.POSerialNumber, s.ReceiptQuantity, s.DOCurrencyRate, s.PricePerDealUnit, s.Id }).ToList();
            var saldoakhirdeliveryorderdetailIds = SAkhirs.Select(x => x.DoDetailId).ToList();
            var saldoakhirdeliveryorderdetails = dbContext.GarmentDeliveryOrderDetails.Where(x => saldoakhirdeliveryorderdetailIds.Contains(x.Id)).Select(s => new { s.CodeRequirment, s.Id }).ToList();
            var saldoakhirintrenalpurchaseorderIds = SAkhirs.Select(x => x.POID).ToList();
            var saldoakhirintrenalpurchaseorders = dbContext.GarmentInternalPurchaseOrders.Where(x => saldoakhirintrenalpurchaseorderIds.Contains(x.Id)).Select(s => new { s.BuyerCode, s.Article, s.Id }).ToList();
            var saldoakhirReceiptCorrectionItemIds = SAkhirs.Select(x => x.RCorId).ToList();
            var saldoakhirReceiptCorrectionItems = dbContext.GarmentReceiptCorrectionItems.Where(x => saldoakhirReceiptCorrectionItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id }).ToList();
            var saldoakhirUnitExpenditureNoteItemIds = SAkhirs.Select(x => x.UENItemsId).ToList();
            var saldoakhirUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => saldoakhirUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id }).ToList();
            var saldoakhirUnitExpenditureNoteIds = SAkhirs.Select(x => x.UENId).ToList();
            var saldoakhirUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => saldoakhirUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id }).ToList();
            var saldoakhirExternalPurchaseOrderItemIds = SAkhirs.Select(x => x.EPOItemId).ToList();
            var saldoakhirExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(x => saldoakhirExternalPurchaseOrderItemIds.Contains(x.Id)).Select(s => new { s.Id }).ToList();
            //var saldoakhirbalancestocks = BalaceStock.Where(x => saldoawalExternalPurchaseOrderItemIds.Contains((long)x.EPOItemId)).Select(s => new { s.ArticleNo, s.ClosePrice, s.CloseStock, s.EPOID, s.EPOItemId, s.BalanceStockId }).ToList();
            foreach (var item in SAkhir)
            {
                var saldoakhirunitreceiptnote = saldoakhirunitreceiptnotes.FirstOrDefault(x => x.Id == item.UrnId);
                var saldoakhiruntreceiptnoteItem = saldoakhiruntreceiptnoteItems.FirstOrDefault(x => x.Id == item.UrnItemId);
                var saldoakhirdeliveryorderdetail = saldoakhirdeliveryorderdetails.FirstOrDefault(x => x.Id == item.DoDetailId);
                var saldoakhirintrenalpurchaseorder = saldoakhirintrenalpurchaseorders.FirstOrDefault(x => x.Id == item.POID);
                var saldoakhirReceiptCorrectionItem = saldoakhirReceiptCorrectionItems.FirstOrDefault(x => x.Id == item.RCorId);
                var saldoakhirUnitExpenditureNoteItem = saldoakhirUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var saldoakhirUnitExpenditureNote = saldoakhirUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                var saldoakhirExternalPurchaseOrderItem = saldoakhirExternalPurchaseOrderItems.FirstOrDefault(x => x.Id == item.EPOItemId);
                //var saldoakhirbalancestock = saldoawalbalancestocks.FirstOrDefault(x => x.EPOItemId == item.EPOItemId);

                saldoakhirs.Add(new AccountingStockTempViewModel
                {
                    Buyer = saldoakhirintrenalpurchaseorder.BuyerCode,
                    CodeRequirment = saldoakhirdeliveryorderdetail.CodeRequirment,
                    ExpenditureTo = saldoakhirUnitExpenditureNote == null ? "-" : saldoakhirUnitExpenditureNote.ExpenditureTo,
                    NoArticle = saldoakhirintrenalpurchaseorder.Article,
                    PlanPo = saldoakhiruntreceiptnoteItem.POSerialNumber,
                    POId = saldoakhirintrenalpurchaseorder.Id,
                    PriceCorrection = saldoakhirReceiptCorrectionItem == null ? 0 : saldoakhirReceiptCorrectionItem.Quantity * saldoakhirReceiptCorrectionItem.PricePerDealUnit * saldoakhiruntreceiptnoteItem.DOCurrencyRate,
                    PriceExpend = saldoakhirUnitExpenditureNoteItem == null ? 0 : saldoakhirUnitExpenditureNoteItem.Quantity * saldoakhirUnitExpenditureNoteItem.PricePerDealUnit * saldoakhiruntreceiptnoteItem.DOCurrencyRate,
                    PriceReceipt = saldoakhiruntreceiptnoteItem.ReceiptQuantity * saldoakhiruntreceiptnoteItem.PricePerDealUnit * (decimal)saldoakhiruntreceiptnoteItem.DOCurrencyRate,
                    ProductName = saldoakhiruntreceiptnoteItem.ProductName,
                    QtyCorrection = saldoakhirReceiptCorrectionItem == null ? 0 : saldoakhirReceiptCorrectionItem.Quantity,
                    QtyExpend = saldoakhirUnitExpenditureNoteItem == null ? 0 : saldoakhirUnitExpenditureNoteItem.Quantity,
                    QtyReceipt = saldoakhiruntreceiptnoteItem.ReceiptQuantity,
                    ReceiptDate = saldoakhirunitreceiptnote.ReceiptDate,
                    RO = saldoakhiruntreceiptnoteItem.RONo,
                    UENNo = saldoakhirunitreceiptnote.UENNo,
                    UnitCode = saldoakhirunitreceiptnote.UnitCode,
                    UnitRequestName = saldoakhirUnitExpenditureNote == null ? "-" : saldoakhirUnitExpenditureNote.UnitRequestName,
                    UnitSenderCode = saldoakhirUnitExpenditureNote == null ? "-" : saldoakhirUnitExpenditureNote.UnitSenderCode,
                    Uom = saldoakhiruntreceiptnoteItem.SmallUomUnit,
                    URNType = saldoakhirunitreceiptnote.URNType,
                    ClosePrice = 0,
                    CloseStock = 0,
                    ProductCode = saldoakhiruntreceiptnoteItem.ProductCode
                });
            }
            var SaldoAkhir = (from query in saldoakhirs
                             group query by new { query.ProductCode, query.ProductName, query.RO, query.PlanPo, query.POId, query.UnitCode, query.UnitSenderCode, query.UnitRequestName } into data
                             select new AccountingStockReportViewModel
                             {
                                 ProductCode = data.Key.ProductCode,
                                 ProductName = data.Key.ProductName,
                                 RO = data.Key.RO,
                                 Buyer = data.FirstOrDefault().Buyer,
                                 PlanPo = data.FirstOrDefault().PlanPo,
                                 NoArticle = data.FirstOrDefault().NoArticle,
                                 BeginningBalanceQty = /*data.Sum(x => x.QtyReceipt) + Convert.ToDecimal(data.Sum(x => x.QtyCorrection)) - Convert.ToDecimal(data.Sum(x => x.QtyExpend))*/ 0,
                                 BeginningBalanceUom = data.FirstOrDefault().Uom,
                                 BeginningBalancePrice = /*data.Sum(x => x.PriceReceipt) + Convert.ToDecimal(data.Sum(x => x.PriceCorrection)) - Convert.ToDecimal(data.Sum(x => x.PriceExpend))*/ 0,
                                 ReceiptCorrectionQty = 0,
                                 //ReceiptPurchaseQty = data.FirstOrDefault().URNType == "PEMBELIAN" && data.FirstOrDefault().UnitCode == unitcode ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptPurchaseQty = data.FirstOrDefault().URNType == "PEMBELIAN" && data.FirstOrDefault().UnitCode == unitcode ? data.FirstOrDefault().QtyReceipt : 0,
                                 ReceiptProcessQty = data.FirstOrDefault().URNType == "PROSES" && data.FirstOrDefault().UnitCode == unitcode ? data.FirstOrDefault().QtyReceipt : 0,
                                 //ReceiptProcessQty = data.FirstOrDefault().URNType == "PROSES" && data.FirstOrDefault().UnitCode == unitcode ? data.FirstOrDefault().QtyReceipt : 0,
                                 ReceiptKon2AQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2A" ? data.FirstOrDefault().QtyReceipt : 0,
                                 ReceiptKon2BQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2B" ? data.FirstOrDefault().QtyReceipt : 0,
                                 //ReceiptKon2BQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2B" ? data.Sum(x => x.QtyReceipt) : 0,
                                 //ReceiptKon2BQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2B" ? data.FirstOrDefault().QtyReceipt : 0,
                                 //ReceiptKon2CQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2C" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon2CQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2C" ? data.FirstOrDefault().QtyReceipt : 0,
                                 //ReceiptKon1MNSQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1A" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon1MNSQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1A" ? data.FirstOrDefault().QtyReceipt : 0,
                                 //ReceiptKon2DQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1B" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon2DQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1B" ? data.FirstOrDefault().QtyReceipt : 0,
                                 ReceiptCorrectionPrice = 0,
                                 //ReceiptPurchasePrice = data.FirstOrDefault().URNType == "PEMBELIAN" && data.FirstOrDefault().UnitCode == unitcode ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptPurchasePrice = data.FirstOrDefault().URNType == "PEMBELIAN" && data.FirstOrDefault().UnitCode == unitcode ? data.FirstOrDefault().PriceReceipt : 0,
                                 //ReceiptProcessPrice = data.FirstOrDefault().URNType == "PROSES" && data.FirstOrDefault().UnitCode == unitcode ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptProcessPrice = data.FirstOrDefault().URNType == "PROSES" && data.FirstOrDefault().UnitCode == unitcode ? data.FirstOrDefault().PriceReceipt : 0,
                                 //ReceiptKon2APrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2A" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2APrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2A" ? data.FirstOrDefault().PriceReceipt : 0,
                                // ReceiptKon2BPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2B" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2BPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2B" ? data.FirstOrDefault().PriceReceipt : 0,
                                 //ReceiptKon2CPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2C" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2CPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2C" ? data.FirstOrDefault().PriceReceipt : 0,
                                 //ReceiptKon1MNSPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1A" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon1MNSPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1A" ? data.FirstOrDefault().PriceReceipt : 0,
                                 //ReceiptKon2DPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1B" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2DPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1B" ? data.FirstOrDefault().PriceReceipt : 0,
                                 ExpendReturQty = data.FirstOrDefault().ExpenditureTo == "EXTERNAL" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendRestQty = 0,
                                 ExpendProcessQty = data.FirstOrDefault().ExpenditureTo == "PROSES" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendSampleQty = data.FirstOrDefault().ExpenditureTo == "SAMPLE" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendKon2AQty = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 2A" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendKon2BQty = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 2B" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendKon2CQty = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 2C" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendKon1MNSQty = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 1A" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendKon2DQty = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 1B" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.QtyExpend) : 0,
                                 ExpendReturPrice = data.FirstOrDefault().ExpenditureTo == "EXTERNAL" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 ExpendRestPrice = 0,
                                 ExpendProcessPrice = data.FirstOrDefault().ExpenditureTo == "PROSES" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 ExpendSamplePrice = data.FirstOrDefault().ExpenditureTo == "SAMPLE" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 ExpendKon2APrice = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 2A" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 ExpendKon2BPrice = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 2B" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 ExpendKon2CPrice = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 2C" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 ExpendKon1MNSPrice = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 1A" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 ExpendKon2DPrice = data.FirstOrDefault().ExpenditureTo == "GUDANG LAIN" && data.FirstOrDefault().UnitRequestName == "CENTRAL 1B" && data.FirstOrDefault().UnitSenderCode == unitcode ? data.Sum(x => x.PriceExpend) : 0,
                                 //EndingBalanceQty = Convert.ToDecimal(Convert.ToDouble(data.Sum(x => x.QtyReceipt)) - data.Sum(x => x.QtyExpend)),
                                 EndingBalanceQty = (decimal)data.Sum(x=>x.CloseStock),
                                // EndingBalancePrice = Convert.ToDecimal(Convert.ToDouble(data.Sum(x => x.PriceReceipt)) - data.Sum(x => x.PriceExpend)),
                                 EndingBalancePrice = data.Sum(x=>x.ClosePrice),
                                 POId = data.FirstOrDefault().POId
                             }).ToList();
            var SaldoAkhirIds = SaldoAkhir.Select(x => x.POId).ToList();

            var SAwal = (from a in dbContext.GarmentUnitReceiptNotes
                         join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                         join d in dbContext.GarmentDeliveryOrderDetails on b.POId equals d.POId
                         join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
                         join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
                         from ty in RC.DefaultIfEmpty()
                         join c in dbContext.GarmentUnitExpenditureNoteItems on b.Id equals c.URNItemId into UE
                         from ww in UE.DefaultIfEmpty()
                         join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                         from dd in UEN.DefaultIfEmpty()
                         join g in dbContext.GarmentExternalPurchaseOrderItems on b.EPOItemId equals g.Id into EPO
                         from gg in EPO.DefaultIfEmpty()
                         where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
                         && a.IsDeleted == false && b.IsDeleted == false
                         && a.CreatedUtc.AddHours(offset).Date > lastdate.Value.Date
                         && a.CreatedUtc.AddHours(offset).Date < DateFrom.Date
                         && a.UnitCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)
                         && SaldoAkhirIds.Contains(f.Id)
                         select new
                         {
                             UrnId = a.Id,
                             UrnItemId = b.Id,
                             DoDetailId = d.Id,
                             POID = f.Id,
                             RCorId = ty == null ? 0 : ty.Id,
                             UENItemsId = ww == null ? 0 : ww.Id,
                             UENId = dd == null ? 0 : dd.Id,
                             EPOItemId = gg == null ? 0 : gg.Id,
                             UENNo = dd == null ? "-" : dd.UENNo,
                             a.UnitCode
                         }).ToList();
            List<AccountingStockTempViewModel> saldoawals = new List<AccountingStockTempViewModel>();
            var SAwals = SAwal.ToList();
            var saldoawalunitreceiptnoteIds = SAwal.Select(x => x.UrnId).ToList();
            var saldoawalunitreceiptnotes = dbContext.GarmentUnitReceiptNotes.Where(x => saldoawalunitreceiptnoteIds.Contains(x.Id)).Select(s => new { s.ReceiptDate, s.URNType, s.UnitCode, s.UENNo, s.Id }).ToList();
            var saldoawalunitreceiptnoteItemIds = SAwal.Select(x => x.UrnItemId).ToList();
            var saldoawaluntreceiptnoteItems = dbContext.GarmentUnitReceiptNoteItems.Where(x => saldoawalunitreceiptnoteItemIds.Contains(x.Id)).Select(s => new { s.ProductCode, s.ProductName, s.RONo, s.SmallUomUnit, s.POSerialNumber, s.ReceiptQuantity, s.DOCurrencyRate, s.PricePerDealUnit, s.Id }).ToList();
            var saldoawaldeliveryorderdetailIds = SAwal.Select(x => x.DoDetailId).ToList();
            var saldoawaldeliveryorderdetails = dbContext.GarmentDeliveryOrderDetails.Where(x => saldoawaldeliveryorderdetailIds.Contains(x.Id)).Select(s => new { s.CodeRequirment, s.Id }).ToList();
            var saldoawalintrenalpurchaseorderIds = SAwal.Select(x => x.POID).ToList();
            var saldoawalintrenalpurchaseorders = dbContext.GarmentInternalPurchaseOrders.Where(x => saldoawalintrenalpurchaseorderIds.Contains(x.Id)).Select(s => new { s.BuyerCode, s.Article, s.Id }).ToList();
            var saldoawalReceiptCorrectionItemIds = SAwal.Select(x => x.RCorId).ToList();
            var saldoawalReceiptCorrectionItems = dbContext.GarmentReceiptCorrectionItems.Where(x => saldoawalReceiptCorrectionItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id }).ToList();
            var saldoawalUnitExpenditureNoteItemIds = SAwal.Select(x => x.UENItemsId).ToList();
            var saldoawalUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => saldoawalUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id }).ToList();
            var saldoawalUnitExpenditureNoteIds = SAwal.Select(x => x.UENId).ToList();
            var saldoawalUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => saldoawalUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id }).ToList();
            var saldoawalExternalPurchaseOrderItemIds = SAwal.Select(x => x.EPOItemId).ToList();
            var saldoawalExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(x => saldoawalExternalPurchaseOrderItemIds.Contains(x.Id)).Select(s => new { s.Id }).ToList();
            var saldoawalbalancestocks = BalaceStock.Where(x => saldoawalExternalPurchaseOrderItemIds.Contains((long)x.EPOItemId)).Select(s => new { s.ArticleNo, s.ClosePrice, s.CloseStock, s.EPOID, s.EPOItemId, s.BalanceStockId }).ToList();
            foreach (var item in SAwal)
            {
                var saldoawalunitreceiptnote = saldoawalunitreceiptnotes.FirstOrDefault(x => x.Id == item.UrnId);
                var saldoawaluntreceiptnoteItem = saldoawaluntreceiptnoteItems.FirstOrDefault(x => x.Id == item.UrnItemId);
                var saldoawaldeliveryorderdetail = saldoawaldeliveryorderdetails.FirstOrDefault(x => x.Id == item.DoDetailId);
                var saldoawalintrenalpurchaseorder = saldoawalintrenalpurchaseorders.FirstOrDefault(x => x.Id == item.POID);
                var saldoawalReceiptCorrectionItem = saldoawalReceiptCorrectionItems.FirstOrDefault(x => x.Id == item.RCorId);
                var saldoawalUnitExpenditureNoteItem = saldoawalUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var saldoawalUnitExpenditureNote = saldoawalUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                var saldoawalExternalPurchaseOrderItem = saldoawalExternalPurchaseOrderItems.FirstOrDefault(x => x.Id == item.EPOItemId);
                var saldoawalbalancestock = saldoawalbalancestocks.FirstOrDefault(x => x.EPOItemId == item.EPOItemId);

                saldoawals.Add(new AccountingStockTempViewModel
                {
                    Buyer = saldoawalintrenalpurchaseorder.BuyerCode,
                    CodeRequirment = saldoawaldeliveryorderdetail.CodeRequirment,
                    ExpenditureTo = saldoawalUnitExpenditureNote == null ? "-" : saldoawalUnitExpenditureNote.ExpenditureTo,
                    NoArticle = saldoawalintrenalpurchaseorder.Article,
                    PlanPo = saldoawaluntreceiptnoteItem.POSerialNumber,
                    POId = saldoawalintrenalpurchaseorder.Id,
                    PriceCorrection = saldoawalReceiptCorrectionItem == null ? 0 : saldoawalReceiptCorrectionItem.Quantity * saldoawalReceiptCorrectionItem.PricePerDealUnit * saldoawaluntreceiptnoteItem.DOCurrencyRate,
                    PriceExpend = saldoawalUnitExpenditureNoteItem == null ? 0 : saldoawalUnitExpenditureNoteItem.Quantity * saldoawalUnitExpenditureNoteItem.PricePerDealUnit * saldoawaluntreceiptnoteItem.DOCurrencyRate,
                    PriceReceipt = saldoawaluntreceiptnoteItem.ReceiptQuantity * saldoawaluntreceiptnoteItem.PricePerDealUnit * (decimal)saldoawaluntreceiptnoteItem.DOCurrencyRate,
                    ProductName = saldoawaluntreceiptnoteItem.ProductName,
                    QtyCorrection = saldoawalReceiptCorrectionItem == null ? 0 : saldoawalReceiptCorrectionItem.Quantity,
                    QtyExpend = saldoawalUnitExpenditureNoteItem == null ? 0 : saldoawalUnitExpenditureNoteItem.Quantity,
                    QtyReceipt = saldoawaluntreceiptnoteItem.ReceiptQuantity,
                    ReceiptDate = saldoawalunitreceiptnote.ReceiptDate,
                    RO = saldoawaluntreceiptnoteItem.RONo,
                    UENNo = saldoawalunitreceiptnote.UENNo,
                    UnitCode = saldoawalunitreceiptnote.UnitCode,
                    UnitRequestName = saldoawalUnitExpenditureNote == null ? "-" : saldoawalUnitExpenditureNote.UnitRequestName,
                    UnitSenderCode = saldoawalUnitExpenditureNote == null ? "-" : saldoawalUnitExpenditureNote.UnitSenderCode,
                    Uom = saldoawaluntreceiptnoteItem.SmallUomUnit,
                    URNType = saldoawalunitreceiptnote.URNType,
                    ClosePrice = (saldoawalbalancestock == null ? 0 : saldoawalbalancestock.ClosePrice) + (saldoawaluntreceiptnoteItem.ReceiptQuantity * saldoawaluntreceiptnoteItem.PricePerDealUnit * (decimal)saldoawaluntreceiptnoteItem.DOCurrencyRate) + (saldoawalReceiptCorrectionItem == null ? 0 : (decimal)saldoawalReceiptCorrectionItem.Quantity * (decimal)saldoawalReceiptCorrectionItem.PricePerDealUnit * (decimal)saldoawaluntreceiptnoteItem.DOCurrencyRate) - (saldoawalUnitExpenditureNoteItem == null ? 0 : (decimal)saldoawalUnitExpenditureNoteItem.Quantity * (decimal)saldoawalUnitExpenditureNoteItem.PricePerDealUnit * (decimal)saldoawaluntreceiptnoteItem.DOCurrencyRate),
                    CloseStock = (saldoawalbalancestock == null ? 0 : saldoawalbalancestock.CloseStock) + (double)saldoawaluntreceiptnoteItem.ReceiptQuantity + (saldoawalReceiptCorrectionItem == null ? 0 : saldoawalReceiptCorrectionItem.Quantity) - (saldoawalUnitExpenditureNoteItem == null ? 0 : saldoawalUnitExpenditureNoteItem.Quantity),
                    ProductCode = saldoawaluntreceiptnoteItem.ProductCode
                });
            }

            var SaldoAwal = (from query in saldoawals
                             group query by new { query.ProductCode, query.ProductName, query.RO, query.PlanPo, query.POId, query.UnitCode, query.UnitSenderCode, query.UnitRequestName } into data
                             select new AccountingStockReportViewModel
                             {
                                 ProductCode = data.Key.ProductCode,
                                 ProductName = data.FirstOrDefault().ProductName,
                                 RO = data.Key.RO,
                                 Buyer = data.FirstOrDefault().Buyer,
                                 PlanPo = data.FirstOrDefault().PlanPo,
                                 NoArticle = data.FirstOrDefault().NoArticle,
                                 //BeginningBalanceQty = (decimal)data.Sum(x => x.CloseStock) + data.Sum(x => x.QtyReceipt) + Convert.ToDecimal(data.Sum(x => x.QtyCorrection)) - Convert.ToDecimal(data.Sum(x => x.QtyExpend)),
                                 BeginningBalanceQty = data.Sum(x=>(decimal)x.CloseStock),
                                 BeginningBalanceUom = data.FirstOrDefault().Uom,
                                 //BeginningBalancePrice = data.Sum(x => x.ClosePrice) + data.Sum(x => x.PriceReceipt) + Convert.ToDecimal(data.Sum(x => x.PriceCorrection)) - Convert.ToDecimal(data.Sum(x => x.PriceExpend)),
                                 BeginningBalancePrice = data.Sum(x=>x.ClosePrice),
                                 ReceiptCorrectionQty = 0,
                                 ReceiptPurchaseQty = 0,
                                 ReceiptProcessQty = 0,
                                 ReceiptKon2AQty = 0,
                                 ReceiptKon2BQty = 0,
                                 ReceiptKon2CQty = 0,
                                 ReceiptKon1MNSQty = 0,
                                 ReceiptKon2DQty = 0,
                                 ReceiptCorrectionPrice = 0,
                                 ReceiptPurchasePrice = 0,
                                 ReceiptProcessPrice = 0,
                                 ReceiptKon2APrice = 0,
                                 ReceiptKon2BPrice = 0,
                                 ReceiptKon2CPrice = 0,
                                 ReceiptKon1MNSPrice = 0,
                                 ReceiptKon2DPrice = 0,
                                 ExpendReturQty = 0,
                                 ExpendRestQty = 0,
                                 ExpendProcessQty = 0,
                                 ExpendSampleQty = 0,
                                 ExpendKon2AQty = 0,
                                 ExpendKon2BQty = 0,
                                 ExpendKon2CQty = 0,
                                 ExpendKon1MNSQty = 0,
                                 ExpendKon2DQty = 0,
                                 ExpendReturPrice = 0,
                                 ExpendRestPrice = 0,
                                 ExpendProcessPrice = 0,
                                 ExpendSamplePrice = 0,
                                 ExpendKon2APrice = 0,
                                 ExpendKon2BPrice = 0,
                                 ExpendKon2CPrice = 0,
                                 ExpendKon1MNSPrice = 0,
                                 ExpendKon2DPrice = 0,
                                 EndingBalanceQty = 0,
                                 EndingBalancePrice = 0,
                                 POId = data.FirstOrDefault().POId
                             }).ToList();

            var Data = (from a in SaldoAkhir
                        join b in SaldoAwal on a.POId equals b.POId into groupdata
                        from bb in groupdata.DefaultIfEmpty()
                        select new AccountingStockReportViewModel {
                            ProductCode = a.ProductCode,
                            ProductName = a.ProductName,
                            RO = a.RO,
                            Buyer = a.Buyer,
                            PlanPo = a.PlanPo,
                            NoArticle = a.NoArticle,
                            BeginningBalanceQty = bb == null ? 0 : Math.Round((decimal)bb.BeginningBalanceQty, 2),
                            BeginningBalanceUom = a.BeginningBalanceUom,
                            BeginningBalancePrice = bb == null ? 0 : Math.Round((decimal)bb.BeginningBalancePrice, 2),
                            ReceiptCorrectionQty = 0,
                            ReceiptPurchaseQty = Math.Round((decimal)a.ReceiptPurchaseQty, 2),
                            ReceiptProcessQty = Math.Round((decimal)a.ReceiptProcessQty, 2),
                            ReceiptKon2AQty = Math.Round((decimal)a.ReceiptKon2AQty, 2),
                            ReceiptKon2BQty = Math.Round((decimal)a.ReceiptKon2BQty, 2),
                            ReceiptKon2CQty = Math.Round((decimal)a.ReceiptKon2CQty, 2),
                            ReceiptKon1MNSQty = Math.Round((decimal)a.ReceiptKon1MNSQty, 2),
                            ReceiptKon2DQty = Math.Round((decimal)a.ReceiptKon2DQty, 2),
                            ReceiptCorrectionPrice = Math.Round((decimal)a.ReceiptCorrectionPrice, 2),
                            ReceiptPurchasePrice = Math.Round((decimal)a.ReceiptPurchasePrice, 2),
                            ReceiptProcessPrice = Math.Round((decimal)a.ReceiptProcessPrice, 2),
                            ReceiptKon2APrice = Math.Round((decimal)a.ReceiptKon2APrice, 2),
                            ReceiptKon2BPrice = Math.Round((decimal)a.ReceiptKon2BPrice, 2),
                            ReceiptKon2CPrice = Math.Round((decimal)a.ReceiptKon2CPrice, 2),
                            ReceiptKon1MNSPrice = Math.Round((decimal)a.ReceiptKon1MNSPrice, 2),
                            ReceiptKon2DPrice = Math.Round((decimal)a.ReceiptKon2DPrice, 2),
                            ExpendReturQty = Math.Round((double)a.ExpendReturQty, 2),
                            ExpendRestQty = Math.Round((double)a.ExpendRestQty, 2),
                            ExpendProcessQty = Math.Round((double)a.ExpendProcessQty, 2),
                            ExpendSampleQty = Math.Round((double)a.ExpendSampleQty, 2),
                            ExpendKon2AQty = Math.Round((double)a.ExpendKon2AQty, 2),
                            ExpendKon2BQty = Math.Round((double)a.ExpendKon2BQty, 2),
                            ExpendKon2CQty = Math.Round((double)a.ExpendKon2CQty, 2),
                            ExpendKon1MNSQty = Math.Round((double)a.ExpendKon1MNSQty, 2),
                            ExpendKon2DQty = Math.Round((double)a.ExpendKon2DQty, 2),
                            ExpendReturPrice = Math.Round((double)a.ExpendReturPrice, 2),
                            ExpendRestPrice = Math.Round((double)a.ExpendRestPrice, 2),
                            ExpendProcessPrice = Math.Round((double)a.ExpendProcessPrice, 2),
                            ExpendSamplePrice = Math.Round((double)a.ExpendSamplePrice, 2),
                            ExpendKon2APrice = Math.Round((double)a.ExpendKon2APrice, 2),
                            ExpendKon2BPrice = Math.Round((double)a.ExpendKon2BPrice, 2),
                            ExpendKon2CPrice = Math.Round((double)a.ExpendKon2CPrice, 2),
                            ExpendKon1MNSPrice = Math.Round((double)a.ExpendKon1MNSPrice, 2),
                            ExpendKon2DPrice = Math.Round((double)a.ExpendKon2DPrice, 2),
                            EndingBalanceQty = Math.Round(((bb == null ? 0 : (decimal)bb.BeginningBalanceQty) + (decimal)a.ReceiptPurchaseQty + (decimal)a.ReceiptProcessQty + (decimal)a.ReceiptKon2AQty + (decimal)a.ReceiptKon2BQty + (decimal)a.ReceiptKon2CQty + (decimal)a.ReceiptKon2DQty + (decimal)a.ReceiptKon1MNSQty + (decimal)a.ReceiptCorrectionQty) - ((decimal)a.ExpendProcessQty + (decimal)a.ExpendSampleQty + (decimal)a.ExpendReturQty + (decimal)a.ExpendRestQty + (decimal)a.ExpendKon2DQty + (decimal)a.ExpendKon2CQty + (decimal)a.ExpendKon2BQty + (decimal)a.ExpendKon2AQty + (decimal)a.ExpendKon1MNSQty), 2),
                           EndingBalancePrice = Math.Round(((bb == null ? 0 : (decimal)bb.BeginningBalancePrice) + (decimal)a.ReceiptPurchasePrice + (decimal)a.ReceiptProcessPrice + (decimal)a.ReceiptKon2APrice + (decimal)a.ReceiptKon2BPrice + (decimal)a.ReceiptKon2CPrice + (decimal)a.ReceiptKon2DPrice + (decimal)a.ReceiptKon1MNSPrice + (decimal)a.ReceiptCorrectionPrice) - ((decimal)a.ExpendProcessPrice + (decimal)a.ExpendSamplePrice + (decimal)a.ExpendReturPrice + (decimal)a.ExpendRestPrice + (decimal)a.ExpendKon2DPrice + (decimal)a.ExpendKon2CPrice + (decimal)a.ExpendKon2BPrice + (decimal)a.ExpendKon2APrice + (decimal)a.ExpendKon1MNSPrice), 2)
                       }).ToList();
           // var EndindQty = Data.FirstOrDefault(x=>x.BeginningBalanceQty != 0).BeginningBalanceQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptPurchaseQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptProcessQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptKon2AQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptKon2BQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptKon2CQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptKon2DQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptKon1MNSQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ReceiptCorrectionQty;
            //var EndingExpendQty = Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendProcessQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendSampleQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendReturQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendRestQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendKon2DQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendKon2CQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendKon2BQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendKon2AQty + Data.FirstOrDefault(x => x.BeginningBalanceQty != 0).ExpendKon1MNSQty;
            return Data;

        }
        public MemoryStream GenerateExcelAStockReport(string ctg, string categoryname, string unitcode, string unitname, DateTime? datefrom, DateTime? dateto, int offset)
        {
            var Query = GetStockQuery(ctg, unitcode, datefrom, dateto, offset);
            #region Pemasukan
            double SaldoAwalQtyTotal = Query.Sum(x => Convert.ToDouble(x.BeginningBalanceQty));
            double SaldoAwalPriceTotal = Query.Sum(x => Convert.ToDouble(x.BeginningBalancePrice));
            double KoreksiQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptCorrectionQty));
            double KoreksiPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptCorrectionPrice));
            double PEMBELIANQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptPurchaseQty));
            double PEMBELIANPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptPurchasePrice));
            double PROSESQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptProcessQty));
            double PROSESPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptProcessPrice));
            double Konfeksi2AQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2AQty));
            double Konfeksi2APriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2APrice));
            double KONFEKSI2BQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2BQty));
            double KONFEKSI2BPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2BPrice));
            double KONFEKSI2CQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2CQty));
            double KONFEKSI2CPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2CPrice));
            double KONFEKSI2DQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2DQty));
            double KONFEKSI2DPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon2DPrice));
            double KONFEKSI1MNSQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon1MNSQty));
            double KONFEKSI1MNSPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon1MNSPrice));
            #endregion
            #region Pemngeluaran
            double? ReturQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendReturQty));
            double? ReturJumlahTotal = Query.Sum(x => Convert.ToDouble(x.ExpendReturPrice));
            double? SisaQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendRestQty));
            double? SisaPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendRestPrice));
            double? ExpendPROSESQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendProcessQty));
            double? ExpendPROSESPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendProcessPrice));
            double? SAMPLEQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendSampleQty));
            double? SAMPLEPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendSamplePrice));
            double? ExpendKONFEKSI2AQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2AQty));
            double? ExpendKonfeksi2APriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2APrice));
            double? ExpendKONFEKSI2BQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2BQty));
            double? ExpendKONFEKSI2BPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2BPrice));
            double? ExpendKONFEKSI2CQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2CQty));
            double? ExpendKONFEKSI2CPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2CPrice));
            double? ExpendKONFEKSI2DQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2DQty));
            double? ExpendKONFEKSI2DPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon2DPrice));
            double? ExpendKONFEKSI1MNSQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon1MNSQty));
            double? ExpendKONFEKSI1MNSPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon1MNSPrice));
            #endregion
            double? EndingQty = Query.Sum(x => Convert.ToDouble(x.EndingBalanceQty));
            double? EndingTotal = Query.Sum(x => Convert.ToDouble(x.EndingBalancePrice));
            DataTable result = new DataTable();
            var headers = new string[] { "No", "Kode", "Nama Barang", "RO", "Buyer", "PlanPO", "No Artikel", "Saldo Awal", "Saldo Awal1", "Saldo Awal2", "P E M A S U K A N", "P E M B E L I A N1", "P E M B E L I A N2", "P E M B E L I A N3", "P E M B E L I A N4", "P E M B E L I A N5", "P E M B E L I A N6", "P E M B E L I A N7", "P E M B E L I A N8", "P E M B E L I A N9", "P E M B E L I A N10", "P E M B E L I A N11", "P E M B E L I A N12", "P E M B E L I A N13", "P E M B E L I A N14", "P E M B E L I A N15", "P E N G E L U A R A N", "P E N G E L U A R A N1", "P E N G E L U A R A N2", "P E N G E L U A R A N3", "P E N G E L U A R A N4", "P E N G E L U A R A N5", "P E N G E L U A R A N6", "P E N G E L U A R A N7", "P E N G E L U A R A N8", "P E N G E L U A R A N9", "P E N G E L U A R A N10", "P E N G E L U A R A N11", "P E N G E L U A R A N12", "P E N G E L U A R A N13", "P E N G E L U A R A N14", "P E N G E L U A R A N15", "P E N G E L U A R A N16", "P E N G E L U A R A N17", "Saldo Akhir", "Saldo Akhir 1" };
            var headers2 = new string[] { "Koreksi", "Pembelian", "Proses", "KONFEKSI 2A", "KONFEKSI 2B", "KONFEKSI 2C", "KONFEKSI 1 MNS", "KONFEKSI 2D", "Retur", "Sisa", "Proses", "Sample", "KONFEKSI 2A", "KONFEKSI 2B", "KONFEKSI 2C", "KONFEKSI 1 MNS", "KONFEKSI 2D" };
            var subheaders = new string[] { "Jumlah", "Sat", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp" };
            for (int i = 0; i < 7; i++)
            {
                result.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(string) });
            }
            result.Columns.Add(new DataColumn() { ColumnName = headers[7], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[8], DataType = typeof(string) });
            for (int i = 9; i < headers.Length; i++)
            {
                result.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(Double) });
            }
            var index = 1;
            foreach (var item in Query)
            {
                var ReceiptPurchaseQty = unitcode == "C2A" ? item.ReceiptPurchaseQty + item.ReceiptKon2AQty : unitcode == "C2B" ? item.ReceiptPurchaseQty + item.ReceiptKon2BQty : unitcode == "C2C" ? item.ReceiptPurchaseQty + item.ReceiptKon2CQty : unitcode == "C1B" ? item.ReceiptPurchaseQty + item.ReceiptKon2DQty : unitcode == "C1A" ? item.ReceiptPurchaseQty + item.ReceiptKon1MNSQty : item.ReceiptPurchaseQty + item.ReceiptKon2AQty + item.ReceiptKon2BQty + item.ReceiptKon2CQty + item.ReceiptKon2DQty + item.ReceiptKon1MNSQty;
                var ReceiptPurchasePrice = unitcode == "C2A" ? item.ReceiptPurchasePrice + item.ReceiptKon2APrice : unitcode == "C2B" ? item.ReceiptPurchasePrice + item.ReceiptKon2BPrice : unitcode == "C2C" ? item.ReceiptPurchasePrice + item.ReceiptKon2CPrice : unitcode == "C1B" ? item.ReceiptPurchaseQty + item.ReceiptKon2DPrice : unitcode == "C1A" ? item.ReceiptPurchasePrice + item.ReceiptKon1MNSPrice : item.ReceiptPurchasePrice + item.ReceiptKon2APrice + item.ReceiptKon2BPrice + item.ReceiptKon2CPrice + item.ReceiptKon2DPrice + item.ReceiptKon1MNSPrice;
                var ReceiptKon2AQty = unitcode == "C2A" ? 0 : item.ReceiptKon2AQty;
                var ReceiptKon2APrice = unitcode == "C2A" ? 0 : item.ReceiptKon2APrice;
                var ReceiptKon2BPrice = unitcode == "C2B" ? 0 : item.ReceiptKon2BPrice;
                var ReceiptKon2BQty = unitcode == "C2B" ? 0 : item.ReceiptKon2BQty;
                var ReceiptKon2CPrice = unitcode == "C2C" ? 0 : item.ReceiptKon2CQty;
                var ReceiptKon2CQty = unitcode == "C2C" ? 0 : item.ReceiptKon2CQty;
                var ReceiptKon2DPrice = unitcode == "C1B" ? 0 : item.ReceiptKon2DPrice;
                var ReceiptKon2DQty = unitcode == "C1B" ? 0 : item.ReceiptKon2DQty;
                var ReceiptKon1MNSQty = unitcode == "C1A" ? 0 : item.ReceiptKon1MNSQty;
                var ReceiptKon1MNSPrice = unitcode == "C1A" ? 0 : item.ReceiptKon1MNSPrice;
                var ReceiptCorrection = item.ReceiptCorrectionPrice;

                result.Rows.Add(index++, item.ProductCode, item.ProductName, item.RO, item.Buyer, item.PlanPo, item.NoArticle,
                    Convert.ToDouble(item.BeginningBalanceQty), item.BeginningBalanceUom,
                    Convert.ToDouble(item.BeginningBalancePrice),
                    Convert.ToDouble(item.ReceiptCorrectionQty),
                    Convert.ToDouble(item.ReceiptCorrectionPrice),
                    Convert.ToDouble(ReceiptPurchaseQty),
                    Convert.ToDouble(ReceiptPurchasePrice),
                    Convert.ToDouble(item.ReceiptProcessQty),
                    Convert.ToDouble(item.ReceiptProcessPrice),
                    Convert.ToDouble(ReceiptKon2AQty),
                    Convert.ToDouble(ReceiptKon2APrice),
                    Convert.ToDouble(ReceiptKon2BQty),
                    Convert.ToDouble(ReceiptKon2BPrice),
                    Convert.ToDouble(ReceiptKon2CQty),
                    Convert.ToDouble(ReceiptKon2CPrice),
                    Convert.ToDouble(ReceiptKon1MNSQty),
                    Convert.ToDouble(ReceiptKon1MNSPrice),
                    Convert.ToDouble(ReceiptKon2DQty),
                    Convert.ToDouble(ReceiptKon2DPrice),
                    Convert.ToDouble(item.ExpendReturQty), item.ExpendReturPrice, item.ExpendRestQty, item.ExpendRestPrice, item.ExpendProcessQty, item.ExpendProcessPrice, item.ExpendSampleQty, item.ExpendSamplePrice, item.ExpendKon2AQty, item.ExpendKon2APrice, item.ExpendKon2BQty, item.ExpendKon2BPrice, item.ExpendKon2CQty, item.ExpendKon2CPrice, item.ExpendKon1MNSQty, item.ExpendKon1MNSPrice, item.ExpendKon2DQty, item.ExpendKon2DPrice, Convert.ToDouble(item.EndingBalanceQty), Convert.ToDouble(item.EndingBalancePrice));
            }

            //result.Rows.Add("", "", "", "", "", "", "",
            //        SaldoAwalQtyTotal, "",
            //        SaldoAwalPriceTotal,
            //        KoreksiQtyTotal,
            //        KoreksiPriceTotal,
            //        PEMBELIANQtyTotal,
            //        PEMBELIANPriceTotal,
            //        PROSESQtyTotal,
            //        PROSESPriceTotal,
            //        Konfeksi2AQtyTotal,
            //        Konfeksi2APriceTotal,
            //        KONFEKSI2BQtyTotal,
            //        KONFEKSI2BPriceTotal,
            //        KONFEKSI2CQtyTotal,
            //        KONFEKSI2CPriceTotal,
            //        KONFEKSI1MNSQtyTotal,
            //        KONFEKSI1MNSPriceTotal,
            //        KONFEKSI2DQtyTotal,
            //        KONFEKSI2DPriceTotal,
            //        ReturQtyTotal, ReturJumlahTotal, SisaQtyTotal, SisaPriceTotal, ExpendPROSESQtyTotal, ExpendPROSESPriceTotal, SAMPLEQtyTotal, SAMPLEPriceTotal, ExpendKONFEKSI2AQtyTotal, ExpendKonfeksi2APriceTotal, ExpendKONFEKSI2BQtyTotal, ExpendKONFEKSI2BPriceTotal, ExpendKONFEKSI2CQtyTotal, ExpendKONFEKSI2CPriceTotal, ExpendKONFEKSI1MNSQtyTotal, ExpendKONFEKSI1MNSPriceTotal, ExpendKONFEKSI2DQtyTotal, ExpendKONFEKSI2DPriceTotal, Convert.ToDouble(EndingQty), Convert.ToDouble(EndingTotal));


            ExcelPackage package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Data");
            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;
            var col1 = (char)('A' + result.Columns.Count);
            string tglawal = DateFrom.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            string tglakhir = DateTo.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            //CultureInfo Id = new CultureInfo("id-ID");
            sheet.Cells[$"A1:{col1}1"].Value = string.Format("LAPORAN FLOW {0}", categoryname);
            sheet.Cells[$"A1:{col1}1"].Merge = true;
            sheet.Cells[$"A1:{col1}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col1}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col1}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col1}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
            sheet.Cells[$"A2:{col1}2"].Merge = true;
            sheet.Cells[$"A2:{col1}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col1}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col1}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col1}3"].Value = string.Format("KONFEKSI : {0}", unitname);
            sheet.Cells[$"A3:{col1}3"].Merge = true;
            sheet.Cells[$"A3:{col1}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col1}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col1}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            sheet.Cells["A7"].LoadFromDataTable(result, false, OfficeOpenXml.Table.TableStyles.Light16);
            sheet.Cells["H4"].Value = headers[7];
            sheet.Cells["H4:J5"].Merge = true;
            sheet.Cells["H4:J5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["K4"].Value = headers[10];
            sheet.Cells["K4:Z4"].Merge = true;
            sheet.Cells["K4:Z4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AA4"].Value = headers[26];
            sheet.Cells["AA4:AR4"].Merge = true;
            sheet.Cells["AA4:AR4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AS4"].Value = headers[44];
            sheet.Cells["AS4:AT5"].Merge = true;
            sheet.Cells["AS4:AT5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["K5"].Value = headers2[0];
            sheet.Cells["K5:L5"].Merge = true;
            sheet.Cells["K5:L5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["M5"].Value = headers2[1];
            sheet.Cells["M5:N5"].Merge = true;
            sheet.Cells["M5:N5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["O5"].Value = headers2[2];
            sheet.Cells["O5:P5"].Merge = true;
            sheet.Cells["O5:P5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["Q5"].Value = headers2[3];
            sheet.Cells["Q5:R5"].Merge = true;
            sheet.Cells["Q5:R5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["S5"].Value = headers2[4];
            sheet.Cells["S5:T5"].Merge = true;
            sheet.Cells["S5:T5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["U5"].Value = headers2[5];
            sheet.Cells["U5:V5"].Merge = true;
            sheet.Cells["U5:V5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["W5"].Value = headers2[6];
            sheet.Cells["W5:X5"].Merge = true;
            sheet.Cells["W5:X5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["Y5"].Value = headers2[7];
            sheet.Cells["Y5:Z5"].Merge = true;
            sheet.Cells["Y5:Z5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AA5"].Value = headers2[8];
            sheet.Cells["AA5:AB5"].Merge = true;
            sheet.Cells["AA5:AB5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AC5"].Value = headers2[9];
            sheet.Cells["AC5:AD5"].Merge = true;
            sheet.Cells["AC5:AD5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AE5"].Value = headers2[10];
            sheet.Cells["AE5:AF5"].Merge = true;
            sheet.Cells["AE5:AF5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AG5"].Value = headers2[11];
            sheet.Cells["AG5:AH5"].Merge = true;
            sheet.Cells["AG5:AH5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AI5"].Value = headers2[12];
            sheet.Cells["AI5:AJ5"].Merge = true;
            sheet.Cells["AI5:AJ5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AK5"].Value = headers2[13];
            sheet.Cells["AK5:AL5"].Merge = true;
            sheet.Cells["AK5:AL5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AM5"].Value = headers2[14];
            sheet.Cells["AM5:AN5"].Merge = true;
            sheet.Cells["AM5:AN5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AO5"].Value = headers2[15];
            sheet.Cells["AO5:AP5"].Merge = true;
            sheet.Cells["AO5:AP5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            sheet.Cells["AQ5"].Value = headers2[16];
            sheet.Cells["AQ5:AR5"].Merge = true;
            sheet.Cells["AQ5:AR5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

            foreach (var i in Enumerable.Range(0, 7))
            {
                var col = (char)('A' + i);
                sheet.Cells[$"{col}4"].Value = headers[i];
                sheet.Cells[$"{col}4:{col}6"].Merge = true;
                sheet.Cells[$"{col}4:{col}6"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            }

            for (var i = 0; i < 19; i++)
            {
                var col = (char)('H' + i);
                sheet.Cells[$"{col}6"].Value = subheaders[i];
                sheet.Cells[$"{col}6"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

            }

            for (var i = 19; i < 39; i++)
            {
                var col = (char)('A' + i - 19);
                sheet.Cells[$"A{col}6"].Value = subheaders[i];
                sheet.Cells[$"A{col}6"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

            }
            sheet.Cells["A4:AS6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A4:AS6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A4:AS6"].Style.Font.Bold = true;
            sheet.Cells[$"A{result.Rows.Count + 6}:G{result.Rows.Count + 6}"].Value = "T O T A L  . . . . . . . . . . . . . . .";
            sheet.Cells[$"A{result.Rows.Count + 6}:G{result.Rows.Count + 6}"].Merge = true;
            sheet.Cells[$"A{result.Rows.Count + 6}:G{result.Rows.Count + 6}"].Style.Font.Bold = true;
            sheet.Cells[$"A{6 + result.Rows.Count}:G{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"A{result.Rows.Count + 6}:G{result.Rows.Count + 6}"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet.Cells[$"A{result.Rows.Count + 6}:G{result.Rows.Count + 6}"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            sheet.Cells[$"H{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", SaldoAwalQtyTotal);
            sheet.Cells[$"H{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"I{6 + result.Rows.Count}"].Value = "";
            sheet.Cells[$"I{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"J{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", SaldoAwalPriceTotal);
            sheet.Cells[$"J{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"K{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KoreksiQtyTotal);
            sheet.Cells[$"K{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"L{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KoreksiPriceTotal);
            sheet.Cells[$"L{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"M{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", PEMBELIANQtyTotal);
            sheet.Cells[$"M{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"N{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", PEMBELIANPriceTotal);
            sheet.Cells[$"N{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"O{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", PROSESQtyTotal);
            sheet.Cells[$"O{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"P{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", PROSESPriceTotal);
            sheet.Cells[$"P{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"Q{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", Konfeksi2AQtyTotal);
            sheet.Cells[$"Q{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"R{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", Konfeksi2APriceTotal);
            sheet.Cells[$"R{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"S{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI2BQtyTotal);
            sheet.Cells[$"S{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"T{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI2BPriceTotal);
            sheet.Cells[$"T{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"U{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI2CQtyTotal);
            sheet.Cells[$"U{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"V{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI2CPriceTotal);
            sheet.Cells[$"V{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"W{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI1MNSQtyTotal);
            sheet.Cells[$"W{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"X{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI1MNSPriceTotal);
            sheet.Cells[$"X{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"Y{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI2DQtyTotal);
            sheet.Cells[$"Y{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"Z{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", KONFEKSI2DPriceTotal);
            sheet.Cells[$"Z{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AA{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", SaldoAwalQtyTotal);
            sheet.Cells[$"AA{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AB{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", SaldoAwalPriceTotal);
            sheet.Cells[$"AB{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AC{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ReturQtyTotal);
            sheet.Cells[$"AC{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AD{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ReturJumlahTotal);
            sheet.Cells[$"AD{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AE{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendPROSESQtyTotal);
            sheet.Cells[$"AE{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AF{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendPROSESPriceTotal);
            sheet.Cells[$"AF{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AG{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", SAMPLEQtyTotal);
            sheet.Cells[$"AG{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AH{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", SAMPLEPriceTotal);
            sheet.Cells[$"AH{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AI{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI2AQtyTotal);
            sheet.Cells[$"AI{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AJ{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKonfeksi2APriceTotal);
            sheet.Cells[$"AJ{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AK{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI2BQtyTotal);
            sheet.Cells[$"AK{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AL{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI2BPriceTotal);
            sheet.Cells[$"AL{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AM{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI2CQtyTotal);
            sheet.Cells[$"AM{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AN{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI2CPriceTotal);
            sheet.Cells[$"AN{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AO{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI2DQtyTotal);
            sheet.Cells[$"AO{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AP{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI2DPriceTotal);
            sheet.Cells[$"AP{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AQ{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI1MNSQtyTotal);
            sheet.Cells[$"AQ{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AR{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", ExpendKONFEKSI1MNSPriceTotal);
            sheet.Cells[$"AR{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AS{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", EndingQty);
            sheet.Cells[$"AS{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AT{6 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", EndingTotal);
            sheet.Cells[$"AT{6 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);


            var widths = new int[] { 5, 10, 20, 15, 7, 20, 20, 10, 7, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };
            foreach (var i in Enumerable.Range(0, headers.Length))
            {
                sheet.Column(i + 1).Width = widths[i];
            }

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;


        }


    }
}
