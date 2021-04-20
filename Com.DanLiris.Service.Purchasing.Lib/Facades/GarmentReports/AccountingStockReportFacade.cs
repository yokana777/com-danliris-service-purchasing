using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel.CostCalculationGarment;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Tuple<List<AccountingStockReportViewModel>, int>> GetStockReportAsync(int offset, string unitcode, string tipebarang, int page, int size, string Order, DateTime? dateFrom, DateTime? dateTo)
        {
            
            List<AccountingStockReportViewModel> Data = await GetStockQueryAsync(tipebarang, unitcode, dateFrom, dateTo, offset);

            Data = Data.OrderBy(x => x.ProductCode).ThenBy(x => x.PlanPo).ToList();
            //int TotalData = Data.Count();
            return Tuple.Create(Data, Data.Count());
        }
        public async Task<List<AccountingStockReportViewModel>> GetStockQueryAsync(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;

            var categories = GetProductCodes(1, int.MaxValue, "{}", "{}");

            var categories1 = ctg == "BB" ? categories.Where(x => x.CodeRequirement == "BB").Select(x => x.Name).ToArray() : ctg == "BP" ? categories.Where(x => x.CodeRequirement == "BP").Select(x => x.Name).ToArray() : ctg == "BE" ? categories.Where(x => x.CodeRequirement == "BE").Select(x => x.Name).ToArray() : categories.Select(x=>x.Name).ToArray();

            var lastdate = dbContext.BalanceStocks.OrderByDescending(x => x.CreateDate).Select(x => x.CreateDate).FirstOrDefault() == null ? new DateTime(1970, 1, 1) : dbContext.BalanceStocks.OrderByDescending(x => x.CreateDate).Select(x => x.CreateDate).FirstOrDefault();

            var BalaceStock = (from a in dbContext.BalanceStocks
                               join b in dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters() on (long)a.EPOItemId equals b.Id
                               join c in dbContext.GarmentExternalPurchaseOrders.IgnoreQueryFilters() on b.GarmentEPOId equals c.Id
                               join d in dbContext.GarmentInternalPurchaseOrders on b.POId equals d.Id
                               where a.CreateDate == lastdate

                               group new { a,b,c, d } by new { b.ProductCode, b.ProductName, b.PO_SerialNumber } into data
                               select new AccountingStockReportViewModel
                               {
                                   ProductCode = data.Key.ProductCode,
                                   ProductName = data.Key.ProductName,
                                   RO = data.FirstOrDefault().b.RONo,
                                   Buyer = data.FirstOrDefault().d.BuyerCode,
                                   PlanPo = data.FirstOrDefault().b.PO_SerialNumber,
                                   NoArticle = data.FirstOrDefault().a.ArticleNo,
                                   BeginningBalanceQty = (decimal)data.Sum(x=>x.a.CloseStock),
                                   BeginningBalanceUom = data.FirstOrDefault().b.SmallUomUnit,
                                   BeginningBalancePrice = (double)data.Sum(x => x.a.ClosePrice),
                                   ReceiptCorrectionQty = 0,
                                   ReceiptPurchaseQty =  0,
                                   ReceiptProcessQty = 0,
                                   ReceiptKon2AQty = 0,
                                   ReceiptKon2BQty = 0,
                                   ReceiptKon2CQty = 0,
                                   ReceiptKon1AQty = 0,
                                   ReceiptKon1BQty = 0,
                                   ReceiptCorrectionPrice = 0,
                                   ReceiptPurchasePrice = 0,
                                   ReceiptProcessPrice = 0,
                                   ReceiptKon2APrice = 0,
                                   ReceiptKon2BPrice = 0,
                                   ReceiptKon2CPrice = 0,
                                   ReceiptKon1APrice = 0,
                                   ReceiptKon1BPrice = 0,
                                   ExpendReturQty = 0,
                                   ExpendRestQty = 0,
                                   ExpendProcessQty = 0,
                                   ExpendSampleQty = 0,
                                   ExpendKon2AQty = 0,
                                   ExpendKon2BQty = 0,
                                   ExpendKon2CQty = 0,
                                   ExpendKon1AQty = 0,
                                   ExpendKon1BQty = 0,
                                   ExpendReturPrice = 0,
                                   ExpendRestPrice = 0,
                                   ExpendProcessPrice = 0,
                                   ExpendSamplePrice = 0,
                                   ExpendKon2APrice = 0,
                                   ExpendKon2BPrice = 0,
                                   ExpendKon2CPrice = 0,
                                   ExpendKon1APrice = 0,
                                   ExpendKon1BPrice = 0,
                                   EndingBalanceQty = 0,
                                   EndingBalancePrice = 0,
                                   POId = data.FirstOrDefault().b.POId

                               }).ToList();

            List<AccountingStockReportViewModel> penerimaan = new List<AccountingStockReportViewModel>();
            List<AccountingStockReportViewModel> pengeluaran = new List<AccountingStockReportViewModel>();
            List<AccountingStockReportViewModel> penerimaanSA = new List<AccountingStockReportViewModel>();
            List<AccountingStockReportViewModel> pengeluaranSA = new List<AccountingStockReportViewModel>();
            #region SaldoAwal
            var IdSATerima = (from a in dbContext.GarmentUnitReceiptNotes
                              join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                              join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
                              from ww in UE.DefaultIfEmpty()
                              join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                              from dd in UEN.DefaultIfEmpty()
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
                                  UENItemsId = ww == null ? 0 : ww.Id,
                                  UENId = dd == null ? 0 : dd.Id,
                                  EPOItemId = b.EPOItemId,
                                  a.UnitCode
                              }).ToList().Distinct();

            var sapenerimaanunitreceiptnoteids = IdSATerima.Select(x => x.UrnId).ToList();
            var sapenerimaanunitreceiptnotes = dbContext.GarmentUnitReceiptNotes.Where(x => sapenerimaanunitreceiptnoteids.Contains(x.Id)).Select(s => new { s.ReceiptDate, s.URNType, s.UnitCode, s.UENNo, s.Id }).ToList();
            var sapenerimaanunitreceiptnoteItemIds = IdSATerima.Select(x => x.UrnItemId).ToList();
            var sapenerimaanuntreceiptnoteItems = dbContext.GarmentUnitReceiptNoteItems.Where(x => sapenerimaanunitreceiptnoteItemIds.Contains(x.Id)).Select(s => new { s.ProductCode, s.ProductName, s.RONo, s.SmallUomUnit, s.POSerialNumber, s.ReceiptQuantity, s.DOCurrencyRate, s.PricePerDealUnit, s.Id, s.SmallQuantity, s.Conversion }).ToList();
            var sapenerimaanUnitExpenditureNoteItemIds = IdSATerima.Select(x => x.UENItemsId).ToList();
            var sapenerimaanUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => sapenerimaanUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id, s.Conversion }).ToList();
            var sapenerimaanUnitExpenditureNoteIds = IdSATerima.Select(x => x.UENId).ToList();
            var sapenerimaanUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => sapenerimaanUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id, s.UENNo }).ToList();
            var sapenerimaanExternalPurchaseOrderItemIds = IdSATerima.Select(x => x.EPOItemId).ToList();

            var sapenerimaanpurchaserequestro = sapenerimaanuntreceiptnoteItems.Select(x => x.RONo).ToList();
            var sapenerimaanpurchaserequestros = dbContext.GarmentPurchaseRequests.Where(x => sapenerimaanpurchaserequestro.Contains(x.RONo)).Select(s => new { s.RONo, s.BuyerCode, s.Article }).ToList();
            foreach (var item in IdSATerima)
            {
                var sapenerimaanunitreceiptnote = sapenerimaanunitreceiptnotes.FirstOrDefault(x => x.Id == item.UrnId);
                var sapenerimaanuntreceiptnoteItem = sapenerimaanuntreceiptnoteItems.FirstOrDefault(x => x.Id == item.UrnItemId);
                var sapenerimaanUnitExpenditureNoteItem = sapenerimaanUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var sapenerimaanUnitExpenditureNote = sapenerimaanUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);

                var sapenerimaanpurchaserequestroes = sapenerimaanpurchaserequestros.FirstOrDefault(x => x.RONo == sapenerimaanuntreceiptnoteItem.RONo);

                penerimaanSA.Add(new AccountingStockReportViewModel
                {
                    ProductCode = sapenerimaanuntreceiptnoteItem.ProductCode,
                    ProductName = sapenerimaanuntreceiptnoteItem.ProductName,
                    RO = sapenerimaanuntreceiptnoteItem.RONo,
                    Buyer = sapenerimaanpurchaserequestroes == null ? "" : sapenerimaanpurchaserequestroes.BuyerCode,
                    PlanPo = sapenerimaanuntreceiptnoteItem.POSerialNumber,
                    NoArticle = sapenerimaanpurchaserequestroes == null ? "" : sapenerimaanpurchaserequestroes.Article,
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = sapenerimaanuntreceiptnoteItem.SmallUomUnit,
                    BeginningBalancePrice = 0,
                    ReceiptCorrectionQty = 0,
                    ReceiptPurchaseQty = sapenerimaanunitreceiptnote.URNType == "PEMBELIAN" ? (decimal)sapenerimaanuntreceiptnoteItem.ReceiptQuantity * (decimal)sapenerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptProcessQty = sapenerimaanunitreceiptnote.URNType == "PROSES" ? sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon2AQty = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C2A") ? (decimal)sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon2BQty = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C2B") ? (decimal)sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon2CQty = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C2C") ? (decimal)sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon1AQty = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C1A") ? (decimal)sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon1BQty = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C1B") ? (decimal)sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptCorrectionPrice = 0,
                    ReceiptPurchasePrice = sapenerimaanunitreceiptnote.URNType == "PEMBELIAN" ? Math.Round(((sapenerimaanuntreceiptnoteItem.PricePerDealUnit / (sapenerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : sapenerimaanuntreceiptnoteItem.Conversion)) * (decimal)sapenerimaanuntreceiptnoteItem.DOCurrencyRate) * (sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptProcessPrice = sapenerimaanunitreceiptnote.URNType == "PROSES" ? Math.Round(((sapenerimaanuntreceiptnoteItem.PricePerDealUnit / (sapenerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : sapenerimaanuntreceiptnoteItem.Conversion)) * (decimal)sapenerimaanuntreceiptnoteItem.DOCurrencyRate) * (sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon2APrice = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C2A") ? Math.Round(((sapenerimaanuntreceiptnoteItem.PricePerDealUnit / (sapenerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : sapenerimaanuntreceiptnoteItem.Conversion)) * (decimal)sapenerimaanuntreceiptnoteItem.DOCurrencyRate) * (sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon2BPrice = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C2B") ? Math.Round(((sapenerimaanuntreceiptnoteItem.PricePerDealUnit / (sapenerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : sapenerimaanuntreceiptnoteItem.Conversion)) * (decimal)sapenerimaanuntreceiptnoteItem.DOCurrencyRate) * (sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon2CPrice = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C2C") ? Math.Round(((sapenerimaanuntreceiptnoteItem.PricePerDealUnit / (sapenerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : sapenerimaanuntreceiptnoteItem.Conversion)) * (decimal)sapenerimaanuntreceiptnoteItem.DOCurrencyRate) * (sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon1APrice = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C1A") ? Math.Round(((sapenerimaanuntreceiptnoteItem.PricePerDealUnit / (sapenerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : sapenerimaanuntreceiptnoteItem.Conversion)) * (decimal)sapenerimaanuntreceiptnoteItem.DOCurrencyRate) * (sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon1BPrice = sapenerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (sapenerimaanUnitExpenditureNote == null ? false : sapenerimaanUnitExpenditureNote.UnitSenderCode == "C1B") ? Math.Round(((sapenerimaanuntreceiptnoteItem.PricePerDealUnit / (sapenerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : sapenerimaanuntreceiptnoteItem.Conversion)) * (decimal)sapenerimaanuntreceiptnoteItem.DOCurrencyRate) * (sapenerimaanuntreceiptnoteItem.ReceiptQuantity * sapenerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ExpendReturQty = 0,
                    ExpendRestQty = 0,
                    ExpendProcessQty = 0,
                    ExpendSampleQty = 0,
                    ExpendKon2AQty = 0,
                    ExpendKon2BQty = 0,
                    ExpendKon2CQty = 0,
                    ExpendKon1AQty = 0,
                    ExpendKon1BQty = 0,
                    ExpendReturPrice = 0,
                    ExpendRestPrice = 0,
                    ExpendProcessPrice = 0,
                    ExpendSamplePrice = 0,
                    ExpendKon2APrice = 0,
                    ExpendKon2BPrice = 0,
                    ExpendKon2CPrice = 0,
                    ExpendKon1APrice = 0,
                    ExpendKon1BPrice = 0,
                    EndingBalanceQty = 0,
                    EndingBalancePrice = 0,
                    POId = 0
                });

            }

            var IdSAPengeluaran = (from a in dbContext.GarmentUnitExpenditureNotes
                                   join b in dbContext.GarmentUnitExpenditureNoteItems on a.Id equals b.UENId
                                   where 
                                   categories1.Contains(b.ProductName)
                                       && a.IsDeleted == false && b.IsDeleted == false
                                       && a.CreatedUtc.AddHours(offset).Date > lastdate.Value.Date
                                        && a.CreatedUtc.AddHours(offset).Date < DateFrom.Date
                                       && a.UnitSenderCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitSenderCode : unitcode)

                                   select new
                                   {
                                       UENId = a.Id,
                                       UENItemsId = b.Id,
                                   }).ToList().Distinct();

            var sapengeluaranUnitExpenditureNoteItemIds = IdSAPengeluaran.Select(x => x.UENItemsId).ToList();
            var sapengeluaranUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => sapengeluaranUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id, s.ProductCode, s.ProductName, s.RONo, s.POSerialNumber, s.UomUnit, s.DOCurrencyRate, s.BasicPrice, s.Conversion }).ToList();
            var sapengeluaranUnitExpenditureNoteIds = IdSAPengeluaran.Select(x => x.UENId).ToList();
            var sapengeluaranUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => sapengeluaranUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id, s.UENNo, s.ExpenditureType }).ToList();
            var sapengeluaranpurchaserequestro = sapengeluaranUnitExpenditureNoteItems.Select(x => x.RONo).ToList();
            var sapengeluaranpurchaserequestros = dbContext.GarmentPurchaseRequests.Where(x => sapengeluaranpurchaserequestro.Contains(x.RONo)).Select(s => new { s.RONo, s.BuyerCode, s.Article }).ToList();
            foreach (var item in IdSAPengeluaran)
            {
               
                var sapengeluaranUnitExpenditureNoteItem = sapengeluaranUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var sapengeluaranUnitExpenditureNote = sapengeluaranUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                var sapengeluaranpurchaserequestroes = sapengeluaranpurchaserequestros.FirstOrDefault(x => x.RONo == sapengeluaranUnitExpenditureNoteItem.RONo);

                pengeluaranSA.Add(new AccountingStockReportViewModel
                {
                    ProductCode = sapengeluaranUnitExpenditureNoteItem.ProductCode,
                    ProductName = sapengeluaranUnitExpenditureNoteItem.ProductName,
                    RO = sapengeluaranUnitExpenditureNoteItem.RONo,
                    Buyer = sapengeluaranpurchaserequestroes == null ? "" : sapengeluaranpurchaserequestroes.BuyerCode,
                    PlanPo = sapengeluaranUnitExpenditureNoteItem.POSerialNumber,
                    NoArticle = sapengeluaranpurchaserequestroes == null ? "" : sapengeluaranpurchaserequestroes.Article,
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = sapengeluaranUnitExpenditureNoteItem.UomUnit,
                    BeginningBalancePrice = 0,
                    ReceiptCorrectionQty = 0,
                    ReceiptPurchaseQty = 0,
                    ReceiptProcessQty = 0,
                    ReceiptKon2AQty = 0,
                    ReceiptKon2BQty = 0,
                    ReceiptKon2CQty = 0,
                    ReceiptKon1AQty = 0,
                    ReceiptKon1BQty = 0,
                    ReceiptCorrectionPrice = 0,
                    ReceiptPurchasePrice = 0,
                    ReceiptProcessPrice = 0,
                    ReceiptKon2APrice = 0,
                    ReceiptKon2BPrice = 0,
                    ReceiptKon2CPrice = 0,
                    ReceiptKon1APrice = 0,
                    ReceiptKon1BPrice = 0,
                    ExpendReturQty = sapengeluaranUnitExpenditureNote.ExpenditureType == "EXTERNAL" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendRestQty = 0,
                    ExpendProcessQty = sapengeluaranUnitExpenditureNote.ExpenditureType == "PROSES" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendSampleQty = sapengeluaranUnitExpenditureNote.ExpenditureType == "SAMPLE" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon2AQty = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2A" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon2BQty = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2B" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon2CQty = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2C/EX. K4" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon1AQty = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1A/EX. K3" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon1BQty = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1B" ? sapengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendReturPrice = sapengeluaranUnitExpenditureNote.ExpenditureType == "EXTERNAL" ? sapengeluaranUnitExpenditureNoteItem.Quantity * sapengeluaranUnitExpenditureNoteItem.PricePerDealUnit * sapengeluaranUnitExpenditureNoteItem.DOCurrencyRate : 0,
                    ExpendRestPrice = 0,
                    ExpendProcessPrice = sapengeluaranUnitExpenditureNote.ExpenditureType == "PROSES" ? sapengeluaranUnitExpenditureNoteItem.Quantity * ((double)sapengeluaranUnitExpenditureNoteItem.BasicPrice / ( sapengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)sapengeluaranUnitExpenditureNoteItem.Conversion)) : 0,
                    ExpendSamplePrice = sapengeluaranUnitExpenditureNote.ExpenditureType == "SAMPLE" ? sapengeluaranUnitExpenditureNoteItem.Quantity * ((double)sapengeluaranUnitExpenditureNoteItem.BasicPrice / (sapengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)sapengeluaranUnitExpenditureNoteItem.Conversion)) : 0,
                    ExpendKon2APrice = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2A" ? sapengeluaranUnitExpenditureNoteItem.Quantity * ((double)sapengeluaranUnitExpenditureNoteItem.BasicPrice / (sapengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)sapengeluaranUnitExpenditureNoteItem.Conversion) ): 0,
                    ExpendKon2BPrice = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2B" ? sapengeluaranUnitExpenditureNoteItem.Quantity * ((double)sapengeluaranUnitExpenditureNoteItem.BasicPrice / (sapengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)sapengeluaranUnitExpenditureNoteItem.Conversion) ): 0,
                    ExpendKon2CPrice = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2C/EX. K4" ? sapengeluaranUnitExpenditureNoteItem.Quantity * ((double)sapengeluaranUnitExpenditureNoteItem.BasicPrice / (sapengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)sapengeluaranUnitExpenditureNoteItem.Conversion) ): 0,
                    ExpendKon1APrice = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1A/EX. K3" ? sapengeluaranUnitExpenditureNoteItem.Quantity * ((double)sapengeluaranUnitExpenditureNoteItem.BasicPrice / (sapengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)sapengeluaranUnitExpenditureNoteItem.Conversion) ): 0,
                    ExpendKon1BPrice = (sapengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || sapengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && sapengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1B" ? sapengeluaranUnitExpenditureNoteItem.Quantity * ((double)sapengeluaranUnitExpenditureNoteItem.BasicPrice / ( sapengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)sapengeluaranUnitExpenditureNoteItem.Conversion) ): 0,
                    EndingBalanceQty = 0,
                    EndingBalancePrice = 0,
                    POId = 0
                });
            }

            var SAwal = BalaceStock.Concat(penerimaanSA).Concat(pengeluaranSA).ToList();
            var SaldoAwal = (from a in SAwal
                             group a by new { a.PlanPo, a.ProductCode } into data
                             select new AccountingStockReportViewModel
                             {
                                 ProductCode = data.Key.ProductCode,
                                 ProductName = data.FirstOrDefault().ProductName,
                                 RO = data.FirstOrDefault().RO,
                                 Buyer = data.FirstOrDefault().Buyer,
                                 PlanPo = data.FirstOrDefault().PlanPo,
                                 NoArticle = data.FirstOrDefault().NoArticle,
                                 BeginningBalanceQty = Math.Round((decimal)data.Sum(x => x.BeginningBalanceQty) + (decimal)data.Sum(x => x.ReceiptCorrectionQty) + (decimal)data.Sum(x => x.ReceiptPurchaseQty) + (decimal)data.Sum(x => x.ReceiptProcessQty) + (decimal)data.Sum(x => x.ReceiptKon2AQty) + (decimal)data.Sum(x => x.ReceiptKon2BQty) + (decimal)data.Sum(x => x.ReceiptKon2CQty) + (decimal)data.Sum(x => x.ReceiptKon1AQty) + (decimal)data.Sum(x => x.ReceiptKon1BQty) - ((decimal)data.Sum(x => x.ExpendReturQty) + (decimal)data.Sum(x => x.ExpendRestQty) + (decimal)data.Sum(x => x.ExpendProcessQty) + (decimal)data.Sum(x => x.ExpendSampleQty) + (decimal)data.Sum(x => x.ExpendKon2AQty) + (decimal)data.Sum(x => x.ExpendKon2BQty) + (decimal)data.Sum(x => x.ExpendKon2CQty) + (decimal)data.Sum(x => x.ExpendKon1AQty) + (decimal)data.Sum(x => x.ExpendKon1BQty)), 2),
                                 BeginningBalanceUom = data.FirstOrDefault().BeginningBalanceUom,
                                 BeginningBalancePrice = Math.Round((double)data.Sum(x => x.BeginningBalancePrice) + (double)data.Sum(x => x.ReceiptCorrectionPrice) + (double)data.Sum(x => x.ReceiptPurchasePrice) + (double)data.Sum(x => x.ReceiptProcessPrice) + (double)data.Sum(x => x.ReceiptKon2APrice) + (double)data.Sum(x => x.ReceiptKon2BPrice) + (double)data.Sum(x => x.ReceiptKon2CPrice) + (double)data.Sum(x => x.ReceiptKon1APrice) + (double)data.Sum(x => x.ReceiptKon1BPrice) - ((double)data.Sum(x => x.ExpendReturPrice) + (double)data.Sum(x => x.ExpendRestPrice) + (double)data.Sum(x => x.ExpendProcessPrice) + (double)data.Sum(x => x.ExpendSamplePrice) + (double)data.Sum(x => x.ExpendKon2APrice) + (double)data.Sum(x => x.ExpendKon2BPrice) + (double)data.Sum(x => x.ExpendKon2CPrice) + (double)data.Sum(x => x.ExpendKon1APrice) + (double)data.Sum(x => x.ExpendKon1BPrice)), 2),
                                 ReceiptCorrectionQty = 0,
                                 ReceiptPurchaseQty = 0,
                                 ReceiptProcessQty = 0,
                                 ReceiptKon2AQty = 0,
                                 ReceiptKon2BQty = 0,
                                 ReceiptKon2CQty = 0,
                                 ReceiptKon1AQty = 0,
                                 ReceiptKon1BQty = 0,
                                 ReceiptCorrectionPrice = 0,
                                 ReceiptPurchasePrice = 0,
                                 ReceiptProcessPrice = 0,
                                 ReceiptKon2APrice = 0,
                                 ReceiptKon2BPrice = 0,
                                 ReceiptKon2CPrice = 0,
                                 ReceiptKon1APrice = 0,
                                 ReceiptKon1BPrice = 0,
                                 ExpendReturQty = 0,
                                 ExpendRestQty = 0,
                                 ExpendProcessQty = 0,
                                 ExpendSampleQty = 0,
                                 ExpendKon2AQty = 0,
                                 ExpendKon2BQty = 0,
                                 ExpendKon2CQty = 0,
                                 ExpendKon1AQty = 0,
                                 ExpendKon1BQty = 0,
                                 ExpendReturPrice = 0,
                                 ExpendRestPrice = 0,
                                 ExpendProcessPrice = 0,
                                 ExpendSamplePrice = 0,
                                 ExpendKon2APrice = 0,
                                 ExpendKon2BPrice = 0,
                                 ExpendKon2CPrice = 0,
                                 ExpendKon1APrice = 0,
                                 ExpendKon1BPrice = 0,
                                 EndingBalanceQty = 0,
                                 EndingBalancePrice = 0,
                                 POId = data.FirstOrDefault().POId
                             }).ToList();
            #endregion
            var IdTerima = (from a in dbContext.GarmentUnitReceiptNotes
                            join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                            join c in dbContext.GarmentUnitExpenditureNoteItems on b.UENItemId equals c.Id into UE
                            from ww in UE.DefaultIfEmpty()
                            join r in dbContext.GarmentUnitExpenditureNotes on ww.UENId equals r.Id into UEN
                            from dd in UEN.DefaultIfEmpty()
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
                                UENItemsId = ww == null ? 0 : ww.Id,
                                UENId = dd == null ? 0 : dd.Id,
                                EPOItemId = b.EPOItemId,
                                a.UnitCode
                            }).ToList().Distinct();
            var penerimaanunitreceiptnoteids = IdTerima.Select(x => x.UrnId).ToList();
            var penerimaanunitreceiptnotes = dbContext.GarmentUnitReceiptNotes.Where(x => penerimaanunitreceiptnoteids.Contains(x.Id)).Select(s => new { s.ReceiptDate, s.URNType, s.UnitCode, s.UENNo, s.Id }).ToList();
            var penerimaanunitreceiptnoteItemIds = IdTerima.Select(x => x.UrnItemId).ToList();
            var penerimaanuntreceiptnoteItems = dbContext.GarmentUnitReceiptNoteItems.Where(x => penerimaanunitreceiptnoteItemIds.Contains(x.Id)).Select(s => new { s.ProductCode, s.ProductName, s.RONo, s.SmallUomUnit, s.POSerialNumber, s.ReceiptQuantity, s.DOCurrencyRate, s.PricePerDealUnit, s.Id, s.SmallQuantity, s.Conversion }).ToList();
            var penerimaanpurchaserequestro = penerimaanuntreceiptnoteItems.Select(x => x.RONo).ToList();
            var penerimaanpurchaserequestros = dbContext.GarmentPurchaseRequests.Where(x => penerimaanpurchaserequestro.Contains(x.RONo)).Select(s => new { s.Article, s.RONo, s.BuyerCode }).ToList();
            var penerimaanUnitExpenditureNoteItemIds = IdTerima.Select(x => x.UENItemsId).ToList();
            var penerimaanUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => penerimaanUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id }).ToList();
            var penerimaanUnitExpenditureNoteIds = IdTerima.Select(x => x.UENId).ToList();
            var penerimaanUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => penerimaanUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id, s.UENNo }).ToList();
            var penerimaanExternalPurchaseOrderItemIds = IdTerima.Select(x => x.EPOItemId).ToList();
            foreach (var item in IdTerima) {
                var penerimaanunitreceiptnote = penerimaanunitreceiptnotes.FirstOrDefault(x => x.Id == item.UrnId);
                var penerimaanuntreceiptnoteItem = penerimaanuntreceiptnoteItems.FirstOrDefault(x => x.Id == item.UrnItemId);
                var penerimaanpurchaserequestroes = penerimaanpurchaserequestros.FirstOrDefault(x => x.RONo == penerimaanuntreceiptnoteItem.RONo);
                var penerimaanUnitExpenditureNoteItem = penerimaanUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var penerimaanUnitExpenditureNote = penerimaanUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);

                penerimaan.Add(new AccountingStockReportViewModel
                {
                    ProductCode = penerimaanuntreceiptnoteItem.ProductCode,
                    ProductName = penerimaanuntreceiptnoteItem.ProductName,
                    RO = penerimaanuntreceiptnoteItem.RONo,
                    Buyer = penerimaanpurchaserequestroes == null ? "" : penerimaanpurchaserequestroes.BuyerCode,
                    PlanPo = penerimaanuntreceiptnoteItem.POSerialNumber,
                    NoArticle = penerimaanpurchaserequestroes == null ? "" : penerimaanpurchaserequestroes.Article,
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = penerimaanuntreceiptnoteItem.SmallUomUnit,
                    BeginningBalancePrice = 0,
                    ReceiptCorrectionQty = 0,
                    ReceiptPurchaseQty = penerimaanunitreceiptnote.URNType == "PEMBELIAN" ? (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptProcessQty = penerimaanunitreceiptnote.URNType == "PROSES" ? (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon2AQty = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C2A") ? (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon2BQty = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C2B") ? (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon2CQty = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C2C") ? (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon1AQty = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C1A") ? (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptKon1BQty = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C1B") ? (decimal)penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion : 0,
                    ReceiptCorrectionPrice = 0,
                    ReceiptPurchasePrice = penerimaanunitreceiptnote.URNType == "PEMBELIAN" ? Math.Round(((penerimaanuntreceiptnoteItem.PricePerDealUnit / (penerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : penerimaanuntreceiptnoteItem.Conversion)) * (decimal)penerimaanuntreceiptnoteItem.DOCurrencyRate) * (penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptProcessPrice = penerimaanunitreceiptnote.URNType == "PROSES" ? Math.Round(((penerimaanuntreceiptnoteItem.PricePerDealUnit / (penerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : penerimaanuntreceiptnoteItem.Conversion)) * (decimal)penerimaanuntreceiptnoteItem.DOCurrencyRate) * (penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon2APrice = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C2A") ? Math.Round(((penerimaanuntreceiptnoteItem.PricePerDealUnit / (penerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : penerimaanuntreceiptnoteItem.Conversion)) * (decimal)penerimaanuntreceiptnoteItem.DOCurrencyRate) * (penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon2BPrice = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C2B") ? Math.Round(((penerimaanuntreceiptnoteItem.PricePerDealUnit / (penerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : penerimaanuntreceiptnoteItem.Conversion)) * (decimal)penerimaanuntreceiptnoteItem.DOCurrencyRate) * (penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon2CPrice = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C2C") ? Math.Round(((penerimaanuntreceiptnoteItem.PricePerDealUnit / (penerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : penerimaanuntreceiptnoteItem.Conversion)) * (decimal)penerimaanuntreceiptnoteItem.DOCurrencyRate) * (penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon1APrice = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C1A") ? Math.Round(((penerimaanuntreceiptnoteItem.PricePerDealUnit / (penerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : penerimaanuntreceiptnoteItem.Conversion)) * (decimal)penerimaanuntreceiptnoteItem.DOCurrencyRate) * (penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ReceiptKon1BPrice = penerimaanunitreceiptnote.URNType == "GUDANG LAIN" && (penerimaanUnitExpenditureNote == null ? false : penerimaanUnitExpenditureNote.UnitSenderCode == "C1B") ? Math.Round(((penerimaanuntreceiptnoteItem.PricePerDealUnit / (penerimaanuntreceiptnoteItem.Conversion == 0 ? 1 : penerimaanuntreceiptnoteItem.Conversion)) * (decimal)penerimaanuntreceiptnoteItem.DOCurrencyRate) * (penerimaanuntreceiptnoteItem.ReceiptQuantity * penerimaanuntreceiptnoteItem.Conversion), 2) : 0,
                    ExpendReturQty = 0,
                    ExpendRestQty = 0,
                    ExpendProcessQty = 0,
                    ExpendSampleQty = 0,
                    ExpendKon2AQty = 0,
                    ExpendKon2BQty = 0,
                    ExpendKon2CQty = 0,
                    ExpendKon1AQty = 0,
                    ExpendKon1BQty = 0,
                    ExpendReturPrice = 0,
                    ExpendRestPrice = 0,
                    ExpendProcessPrice = 0,
                    ExpendSamplePrice = 0,
                    ExpendKon2APrice = 0,
                    ExpendKon2BPrice = 0,
                    ExpendKon2CPrice = 0,
                    ExpendKon1APrice = 0,
                    ExpendKon1BPrice = 0,
                    EndingBalanceQty = 0,
                    EndingBalancePrice = 0,
                    POId = 0
                });

            }
            var IdPengeluaran = (from a in dbContext.GarmentUnitExpenditureNotes
                                 join b in dbContext.GarmentUnitExpenditureNoteItems on a.Id equals b.UENId
                                 where categories1.Contains(b.ProductName)
                                     && a.IsDeleted == false && b.IsDeleted == false
                                     && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                                     && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                                     && a.UnitSenderCode == (string.IsNullOrWhiteSpace(unitcode) ? a.UnitSenderCode : unitcode)

                                 select new
                                 {
                                     UENId = a.Id,
                                     UENItemsId = b.Id,
                                 }).ToList().Distinct();
            var pengeluaranUnitExpenditureNoteItemIds = IdPengeluaran.Select(x => x.UENItemsId).ToList();
            var pengeluaranUnitExpenditureNoteItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => pengeluaranUnitExpenditureNoteItemIds.Contains(x.Id)).Select(s => new { s.Quantity, s.PricePerDealUnit, s.Id, s.ProductCode, s.ProductName, s.RONo, s.POSerialNumber, s.UomUnit, s.DOCurrencyRate, s.BasicPrice, s.Conversion }).ToList();
            var pengeluaranUnitExpenditureNoteIds = IdPengeluaran.Select(x => x.UENId).ToList();
            var pengeluaranUnitExpenditureNotes = dbContext.GarmentUnitExpenditureNotes.Where(x => pengeluaranUnitExpenditureNoteIds.Contains(x.Id)).Select(s => new { s.UnitSenderCode, s.UnitRequestName, s.ExpenditureTo, s.Id, s.UENNo, s.ExpenditureType }).ToList();
            var pengeluaranpurchaserequestro = pengeluaranUnitExpenditureNoteItems.Select(x => x.RONo).ToList();
            var pengeluaranpurchaserequestros = dbContext.GarmentPurchaseRequests.Where(x => pengeluaranpurchaserequestro.Contains(x.RONo)).Select(s => new { s.Article, s.RONo, s.BuyerCode }).ToList();
            foreach (var item in IdPengeluaran) {
                
                var pengeluaranUnitExpenditureNoteItem = pengeluaranUnitExpenditureNoteItems.FirstOrDefault(x => x.Id == item.UENItemsId);
                var pengeluaranUnitExpenditureNote = pengeluaranUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                var pengeluaranpurchaserequestroes = pengeluaranpurchaserequestros.FirstOrDefault(x => x.RONo == pengeluaranUnitExpenditureNoteItem.RONo);

                pengeluaran.Add(new AccountingStockReportViewModel
                {
                    ProductCode = pengeluaranUnitExpenditureNoteItem.ProductCode,
                    ProductName = pengeluaranUnitExpenditureNoteItem.ProductName,
                    RO = pengeluaranUnitExpenditureNoteItem.RONo,
                    Buyer = pengeluaranpurchaserequestroes == null ? "" : pengeluaranpurchaserequestroes.BuyerCode,
                    PlanPo = pengeluaranUnitExpenditureNoteItem.POSerialNumber,
                    NoArticle = pengeluaranpurchaserequestroes == null ? "" : pengeluaranpurchaserequestroes.Article,
                    BeginningBalanceQty = 0,
                    BeginningBalanceUom = pengeluaranUnitExpenditureNoteItem.UomUnit,
                    BeginningBalancePrice = 0,
                    ReceiptCorrectionQty = 0,
                    ReceiptPurchaseQty =  0,
                    ReceiptProcessQty = 0,
                    ReceiptKon2AQty = 0,
                    ReceiptKon2BQty = 0,
                    ReceiptKon2CQty = 0,
                    ReceiptKon1AQty = 0,
                    ReceiptKon1BQty = 0,
                    ReceiptCorrectionPrice = 0,
                    ReceiptPurchasePrice = 0,
                    ReceiptProcessPrice = 0,
                    ReceiptKon2APrice = 0,
                    ReceiptKon2BPrice = 0,
                    ReceiptKon2CPrice = 0,
                    ReceiptKon1APrice = 0,
                    ReceiptKon1BPrice = 0,
                    ExpendReturQty = pengeluaranUnitExpenditureNote.ExpenditureType == "EXTERNAL" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendRestQty = pengeluaranUnitExpenditureNote.ExpenditureType == "SISA" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendProcessQty = pengeluaranUnitExpenditureNote.ExpenditureType == "PROSES" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendSampleQty = pengeluaranUnitExpenditureNote.ExpenditureType == "SAMPLE" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon2AQty = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2A" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon2BQty = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2B" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon2CQty = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2C/EX. K4" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon1AQty = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1A/EX. K3" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendKon1BQty = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1B" ? pengeluaranUnitExpenditureNoteItem.Quantity : 0,
                    ExpendReturPrice = pengeluaranUnitExpenditureNote.ExpenditureType == "EXTERNAL" ? pengeluaranUnitExpenditureNoteItem.Quantity * pengeluaranUnitExpenditureNoteItem.PricePerDealUnit * pengeluaranUnitExpenditureNoteItem.DOCurrencyRate : 0,
                    ExpendRestPrice = pengeluaranUnitExpenditureNote.ExpenditureType == "SISA" ? pengeluaranUnitExpenditureNoteItem.Quantity * pengeluaranUnitExpenditureNoteItem.PricePerDealUnit * pengeluaranUnitExpenditureNoteItem.DOCurrencyRate : 0,
                    ExpendProcessPrice = pengeluaranUnitExpenditureNote.ExpenditureType == "PROSES" ? pengeluaranUnitExpenditureNoteItem.Quantity * pengeluaranUnitExpenditureNoteItem.PricePerDealUnit * pengeluaranUnitExpenditureNoteItem.DOCurrencyRate : 0,
                    ExpendSamplePrice = pengeluaranUnitExpenditureNote.ExpenditureType == "SAMPLE" ? pengeluaranUnitExpenditureNoteItem.Quantity * pengeluaranUnitExpenditureNoteItem.PricePerDealUnit * pengeluaranUnitExpenditureNoteItem.DOCurrencyRate : 0,
                    ExpendKon2APrice = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2A" ? pengeluaranUnitExpenditureNoteItem.Quantity * ((double)pengeluaranUnitExpenditureNoteItem.BasicPrice / (pengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)pengeluaranUnitExpenditureNoteItem.Conversion)) : 0,
                    ExpendKon2BPrice = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2B" ? pengeluaranUnitExpenditureNoteItem.Quantity * ((double)pengeluaranUnitExpenditureNoteItem.BasicPrice / (pengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)pengeluaranUnitExpenditureNoteItem.Conversion)) : 0,
                    ExpendKon2CPrice = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 2C/EX. K4" ? pengeluaranUnitExpenditureNoteItem.Quantity * ((double)pengeluaranUnitExpenditureNoteItem.BasicPrice / (pengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)pengeluaranUnitExpenditureNoteItem.Conversion)) : 0,
                    ExpendKon1APrice = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1A/EX. K3" ? pengeluaranUnitExpenditureNoteItem.Quantity * ((double)pengeluaranUnitExpenditureNoteItem.BasicPrice / (pengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)pengeluaranUnitExpenditureNoteItem.Conversion)) : 0,
                    ExpendKon1BPrice = (pengeluaranUnitExpenditureNote.ExpenditureType == "GUDANG LAIN" || pengeluaranUnitExpenditureNote.ExpenditureType == "TRANSFER") && pengeluaranUnitExpenditureNote.UnitRequestName == "CENTRAL 1B" ? pengeluaranUnitExpenditureNoteItem.Quantity * ((double)pengeluaranUnitExpenditureNoteItem.BasicPrice / (pengeluaranUnitExpenditureNoteItem.Conversion == 0 ? 1 : (double)pengeluaranUnitExpenditureNoteItem.Conversion)) : 0,
                    EndingBalanceQty = 0,
                    EndingBalancePrice = 0,
                    POId = 0
                });
            }

            var SAkhir = penerimaan.Concat(pengeluaran).ToList();
            var SaldoAkhir = (from a in SAkhir
                             group a by new { a.PlanPo, a.ProductCode } into data
                             select new AccountingStockReportViewModel
                             {
                                 ProductCode = data.Key.ProductCode,
                                 ProductName = data.FirstOrDefault().ProductName,
                                 RO = data.FirstOrDefault().RO,
                                 Buyer = data.FirstOrDefault().Buyer,
                                 PlanPo = data.FirstOrDefault().PlanPo,
                                 NoArticle = data.FirstOrDefault().NoArticle,
                                 BeginningBalanceQty = Math.Round((decimal)data.Sum(x => x.BeginningBalanceQty), 2),
                                 BeginningBalanceUom = data.FirstOrDefault().BeginningBalanceUom,
                                 BeginningBalancePrice = Math.Round((double)data.Sum(x => x.BeginningBalancePrice), 2),
                                 ReceiptCorrectionQty = Math.Round((decimal)data.Sum(x => x.ReceiptCorrectionQty), 2),
                                 ReceiptPurchaseQty = Math.Round((decimal)data.Sum(x => x.ReceiptPurchaseQty), 2),
                                 ReceiptProcessQty = Math.Round((decimal)data.Sum(x => x.ReceiptProcessQty), 2),
                                 ReceiptKon2AQty = Math.Round((decimal)data.Sum(x => x.ReceiptKon2AQty), 2),
                                 ReceiptKon2BQty = Math.Round((decimal)data.Sum(x => x.ReceiptKon2BQty), 2),
                                 ReceiptKon2CQty = Math.Round((decimal)data.Sum(x => x.ReceiptKon2CQty), 2),
                                 ReceiptKon1AQty = Math.Round((decimal)data.Sum(x => x.ReceiptKon1AQty), 2),
                                 ReceiptKon1BQty = Math.Round((decimal)data.Sum(x => x.ReceiptKon1BQty), 2),
                                 ReceiptCorrectionPrice = Math.Round((decimal)data.Sum(x => x.ReceiptCorrectionPrice), 2),
                                 ReceiptPurchasePrice = Math.Round((decimal)data.Sum(x => x.ReceiptPurchasePrice), 2),
                                 ReceiptProcessPrice = Math.Round((decimal)data.Sum(x => x.ReceiptProcessPrice), 2),
                                 ReceiptKon2APrice = Math.Round((decimal)data.Sum(x => x.ReceiptKon2APrice), 2),
                                 ReceiptKon2BPrice = Math.Round((decimal)data.Sum(x => x.ReceiptKon2BPrice), 2),
                                 ReceiptKon2CPrice = Math.Round((decimal)data.Sum(x => x.ReceiptKon2CPrice), 2),
                                 ReceiptKon1APrice = Math.Round((decimal)data.Sum(x => x.ReceiptKon1APrice), 2),
                                 ReceiptKon1BPrice = Math.Round((decimal)data.Sum(x => x.ReceiptKon1BPrice), 2),
                                 ExpendReturQty = Math.Round((double)data.Sum(x => x.ExpendReturQty), 2),
                                 ExpendRestQty = Math.Round((double)data.Sum(x => x.ExpendRestQty), 2),
                                 ExpendProcessQty = Math.Round((double)data.Sum(x => x.ExpendProcessQty), 2),
                                 ExpendSampleQty = Math.Round((double)data.Sum(x => x.ExpendSampleQty), 2),
                                 ExpendKon2AQty = Math.Round((double)data.Sum(x => x.ExpendKon2AQty), 2),
                                 ExpendKon2BQty = Math.Round((double)data.Sum(x => x.ExpendKon2BQty), 2),
                                 ExpendKon2CQty = Math.Round((double)data.Sum(x => x.ExpendKon2CQty), 2),
                                 ExpendKon1AQty = Math.Round((double)data.Sum(x => x.ExpendKon1AQty), 2),
                                 ExpendKon1BQty = Math.Round((double)data.Sum(x => x.ExpendKon1BQty), 2),
                                 ExpendReturPrice = Math.Round((double)data.Sum(x => x.ExpendReturPrice), 2),
                                 ExpendRestPrice = Math.Round((double)data.Sum(x => x.ExpendRestPrice), 2),
                                 ExpendProcessPrice = Math.Round((double)data.Sum(x => x.ExpendProcessPrice), 2),
                                 ExpendSamplePrice = Math.Round((double)data.Sum(x => x.ExpendSamplePrice), 2),
                                 ExpendKon2APrice = Math.Round((double)data.Sum(x => x.ExpendKon2APrice), 2),
                                 ExpendKon2BPrice = Math.Round((double)data.Sum(x => x.ExpendKon2BPrice), 2),
                                 ExpendKon2CPrice = Math.Round((double)data.Sum(x => x.ExpendKon2CPrice), 2),
                                 ExpendKon1APrice = Math.Round((double)data.Sum(x => x.ExpendKon1APrice), 2),
                                 ExpendKon1BPrice = Math.Round((double)data.Sum(x => x.ExpendKon1BPrice), 2),
                                 EndingBalanceQty = 0,
                                 EndingBalancePrice = 0,
                                 POId = data.FirstOrDefault().POId


                             }).ToList();

            var SaldoAkhirs2 = SaldoAwal.Concat(SaldoAkhir).ToList();
            var SaldoAkhirs = (from a in SaldoAkhirs2
                              group a by new { a.PlanPo, a.ProductCode } into data
                              select new AccountingStockReportViewModel
                              {
                                  ProductCode = data.Key.ProductCode,
                                  ProductName = data.FirstOrDefault().ProductName,
                                  RO = data.FirstOrDefault().RO,
                                  Buyer = data.FirstOrDefault().Buyer,
                                  PlanPo = data.Key.PlanPo,
                                  NoArticle = data.FirstOrDefault().NoArticle,
                                  BeginningBalanceQty = data.Sum(x => x.BeginningBalanceQty),
                                  BeginningBalanceUom = data.FirstOrDefault().BeginningBalanceUom,
                                  BeginningBalancePrice = data.Sum(x => x.BeginningBalancePrice),
                                  ReceiptCorrectionQty = data.Sum(x => x.ReceiptCorrectionQty),
                                  ReceiptPurchaseQty = data.Sum(x => x.ReceiptPurchaseQty),
                                  ReceiptProcessQty = data.Sum(x => x.ReceiptProcessQty),
                                  ReceiptKon2AQty = data.Sum(x => x.ReceiptKon2AQty),
                                  ReceiptKon2BQty = data.Sum(x => x.ReceiptKon2BQty),
                                  ReceiptKon2CQty = data.Sum(x => x.ReceiptKon2CQty),
                                  ReceiptKon1AQty = data.Sum(x => x.ReceiptKon1AQty),
                                  ReceiptKon1BQty = data.Sum(x => x.ReceiptKon1BQty),
                                  ReceiptCorrectionPrice = data.Sum(x => x.ReceiptCorrectionPrice),
                                  ReceiptPurchasePrice = data.Sum(x => x.ReceiptPurchasePrice),
                                  ReceiptProcessPrice = data.Sum(x => x.ReceiptProcessPrice),
                                  ReceiptKon2APrice = data.Sum(x => x.ReceiptKon2APrice),
                                  ReceiptKon2BPrice = data.Sum(x => x.ReceiptKon2BPrice),
                                  ReceiptKon2CPrice = data.Sum(x => x.ReceiptKon2CPrice),
                                  ReceiptKon1APrice = data.Sum(x => x.ReceiptKon1APrice),
                                  ReceiptKon1BPrice = data.Sum(x => x.ReceiptKon1BPrice),
                                  ExpendReturQty = data.Sum(x => x.ExpendReturQty),
                                  ExpendRestQty = data.Sum(x => x.ExpendRestQty),
                                  ExpendProcessQty = data.Sum(x => x.ExpendProcessQty),
                                  ExpendSampleQty = data.Sum(x => x.ExpendSampleQty),
                                  ExpendKon2AQty = data.Sum(x => x.ExpendKon2AQty),
                                  ExpendKon2BQty = data.Sum(x => x.ExpendKon2BQty),
                                  ExpendKon2CQty = data.Sum(x => x.ExpendKon2CQty),
                                  ExpendKon1AQty = data.Sum(x => x.ExpendKon1AQty),
                                  ExpendKon1BQty = data.Sum(x => x.ExpendKon1BQty),
                                  ExpendReturPrice = data.Sum(x => x.ExpendReturPrice),
                                  ExpendRestPrice = data.Sum(x => x.ExpendRestPrice),
                                  ExpendProcessPrice = data.Sum(x => x.ExpendProcessPrice),
                                  ExpendSamplePrice = data.Sum(x => x.ExpendSamplePrice),
                                  ExpendKon2APrice = data.Sum(x => x.ExpendKon2APrice),
                                  ExpendKon2BPrice = data.Sum(x => x.ExpendKon2BPrice),
                                  ExpendKon2CPrice = data.Sum(x => x.ExpendKon2CPrice),
                                  ExpendKon1APrice = data.Sum(x => x.ExpendKon1APrice),
                                  ExpendKon1BPrice = data.Sum(x => x.ExpendKon1BPrice),
                                  EndingBalanceQty = Math.Round((decimal)data.Sum(x => x.BeginningBalanceQty) + (decimal)data.Sum(x => x.ReceiptCorrectionQty) + (decimal)data.Sum(a => a.ReceiptPurchaseQty) + (decimal)data.Sum(a => a.ReceiptProcessQty) + (decimal)data.Sum(a => a.ReceiptKon2AQty) + (decimal)data.Sum(a => a.ReceiptKon2BQty) + (decimal)data.Sum(a => a.ReceiptKon2CQty) + (decimal)data.Sum(a => a.ReceiptKon1AQty) + (decimal)data.Sum(a => a.ReceiptKon1BQty) - ((decimal)data.Sum(a => a.ExpendReturQty) + (decimal)data.Sum(a => a.ExpendSampleQty) + (decimal)data.Sum(a => a.ExpendRestQty) + (decimal)data.Sum(a => a.ExpendProcessQty) + (decimal)data.Sum(a => a.ExpendKon2AQty) + (decimal)data.Sum(a => a.ExpendKon2BQty) + (decimal)data.Sum(a => a.ExpendKon2CQty) + (decimal)data.Sum(a => a.ExpendKon1AQty) + (decimal)data.Sum(a => a.ExpendKon1BQty)), 2),                               
                                  EndingBalancePrice = Math.Round((double)data.Sum(a => a.BeginningBalancePrice) + (double)data.Sum(a => a.ReceiptCorrectionPrice) + (double)data.Sum(a => a.ReceiptPurchasePrice) + (double)data.Sum(a => a.ReceiptProcessPrice) + (double)data.Sum(a => a.ReceiptKon2APrice) + (double)data.Sum(a => a.ReceiptKon2BPrice) + (double)data.Sum(a => a.ReceiptKon2CPrice) + (double)data.Sum(a => a.ReceiptKon1APrice) + (double)data.Sum(a => a.ReceiptKon1BPrice) - ((double)data.Sum(a => a.ExpendReturPrice) + (double)data.Sum(a => a.ExpendRestPrice) + (double)data.Sum(a => a.ExpendProcessPrice) + (double)data.Sum(a => a.ExpendSamplePrice) + (double)data.Sum(a => a.ExpendKon2APrice) + (double)data.Sum(a => a.ExpendKon2BPrice) + (double)data.Sum(a => a.ExpendKon2CPrice) + (double)data.Sum(a => a.ExpendKon1APrice) + (double)data.Sum(a => a.ExpendKon1BPrice)), 2),
                                  POId = data.FirstOrDefault().POId
                              }).ToList();


            return SaldoAkhirs.ToList();

        }
        public async Task<MemoryStream> GenerateExcelAStockReportAsync(string ctg, string categoryname, string unitcode, string unitname, DateTime? datefrom, DateTime? dateto, int offset)
        {
            var Query = await GetStockQueryAsync(ctg, unitcode, datefrom, dateto, offset);
            Query = Query.OrderBy(x => x.ProductCode).ThenBy(x => x.PlanPo).ToList();
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
            double KONFEKSI1BQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon1BQty));
            double KONFEKSI1BPriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon1BPrice));
            double KONFEKSI1AQtyTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon1AQty));
            double KONFEKSI1APriceTotal = Query.Sum(x => Convert.ToDouble(x.ReceiptKon1APrice));
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
            double? ExpendKONFEKSI1BQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon1BQty));
            double? ExpendKONFEKSI1BPriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon1BPrice));
            double? ExpendKONFEKSI1AQtyTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon1AQty));
            double? ExpendKONFEKSI1APriceTotal = Query.Sum(x => Convert.ToDouble(x.ExpendKon1APrice));
            #endregion
            double? EndingQty = Query.Sum(x => Convert.ToDouble(x.EndingBalanceQty));
            double? EndingTotal = Query.Sum(x => Convert.ToDouble(x.EndingBalancePrice));
            DataTable result = new DataTable();
            var headers = new string[] { "No", "Kode", "Nama Barang", "RO", "Buyer", "PlanPO", "No Artikel", "Saldo Awal", "Saldo Awal1", "Saldo Awal2", "P E M A S U K A N", "P E M B E L I A N1", "P E M B E L I A N2", "P E M B E L I A N3", "P E M B E L I A N4", "P E M B E L I A N5", "P E M B E L I A N6", "P E M B E L I A N7", "P E M B E L I A N8", "P E M B E L I A N9", "P E M B E L I A N10", "P E M B E L I A N11", "P E M B E L I A N12", "P E M B E L I A N13", "P E M B E L I A N14", "P E M B E L I A N15", "P E N G E L U A R A N", "P E N G E L U A R A N1", "P E N G E L U A R A N2", "P E N G E L U A R A N3", "P E N G E L U A R A N4", "P E N G E L U A R A N5", "P E N G E L U A R A N6", "P E N G E L U A R A N7", "P E N G E L U A R A N8", "P E N G E L U A R A N9", "P E N G E L U A R A N10", "P E N G E L U A R A N11", "P E N G E L U A R A N12", "P E N G E L U A R A N13", "P E N G E L U A R A N14", "P E N G E L U A R A N15", "P E N G E L U A R A N16", "P E N G E L U A R A N17", "Saldo Akhir", "Saldo Akhir 1" };
            var headers2 = new string[] { "Koreksi", "Pembelian", "Proses", "KONFEKSI 2A", "KONFEKSI 2B", "KONFEKSI 2C/EX.K4", "KONFEKSI 1A/EX.K3",  "KONFEKSI 1B", "Retur", "Sisa", "Proses", "Sample", "KONFEKSI 2A", "KONFEKSI 2B", "KONFEKSI 2C/EX.K4", "KONFEKSI 1A/EX. K3", "KONFEKSI 1B" };
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
                var ReceiptPurchaseQty = unitcode == "C2A" ? item.ReceiptPurchaseQty + item.ReceiptKon2AQty : unitcode == "C2B" ? item.ReceiptPurchaseQty + item.ReceiptKon2BQty : unitcode == "C2C" ? item.ReceiptPurchaseQty + item.ReceiptKon2CQty : unitcode == "C1B" ? item.ReceiptPurchaseQty + item.ReceiptKon1BQty : unitcode == "C1A" ? item.ReceiptPurchaseQty + item.ReceiptKon1AQty : item.ReceiptPurchaseQty + item.ReceiptKon2AQty + item.ReceiptKon2BQty + item.ReceiptKon2CQty + item.ReceiptKon1BQty + item.ReceiptKon1AQty;
                var ReceiptPurchasePrice = unitcode == "C2A" ? item.ReceiptPurchasePrice + item.ReceiptKon2APrice : unitcode == "C2B" ? item.ReceiptPurchasePrice + item.ReceiptKon2BPrice : unitcode == "C2C" ? item.ReceiptPurchasePrice + item.ReceiptKon2CPrice : unitcode == "C1B" ? item.ReceiptPurchasePrice + item.ReceiptKon1BPrice : unitcode == "C1A" ? item.ReceiptPurchasePrice + item.ReceiptKon1APrice : item.ReceiptPurchasePrice + item.ReceiptKon2APrice + item.ReceiptKon2BPrice + item.ReceiptKon2CPrice + item.ReceiptKon1BPrice + item.ReceiptKon1APrice;
                var ReceiptKon2AQty = unitcode == "C2A" ? 0 : item.ReceiptKon2AQty;
                var ReceiptKon2APrice = unitcode == "C2A" ? 0 : item.ReceiptKon2APrice;
                var ReceiptKon2BPrice = unitcode == "C2B" ? 0 : item.ReceiptKon2BPrice;
                var ReceiptKon2BQty = unitcode == "C2B" ? 0 : item.ReceiptKon2BQty;
                var ReceiptKon2CPrice = unitcode == "C2C" ? 0 : item.ReceiptKon2CPrice;
                var ReceiptKon2CQty = unitcode == "C2C" ? 0 : item.ReceiptKon2CQty;
                var ReceiptKon1BPrice = unitcode == "C1B" ? 0 : item.ReceiptKon1BPrice;
                var ReceiptKon1BQty = unitcode == "C1B" ? 0 : item.ReceiptKon1BQty;
                var ReceiptKon1AQty = unitcode == "C1A" ? 0 : item.ReceiptKon1AQty;
                var ReceiptKon1APrice = unitcode == "C1A" ? 0 : item.ReceiptKon1APrice;
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
                    Convert.ToDouble(ReceiptKon1AQty),
                    Convert.ToDouble(ReceiptKon1APrice),
                    Convert.ToDouble(ReceiptKon1BQty),
                    Convert.ToDouble(ReceiptKon1BPrice),
                    Convert.ToDouble(item.ExpendReturQty), 
                    item.ExpendReturPrice, 
                    item.ExpendRestQty, 
                    item.ExpendRestPrice, 
                    item.ExpendProcessQty, 
                    item.ExpendProcessPrice, 
                    item.ExpendSampleQty, 
                    item.ExpendSamplePrice, 
                    item.ExpendKon2AQty, 
                    item.ExpendKon2APrice, 
                    item.ExpendKon2BQty, 
                    item.ExpendKon2BPrice, 
                    item.ExpendKon2CQty, 
                    item.ExpendKon2CPrice, 
                    item.ExpendKon1AQty, 
                    item.ExpendKon1APrice, 
                    item.ExpendKon1BQty, 
                    item.ExpendKon1BPrice, 
                    Convert.ToDouble(item.EndingBalanceQty), 
                    Convert.ToDouble(item.EndingBalancePrice));
            }

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
            sheet.Cells[$"A{result.Rows.Count + 7}:G{result.Rows.Count + 7}"].Value = "T O T A L  . . . . . . . . . . . . . . .";
            sheet.Cells[$"A{result.Rows.Count + 7}:G{result.Rows.Count + 7}"].Merge = true;
            sheet.Cells[$"A{result.Rows.Count + 7}:G{result.Rows.Count + 7}"].Style.Font.Bold = true;
            sheet.Cells[$"A{7 + result.Rows.Count}:G{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"A{result.Rows.Count + 7}:G{result.Rows.Count + 7}"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet.Cells[$"A{result.Rows.Count + 7}:G{result.Rows.Count + 7}"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            sheet.Cells[$"H{7 + result.Rows.Count}"].Value = string.Format("{0:0,0.00}", SaldoAwalQtyTotal);
            sheet.Cells[$"H{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"I{7 + result.Rows.Count}"].Value = "";
            sheet.Cells[$"I{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"J{7 + result.Rows.Count}"].Value = SaldoAwalPriceTotal;
            sheet.Cells[$"J{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"K{7 + result.Rows.Count}"].Value = KoreksiQtyTotal;
            sheet.Cells[$"K{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"L{7 + result.Rows.Count}"].Value = KoreksiPriceTotal;
            sheet.Cells[$"L{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"M{7 + result.Rows.Count}"].Value = PEMBELIANQtyTotal;
            sheet.Cells[$"M{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"N{7 + result.Rows.Count}"].Value = PEMBELIANPriceTotal;
            sheet.Cells[$"N{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"O{7 + result.Rows.Count}"].Value = PROSESQtyTotal;
            sheet.Cells[$"O{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"P{7 + result.Rows.Count}"].Value = PROSESPriceTotal;
            sheet.Cells[$"P{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"Q{7 + result.Rows.Count}"].Value = Konfeksi2AQtyTotal;
            sheet.Cells[$"Q{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"R{7 + result.Rows.Count}"].Value = Konfeksi2APriceTotal;
            sheet.Cells[$"R{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"S{7 + result.Rows.Count}"].Value = KONFEKSI2BQtyTotal;
            sheet.Cells[$"S{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"T{7 + result.Rows.Count}"].Value = KONFEKSI2BPriceTotal;
            sheet.Cells[$"T{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"U{7 + result.Rows.Count}"].Value = KONFEKSI2CQtyTotal;
            sheet.Cells[$"U{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"V{7 + result.Rows.Count}"].Value = KONFEKSI2CPriceTotal;
            sheet.Cells[$"V{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"W{7 + result.Rows.Count}"].Value = KONFEKSI1AQtyTotal;
            sheet.Cells[$"W{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"X{7 + result.Rows.Count}"].Value = KONFEKSI1APriceTotal;
            sheet.Cells[$"X{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"Y{7 + result.Rows.Count}"].Value = KONFEKSI1BQtyTotal;
            sheet.Cells[$"Y{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"Z{7 + result.Rows.Count}"].Value = KONFEKSI1BPriceTotal;
            sheet.Cells[$"Z{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AA{7 + result.Rows.Count}"].Value = ReturQtyTotal;
            sheet.Cells[$"AA{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AB{7 + result.Rows.Count}"].Value = ReturJumlahTotal;
            sheet.Cells[$"AB{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AC{7 + result.Rows.Count}"].Value = SisaQtyTotal;
            sheet.Cells[$"AC{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AD{7 + result.Rows.Count}"].Value = SisaPriceTotal;
            sheet.Cells[$"AD{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AE{7 + result.Rows.Count}"].Value = ExpendPROSESQtyTotal;
            sheet.Cells[$"AE{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AF{7 + result.Rows.Count}"].Value = ExpendPROSESPriceTotal;
            sheet.Cells[$"AF{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AG{7 + result.Rows.Count}"].Value = SAMPLEQtyTotal;
            sheet.Cells[$"AG{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AH{7 + result.Rows.Count}"].Value = SAMPLEPriceTotal;
            sheet.Cells[$"AH{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AI{7 + result.Rows.Count}"].Value = ExpendKONFEKSI2AQtyTotal;
            sheet.Cells[$"AI{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AJ{7 + result.Rows.Count}"].Value = ExpendKonfeksi2APriceTotal;
            sheet.Cells[$"AJ{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AK{7 + result.Rows.Count}"].Value = ExpendKONFEKSI2BQtyTotal;
            sheet.Cells[$"AK{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AL{7 + result.Rows.Count}"].Value = ExpendKONFEKSI2BPriceTotal;
            sheet.Cells[$"AL{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AM{7 + result.Rows.Count}"].Value = ExpendKONFEKSI2CQtyTotal;
            sheet.Cells[$"AM{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AN{7 + result.Rows.Count}"].Value = ExpendKONFEKSI2CPriceTotal;
            sheet.Cells[$"AN{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AO{7 + result.Rows.Count}"].Value = ExpendKONFEKSI1AQtyTotal;
            sheet.Cells[$"AO{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AP{7 + result.Rows.Count}"].Value = ExpendKONFEKSI1APriceTotal;
            sheet.Cells[$"AP{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AQ{7 + result.Rows.Count}"].Value = ExpendKONFEKSI1BQtyTotal;
            sheet.Cells[$"AQ{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AR{7 + result.Rows.Count}"].Value = ExpendKONFEKSI1BPriceTotal;
            sheet.Cells[$"AR{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AS{7 + result.Rows.Count}"].Value = EndingQty;
            sheet.Cells[$"AS{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[$"AT{7 + result.Rows.Count}"].Value = EndingTotal;
            sheet.Cells[$"AT{7 + result.Rows.Count}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);


            var widths = new int[] { 5, 10, 20, 15, 7, 20, 20, 10, 7, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };
            foreach (var i in Enumerable.Range(0, headers.Length))
            {
                sheet.Column(i + 1).Width = widths[i];
            }

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;


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
