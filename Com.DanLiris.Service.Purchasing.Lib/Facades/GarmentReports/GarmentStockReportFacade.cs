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

        public List<GarmentStockReportViewModel> GetStockQuery(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;

            var lastdate = dbContext.BalanceStocks.OrderByDescending(x => x.CreateDate).Select(x => x.CreateDate).FirstOrDefault();

            var BalaceStock = (from a in dbContext.BalanceStocks
                              where a.CreateDate == lastdate
                              group a by new { a.ArticleNo, a.EPOID, a.EPOItemId } into data
                              select new
                              {
                                  ArticleNo = data.FirstOrDefault().ArticleNo,
                                  EPOID = data.FirstOrDefault().EPOID,
                                  EPOItemId = data.FirstOrDefault().EPOItemId,
                                  CloseStock = (double)data.Sum(x => x.CloseStock),
                                  ClosePrice = (decimal)data.Sum(x => x.ClosePrice)
                              }).ToList();
            var SaldoAwal = (from a in dbContext.GarmentUnitReceiptNotes
                            join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                            join c in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals c.Id
                            join d in BalaceStock on b.EPOItemId equals d.EPOItemId.Value into balance
                            from bb in balance.DefaultIfEmpty()
                            join e in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals e.Id into UENItem
                            from ee in UENItem.DefaultIfEmpty()
                            join h in dbContext.GarmentReceiptCorrectionItems on b.Id equals h.URNItemId into RC
                            from hh in RC.DefaultIfEmpty()
                            join f in dbContext.GarmentUnitExpenditureNotes on ee.UENId equals f.Id into UEN
                            from ff in UEN.DefaultIfEmpty()
                            join g in dbContext.GarmentExternalPurchaseOrders on Convert.ToInt32(bb.EPOID) equals g.Id into Exter
                            from gg in Exter.DefaultIfEmpty()
                            where c.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? c.CodeRequirment : ctg)
                            && a.IsDeleted == false && b.IsDeleted == false
                            && a.CreatedUtc > lastdate.Value && a.CreatedUtc < DateFrom
                            select new
                            {
                                EPOID = bb.EPOID,
                                EPOItemId = bb == null ? 0 : bb.EPOItemId,
                                SAQty = (bb != null ? bb.CloseStock : 0) + (hh != null ? hh.CorrectionQuantity : 0) + (double)b.ReceiptQuantity - (ee != null ? ee.Quantity : 0),
                                SAPrice = (bb != null ? bb.ClosePrice : 0) + (hh != null ? (decimal)hh.PricePerDealUnit * (decimal)hh.CorrectionQuantity : 0) +  b.PricePerDealUnit * b.ReceiptQuantity - (ee != null ? (decimal)ee.PricePerDealUnit * (decimal)ee.Quantity : 0)
                            });
            //var SaldoAkhir = (from a in dbContext.GarmentUnitReceiptNotes
            //                  join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
            //                  join h in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals h.Id
            //                  join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
            //                  join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
            //                  from ty in RC.DefaultIfEmpty()
            //                  join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
            //                  from ww in UE.DefaultIfEmpty()
            //                  join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
            //                  from dd in UEN.DefaultIfEmpty()
            //                  join epoItem in dbContext.GarmentExternalPurchaseOrderItems on b.EPOItemId equals epoItem.Id into EP
            //                  from epoItem in EP.DefaultIfEmpty()
            //                  join epo in dbContext.GarmentExternalPurchaseOrders on epoItem.GarmentEPOId equals epo.Id into EPO
            //                  from epo in EPO.DefaultIfEmpty()
            //                  where h.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? h.CodeRequirment : ctg)
            //                  && a.IsDeleted == false && b.IsDeleted == false
            //                  && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
            //                  && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
            //                  select new
            //                  {
            //                      ReceiptDate = a.ReceiptDate,
            //                      CodeRequirment = h.CodeRequirment,
            //                      ProductCode = b.ProductCode,
            //                      ProductName = b.ProductName,
            //                      ProductRemark = b.ProductRemark,
            //                      RO = b.RONo,
            //                      Uom = b.UomUnit,
            //                      Buyer = f.BuyerCode,
            //                      PlanPo = b.POSerialNumber,
            //                      NoArticle = f.Article,
            //                      QtyReceipt = b.ReceiptQuantity,
            //                      QtyCorrection = ty.POSerialNumber == null ? 0 : ty.CorrectionQuantity,
            //                      QtyExpend = ww.POSerialNumber == null ? 0 : ww.Quantity,
            //                      PriceReceipt = b.PricePerDealUnit,
            //                      PriceCorrection = ty.POSerialNumber == null ? 0 : ty.PricePerDealUnit,
            //                      PriceExpend = ww.POSerialNumber == null ? 0 : ww.PricePerDealUnit,
            //                      EPOId = epo.Id,
            //                      EPOItem = epoItem.Id,
            //                      URNType = a.URNType,
            //                      UnitCode = a.UnitCode,
            //                      UENNo = dd.UENNo == null ? "-" : dd.UENNo ,
            //                      UnitSenderCode = dd.UnitSenderCode == null ? "-" : dd.UnitSenderCode,
            //                      UnitRequestName = dd.UnitRequestName == null ? "-" : dd.UnitRequestName,
            //                      ExpenditureTo = dd.ExpenditureTo == null ? "-" : dd.ExpenditureTo,
            //                      PaymentMethod = epo.PaymentMethod == null ? "-" : epo.PaymentMethod,
            //                      a.IsDeleted
            //                  }).Distinct().ToList();
            var SaldoAkhir = (from a in dbContext.GarmentUnitReceiptNotes
                              join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                              join h in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals h.Id
                              join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
                              join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
                              from ty in RC.DefaultIfEmpty()
                              join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
                              from ww in UE.DefaultIfEmpty()
                              join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                              from dd in UEN.DefaultIfEmpty()
                              join epoItem in dbContext.GarmentExternalPurchaseOrderItems on b.EPOItemId equals epoItem.Id into EP
                              from epoItem in EP.DefaultIfEmpty()
                              join epo in dbContext.GarmentExternalPurchaseOrders on epoItem.GarmentEPOId equals epo.Id into EPO
                              from epo in EPO.DefaultIfEmpty()
                              where h.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? h.CodeRequirment : ctg)
                              && a.IsDeleted == false && b.IsDeleted == false
                              && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                              && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                              select new
                              {
                                  URNId = a.Id,
                                  URNItemsId = b.Id,
                                  DODEtailId = h.Id,
                                  IPOId = f.Id,
                                  RCorrId = ty == null ? 0 : ty.Id,
                                  UENItemsId = ww == null ? 0 : ww.Id,
                                  UENId = dd == null ? 0: dd.Id,
                                  EPOItemId = epoItem == null ? 0 : epoItem.Id,
                                  EPOId = epo == null ? 0 : epo.Id,
                                  UnitCode = a.UnitCode,
                                  UnitSenderCode = dd == null ? "-" : dd.UnitSenderCode

                              }).Distinct().ToList();
            //SaldoAkhir = SaldoAkhir.Where(x => x.UENNo.Contains((String.IsNullOrWhiteSpace(unitcode) ? x.UnitCode : unitcode)) || x.UnitCode == ((String.IsNullOrWhiteSpace(unitcode) ? x.UnitCode : unitcode))).Select(x => x).ToList();
            SaldoAkhir = SaldoAkhir.Where(x => x.UnitSenderCode == (String.IsNullOrWhiteSpace(unitcode) ? x.UnitSenderCode : unitcode) || x.UnitCode == ((String.IsNullOrWhiteSpace(unitcode) ? x.UnitCode : unitcode))).Select(x => x).ToList();
            var unitreceiptnoteIds = SaldoAkhir.Select(x => x.URNId).ToList();
            var unitreceiptnotes = dbContext.GarmentUnitReceiptNotes.Where(x=> unitreceiptnoteIds.Contains(x.Id)).Select(s=> new { s.ReceiptDate,  s.URNType, s.UnitCode, s.Id }).ToList();
            var unitreceiptnoteItemIds = SaldoAkhir.Select(x => x.URNItemsId).ToList();
            var unitreceiptnoteItems = dbContext.GarmentUnitReceiptNoteItems.Where(x=> unitreceiptnoteItemIds.Contains(x.Id)).Select(s=> new { s.ProductCode, s.ProductName, s.ProductRemark, s.RONo, s.UomUnit, s.POSerialNumber, s.ReceiptQuantity, s.PricePerDealUnit, s.Id }).ToList();
            var deliveryorderdetailid = SaldoAkhir.Select(x => x.DODEtailId).ToList();
            var deliveryorderdetails = dbContext.GarmentDeliveryOrderDetails.Where(x=> deliveryorderdetailid.Contains(x.Id)).Select(s=> new { s.CodeRequirment, s.Id }).ToList();
            var purchaseorderIds = SaldoAkhir.Select(x => x.IPOId).ToList();
            var purchaseorders = dbContext.GarmentInternalPurchaseOrders.Where(x=>purchaseorderIds.Contains(x.Id)).Select(s=> new { s.BuyerCode, s.Id, s.Article }).ToList();
            var receiptCorrectionitemIds = SaldoAkhir.Select(x => x.RCorrId).ToList();
            var receiptCorrectionitems = dbContext.GarmentReceiptCorrectionItems.Where(x => receiptCorrectionitemIds.Contains(x.Id)).Select(s => new { s.CorrectionQuantity, s.PricePerDealUnit, s.Id }).ToList();
            var unitexpenditureitemIds = SaldoAkhir.Select(x => x.UENItemsId).ToList();
            var unitexpenditureitems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => unitexpenditureitemIds.Contains(x.Id)).Select(s => new { s.PricePerDealUnit, s.Quantity, s.Id }).ToList();
            var unitexpenditureIds = SaldoAkhir.Select(x => x.UENId).ToList();
            var unitexpenditures = dbContext.GarmentUnitExpenditureNotes.Where(x => unitexpenditureIds.Contains(x.Id)).Select(s => new { s.UnitRequestCode, s.UnitSenderCode, s.ExpenditureTo, s.Id }).ToList();
            var externalpurchaseorderitemIds = SaldoAkhir.Select(x => x.EPOItemId).ToList();
            var externalpurchaseorderitems = dbContext.GarmentExternalPurchaseOrderItems.Where(x => externalpurchaseorderitemIds.Contains(x.Id)).Select(s => new { s.Id }).ToList();
            var externalpurchaseorderIds = SaldoAkhir.Select(x => x.EPOId).ToList();
            var externalpurchaseorders = dbContext.GarmentExternalPurchaseOrders.Where(x => externalpurchaseorderIds.Contains(x.Id)).Select(s => new { s.PaymentMethod, s.Id }).ToList();
            var balancestocks = SaldoAwal.Where(x => externalpurchaseorderitemIds.Contains((long)x.EPOItemId)).Select(s => new { s.SAQty, s.SAPrice, s.EPOItemId}).ToList();
            List<GarmentStockReportViewModel> stock = new List<GarmentStockReportViewModel>();
            foreach (var item in SaldoAkhir)
            {
                var unitreceiptnote = unitreceiptnotes.Where(x => x.Id.Equals(item.URNId)).FirstOrDefault();
                var unitreceiptnoteItem = unitreceiptnoteItems.FirstOrDefault(x => x.Id.Equals(item.URNItemsId));
                var deliveryorderdetail = deliveryorderdetails.FirstOrDefault(x => x.Id.Equals(item.DODEtailId));
                var purchaseorder = purchaseorders.FirstOrDefault(x => x.Id.Equals(item.IPOId));
                var receiptCorrectionitem = receiptCorrectionitems.FirstOrDefault(x => x.Id.Equals(item.RCorrId));
                var unitexpenditureitem = unitexpenditureitems.FirstOrDefault(x => x.Id.Equals(item.URNItemsId));
                var unitexpenditure = unitexpenditures.FirstOrDefault(x => x.Id.Equals(item.URNId));
                var externalpurchaseorderitem = externalpurchaseorderitems.FirstOrDefault(x => x.Id.Equals(item.EPOItemId));
                var externalpurchaseorder = externalpurchaseorders.FirstOrDefault(x => x.Id.Equals(item.EPOId));
                var balancestock = balancestocks.FirstOrDefault(x => x.EPOItemId.Equals(item.EPOItemId));

                if(balancestock == null)
                {
                    stock.Add(new GarmentStockReportViewModel
                    {
                        BeginningBalanceQty = 0,
                        BeginningBalanceUom = unitreceiptnoteItem.UomUnit,
                        Buyer = purchaseorder.BuyerCode,
                        EndingBalanceQty = unitreceiptnoteItem.ReceiptQuantity + (receiptCorrectionitem == null ? 0 : (decimal)receiptCorrectionitem.CorrectionQuantity) - (unitexpenditureitem == null ? 0 : (decimal)unitexpenditureitem.Quantity),
                        EndingUom = unitreceiptnoteItem.UomUnit,
                        ExpandUom = unitreceiptnoteItem.UomUnit,
                        ExpendQty = unitexpenditureitem == null ? 0 : unitexpenditureitem.Quantity,
                        NoArticle = purchaseorder.Article,
                        PaymentMethod = externalpurchaseorder.PaymentMethod,
                        PlanPo = unitreceiptnoteItem == null ? "-" : unitreceiptnoteItem.POSerialNumber,
                        POId = unitexpenditure == null ? 0 : unitexpenditure.Id,
                        ProductCode = unitreceiptnoteItem.ProductCode,
                        ProductName = unitreceiptnoteItem.ProductName,
                        ProductRemark = unitreceiptnoteItem.ProductRemark,
                        ReceiptCorrectionQty = receiptCorrectionitem == null ? 0 : (decimal)receiptCorrectionitem.CorrectionQuantity,
                        ReceiptQty = unitreceiptnoteItem.ReceiptQuantity,
                        ReceiptUom = unitreceiptnoteItem.UomUnit,
                        RO = unitreceiptnoteItem.RONo
                    });
                }
                else
                {
                    stock.Add(new GarmentStockReportViewModel
                    {
                        BeginningBalanceQty = (decimal)balancestock.SAQty,
                        BeginningBalanceUom = unitreceiptnoteItem.UomUnit,
                        Buyer = purchaseorder.BuyerCode,
                        EndingBalanceQty = unitreceiptnoteItem.ReceiptQuantity + (receiptCorrectionitem == null ? 0 : (decimal)receiptCorrectionitem.CorrectionQuantity) - (unitexpenditureitem == null ? 0 : (decimal)unitexpenditureitem.Quantity),
                        EndingUom = unitreceiptnoteItem.UomUnit,
                        ExpandUom = unitreceiptnoteItem.UomUnit,
                        ExpendQty = unitexpenditureitem == null ? 0 : unitexpenditureitem.Quantity,
                        NoArticle = purchaseorder.Article,
                        PaymentMethod = externalpurchaseorder.PaymentMethod,
                        PlanPo = unitreceiptnoteItem == null ? "-" : unitreceiptnoteItem.POSerialNumber,
                        POId = unitexpenditure == null ? 0 : unitexpenditure.Id,
                        ProductCode = unitreceiptnoteItem.ProductCode,
                        ProductName = unitreceiptnoteItem.ProductName,
                        ProductRemark = unitreceiptnoteItem.ProductRemark,
                        ReceiptCorrectionQty = receiptCorrectionitem == null ? 0 : (decimal)receiptCorrectionitem.CorrectionQuantity,
                        ReceiptQty = unitreceiptnoteItem.ReceiptQuantity,
                        ReceiptUom = unitreceiptnoteItem.UomUnit,
                        RO = unitreceiptnoteItem.RONo
                    });
                }

                //var SA = SaldoAwal.Where(x => x.EPOItemId == item.EPOItem).FirstOrDefault();
                //if (SA == null)
                //{
                //    stock.Add(new GarmentStockReportViewModel
                //    {
                //        BeginningBalanceQty = 0,
                //        BeginningBalanceUom = item.Uom,
                //        Buyer = item.Buyer,
                //        EndingBalanceQty = item.QtyReceipt + (decimal)item.QtyCorrection - (decimal)item.QtyExpend,
                //        EndingUom = item.Uom,
                //        ExpandUom = item.Uom,
                //        ExpendQty = item.QtyExpend,
                //        NoArticle = item.NoArticle,
                //        PaymentMethod = item.PaymentMethod,
                //        PlanPo = item.PlanPo,
                //        POId = item.EPOId,
                //        ProductCode = item.ProductCode,
                //        ProductName = item.ProductName,
                //        ProductRemark = item.ProductRemark,
                //        ReceiptCorrectionQty = (decimal)item.QtyCorrection,
                //        ReceiptQty = item.QtyReceipt,
                //        ReceiptUom = item.Uom,
                //        RO = item.RO
                //    });
                //}
                //else
                //{
                //    stock.Add(new GarmentStockReportViewModel
                //    {
                //        BeginningBalanceQty = (decimal)SA.SAQty,
                //        BeginningBalanceUom = item.Uom,
                //        Buyer = item.Buyer,
                //        EndingBalanceQty = item.QtyReceipt + (decimal)item.QtyCorrection - (decimal)item.QtyExpend,
                //        EndingUom = item.Uom,
                //        ExpandUom = item.Uom,
                //        ExpendQty = item.QtyExpend,
                //        NoArticle = item.NoArticle,
                //        PaymentMethod = item.PaymentMethod,
                //        PlanPo = item.PlanPo,
                //        POId = item.EPOId,
                //        ProductCode = item.ProductCode,
                //        ProductName = item.ProductName,
                //        ProductRemark = item.ProductRemark,
                //        ReceiptCorrectionQty = (decimal)item.QtyCorrection,
                //        ReceiptQty = item.QtyReceipt,
                //        ReceiptUom = item.Uom,
                //        RO = item.RO
                //    });
                //}

            }

            stock = (from a in stock
                     group a by a.PlanPo into data
                     select new GarmentStockReportViewModel
                     {
                         BeginningBalanceQty = data.Sum(x => x.BeginningBalanceQty),
                         BeginningBalanceUom = data.FirstOrDefault().BeginningBalanceUom,
                         Buyer = data.FirstOrDefault().Buyer,
                         EndingBalanceQty = data.Sum(x => x.EndingBalanceQty),
                         EndingUom = data.FirstOrDefault().EndingUom,
                         ExpandUom = data.FirstOrDefault().ExpandUom,
                         ExpendQty = data.FirstOrDefault().ExpendQty,
                         NoArticle = data.FirstOrDefault().NoArticle,
                         PaymentMethod = data.FirstOrDefault().PaymentMethod,
                         PlanPo = data.FirstOrDefault().PlanPo,
                         POId = data.FirstOrDefault().POId,
                         ProductCode = data.FirstOrDefault().ProductCode,
                         ProductName = data.FirstOrDefault().ProductName,
                         ProductRemark = data.FirstOrDefault().ProductRemark,
                         ReceiptCorrectionQty = data.Sum(x => x.ReceiptCorrectionQty),
                         ReceiptQty = data.Sum(x => x.ReceiptQty),
                         ReceiptUom = data.FirstOrDefault().ReceiptUom,
                         RO = data.FirstOrDefault().RO
                     }).ToList();

            //var SaldoAkhir2 = (from a in SaldoAkhir
            //                  join b in SaldoAwal on a.EPOItem equals b.EPOItemId.Value into SAwal
            //                  from bb in SAwal.DefaultIfEmpty()
            //                  select new GarmentStockReportViewModel
            //                  {
            //                      //BeginningBalanceQty = bb != null ? (decimal)bb.SAQty.Value : 0,
            //                      BeginningBalanceUom = a.Uom,
            //                      Buyer = a.Buyer,
            //                      //EndingBalanceQty = a.QtyReceipt + (decimal)a.QtyCorrection - (decimal)a.QtyExpend,
            //                      EndingUom = a.Uom,
            //                      ExpandUom = a.Uom,
            //                      //ExpendQty = a.QtyExpend,
            //                      NoArticle = a.NoArticle,
            //                      PaymentMethod = a.PaymentMethod,
            //                      PlanPo = a.PlanPo,
            //                      POId = a.EPOId,
            //                      ProductCode = a.ProductCode,
            //                      ProductName = a.ProductName,
            //                      ProductRemark = a.ProductRemark,
            //                      //ReceiptCorrectionQty = (decimal)a.QtyCorrection,
            //                      ReceiptQty = a.QtyReceipt,
            //                      ReceiptUom = a.Uom,
            //                      RO = a.RO

                    //                  }).ToList();
            return stock;
            //var PPAwal = (from a in dbContext.GarmentUnitReceiptNotes
            //              join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
            //              join d in dbContext.GarmentDeliveryOrderDetails on b.POId equals d.POId
            //              join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
            //              join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
            //              from ty in RC.DefaultIfEmpty()
            //              join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
            //              from ww in UE.DefaultIfEmpty()
            //              join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
            //              from dd in UEN.DefaultIfEmpty()
            //              join epoItem in dbContext.GarmentExternalPurchaseOrderItems on b.POId equals epoItem.POId into EP
            //              from epoItem in EP.DefaultIfEmpty()
            //              join epo in dbContext.GarmentExternalPurchaseOrders on epoItem.GarmentEPOId equals epo.Id into EPO
            //              from epo in EPO.DefaultIfEmpty()
            //              where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
            //              && a.IsDeleted == false && b.IsDeleted == false
            //              //String.IsNullOrEmpty(a.UENNo) ? false : a.UENNo.Contains(unitcode)
            //              //|| a.UnitCode == unitcode
            //              //a.UENNo.Contains(unitcode) || a.UnitCode == unitcode
            //              //a.UnitCode == unitcode || a.UENNo.Contains(unitcode)

                             //              //&& a.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                             //              && a.CreatedUtc.AddHours(offset).Date < DateFrom.Date
                             //              select new
                             //              {
                             //                  ReceiptDate = a.ReceiptDate,
                             //                  CodeRequirment = d.CodeRequirment,
                             //                  ProductCode = b.ProductCode,
                             //                  ProductName = b.ProductName,
                             //                  ProductRemark = b.ProductRemark,
                             //                  RO = b.RONo,
                             //                  Uom = b.UomUnit,
                             //                  Buyer = f.BuyerCode,
                             //                  PlanPo = b.POSerialNumber,
                             //                  NoArticle = f.Article,
                             //                  QtyReceipt = b.ReceiptQuantity,
                             //                  QtyCorrection = ty.POSerialNumber == null ? 0 : ty.Quantity,
                             //                  QtyExpend = ww.POSerialNumber == null ? 0 : ww.Quantity,
                             //                  PriceReceipt = b.PricePerDealUnit,
                             //                  PriceCorrection = ty.POSerialNumber == null ? 0 : ty.PricePerDealUnit,
                             //                  PriceExpend = ww.POSerialNumber == null ? 0 : ww.PricePerDealUnit,
                             //                  POId = b.POId,
                             //                  URNType = a.URNType,
                             //                  UnitCode = a.UnitCode,
                             //                  UENNo = a.UENNo,
                             //                  UnitSenderCode = dd.UnitSenderCode == null ? "-" : dd.UnitSenderCode,
                             //                  UnitRequestName = dd.UnitRequestName == null ? "-" : dd.UnitRequestName,
                             //                  ExpenditureTo = dd.ExpenditureTo == null ? "-" : dd.ExpenditureTo,
                             //                  PaymentMethod = epo.PaymentMethod == null ? "-": epo.PaymentMethod,
                             //                  a.IsDeleted
                             //              });
                             //var CobaPP = from a in PPAwal
                             //                 //where a.ReceiptDate.AddHours(offset).Date < DateFrom.Date && a.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? a.CodeRequirment : ctg) && a.IsDeleted == false
                             //             where a.UENNo.Contains((String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)) || a.UnitCode == (String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)
                             //             select a;
                             //var PPAkhir = from a in dbContext.GarmentUnitReceiptNotes
                             //              join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                             //              join d in dbContext.GarmentDeliveryOrderDetails on b.POId equals d.POId
                             //              join f in dbContext.GarmentInternalPurchaseOrders on b.POId equals f.Id
                             //              //join f in SaldoAwal on b.POId equals f.POID
                             //              join e in dbContext.GarmentReceiptCorrectionItems on b.Id equals e.URNItemId into RC
                             //              from ty in RC.DefaultIfEmpty()
                             //              join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
                             //              from ww in UE.DefaultIfEmpty()
                             //              join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                             //              from dd in UEN.DefaultIfEmpty()
                             //              join epoItem in dbContext.GarmentExternalPurchaseOrderItems on b.POId equals epoItem.POId into EP
                             //              from epoItem in EP.DefaultIfEmpty()
                             //              join epo in dbContext.GarmentExternalPurchaseOrders on  epoItem.GarmentEPOId equals epo.Id into EPO
                             //              from epo in EPO.DefaultIfEmpty()

                             //              where d.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? d.CodeRequirment : ctg)
                             //              && a.IsDeleted == false && b.IsDeleted == false
                             //              //String.IsNullOrEmpty(a.UENNo) ? false : a.UENNo.Contains(unitcode)
                             //              //|| a.UnitCode == unitcode
                             //              //a.UnitCode == unitcode || a.UENNo.Contains(unitcode)
                             //              // a.UENNo.Contains(unitcode) || a.UnitCode == unitcode     /*String.IsNullOrEmpty(a.UENNo) ? true :*/ 
                             //             && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                             //              && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date

                             //              select new
                             //              {
                             //                  ReceiptDate = a.ReceiptDate,
                             //                  CodeRequirment = d.CodeRequirment,
                             //                  ProductCode = b.ProductCode,
                             //                  ProductName = b.ProductName,
                             //                  ProductRemark = b.ProductRemark,
                             //                  RO = b.RONo,
                             //                  Uom = b.UomUnit,
                             //                  Buyer = f.BuyerCode,
                             //                  PlanPo = b.POSerialNumber,
                             //                  NoArticle = f.Article,
                             //                  QtyReceipt = b.ReceiptQuantity,
                             //                  QtyCorrection = ty.POSerialNumber == null ? 0 : ty.Quantity,
                             //                  QtyExpend = ww.POSerialNumber == null ? 0 : ww.Quantity,
                             //                  PriceReceipt = b.PricePerDealUnit,
                             //                  PriceCorrection = ty.POSerialNumber == null ? 0 : ty.PricePerDealUnit,
                             //                  PriceExpend = ww.POSerialNumber == null ? 0 : ww.PricePerDealUnit,
                             //                  POId = b.POId,
                             //                  URNType = a.URNType,
                             //                  UnitCode = a.UnitCode,
                             //                  UENNo = a.UENNo,
                             //                  UnitSenderCode = dd.UnitSenderCode == null ? "-" : dd.UnitSenderCode,
                             //                  UnitRequestName = dd.UnitRequestName == null ? "-" : dd.UnitRequestName,
                             //                  ExpenditureTo = dd.ExpenditureTo == null ? "-" : dd.ExpenditureTo,
                             //                  PaymentMethod = epo.PaymentMethod == null ? "-" : epo.PaymentMethod,
                             //                  a.IsDeleted
                             //              };
                             //var CobaPPAkhir = from a in PPAkhir
                             //                  where a.UENNo.Contains((String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)) || a.UnitCode == (String.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)
                             //                  //where a.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                             //                  //      && a.ReceiptDate.AddHours(offset).Date <= DateTo.Date
                             //                  //      && a.CodeRequirment == (String.IsNullOrWhiteSpace(ctg) ? a.CodeRequirment : ctg)
                             //                  //      && a.IsDeleted == false 
                             //                  select a;
                             //var SaldoAwal = from query in CobaPP
                             //                group query by new { query.ProductCode, query.ProductName, query.RO, query.PlanPo, query.POId, query.UnitCode, query.UnitSenderCode, query.UnitRequestName } into data
                             //                select new GarmentStockReportViewModel
                             //                {
                             //                    ProductCode = data.Key.ProductCode,
                             //                    RO = data.Key.RO,
                             //                    PlanPo = data.FirstOrDefault().PlanPo,
                             //                    NoArticle = data.FirstOrDefault().NoArticle,
                             //                    ProductName = data.FirstOrDefault().ProductName,
                             //                    ProductRemark= data.FirstOrDefault().ProductRemark,
                             //                    Buyer = data.FirstOrDefault().Buyer,
                             //                    BeginningBalanceQty = data.Sum(x => x.QtyReceipt) + Convert.ToDecimal(data.Sum(x => x.QtyCorrection)) - Convert.ToDecimal(data.Sum(x => x.QtyExpend)),
                             //                    BeginningBalanceUom = data.FirstOrDefault().Uom,
                             //                    ReceiptCorrectionQty = 0,
                             //                    ReceiptQty =0,
                             //                    ReceiptUom =data.FirstOrDefault().Uom,
                             //                    ExpendQty =0,
                             //                    ExpandUom = data.FirstOrDefault().Uom,
                             //                    EndingBalanceQty = 0,
                             //                    EndingUom= data.FirstOrDefault().Uom,
                             //                    POId = data.FirstOrDefault().POId,
                             //                    PaymentMethod = data.FirstOrDefault().PaymentMethod

                             //                };
                             //var SaldoAkhir = from query in CobaPPAkhir
                             //                 group query by new { query.ProductCode, query.ProductName, query.RO, query.PlanPo, query.POId, query.UnitCode, query.UnitSenderCode, query.UnitRequestName } into data
                             //                 select new GarmentStockReportViewModel
                             //                 {
                             //                     ProductCode = data.Key.ProductCode,
                             //                     RO = data.Key.RO,
                             //                     PlanPo = data.FirstOrDefault().PlanPo,
                             //                     NoArticle = data.FirstOrDefault().NoArticle,
                             //                     ProductName = data.FirstOrDefault().ProductName,
                             //                     ProductRemark = data.FirstOrDefault().ProductRemark,
                             //                     Buyer = data.FirstOrDefault().Buyer,
                             //                     BeginningBalanceQty =0,
                             //                     BeginningBalanceUom = data.FirstOrDefault().Uom,
                             //                     ReceiptCorrectionQty = 0,
                             //                     ReceiptQty = data.Sum(x => x.QtyReceipt),
                             //                     ReceiptUom = data.FirstOrDefault().Uom,
                             //                     ExpendQty = data.Sum(x => x.QtyExpend),
                             //                     ExpandUom = data.FirstOrDefault().Uom,
                             //                     EndingBalanceQty = Convert.ToDecimal(Convert.ToDouble(data.Sum(x => x.QtyReceipt)) - data.Sum(x => x.QtyExpend)),
                             //                     EndingUom = data.FirstOrDefault().Uom,
                             //                     POId = data.FirstOrDefault().POId,
                             //                     PaymentMethod = data.FirstOrDefault().PaymentMethod
                             //                 };
                             //List<GarmentStockReportViewModel> Data1 = SaldoAwal.Concat(SaldoAkhir).ToList();
                             //var Data = (from query in Data1
                             //            group query by new { query.POId, query.ProductCode, query.RO } into groupdata
                             //            select new GarmentStockReportViewModel
                             //            {
                             //                ProductCode = groupdata.FirstOrDefault().ProductCode == null ? "-" : groupdata.FirstOrDefault().ProductCode,
                             //                RO = groupdata.FirstOrDefault().RO == null ? "-" : groupdata.FirstOrDefault().RO,
                             //                PlanPo = groupdata.FirstOrDefault().PlanPo == null ? "-" : groupdata.FirstOrDefault().PlanPo,
                             //                NoArticle = groupdata.FirstOrDefault().NoArticle == null ? "-" : groupdata.FirstOrDefault().NoArticle,
                             //                ProductName = groupdata.FirstOrDefault().ProductName == null ? "-" : groupdata.FirstOrDefault().ProductName,
                             //                ProductRemark = groupdata.FirstOrDefault().ProductRemark,
                             //                Buyer = groupdata.FirstOrDefault().Buyer == null ? "-" : groupdata.FirstOrDefault().Buyer,
                             //                BeginningBalanceQty = groupdata.Sum(x => x.BeginningBalanceQty),
                             //                BeginningBalanceUom = groupdata.FirstOrDefault().BeginningBalanceUom,
                             //                ReceiptCorrectionQty = 0,
                             //                ReceiptQty = groupdata.Sum(x => x.ReceiptQty),
                             //                ReceiptUom = groupdata.FirstOrDefault().ReceiptUom,
                             //                ExpendQty = groupdata.Sum(x => x.ExpendQty),
                             //                ExpandUom = groupdata.FirstOrDefault().ExpandUom,
                             //                EndingBalanceQty = Convert.ToDecimal((groupdata.Sum(x => x.BeginningBalanceQty) + groupdata.Sum(x => x.ReceiptQty) + 0) - (Convert.ToDecimal(groupdata.Sum(x => x.ExpendQty)) )),
                             //                EndingUom = groupdata.FirstOrDefault().EndingUom,
                             //                PaymentMethod = groupdata.FirstOrDefault().PaymentMethod
                             //            });

                             //return Data.AsQueryable();
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
            decimal BeginningQtyTotal = 0;
            decimal ReceiptQtyTotal = 0;
            double ExpendQtyTotal = 0;
            decimal EndingQtyTotal = 0;
            foreach (var item in Query)
            {
                BeginningQtyTotal += item.BeginningBalanceQty;
                ReceiptQtyTotal += item.ReceiptQty;
                ExpendQtyTotal += item.ExpendQty;
                EndingQtyTotal += item.EndingBalanceQty;

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

            var a = Query.Count();
            sheet.Cells[$"A{3 + a}"].Value = "T O T A L  . . . . . . . . . . . . . . .";
            sheet.Cells[$"A{3 + a}:H{3 + a}"].Merge = true;
            sheet.Cells[$"A{3 + a}:H{3 + a}"].Style.Font.Bold = true;
            sheet.Cells[$"A{3 + a}:H{3 + a}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[$"A{3 + a}:H{3 + a}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells[$"I{3 + a}"].Value = BeginningQtyTotal;
            sheet.Cells[$"K{3 + a}"].Value = ReceiptQtyTotal;
            sheet.Cells[$"N{3 + a}"].Value = ExpendQtyTotal;
            sheet.Cells[$"P{3 + a}"].Value = EndingQtyTotal;

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
