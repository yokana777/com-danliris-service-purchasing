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
        public IEnumerable<AccountingStockReportViewModel> GetStockQuery(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
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
                          where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
                          //String.IsNullOrEmpty(a.UENNo) ? false : a.UENNo.Contains(unitcode)
                          //|| a.UnitCode == unitcode
                          //a.UENNo.Contains(unitcode) || a.UnitCode == unitcode
                          //a.UnitCode == unitcode || a.UENNo.Contains(unitcode)

                          //&& a.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                          && a.ReceiptDate.AddHours(offset).Date < DateFrom.Date
                          select new
                          {
                              ReceiptDate = a.ReceiptDate,
                              CodeRequirment = d.CodeRequirment,
                              ProductCode = b.ProductCode,
                              ProductName = b.ProductName,
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
                              a.IsDeleted
                          });
            var CobaPP = from a in PPAwal
                             //where a.ReceiptDate.AddHours(offset).Date < DateFrom.Date && a.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? a.CodeRequirment : ctg) && a.IsDeleted == false
                         where a.UENNo.Contains(unitcode) || a.UnitCode == unitcode
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
                          where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
                          //String.IsNullOrEmpty(a.UENNo) ? false : a.UENNo.Contains(unitcode)
                          //|| a.UnitCode == unitcode
                          //a.UnitCode == unitcode || a.UENNo.Contains(unitcode)
                          // a.UENNo.Contains(unitcode) || a.UnitCode == unitcode     /*String.IsNullOrEmpty(a.UENNo) ? true :*/ 
                         && a.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                          && a.ReceiptDate.AddHours(offset).Date <= DateTo.Date

                          select new
                          {
                              ReceiptDate = a.ReceiptDate,
                              CodeRequirment = d.CodeRequirment,
                              ProductCode = b.ProductCode,
                              ProductName = b.ProductName,
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
                              a.IsDeleted
                          };
            var CobaPPAkhir = from a in PPAkhir
                              where a.UENNo.Contains(unitcode) || a.UnitCode == unitcode
                              //where a.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                              //      && a.ReceiptDate.AddHours(offset).Date <= DateTo.Date
                              //      && a.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? a.CodeRequirment : ctg)
                              //      && a.IsDeleted == false 
                              select a;

            var SaldoAwal = from query in CobaPP
                            group query by new { query.ProductCode, query.ProductName, query.RO, query.PlanPo, query.POId, query.UnitCode, query.UnitSenderCode, query.UnitRequestName } into data
                            select new AccountingStockReportViewModel
                            {
                                ProductCode = data.Key.ProductCode,
                                ProductName = data.FirstOrDefault().ProductName,
                                RO = data.Key.RO,
                                Buyer = data.FirstOrDefault().Buyer,
                                PlanPo = data.FirstOrDefault().PlanPo,
                                NoArticle = data.FirstOrDefault().NoArticle,
                                BeginningBalanceQty = data.Sum(x => x.QtyReceipt) + Convert.ToDecimal(data.Sum(x => x.QtyCorrection)) - Convert.ToDecimal(data.Sum(x => x.QtyExpend)),
                                BeginningBalanceUom = data.FirstOrDefault().Uom,
                                BeginningBalancePrice = data.Sum(x => x.PriceReceipt) + Convert.ToDecimal(data.Sum(x => x.PriceCorrection)) - Convert.ToDecimal(data.Sum(x => x.PriceExpend)),
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
                            };
            var SaldoAkhir = from query in CobaPPAkhir
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
                                 ReceiptPurchaseQty = data.FirstOrDefault().URNType == "PEMBELIAN" && data.FirstOrDefault().UnitCode == unitcode ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptProcessQty = data.FirstOrDefault().URNType == "PROSES" && data.FirstOrDefault().UnitCode == unitcode ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon2AQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2A" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon2BQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2B" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon2CQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2C" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon1MNSQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1A" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptKon2DQty = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1B" ? data.Sum(x => x.QtyReceipt) : 0,
                                 ReceiptCorrectionPrice = 0,
                                 ReceiptPurchasePrice = data.FirstOrDefault().URNType == "PEMBELIAN" && data.FirstOrDefault().UnitCode == unitcode ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptProcessPrice = data.FirstOrDefault().URNType == "PROSES" && data.FirstOrDefault().UnitCode == unitcode ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2APrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2A" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2BPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2B" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2CPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C2C" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon1MNSPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1A" ? data.Sum(x => x.PriceReceipt) : 0,
                                 ReceiptKon2DPrice = data.FirstOrDefault().URNType == "GUDANG LAIN" && data.FirstOrDefault().UENNo.Substring(3, 3) == "C1B" ? data.Sum(x => x.PriceReceipt) : 0,
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
                                 EndingBalanceQty = Convert.ToDecimal(Convert.ToDouble(data.Sum(x => x.QtyReceipt)) - data.Sum(x => x.QtyExpend)),
                                 EndingBalancePrice = Convert.ToDecimal(Convert.ToDouble(data.Sum(x => x.PriceReceipt)) - data.Sum(x => x.PriceExpend)),
                                 POId = data.FirstOrDefault().POId
                             };

            List<AccountingStockReportViewModel> Data1 = SaldoAwal.Concat(SaldoAkhir).ToList();

            var Data = (from query in Data1
                        group query by new { query.POId, query.ProductCode, query.RO } into groupdata
                        select new AccountingStockReportViewModel
                        {
                            ProductCode = groupdata.FirstOrDefault().ProductCode == null ? "-" : groupdata.FirstOrDefault().ProductCode,
                            ProductName = groupdata.FirstOrDefault().ProductName == null ? "-" : groupdata.FirstOrDefault().ProductName,
                            RO = groupdata.FirstOrDefault().RO == null ? "-" : groupdata.FirstOrDefault().RO,
                            Buyer = groupdata.FirstOrDefault().Buyer == null ? "-" : groupdata.FirstOrDefault().Buyer,
                            PlanPo = groupdata.FirstOrDefault().PlanPo == null ? "-" : groupdata.FirstOrDefault().PlanPo,
                            NoArticle = groupdata.FirstOrDefault().NoArticle == null ? "-" : groupdata.FirstOrDefault().NoArticle,
                            BeginningBalanceQty = groupdata.Sum(x => x.BeginningBalanceQty),
                            BeginningBalanceUom = groupdata.FirstOrDefault().BeginningBalanceUom,
                            BeginningBalancePrice = groupdata.Sum(x => x.BeginningBalancePrice),
                            ReceiptCorrectionQty = 0,
                            ReceiptPurchaseQty = groupdata.Sum(x => x.ReceiptPurchaseQty),
                            ReceiptProcessQty = groupdata.Sum(x => x.ReceiptProcessQty),
                            ReceiptKon2AQty = groupdata.Sum(x => x.ReceiptKon2AQty),
                            ReceiptKon2BQty = groupdata.Sum(x => x.ReceiptKon2BQty),
                            ReceiptKon2CQty = groupdata.Sum(x => x.ReceiptKon2CQty),
                            ReceiptKon1MNSQty = groupdata.Sum(x => x.ReceiptKon1MNSQty),
                            ReceiptKon2DQty = groupdata.Sum(x => x.ReceiptKon2DQty),
                            ReceiptCorrectionPrice = 0,
                            ReceiptPurchasePrice = groupdata.Sum(x => x.ReceiptPurchasePrice),
                            ReceiptProcessPrice = groupdata.Sum(x => x.ReceiptProcessPrice),
                            ReceiptKon2APrice = groupdata.Sum(x => x.ReceiptKon2APrice),
                            ReceiptKon2BPrice = groupdata.Sum(x => x.ReceiptKon2BPrice),
                            ReceiptKon2CPrice = groupdata.Sum(x => x.ReceiptKon2CPrice),
                            ReceiptKon1MNSPrice = groupdata.Sum(x => x.ReceiptKon1MNSPrice),
                            ReceiptKon2DPrice = groupdata.Sum(x => x.ReceiptKon2DPrice),
                            ExpendReturQty = groupdata.Sum(x => x.ExpendReturQty),
                            ExpendRestQty = 0,
                            ExpendProcessQty = groupdata.Sum(x => x.ExpendProcessQty),
                            ExpendSampleQty = groupdata.Sum(x => x.ExpendSampleQty),
                            ExpendKon2AQty = groupdata.Sum(x => x.ExpendKon2AQty),
                            ExpendKon2BQty = groupdata.Sum(x => x.ExpendKon2BQty),
                            ExpendKon2CQty = groupdata.Sum(x => x.ExpendKon2CQty),
                            ExpendKon1MNSQty = groupdata.Sum(x => x.ExpendKon1MNSQty),
                            ExpendKon2DQty = groupdata.Sum(x => x.ExpendKon2DQty),
                            ExpendReturPrice = groupdata.Sum(x => x.ExpendReturPrice),
                            ExpendRestPrice = 0,
                            ExpendProcessPrice = groupdata.Sum(x => x.ExpendProcessPrice),
                            ExpendSamplePrice = groupdata.Sum(x => x.ExpendSamplePrice),
                            ExpendKon2APrice = groupdata.Sum(x => x.ExpendKon2APrice),
                            ExpendKon2BPrice = groupdata.Sum(x => x.ExpendKon2BPrice),
                            ExpendKon2CPrice = groupdata.Sum(x => x.ExpendKon2CPrice),
                            ExpendKon1MNSPrice = groupdata.Sum(x => x.ExpendKon1MNSPrice),
                            ExpendKon2DPrice = groupdata.Sum(x => x.ExpendKon2DPrice),
                            EndingBalanceQty = Convert.ToDecimal((groupdata.Sum(x => x.BeginningBalanceQty) + groupdata.Sum(x => x.ReceiptPurchaseQty) + groupdata.Sum(x => x.ReceiptProcessQty) + groupdata.Sum(x => x.ReceiptKon2AQty) + groupdata.Sum(x => x.ReceiptKon2BQty) + groupdata.Sum(x => x.ReceiptKon2CQty) + groupdata.Sum(x => x.ReceiptKon1MNSQty) + groupdata.Sum(x => x.ReceiptKon2DQty) + 0) - (Convert.ToDecimal(groupdata.Sum(x => x.ExpendProcessQty)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendSampleQty)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2AQty)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2BQty)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2CQty)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon1MNSQty)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2DQty)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendReturPrice)))),
                            EndingBalancePrice = Convert.ToDecimal((groupdata.Sum(x => x.BeginningBalancePrice) + groupdata.Sum(x => x.ReceiptPurchasePrice) + groupdata.Sum(x => x.ReceiptProcessPrice) + groupdata.Sum(x => x.ReceiptKon2APrice) + groupdata.Sum(x => x.ReceiptKon2BPrice) + groupdata.Sum(x => x.ReceiptKon2CPrice) + groupdata.Sum(x => x.ReceiptKon1MNSPrice) + groupdata.Sum(x => x.ReceiptKon2DPrice) + 0) - (Convert.ToDecimal(groupdata.Sum(x => x.ExpendReturPrice)) + 0 + Convert.ToDecimal(groupdata.Sum(x => x.ExpendProcessPrice)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendSamplePrice)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2APrice)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2BPrice)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2CPrice)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon1MNSPrice)) + Convert.ToDecimal(groupdata.Sum(x => x.ExpendKon2DPrice))))
                        });


            return Data.AsQueryable();

        }
        public MemoryStream GenerateExcelAStockReport(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            var Query = GetStockQuery(ctg, unitcode, datefrom, dateto, offset);
            DataTable result = new DataTable();
            var headers = new string[] { "No", "Kode", "Nama Barang", "RO", "Buyer", "PlanPO", "No Artikel", "Saldo Awal", "Saldo Awal1", "Saldo Awal2", "P E M B E L I A N", "P E M B E L I A N1", "P E M B E L I A N2", "P E M B E L I A N3", "P E M B E L I A N4", "P E M B E L I A N5", "P E M B E L I A N6", "P E M B E L I A N7", "P E M B E L I A N8", "P E M B E L I A N9", "P E M B E L I A N10", "P E M B E L I A N11", "P E M B E L I A N12", "P E M B E L I A N13", "P E M B E L I A N14", "P E M B E L I A N15", "P E N G E L U A R A N", "P E N G E L U A R A N1", "P E N G E L U A R A N2", "P E N G E L U A R A N3", "P E N G E L U A R A N4", "P E N G E L U A R A N5", "P E N G E L U A R A N6", "P E N G E L U A R A N7", "P E N G E L U A R A N8", "P E N G E L U A R A N9", "P E N G E L U A R A N10", "P E N G E L U A R A N11", "P E N G E L U A R A N12", "P E N G E L U A R A N13", "P E N G E L U A R A N14", "P E N G E L U A R A N15", "P E N G E L U A R A N16", "P E N G E L U A R A N17", "Saldo Akhir", "Saldo Akhir 1" };
            var headers2 = new string[] { "Koreksi", "Pembelian", "Proses", "KONFEKSI 2A", "KONFEKSI 2B", "KONFEKSI 2C", "KONFEKSI 1 MNS", "KONFEKSI 2D", "Retur", "Sisa", "Proses", "Sample", "KONFEKSI 2A", "KONFEKSI 2B", "KONFEKSI 2C", "KONFEKSI 1 MNS", "KONFEKSI 2D" };
            var subheaders = new string[] { "Jumlah", "Sat", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp", "Qty", "Rp" };
            for (int i = 0; i < headers.Length; i++)
            {
                result.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(string) });
            }
            var index = 1;
            foreach (var item in Query)
            {
                var ReceiptPurchaseQty = unitcode == "C2A" ? item.ReceiptPurchaseQty + item.ReceiptKon2AQty : unitcode == "C2B" ? item.ReceiptPurchaseQty + item.ReceiptKon2BQty : unitcode == "C2C" ? item.ReceiptPurchaseQty + item.ReceiptKon2CQty : unitcode == "C1B" ? item.ReceiptPurchaseQty + item.ReceiptKon2DQty : item.ReceiptPurchaseQty + item.ReceiptKon1MNSQty;
                var ReceiptPurchasePrice = unitcode == "C2A" ? item.ReceiptPurchasePrice + item.ReceiptKon2APrice : unitcode == "C2B" ? item.ReceiptPurchasePrice + item.ReceiptKon2BPrice : unitcode == "C2C" ? item.ReceiptPurchasePrice + item.ReceiptKon2CPrice : unitcode == "C1B" ? item.ReceiptPurchaseQty + item.ReceiptKon1MNSPrice : item.ReceiptPurchasePrice + item.ReceiptKon2DPrice;
                var ReceiptKon2AQty = unitcode == "C2A" ? "-" : item.ReceiptKon2AQty.ToString();
                var ReceiptKon2APrice = unitcode == "C2A" ? "-" : item.ReceiptKon2APrice.ToString();
                var ReceiptKon2BPrice = unitcode == "C2B" ? "-" : item.ReceiptKon2BPrice.ToString();
                var ReceiptKon2BQty = unitcode == "C2B" ? "-" : item.ReceiptKon2BQty.ToString();
                var ReceiptKon2CPrice = unitcode == "C2C" ? "-" : item.ReceiptKon2CQty.ToString();
                var ReceiptKon2CQty = unitcode == "C2C" ? "-" : item.ReceiptKon2CQty.ToString();
                var ReceiptKon2DPrice = unitcode == "C1B" ? "-" : item.ReceiptKon2DPrice.ToString();
                var ReceiptKon2DQty = unitcode == "C1B" ? "-" : item.ReceiptKon2DQty.ToString();
                var ReceiptKon1MNSQty = unitcode == "C1A" ? "-" : item.ReceiptKon1MNSQty.ToString();
                var ReceiptKon1MNSPrice = unitcode == "C1A" ? "-" : item.ReceiptKon1MNSPrice.ToString();
                var ReceiptCorrection = item.ReceiptCorrectionPrice;

                result.Rows.Add(index++, item.ProductCode, item.ProductName, item.RO, item.Buyer, item.PlanPo, item.NoArticle, Convert.ToDouble(item.BeginningBalanceQty), item.BeginningBalanceUom, Convert.ToDouble(item.BeginningBalancePrice), Convert.ToDouble(item.ReceiptCorrectionQty), Convert.ToDouble(item.ReceiptCorrectionPrice), Convert.ToDouble(ReceiptPurchaseQty), Convert.ToDouble(ReceiptPurchasePrice), Convert.ToDouble(item.ReceiptProcessQty), Convert.ToDouble(item.ReceiptProcessPrice), ReceiptKon2AQty, ReceiptKon2APrice, ReceiptKon2BQty, ReceiptKon2BPrice, ReceiptKon2CQty, ReceiptKon2CPrice, ReceiptKon1MNSQty, ReceiptKon1MNSPrice, ReceiptKon2DQty, ReceiptKon2DPrice, item.ExpendReturQty, item.ExpendReturPrice, item.ExpendRestQty, item.ExpendRestPrice, item.ExpendProcessQty, item.ExpendProcessPrice, item.ExpendSampleQty, item.ExpendSamplePrice, item.ExpendKon2AQty, item.ExpendKon2APrice, item.ExpendKon2BQty, item.ExpendKon2BPrice, item.ExpendKon2CQty, item.ExpendKon2CPrice, item.ExpendKon1MNSQty, item.ExpendKon1MNSPrice, item.ExpendKon2DQty, item.ExpendKon2DPrice, Convert.ToDouble(item.EndingBalanceQty), Convert.ToDouble(item.EndingBalancePrice));

            }

            ExcelPackage package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Data");

            sheet.Cells["A4"].LoadFromDataTable(result, false, OfficeOpenXml.Table.TableStyles.Light16);
            sheet.Cells["H1"].Value = headers[7];
            sheet.Cells["H1:J2"].Merge = true;
            sheet.Cells["K1"].Value = headers[10];
            sheet.Cells["K1:Z1"].Merge = true;
            sheet.Cells["AA1"].Value = headers[26];
            sheet.Cells["AA1:AR1"].Merge = true;
            sheet.Cells["AS1"].Value = headers[44];
            sheet.Cells["AS1:AT2"].Merge = true;
            sheet.Cells["K2"].Value = headers2[0];
            sheet.Cells["K2:L2"].Merge = true;
            sheet.Cells["M2"].Value = headers2[1];
            sheet.Cells["M2:N2"].Merge = true;
            sheet.Cells["O2"].Value = headers2[2];
            sheet.Cells["O2:P2"].Merge = true;
            sheet.Cells["Q2"].Value = headers2[3];
            sheet.Cells["Q2:R2"].Merge = true;
            sheet.Cells["S2"].Value = headers2[4];
            sheet.Cells["S2:T2"].Merge = true;
            sheet.Cells["U2"].Value = headers2[5];
            sheet.Cells["U2:V2"].Merge = true;
            sheet.Cells["W2"].Value = headers2[6];
            sheet.Cells["W2:X2"].Merge = true;
            sheet.Cells["Y2"].Value = headers2[7];
            sheet.Cells["Y2:Z2"].Merge = true;
            sheet.Cells["AA2"].Value = headers2[8];
            sheet.Cells["AA2:AB2"].Merge = true;
            sheet.Cells["AB2"].Value = headers2[9];
            sheet.Cells["AC2:AD2"].Merge = true;
            sheet.Cells["AE2"].Value = headers2[10];
            sheet.Cells["AE2:AF2"].Merge = true;
            sheet.Cells["AG2"].Value = headers2[11];
            sheet.Cells["AG2:AH2"].Merge = true;
            sheet.Cells["AI2"].Value = headers2[12];
            sheet.Cells["AI2:AJ2"].Merge = true;
            sheet.Cells["AK2"].Value = headers2[13];
            sheet.Cells["AK2:AL2"].Merge = true;
            sheet.Cells["AM2"].Value = headers2[14];
            sheet.Cells["AM2:AN2"].Merge = true;
            sheet.Cells["AO2"].Value = headers2[15];
            sheet.Cells["AO2:AP2"].Merge = true;
            sheet.Cells["AQ2"].Value = headers2[16];
            sheet.Cells["AQ2:AR2"].Merge = true;

            foreach (var i in Enumerable.Range(0, 7))
            {
                var col = (char)('A' + i);
                sheet.Cells[$"{col}1"].Value = headers[i];
                sheet.Cells[$"{col}1:{col}3"].Merge = true;
            }

            for (var i = 0; i < 19; i++)
            {
                var col = (char)('H' + i);
                sheet.Cells[$"{col}3"].Value = subheaders[i];

            }

            for (var i = 19; i < 39; i++)
            {
                var col = (char)('A' + i - 19);
                sheet.Cells[$"A{col}3"].Value = subheaders[i];

            }
            sheet.Cells["A1:AS3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A1:AS3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A1:AS3"].Style.Font.Bold = true;

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
