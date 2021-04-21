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
using System.Globalization;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Newtonsoft.Json;

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

            var categories = GetProductCodes(1, int.MaxValue, "{}", "{}");

            var categories1 = ctg == "BB" ? categories.Where(x => x.CodeRequirement == "BB").Select(x => x.Name).ToArray() : ctg == "BP" ? categories.Where(x => x.CodeRequirement == "BP").Select(x => x.Name).ToArray() : ctg == "BE" ? categories.Where(x => x.CodeRequirement == "BE").Select(x => x.Name).ToArray() : categories.Select(x=>x.Name).ToArray();

            var lastdate = dbContext.BalanceStocks.OrderByDescending(x => x.CreateDate).Select(x => x.CreateDate).FirstOrDefault();

            var BalaceStock = (from a in dbContext.BalanceStocks
                               join b in dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters() on (long)a.EPOItemId equals b.Id
                               join c in dbContext.GarmentExternalPurchaseOrders.IgnoreQueryFilters() on b.GarmentEPOId equals c.Id
                               join d in dbContext.GarmentInternalPurchaseOrders on b.POId equals d.Id
                               where a.CreateDate == lastdate

                               group new { a, b, c, d } by new { b.ProductCode, b.ProductName, b.PO_SerialNumber } into data

                               select new GarmentStockReportViewModel
                               {
                                  BeginningBalanceQty = (decimal)data.Sum(x=>x.a.CloseStock),
                                  BeginningBalanceUom = data.FirstOrDefault().b.SmallUomUnit,
                                  Buyer = data.FirstOrDefault().d.BuyerCode,
                                  EndingBalanceQty = 0,
                                  EndingUom = data.FirstOrDefault().b.SmallUomUnit,
                                  ExpandUom = data.FirstOrDefault().b.SmallUomUnit,
                                  ExpendQty = 0,
                                  NoArticle = data.FirstOrDefault().a.ArticleNo,
                                  PaymentMethod = data.FirstOrDefault().c.PaymentMethod,
                                  PlanPo = data.Key.PO_SerialNumber,
                                  POId = data.FirstOrDefault().b.POId,
                                  ProductCode = data.Key.ProductCode,
                                  ProductName = data.Key.ProductName,
                                  ProductRemark = data.FirstOrDefault().b.Remark,
                                  ReceiptCorrectionQty = 0,
                                  ReceiptQty = 0,
                                  ReceiptUom = data.FirstOrDefault().b.SmallUomUnit,
                                  RO = data.FirstOrDefault().b.RONo,
                                  UNitCode = "",
                                  UnitSenderCode = "",
                                  UnitRequestCode = ""
                               }).ToList();

            List<GarmentStockReportViewModel> penerimaan = new List<GarmentStockReportViewModel>();
            List<GarmentStockReportViewModel> pengeluaran = new List<GarmentStockReportViewModel>();
            List<GarmentStockReportViewModel> penerimaanSA = new List<GarmentStockReportViewModel>();
            List<GarmentStockReportViewModel> pengeluaranSA = new List<GarmentStockReportViewModel>();

            #region SaldoAwal
            var IdSATerima = (from a in dbContext.GarmentUnitReceiptNotes
                              join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                              where
                              categories1.Contains(b.ProductName) 
                              && a.IsDeleted == false && b.IsDeleted == false
                              &&
                              a.CreatedUtc.AddHours(offset).Date > lastdate
                              && a.CreatedUtc.AddHours(offset).Date < DateFrom.Date
                              && a.UnitCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)

                              select new
                              {
                                  UrnId = a.Id,
                                  UrnItemId = b.Id,
                                  //UENNo = dd == null ? "-" : dd.UENNo,
                                  a.UnitCode
                              }).ToList().Distinct();
            var sapenerimaanunitreceiptnoteids = IdSATerima.Select(x => x.UrnId).ToList();
            var sapenerimaanunitreceiptnotes = dbContext.GarmentUnitReceiptNotes.Where(x => sapenerimaanunitreceiptnoteids.Contains(x.Id)).Select(s => new { s.ReceiptDate, s.URNType, s.UnitCode, s.UENNo, s.Id }).ToList();
            var sapenerimaanunitreceiptnoteItemIds = IdSATerima.Select(x => x.UrnItemId).ToList();
            var sapenerimaanuntreceiptnoteItems = dbContext.GarmentUnitReceiptNoteItems.Where(x => sapenerimaanunitreceiptnoteItemIds.Contains(x.Id)).Select(s => new { s.ProductCode, s.ProductName, s.RONo, s.SmallUomUnit, s.POSerialNumber, s.ReceiptQuantity, s.DOCurrencyRate, s.PricePerDealUnit, s.Id, s.SmallQuantity, s.Conversion, s.ProductRemark, s.EPOItemId }).ToList();
            //var sapenerimaandeliveryorderdetailIds = IdSATerima.Select(x => x.DoDetailId).ToList();
            //var sapenerimaandeliveryorderdetails = dbContext.GarmentDeliveryOrderDetails.Where(x => sapenerimaandeliveryorderdetailIds.Contains(x.Id)).Select(s => new { s.CodeRequirment, s.Id, s.DOQuantity }).ToList();
            var sapenerimaanExternalPurchaseOrderItemIds = sapenerimaanuntreceiptnoteItems.Select(x => x.EPOItemId).ToList();
            var sapenerimaanExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters().Where(x => sapenerimaanExternalPurchaseOrderItemIds.Contains(x.Id)).Select(s => new { s.GarmentEPOId, s.Id, s.PO_SerialNumber }).ToList();
            var sapenerimaanExternalPurchaseOrderIds = sapenerimaanExternalPurchaseOrderItems.Select(x => x.GarmentEPOId ).ToList();
            var sapenerimaanExternalPurchaseOrders = dbContext.GarmentExternalPurchaseOrders.IgnoreQueryFilters().Where(x => sapenerimaanExternalPurchaseOrderIds.Contains(x.Id)).Select(s => new { s.Id, s.PaymentMethod }).ToList();
            var sapenerimaanpurchaserequestros = sapenerimaanuntreceiptnoteItems.Select(x => x.RONo).ToList();
            var sapenerimaanpurchaserequests = dbContext.GarmentPurchaseRequests.Where(x=> sapenerimaanpurchaserequestros.Contains(x.RONo)).Select(x => new { x.BuyerCode, x.Article, x.RONo }).ToList();
            //var sapenerimaanintrenalpurchaseorders = dbContext.GarmentInternalPurchaseOrders.Where(x => sapenerimaanintrenalpurchaseorderIds.Contains(x.Id)).Select(s => new { s.BuyerCode, s.Article, s.Id }).ToList();
            foreach (var item in IdSATerima)
            {
                var sapenerimaanunitreceiptnote = sapenerimaanunitreceiptnotes.FirstOrDefault(x => x.Id == item.UrnId);
                var sapenerimaanuntreceiptnoteItem = sapenerimaanuntreceiptnoteItems.FirstOrDefault(x => x.Id == item.UrnItemId);
                //var sapenerimaandeliveryorderdetail = sapenerimaandeliveryorderdetails.FirstOrDefault(x => x.Id == item.DoDetailId);
                var sapenerimaanExternalPurchaseOrderitem = sapenerimaanExternalPurchaseOrderItems.FirstOrDefault(x => x.Id == sapenerimaanuntreceiptnoteItem.EPOItemId);
                var sapenerimaanExternalPurchaseOrder = sapenerimaanExternalPurchaseOrders.FirstOrDefault(x => x.Id == sapenerimaanExternalPurchaseOrderitem.GarmentEPOId);
                var sapenerimaanpurchaserequest = sapenerimaanpurchaserequests.FirstOrDefault(x => x.RONo == sapenerimaanuntreceiptnoteItem.RONo);

                penerimaanSA.Add(new GarmentStockReportViewModel
                {
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = sapenerimaanuntreceiptnoteItem.SmallUomUnit,
                    Buyer = sapenerimaanpurchaserequest == null ? "" : sapenerimaanpurchaserequest.BuyerCode,
                    EndingBalanceQty = 0,
                    EndingUom = sapenerimaanuntreceiptnoteItem.SmallUomUnit,
                    ExpandUom = sapenerimaanuntreceiptnoteItem.SmallUomUnit,
                    ExpendQty = 0,
                    NoArticle = sapenerimaanpurchaserequest == null ? "" : sapenerimaanpurchaserequest.Article,
                    PaymentMethod = sapenerimaanExternalPurchaseOrder == null ? "" : sapenerimaanExternalPurchaseOrder.PaymentMethod,
                    PlanPo = sapenerimaanuntreceiptnoteItem.POSerialNumber,
                    //POId = sapenerimaanintrenalpurchaseorder.Id,
                    ProductCode = sapenerimaanuntreceiptnoteItem.ProductCode,
                    ProductName = sapenerimaanuntreceiptnoteItem.ProductName,
                    ProductRemark = sapenerimaanuntreceiptnoteItem.ProductRemark,
                    ReceiptCorrectionQty = 0,
                    ReceiptQty = (decimal)sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion,
                    ReceiptUom = sapenerimaanuntreceiptnoteItem.SmallUomUnit,
                    RO = sapenerimaanuntreceiptnoteItem.RONo,
                    UNitCode = "",
                    UnitSenderCode = "",
                    UnitRequestCode = ""
                });
            }

            var IdSAPengeluaran = (from a in dbContext.GarmentUnitExpenditureNotes
                                   join b in dbContext.GarmentUnitExpenditureNoteItems on a.Id equals b.UENId
                                   where categories1.Contains(b.ProductName)
                                       && a.IsDeleted == false && b.IsDeleted == false
                                       && a.CreatedUtc.AddHours(offset).Date > lastdate.Value.Date
                                        && a.CreatedUtc.AddHours(offset).Date < DateFrom.Date
                                       && a.UnitSenderCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitSenderCode : unitcode)

                                   select new
                                   {
                                       UENId = a.Id,
                                       UENItemsId = b.Id
                                   }).ToList().Distinct();

            var sapengeluaranUnitExpenditureNoteItemIds = IdSAPengeluaran.Select(x => x.UENItemsId).ToList();
            var sapengeluaranUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => sapengeluaranUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.UnitDOItemId, s.Quantity, s.PricePerDealUnit, s.Id, s.ProductCode, s.ProductName, s.RONo, s.POSerialNumber, s.UomUnit, s.DOCurrencyRate, s.ProductRemark, s.URNItemId, s.EPOItemId }).ToList();
            var sapengeluaranUnitExpenditureNoteIds = IdSAPengeluaran.Select(x => x.UENId).ToList();
            var sapengeluaranUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => sapengeluaranUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id, s.UENNo, s.ExpenditureType }).ToList();
            var sapengeluaranunitdeliveryorderItemIds = sapengeluaranUnitExpenditureNoteItems.Select(x => x.UnitDOItemId);
            var sapengeluaranunitdeliveryorderitems = dbContext.GarmentUnitDeliveryOrderItems.Where(x => sapengeluaranunitdeliveryorderItemIds.Contains(x.Id)).Select(x => new { x.URNItemId, x.Id }).ToList();
            var sapengeluaranunitreceiptnoteitemslIds = sapengeluaranunitdeliveryorderitems.Select(x => x.URNItemId).ToList();
            var sapengeluaranunitreceiptnoteitems = dbContext.GarmentUnitReceiptNoteItems.IgnoreQueryFilters().Where(x => sapengeluaranunitreceiptnoteitemslIds.Contains(x.Id)).Select(x => new { x.EPOItemId, x.Id }).ToList();
            //var sapengeluarandeliveryorderdetails = dbContext.GarmentDeliveryOrderDetails.Where(x => sapengeluarandeliveryorderdetailIds.Contains(x.Id)).Select(s => new { s.CodeRequirment, s.Id, s.DOQuantity }).ToList();
            
            var sapengeluaranpurchaserequestros = sapengeluaranUnitExpenditureNoteItems.Select(x => x.RONo ).ToList();
            var sapengeluaranpurchaserequests = dbContext.GarmentPurchaseRequests.Where(x => sapengeluaranpurchaserequestros.Contains(x.RONo)).Select(x => new { x.BuyerCode, x.RONo, x.Article }).ToList();
            var sapengeluaranexternalpurchaseorderitemros = sapengeluaranUnitExpenditureNoteItems.Select(x => x.RONo).ToList();
            var sapengeluaranexternalpurchaseorderitems = dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters().Where(x => sapengeluaranexternalpurchaseorderitemros.Contains(x.RONo)).Select(s => new { s.GarmentEPOId, s.Id, s.RONo }).ToList();
            var sapengeluaranexternalpurchaseorderIds = sapengeluaranexternalpurchaseorderitems.Select(x => x.GarmentEPOId).ToList();
            var sapengeluaranexternalpurchaseorders = dbContext.GarmentExternalPurchaseOrders.IgnoreQueryFilters().Where(x => sapengeluaranexternalpurchaseorderIds.Contains(x.Id)).Select(x => new { x.PaymentMethod, x.Id }).ToList();

            foreach (var item in IdSAPengeluaran)
            {
                var sapengeluaranUnitExpenditureNoteItem = sapengeluaranUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var sapengeluaranUnitExpenditureNote = sapengeluaranUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                var sapengeluaranunitdeliveryorderitem = sapengeluaranunitdeliveryorderitems.FirstOrDefault(x => x.Id == sapengeluaranUnitExpenditureNoteItem.UnitDOItemId);
                var sapengeluaranunitreceiptnoteitem = sapengeluaranunitreceiptnoteitems.FirstOrDefault(x => x.Id == sapengeluaranunitdeliveryorderitem.URNItemId);
                var sapengeluaranexternalpurchaseorderitem = sapengeluaranexternalpurchaseorderitems.FirstOrDefault(x => x.RONo == sapengeluaranUnitExpenditureNoteItem.RONo);
                var sapengeluaranexternalpurchaseorder = sapengeluaranexternalpurchaseorders.FirstOrDefault(x => x.Id == sapengeluaranexternalpurchaseorderitem.GarmentEPOId);
                var sapengeluaranpurchaserequest = sapengeluaranpurchaserequests.FirstOrDefault(x => x.RONo == sapengeluaranUnitExpenditureNoteItem.RONo);

                pengeluaranSA.Add(new GarmentStockReportViewModel
                {
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = sapengeluaranUnitExpenditureNoteItem.UomUnit,
                    Buyer = sapengeluaranpurchaserequest == null ? "" : sapengeluaranpurchaserequest.BuyerCode,
                    EndingBalanceQty = 0,
                    EndingUom = sapengeluaranUnitExpenditureNoteItem.UomUnit,
                    ExpandUom = sapengeluaranUnitExpenditureNoteItem.UomUnit,
                    ExpendQty = sapengeluaranUnitExpenditureNoteItem.Quantity,
                    NoArticle = sapengeluaranpurchaserequest == null ? "" : sapengeluaranpurchaserequest.Article,
                    PaymentMethod = sapengeluaranexternalpurchaseorder == null ? "" : sapengeluaranexternalpurchaseorder.PaymentMethod,
                    PlanPo = sapengeluaranUnitExpenditureNoteItem.POSerialNumber,
                    ProductCode = sapengeluaranUnitExpenditureNoteItem.ProductCode,
                    ProductName = sapengeluaranUnitExpenditureNoteItem.ProductName,
                    ProductRemark = sapengeluaranUnitExpenditureNoteItem.ProductRemark,
                    ReceiptCorrectionQty = 0,
                    ReceiptQty = 0,
                    ReceiptUom = sapengeluaranUnitExpenditureNoteItem.UomUnit,
                    RO = sapengeluaranUnitExpenditureNoteItem.RONo,
                    UNitCode = "",
                    UnitSenderCode = "",
                    UnitRequestCode = ""
                });
            }
            var SAwal = BalaceStock.Concat(penerimaanSA).Concat(pengeluaranSA).ToList();

            var SaldoAwal = (from a in SAwal
                            group a by new { a.ProductCode, a.PlanPo } into data
                            select new GarmentStockReportViewModel
                            {
                                BeginningBalanceQty = Math.Round(data.Sum(x => x.BeginningBalanceQty) + data.Sum(x => x.ReceiptQty) + data.Sum(x => x.ReceiptCorrectionQty) - (decimal)data.Sum(x => x.ExpendQty),2),
                                BeginningBalanceUom = data.FirstOrDefault().BeginningBalanceUom,
                                Buyer = data.FirstOrDefault().Buyer,
                                EndingBalanceQty = 0,
                                EndingUom = data.FirstOrDefault().EndingUom,
                                ExpandUom = data.FirstOrDefault().ExpandUom,
                                ExpendQty = 0,
                                NoArticle = data.FirstOrDefault().NoArticle,
                                PaymentMethod = data.FirstOrDefault().PaymentMethod,
                                PlanPo = data.FirstOrDefault().PlanPo,
                                POId = data.FirstOrDefault().POId,
                                ProductCode = data.FirstOrDefault().ProductCode,
                                ProductName = data.FirstOrDefault().ProductName,
                                ProductRemark = data.FirstOrDefault().ProductRemark,
                                ReceiptCorrectionQty = 0,
                                ReceiptQty = 0,
                                ReceiptUom = data.FirstOrDefault().ReceiptUom,
                                RO = data.FirstOrDefault().RO,
                                UNitCode = data.FirstOrDefault().UNitCode,
                                UnitSenderCode = data.FirstOrDefault().UnitSenderCode,
                                UnitRequestCode = data.FirstOrDefault().UnitRequestCode
                            }).ToList();
            #endregion
            #region Now
            var IdTerima = (from a in dbContext.GarmentUnitReceiptNotes
                            join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                            where
                            categories1.Contains(b.ProductName)
                            && a.IsDeleted == false && b.IsDeleted == false
                            && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                            && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                            && a.UnitCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitCode : unitcode)
                            select new
                            {
                                UrnId = a.Id,
                                UrnItemId = b.Id,
                                //UENNo = dd == null ? "-" : dd.UENNo,
                                a.UnitCode
                            }).ToList().Distinct();

            var penerimaanunitreceiptnoteids = IdTerima.Select(x => x.UrnId).ToList();
            var penerimaanunitreceiptnotes = dbContext.GarmentUnitReceiptNotes.Where(x => penerimaanunitreceiptnoteids.Contains(x.Id)).Select(s => new { s.ReceiptDate, s.URNType, s.UnitCode, s.UENNo, s.Id }).ToList();
            var penerimaanunitreceiptnoteItemIds = IdTerima.Select(x => x.UrnItemId).ToList();
            var penerimaanuntreceiptnoteItems = dbContext.GarmentUnitReceiptNoteItems.Where(x => penerimaanunitreceiptnoteItemIds.Contains(x.Id)).Select(s => new { s.EPOItemId, s.ProductCode, s.ProductName, s.RONo, s.SmallUomUnit, s.POSerialNumber, s.ReceiptQuantity, s.DOCurrencyRate, s.PricePerDealUnit, s.Id, s.SmallQuantity, s.Conversion, s.ProductRemark }).ToList();
            var penerimaanExternalPurchaseOrderItemIds = penerimaanuntreceiptnoteItems.Select(x => x.EPOItemId).ToList();
            var penerimaanExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters().Where(x => penerimaanExternalPurchaseOrderItemIds.Contains(x.Id)).Select(s => new { s.GarmentEPOId, s.Id, s.PO_SerialNumber }).ToList();
            var penerimaanExternalPurchaseOrderIds = penerimaanExternalPurchaseOrderItems.Select(x => x.GarmentEPOId).ToList();
            var penerimaanExternalPurchaseOrders = dbContext.GarmentExternalPurchaseOrders.IgnoreQueryFilters().Where(x => penerimaanExternalPurchaseOrderIds.Contains(x.Id)).Select(s => new { s.Id, s.PaymentMethod }).ToList();
            var penerimaanpurchaserequestros = penerimaanuntreceiptnoteItems.Select(x => x.RONo).ToList();
            var penerimaanpurchaserequests = dbContext.GarmentPurchaseRequests.Where(x => penerimaanpurchaserequestros.Contains(x.RONo)).Select(x => new { x.BuyerCode, x.Article, x.RONo }).ToList();

            foreach (var item in IdTerima) {
                var penerimaanunitreceiptnote = penerimaanunitreceiptnotes.FirstOrDefault(x => x.Id == item.UrnId);
                var penerimaanuntreceiptnoteItem = penerimaanuntreceiptnoteItems.FirstOrDefault(x => x.Id == item.UrnItemId);
                var penerimaanExternalPurchaseOrderitem = penerimaanExternalPurchaseOrderItems.FirstOrDefault(x => x.Id == penerimaanuntreceiptnoteItem.EPOItemId);
                var penerimaanExternalPurchaseOrder = penerimaanExternalPurchaseOrders.FirstOrDefault(x => x.Id == penerimaanExternalPurchaseOrderitem.GarmentEPOId);
                var penerimaanpurchaserequest = penerimaanpurchaserequests.FirstOrDefault(x => x.RONo == penerimaanuntreceiptnoteItem.RONo);

                penerimaan.Add(new GarmentStockReportViewModel
                {
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = penerimaanuntreceiptnoteItem.SmallUomUnit,
                    Buyer = penerimaanpurchaserequest == null ? "" : penerimaanpurchaserequest.BuyerCode,
                    EndingBalanceQty = 0,
                    EndingUom = penerimaanuntreceiptnoteItem.SmallUomUnit,
                    ExpandUom = penerimaanuntreceiptnoteItem.SmallUomUnit,
                    ExpendQty = 0,
                    NoArticle = penerimaanpurchaserequest == null ? "" : penerimaanpurchaserequest.Article,
                    PaymentMethod = penerimaanExternalPurchaseOrder == null ? "" : penerimaanExternalPurchaseOrder.PaymentMethod,
                    PlanPo = penerimaanuntreceiptnoteItem.POSerialNumber,
                    ProductCode = penerimaanuntreceiptnoteItem.ProductCode,
                    ProductName = penerimaanuntreceiptnoteItem.ProductName,
                    ProductRemark = penerimaanuntreceiptnoteItem.ProductRemark,
                    ReceiptCorrectionQty = 0,
                    ReceiptQty = (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion,
                    ReceiptUom = penerimaanuntreceiptnoteItem.SmallUomUnit,
                    RO = penerimaanuntreceiptnoteItem.RONo,
                    UNitCode = "",
                    UnitSenderCode = "",
                    UnitRequestCode = ""
                });
            }
            var IdPengeluaran = (from a in dbContext.GarmentUnitExpenditureNotes
                                 join b in dbContext.GarmentUnitExpenditureNoteItems on a.Id equals b.UENId
                                 where categories1.Contains(b.ProductName)
                                   && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                                     && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                                     && a.UnitSenderCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitSenderCode : unitcode)

                                 select new
                                 {
                                     UENId = a.Id,
                                     UENItemsId = b.Id
                                 }).ToList().Distinct();
            var pengeluaranUnitExpenditureNoteItemIds = IdPengeluaran.Select(x => x.UENItemsId).ToList();
            var pengeluaranUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => pengeluaranUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.UnitDOItemId, s.Quantity, s.PricePerDealUnit, s.Id, s.ProductCode, s.ProductName, s.RONo, s.POSerialNumber, s.UomUnit, s.DOCurrencyRate, s.URNItemId, s.ProductRemark }).ToList();
            var pengeluaranUnitExpenditureNoteIds = IdPengeluaran.Select(x => x.UENId).ToList();
            var pengeluaranUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => pengeluaranUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id, s.UENNo }).ToList();
            var pengeluaranunitdeliveryorderItemIds = pengeluaranUnitExpenditureNoteItems.Select(x => x.UnitDOItemId);
            var pengeluaranunitdeliveryorderitems = dbContext.GarmentUnitDeliveryOrderItems.Where(x => pengeluaranunitdeliveryorderItemIds.Contains(x.Id)).Select(x => new { x.URNItemId, x.Id }).ToList();
            var pengeluaranunitreceiptnoteitemslIds = pengeluaranunitdeliveryorderitems.Select(x => x.URNItemId).ToList();
            var pengeluaranunitreceiptnoteitems = dbContext.GarmentUnitReceiptNoteItems.Where(x => pengeluaranunitreceiptnoteitemslIds.Contains(x.Id)).Select(x => new { x.EPOItemId, x.Id }).ToList();
            //var sapengeluarandeliveryorderdetails = dbContext.GarmentDeliveryOrderDetails.Where(x => sapengeluarandeliveryorderdetailIds.Contains(x.Id)).Select(s => new { s.CodeRequirment, s.Id, s.DOQuantity }).ToList();
            var pengeluaranexternalpurchaseorderitemIds = pengeluaranunitreceiptnoteitems.Select(x => x.EPOItemId).ToList();
            var pengeluaranexternalpurchaseorderitems = dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters().Where(x => pengeluaranexternalpurchaseorderitemIds.Contains(x.Id)).Select(s => new { s.GarmentEPOId, s.Id }).ToList();
            var pengeluaranexternalpurchaseorderIds = pengeluaranexternalpurchaseorderitems.Select(x => x.GarmentEPOId).ToList();
            var pengeluaranexternalpurchaseorders = dbContext.GarmentExternalPurchaseOrders.IgnoreQueryFilters().Where(x => pengeluaranexternalpurchaseorderIds.Contains(x.Id)).Select(x => new { x.PaymentMethod, x.Id }).ToList();
            var pengeluaranpurchaserequestros = pengeluaranUnitExpenditureNoteItems.Select(x => x.RONo).ToList();
            var pengeluaranpurchaserequests = dbContext.GarmentPurchaseRequests.Where(x => pengeluaranpurchaserequestros.Contains(x.RONo)).Select(x => new { x.BuyerCode, x.RONo, x.Article }).ToList();

            foreach (var item in IdPengeluaran) {
                var pengeluaranUnitExpenditureNoteItem = pengeluaranUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var pengeluaranUnitExpenditureNote = pengeluaranUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                var pengeluaranunitdeliveryorderitem = pengeluaranunitdeliveryorderitems.FirstOrDefault(x => x.Id == pengeluaranUnitExpenditureNoteItem.UnitDOItemId);
                var pengeluaranunitreceiptnoteitem = pengeluaranunitreceiptnoteitems.FirstOrDefault(x => x.Id == pengeluaranunitdeliveryorderitem.URNItemId);
                var pengeluaranexternalpurchaseorderitem = pengeluaranexternalpurchaseorderitems.FirstOrDefault(x => x.Id == pengeluaranunitreceiptnoteitem.EPOItemId);
                var pengeluaranexternalpurchaseorder = pengeluaranexternalpurchaseorders.FirstOrDefault(x => x.Id == pengeluaranexternalpurchaseorderitem.GarmentEPOId);
                var pengeluaranpurchaserequest = pengeluaranpurchaserequests.FirstOrDefault(x => x.RONo == pengeluaranUnitExpenditureNoteItem.RONo);

                pengeluaran.Add(new GarmentStockReportViewModel
                {
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = pengeluaranUnitExpenditureNoteItem.UomUnit,
                    Buyer = pengeluaranpurchaserequest == null ? "" : pengeluaranpurchaserequest.BuyerCode,
                    EndingBalanceQty = 0,
                    EndingUom = pengeluaranUnitExpenditureNoteItem.UomUnit,
                    ExpandUom = pengeluaranUnitExpenditureNoteItem.UomUnit,
                    ExpendQty = pengeluaranUnitExpenditureNoteItem.Quantity,
                    NoArticle = pengeluaranpurchaserequest == null ? "" : pengeluaranpurchaserequest.Article,
                    PaymentMethod = pengeluaranexternalpurchaseorder == null ? "" : pengeluaranexternalpurchaseorder.PaymentMethod,
                    PlanPo = pengeluaranUnitExpenditureNoteItem.POSerialNumber,
                    ProductCode = pengeluaranUnitExpenditureNoteItem.ProductCode,
                    ProductName = pengeluaranUnitExpenditureNoteItem.ProductName,
                    ProductRemark = pengeluaranUnitExpenditureNoteItem.ProductRemark,
                    ReceiptCorrectionQty = 0,
                    ReceiptQty = 0,
                    ReceiptUom = pengeluaranUnitExpenditureNoteItem.UomUnit,
                    RO = pengeluaranUnitExpenditureNoteItem.RONo,
                    UNitCode = "",
                    UnitSenderCode = "",
                    UnitRequestCode = ""
                });

            }

            var SAkhir = penerimaan.Concat(pengeluaran).ToList();
            var SaldoAkhir = (from a in SAkhir
                              group a by new { a.PlanPo, a.ProductCode } into data
                              select new GarmentStockReportViewModel
                              {
                                  BeginningBalanceQty = Math.Round(data.Sum(x => x.BeginningBalanceQty), 2),
                                  BeginningBalanceUom = data.FirstOrDefault().BeginningBalanceUom,
                                  Buyer = data.FirstOrDefault().Buyer,
                                  EndingBalanceQty = Math.Round(data.Sum(x => x.EndingBalanceQty), 2),
                                  EndingUom = data.FirstOrDefault().EndingUom,
                                  ExpandUom = data.FirstOrDefault().ExpandUom,
                                  ExpendQty = Math.Round(data.Sum(x => x.ExpendQty), 2),
                                  NoArticle = data.FirstOrDefault().NoArticle,
                                  PaymentMethod = data.FirstOrDefault().PaymentMethod,
                                  PlanPo = data.FirstOrDefault().PlanPo,
                                  POId = data.FirstOrDefault().POId,
                                  ProductCode = data.FirstOrDefault().ProductCode,
                                  ProductName = data.FirstOrDefault().ProductName,
                                  ProductRemark = data.FirstOrDefault().ProductRemark,
                                  ReceiptCorrectionQty = Math.Round(data.Sum(x => x.ReceiptCorrectionQty), 2),
                                  ReceiptQty = Math.Round(data.Sum(x => x.ReceiptQty), 2),
                                  ReceiptUom = data.FirstOrDefault().ReceiptUom,
                                  RO = data.FirstOrDefault().RO,
                                  UNitCode = data.FirstOrDefault().UNitCode,
                                  UnitSenderCode = data.FirstOrDefault().UnitSenderCode,
                                  UnitRequestCode = data.FirstOrDefault().UnitRequestCode
                              }).ToList();
            #endregion

            var SaldoAkhirs2 = SaldoAwal.Concat(SaldoAkhir).ToList();
            var stock = (from a in SaldoAkhirs2
                         group a by new { a.PlanPo, a.ProductCode } into data
                         select new GarmentStockReportViewModel
                         {
                             BeginningBalanceQty = Math.Round(data.Sum(x => x.BeginningBalanceQty), 2),
                             BeginningBalanceUom = data.FirstOrDefault().BeginningBalanceUom,
                             Buyer = data.FirstOrDefault().Buyer,
                             EndingBalanceQty = Math.Round(data.Sum(x => x.BeginningBalanceQty) + data.Sum(x => x.ReceiptQty) + data.Sum(x => x.ReceiptCorrectionQty) - (decimal)data.Sum(x => x.ExpendQty), 2),
                             EndingUom = data.FirstOrDefault().EndingUom,
                             ExpandUom = data.FirstOrDefault().ExpandUom,
                             ExpendQty = Math.Round(data.Sum(x => x.ExpendQty), 2),
                             NoArticle = data.FirstOrDefault().NoArticle,
                             PaymentMethod = data.FirstOrDefault().PaymentMethod,
                             PlanPo = data.FirstOrDefault().PlanPo,
                             POId = data.FirstOrDefault().POId,
                             ProductCode = data.FirstOrDefault().ProductCode,
                             ProductName = data.FirstOrDefault().ProductName,
                             ProductRemark = data.FirstOrDefault().ProductRemark,
                             ReceiptCorrectionQty = Math.Round(data.Sum(x => x.ReceiptCorrectionQty), 2),
                             ReceiptQty = Math.Round(data.Sum(x => x.ReceiptQty), 2),
                             ReceiptUom = data.FirstOrDefault().ReceiptUom,
                             RO = data.FirstOrDefault().RO,
                             UNitCode = data.FirstOrDefault().UNitCode,
                             UnitSenderCode = data.FirstOrDefault().UnitSenderCode,
                             UnitRequestCode = data.FirstOrDefault().UnitRequestCode
                         }).ToList();

            foreach(var i in stock)
            {
                i.BeginningBalanceQty = i.BeginningBalanceQty > 0 ? i.BeginningBalanceQty : 0;
                i.EndingBalanceQty = i.EndingBalanceQty > 0 ? i.EndingBalanceQty : 0;
            }

            return stock.Where(x => (x.ProductCode != "EMB001") && (x.ProductCode != "WSH001") && (x.ProductCode != "PRC001") && (x.ProductCode != "APL001") && (x.ProductCode != "QLT001") && (x.ProductCode != "SMT001") && (x.ProductCode != "GMT001") && (x.ProductCode != "PRN001") && (x.ProductCode != "SMP001")).ToList(); ;
            //return SaldoAwal;

        }

        public Tuple<List<GarmentStockReportViewModel>, int> GetStockReport(int offset, string unitcode, string tipebarang, int page, int size, string Order, DateTime? dateFrom, DateTime? dateTo)
        {
            //var Query = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset);
            //Query = Query.OrderByDescending(x => x.SupplierName).ThenBy(x => x.Dono);
            List<GarmentStockReportViewModel> Data = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset).ToList();
            Data = Data.Where(x => (x.BeginningBalanceQty > 0) || (x.EndingBalanceQty > 0) || (x.ReceiptCorrectionQty > 0) || (x.ReceiptQty > 0) || (x.ExpendQty > 0)).ToList();
            Data = Data.OrderBy(x => x.ProductCode).ThenBy(x => x.PlanPo).ToList();
            //int TotalData = Data.Count();
            return Tuple.Create(Data, Data.Count());
        }

        public MemoryStream GenerateExcelStockReport(string ctg, string categoryname, string unitname, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            var data = GetStockQuery(ctg, unitcode, datefrom, dateto, offset);
            data = data.Where(x => (x.BeginningBalanceQty != 0) || (x.EndingBalanceQty != 0) || (x.ReceiptCorrectionQty > 0) || (x.ReceiptQty > 0) || (x.ExpendQty > 0)).ToList();
            var Query = data.OrderBy(x => x.ProductCode).ThenBy(x => x.PlanPo).ToList();
            DataTable result = new DataTable();
            var headers = new string[] { "No","Kode Barang", "No RO", "Plan PO", "Artikel", "Nama Barang","Keterangan Barang", "Buyer","Saldo Awal","Saldo Awal2", "Penerimaan", "Penerimaan1", "Penerimaan2","Pengeluaran","Pengeluaran1", "Saldo Akhir", "Saldo Akhir1", "Asal" }; 
            var subheaders = new string[] { "Jumlah", "Sat", "Jumlah", "Koreksi", "Sat", "Jumlah", "Sat", "Jumlah", "Sat" };
            for (int i = 0; i < 8; i++)
            {
                result.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(string) });
            }

            result.Columns.Add(new DataColumn() { ColumnName = headers[8], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[9], DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[10], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[11], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[12], DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[13], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[14], DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[15], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[16], DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[17], DataType = typeof(String) });
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

            var col = (char)('A' + result.Columns.Count);
            string tglawal = new DateTimeOffset(datefrom.Value).ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            string tglakhir = new DateTimeOffset(dateto.Value).ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            sheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN STOCK GUDANG {0}", categoryname);
            sheet.Cells[$"A1:{col}1"].Merge = true;
            sheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
            sheet.Cells[$"A2:{col}2"].Merge = true;
            sheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col}3"].Value = string.Format("KONFEKSI : {0}", unitname);
            sheet.Cells[$"A3:{col}3"].Merge = true;
            sheet.Cells[$"A3:{col}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


            sheet.Cells["A7"].LoadFromDataTable(result, false, OfficeOpenXml.Table.TableStyles.Light16);
            sheet.Cells["I5"].Value = headers[8];
            sheet.Cells["I5:J5"].Merge = true;

            sheet.Cells["K5"].Value = headers[10];
            sheet.Cells["K5:M5"].Merge = true;
            sheet.Cells["N5"].Value = headers[13];
            sheet.Cells["N5:O5"].Merge = true;
            sheet.Cells["P5"].Value = headers[15];
            sheet.Cells["P5:Q5"].Merge = true;

            foreach (var i in Enumerable.Range(0, 8))
            {
                col = (char)('A' + i);
                sheet.Cells[$"{col}5"].Value = headers[i];
                sheet.Cells[$"{col}5:{col}6"].Merge = true;
            }

            for (var i = 0; i < 9; i++)
            {
                col = (char)('I' + i);
                sheet.Cells[$"{col}6"].Value = subheaders[i];

            }

            foreach (var i in Enumerable.Range(0, 1))
            {
                col = (char)('R' + i);
                sheet.Cells[$"{col}5"].Value = headers[i + 17];
                sheet.Cells[$"{col}5:{col}6"].Merge = true;
            }

            sheet.Cells["A5:R6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A5:R6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A5:R6"].Style.Font.Bold = true;
            var widths = new int[] {10, 15, 15, 20, 20, 15, 20, 15, 10, 10, 10, 10, 10, 10, 10, 10, 10,15 };
            foreach (var i in Enumerable.Range(0, headers.Length))
            {
                sheet.Column(i + 1).Width = widths[i];
            }

            var a = Query.Count();
            sheet.Cells[$"A{6 + a}"].Value = "T O T A L  . . . . . . . . . . . . . . .";
            sheet.Cells[$"A{6 + a}:H{6 + a}"].Merge = true;
            sheet.Cells[$"A{6 + a}:H{6 + a}"].Style.Font.Bold = true;
            sheet.Cells[$"A{6 + a}:H{6 + a}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[$"A{6 + a}:H{6 + a}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells[$"I{6 + a}"].Value = BeginningQtyTotal;
            sheet.Cells[$"K{6 + a}"].Value = ReceiptQtyTotal;
            sheet.Cells[$"N{6 + a}"].Value = ExpendQtyTotal;
            sheet.Cells[$"P{6 + a}"].Value = EndingQtyTotal;

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;


        }

        String NumberFormat(double? numb)
        {

            var number = string.Format("{0:0,0.00}", numb);

            return number;
        }

        private class SaldoAwal {
            
            public long EPOID { get; set; }
            public long EPOItemId { get; set; }
            public double BeginningBalanceQty { get; set; }
            public decimal BeginningBaancePrice { get; set; }

        }


        private List<GarmentCategoryViewModel> GetProductCodes(int page, int size, string order, string filter)
        {
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var garmentSupplierUri = APIEndpoint.Core + $"master/garment-categories";
                string queryUri = "?page=" + page + "&size=" + size + "&order=" + order + "&filter=" + filter;
                string uri = garmentSupplierUri + queryUri;
                var response = httpClient.GetAsync($"{uri}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                List<GarmentCategoryViewModel> viewModel = JsonConvert.DeserializeObject<List<GarmentCategoryViewModel>>(result.GetValueOrDefault("data").ToString());
                return viewModel;
            }
            else
            {
                List<GarmentCategoryViewModel> viewModel = null;
                return viewModel;
            }
        }
    }
}
