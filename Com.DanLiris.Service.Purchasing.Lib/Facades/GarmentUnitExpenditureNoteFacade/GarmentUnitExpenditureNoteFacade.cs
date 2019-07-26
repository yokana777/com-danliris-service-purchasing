using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentCorrectionNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInventoryModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitExpenditureNoteFacade
{
    public class GarmentUnitExpenditureNoteFacade : IGarmentUnitExpenditureNoteFacade
    {
        private readonly string USER_AGENT = "Facade";

        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentUnitExpenditureNote> dbSet;
        private readonly DbSet<GarmentUnitDeliveryOrder> dbSetGarmentUnitDeliveryOrder;
        private readonly DbSet<GarmentUnitDeliveryOrderItem> dbSetGarmentUnitDeliveryOrderItem;
        private readonly DbSet<GarmentInventoryDocument> dbSetGarmentInventoryDocument;
        private readonly DbSet<GarmentInventoryMovement> dbSetGarmentInventoryMovement;
        private readonly DbSet<GarmentInventorySummary> dbSetGarmentInventorySummary;
        private readonly DbSet<GarmentUnitReceiptNoteItem> dbSetGarmentUnitReceiptNoteItem;
        private readonly DbSet<GarmentCorrectionNote> dbSetGarmentCorrectionNote;
        private readonly DbSet<GarmentExternalPurchaseOrder> dbSetGarmentExternalPurchaseOrder;

        //private GarmentReturnCorrectionNoteFacade garmentReturnCorrectionNoteFacade;


        private readonly IMapper mapper;

        public GarmentUnitExpenditureNoteFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentUnitExpenditureNote>();
            dbSetGarmentInventoryDocument = dbContext.Set<GarmentInventoryDocument>();
            dbSetGarmentInventoryMovement = dbContext.Set<GarmentInventoryMovement>();
            dbSetGarmentInventorySummary = dbContext.Set<GarmentInventorySummary>();
            dbSetGarmentUnitDeliveryOrder = dbContext.Set<GarmentUnitDeliveryOrder>();
            dbSetGarmentUnitDeliveryOrderItem = dbContext.Set<GarmentUnitDeliveryOrderItem>();
            dbSetGarmentUnitReceiptNoteItem = dbContext.Set<GarmentUnitReceiptNoteItem>();
            dbSetGarmentCorrectionNote= dbContext.Set<GarmentCorrectionNote>();
            dbSetGarmentExternalPurchaseOrder= dbContext.Set<GarmentExternalPurchaseOrder>();

            mapper = (IMapper)serviceProvider.GetService(typeof(IMapper));

            //this.garmentReturnCorrectionNoteFacade = (GarmentReturnCorrectionNoteFacade)serviceProvider.GetService(typeof(GarmentReturnCorrectionNoteFacade));
        }

        public async Task<int> Create(GarmentUnitExpenditureNote garmentUnitExpenditureNote)
        {
            int Created = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    garmentUnitExpenditureNote.IsPreparing = false;
                    EntityExtension.FlagForCreate(garmentUnitExpenditureNote, identityService.Username, USER_AGENT);
                    garmentUnitExpenditureNote.UENNo = await GenerateNo(garmentUnitExpenditureNote);
                    var garmentUnitDeliveryOrder = dbSetGarmentUnitDeliveryOrder.First(d => d.Id == garmentUnitExpenditureNote.UnitDOId);
                    EntityExtension.FlagForUpdate(garmentUnitDeliveryOrder, identityService.Username, USER_AGENT);
                    garmentUnitDeliveryOrder.IsUsed = true;

                    var UnitDO = dbContext.GarmentUnitDeliveryOrders.Include(d=>d.Items).FirstOrDefault(d => d.Id.Equals(garmentUnitExpenditureNote.UnitDOId));
                    foreach (var unitDOItem in UnitDO.Items)
                    {
                        var unitExSaved = garmentUnitExpenditureNote.Items.FirstOrDefault(d => d.UnitDOItemId == unitDOItem.Id);
                        if (unitExSaved == null || unitExSaved.IsSave == false)
                        {
                            var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == unitDOItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)unitDOItem.Quantity;
                            unitDOItem.Quantity = 0;
                        }
                    }

                    var garmentUnitExpenditureNoteItems = garmentUnitExpenditureNote.Items.Where(x => x.IsSave).ToList();
                    foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNoteItems)
                    {
                        EntityExtension.FlagForCreate(garmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                        var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == garmentUnitExpenditureNoteItem.UnitDOItemId);
                        var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == garmentUnitExpenditureNoteItem.URNItemId);
                        if (garmentUnitDeliveryOrderItem != null && garmentUnitReceiptNoteItem != null)
                        {
                            if (garmentUnitDeliveryOrderItem.Quantity != garmentUnitExpenditureNoteItem.Quantity)
                            {
                                EntityExtension.FlagForUpdate(garmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);
                                garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - ((decimal)garmentUnitDeliveryOrderItem.Quantity - (decimal)garmentUnitExpenditureNoteItem.Quantity);
                                garmentUnitDeliveryOrderItem.Quantity = garmentUnitExpenditureNoteItem.Quantity;
                            }
                            garmentUnitExpenditureNoteItem.DOCurrencyRate = garmentUnitDeliveryOrderItem.DOCurrencyRate == null ? 0 : garmentUnitDeliveryOrderItem.DOCurrencyRate;
                            garmentUnitExpenditureNoteItem.Conversion = garmentUnitReceiptNoteItem.Conversion;
                            var basicPrice = (garmentUnitExpenditureNoteItem.PricePerDealUnit * Math.Round(garmentUnitExpenditureNoteItem.Quantity / (double)garmentUnitExpenditureNoteItem.Conversion, 2) * garmentUnitExpenditureNoteItem.DOCurrencyRate) / (double)garmentUnitExpenditureNoteItem.Conversion;
                            garmentUnitExpenditureNoteItem.BasicPrice = Math.Round((decimal)basicPrice, 4);
                        }
                    }

                    //var garmentUENIsSaveFalse = garmentUnitExpenditureNote.Items.Where(d => d.IsSave == false).ToList();

                    //if (garmentUENIsSaveFalse.Count > 0)
                    //{
                    //    foreach (var itemFalseIsSave in garmentUENIsSaveFalse)
                    //    {
                    //        var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == itemFalseIsSave.UnitDOItemId);
                    //        var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == itemFalseIsSave.URNItemId);
                    //        EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                    //        garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)itemFalseIsSave.Quantity;
                    //        garmentUnitDeliveryOrderItem.Quantity = 0;
                    //    }
                    //}
                    

                    if (garmentUnitExpenditureNote.ExpenditureType == "TRANSFER")
                    {
                        var garmentInventoryDocumentTransferOutStorage = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "OUT");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentTransferOutStorage);

                        var garmentInventoryDocumentTransferInStorageRequest = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "IN");
                        garmentInventoryDocumentTransferInStorageRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                        garmentInventoryDocumentTransferInStorageRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                        garmentInventoryDocumentTransferInStorageRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentTransferInStorageRequest);

                        var garmentInventoryDocumentTransferOutStorageRequest = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "OUT");
                        garmentInventoryDocumentTransferOutStorageRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                        garmentInventoryDocumentTransferOutStorageRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                        garmentInventoryDocumentTransferOutStorageRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentTransferOutStorageRequest);

                        foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
                        {
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitExpenditureNoteItem.ProductId && s.StorageId == garmentUnitExpenditureNote.StorageId && s.UomId == garmentUnitExpenditureNoteItem.UomId);

                            var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExisting, "OUT");
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                            if (garmentInventorySummaryExisting == null)
                            {
                                var garmentInventorySummary = GenerateGarmentInventorySummary(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventoryMovement);
                                dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                                garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                            }

                            var garmentInventorySummaryExistingRequest = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitExpenditureNoteItem.ProductId && s.StorageId == garmentUnitExpenditureNote.StorageRequestId && s.UomId == garmentUnitExpenditureNoteItem.UomId);

                            var garmentInventoryMovementRequestIn = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExistingRequest, "IN");
                            garmentInventoryMovementRequestIn.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                            garmentInventoryMovementRequestIn.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                            garmentInventoryMovementRequestIn.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                            garmentInventoryMovementRequestIn.After = garmentInventoryMovementRequestIn.Before + (decimal)garmentUnitExpenditureNoteItem.Quantity;
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementRequestIn);

                            var garmentInventoryMovementRequestOut = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExistingRequest, "OUT");
                            garmentInventoryMovementRequestOut.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                            garmentInventoryMovementRequestOut.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                            garmentInventoryMovementRequestOut.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                            garmentInventoryMovementRequestOut.Before = garmentInventoryMovementRequestIn.After;
                            garmentInventoryMovementRequestOut.After = garmentInventoryMovementRequestOut.Before - (decimal)garmentUnitExpenditureNoteItem.Quantity;

                            //if (garmentInventorySummaryExistingRequest == null || garmentInventorySummaryExistingRequest.Quantity == 0)
                            //{
                            //    garmentInventoryMovementRequestOut.Before = garmentInventoryMovementRequestIn.After;
                            //    garmentInventoryMovementRequestOut.After = garmentInventoryMovementRequestIn.Before;
                            //}
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementRequestOut);

                            if (garmentInventorySummaryExistingRequest == null)
                            {
                                var garmentInventorySummaryRequest = GenerateGarmentInventorySummary(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventoryMovementRequestOut);
                                garmentInventorySummaryRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                                garmentInventorySummaryRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                                garmentInventorySummaryRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                                dbSetGarmentInventorySummary.Add(garmentInventorySummaryRequest);
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(garmentInventorySummaryExistingRequest, identityService.Username, USER_AGENT);
                                garmentInventorySummaryExistingRequest.Quantity = garmentInventoryMovementRequestOut.After;
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var garmentInventoryDocument = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "OUT");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocument);

                        foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
                        {
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitExpenditureNoteItem.ProductId && s.StorageId == garmentUnitExpenditureNote.StorageId && s.UomId == garmentUnitExpenditureNoteItem.UomId);

                            var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExisting, "OUT");
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                            if (garmentInventorySummaryExisting == null)
                            {
                                var garmentInventorySummary = GenerateGarmentInventorySummary(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventoryMovement);
                                dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                                garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    GarmentCorrectionNote Correction = null;
                    bool suppType = true;
                    List<GarmentExternalPurchaseOrder> newEPOList = new List<GarmentExternalPurchaseOrder>();

                    if (garmentUnitExpenditureNote.ExpenditureType == "EXTERNAL")
                    {
                        List<long> epoItemIds = new List<long>();
                        List<long> epoIds = new List<long>();
                        GarmentDeliveryOrder garmentDeliveryOrder = dbContext.GarmentDeliveryOrders.FirstOrDefault(d => d.Id.Equals(garmentUnitDeliveryOrder.DOId));
                        
                        List<GarmentCorrectionNoteItem> correctionNoteItems = new List<GarmentCorrectionNoteItem>();
                        decimal totalPrice = 0;
                        foreach (var item in garmentUnitExpenditureNote.Items)
                        {
                            GarmentUnitDeliveryOrderItem garmentUnitDeliveryOrderItem = dbContext.GarmentUnitDeliveryOrderItems.FirstOrDefault(d => d.Id == item.UnitDOItemId);
                            GarmentDeliveryOrderDetail garmentDeliveryOrderDetail = dbContext.GarmentDeliveryOrderDetails.FirstOrDefault(d => d.Id.Equals(item.DODetailId));
                            GarmentDeliveryOrderItem garmentDeliveryOrderItem = dbContext.GarmentDeliveryOrderItems.FirstOrDefault(d => d.Id.Equals(garmentDeliveryOrderDetail.GarmentDOItemId));

                            double priceTotalAfter = Math.Round((Math.Round(garmentUnitDeliveryOrderItem.ReturQuantity,2) *(double) garmentDeliveryOrderDetail.PricePerDealUnitCorrection),2);

                            GarmentCorrectionNoteItem correctionNoteItem = new GarmentCorrectionNoteItem
                            {
                                POId = garmentDeliveryOrderDetail.POId,
                                EPOId = garmentDeliveryOrderItem.EPOId,
                                DODetailId = item.DODetailId,
                                EPONo = garmentDeliveryOrderItem.EPONo,
                                PRId = garmentDeliveryOrderDetail.PRId,
                                POSerialNumber = garmentDeliveryOrderDetail.POSerialNumber,
                                RONo = garmentDeliveryOrderDetail.RONo,
                                PricePerDealUnitAfter = (decimal)garmentDeliveryOrderDetail.PricePerDealUnitCorrection,
                                PricePerDealUnitBefore = (decimal)garmentDeliveryOrderDetail.PricePerDealUnitCorrection,
                                PriceTotalBefore = (decimal)garmentDeliveryOrderDetail.PriceTotalCorrection,
                                PriceTotalAfter = (decimal)priceTotalAfter,
                                Quantity = (decimal)garmentUnitDeliveryOrderItem.ReturQuantity,
                                UomId = (long)garmentUnitDeliveryOrderItem.ReturUomId,
                                UomIUnit = garmentUnitDeliveryOrderItem.ReturUomUnit,
                                PRNo = garmentDeliveryOrderDetail.PRNo,
                                ProductCode = item.ProductCode,
                                ProductId = item.ProductId,
                                ProductName = item.ProductName
                            };
                            correctionNoteItems.Add(correctionNoteItem);
                            totalPrice += correctionNoteItem.PriceTotalAfter;

                            epoItemIds.Add(garmentUnitDeliveryOrderItem.EPOItemId);
                            if (!epoIds.Contains(garmentDeliveryOrderItem.EPOId))
                            {
                                epoIds.Add(garmentDeliveryOrderItem.EPOId);
                            }
                        }

                        List<string> listEpoNo = new List<string>();

                        foreach (var epoId in epoIds)
                        {
                            GarmentExternalPurchaseOrder garmentExternalPurchaseOrder = dbContext.GarmentExternalPurchaseOrders.FirstOrDefault(p => p.Id.Equals(epoId));
                            
                            List<long> garmentExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(p => p.GarmentEPOId.Equals(epoId)).Select(p => p.Id).ToList();
                            List<GarmentExternalPurchaseOrderItem> epoItems = new List<GarmentExternalPurchaseOrderItem>();
                            foreach (var garmentExternalPurchaseOrderItemId in garmentExternalPurchaseOrderItems)
                            {
                                if (epoItemIds.Contains(garmentExternalPurchaseOrderItemId))
                                {
                                    var BUKItem = garmentUnitExpenditureNote.Items.FirstOrDefault(a => a.EPOItemId == garmentExternalPurchaseOrderItemId);
                                    GarmentExternalPurchaseOrderItem garmentExternalPurchaseOrderItem = dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(p => p.Id.Equals(garmentExternalPurchaseOrderItemId));
                                    GarmentExternalPurchaseOrderItem newItem = new GarmentExternalPurchaseOrderItem
                                    {
                                        BudgetPrice = garmentExternalPurchaseOrderItem.BudgetPrice,
                                        Conversion = garmentExternalPurchaseOrderItem.Conversion,
                                        DealQuantity = BUKItem.Quantity/(double) BUKItem.Conversion,
                                        DealUomId = garmentExternalPurchaseOrderItem.DealUomId,
                                        DealUomUnit = garmentExternalPurchaseOrderItem.DealUomUnit,
                                        DefaultQuantity = BUKItem.Quantity / (double)BUKItem.Conversion,
                                        DefaultUomId = garmentExternalPurchaseOrderItem.DefaultUomId,
                                        DefaultUomUnit = garmentExternalPurchaseOrderItem.DefaultUomUnit,
                                        DOQuantity = 0,
                                        IsOverBudget = false,
                                        OverBudgetRemark = garmentExternalPurchaseOrderItem.OverBudgetRemark,
                                        POId = garmentExternalPurchaseOrderItem.POId,
                                        PONo = garmentExternalPurchaseOrderItem.PONo,
                                        PO_SerialNumber = garmentExternalPurchaseOrderItem.PO_SerialNumber + "-R",
                                        PricePerDealUnit = garmentExternalPurchaseOrderItem.PricePerDealUnit,
                                        PRId = garmentExternalPurchaseOrderItem.PRId,
                                        PRNo = garmentExternalPurchaseOrderItem.PRNo,
                                        ProductCode = garmentExternalPurchaseOrderItem.ProductCode,
                                        ProductId = garmentExternalPurchaseOrderItem.ProductId,
                                        ProductName = garmentExternalPurchaseOrderItem.ProductName,
                                        ReceiptQuantity = 0,
                                        Remark = garmentExternalPurchaseOrderItem.Remark,
                                        RONo = garmentExternalPurchaseOrderItem.RONo,
                                        SmallQuantity = BUKItem.Quantity,
                                        SmallUomId = garmentExternalPurchaseOrderItem.SmallUomId,
                                        SmallUomUnit = garmentExternalPurchaseOrderItem.SmallUomUnit,
                                        UsedBudget = 0,
                                        Article = garmentExternalPurchaseOrderItem.Article

                                    };
                                    epoItems.Add(newItem);
                                }
                            }
                            var EPONo = garmentExternalPurchaseOrder.EPONo + "-R";
                            GarmentExternalPurchaseOrder existEPO = dbContext.GarmentExternalPurchaseOrders.Where(p => p.EPONo.StartsWith(EPONo)).OrderByDescending(o => o.EPONo).FirstOrDefault();
                            if(listEpoNo.Count > 0)
                            {
                                string lastEpo = listEpoNo.Last();
                                var LastEPONo= lastEpo.Substring(lastEpo.IndexOf("R")+1);
                                int lastNo= Int32.Parse(LastEPONo) + 1;
                                EPONo = EPONo + lastNo.ToString();
                                listEpoNo.Add(EPONo);
                            }
                            else if (existEPO != null )
                            {
                                var LastEPONo = existEPO.EPONo.Substring(existEPO.EPONo.IndexOf("R") + 1);
                                int lastNo = Int32.Parse(LastEPONo) + 1;
                                EPONo = EPONo + lastNo.ToString();
                                //int lastNoNumber = Int32.Parse(existEPO.EPONo.Replace(EPONo, string.Empty)) + 1;
                                //EPONo = EPONo + lastNoNumber.ToString();
                                listEpoNo.Add(EPONo);
                            }
                            else
                            {
                                EPONo = EPONo + "1";
                                listEpoNo.Add(EPONo);
                            }
                            GarmentExternalPurchaseOrder newEpo = new GarmentExternalPurchaseOrder
                            {
                                EPONo = EPONo,
                                Category = garmentExternalPurchaseOrder.Category,
                                CurrencyCode = garmentExternalPurchaseOrder.CurrencyCode,
                                CurrencyId = garmentExternalPurchaseOrder.CurrencyId,
                                CurrencyRate = garmentExternalPurchaseOrder.CurrencyRate,
                                DarkPerspiration = garmentExternalPurchaseOrder.DarkPerspiration,
                                DeliveryDate = garmentExternalPurchaseOrder.DeliveryDate,
                                DryRubbing = garmentExternalPurchaseOrder.DryRubbing,
                                FreightCostBy = garmentExternalPurchaseOrder.FreightCostBy,
                                IncomeTaxId = garmentExternalPurchaseOrder.IncomeTaxId,
                                IncomeTaxName = garmentExternalPurchaseOrder.IncomeTaxName,
                                IncomeTaxRate = garmentExternalPurchaseOrder.IncomeTaxRate,
                                IsApproved = garmentExternalPurchaseOrder.IsApproved,
                                IsCanceled = garmentExternalPurchaseOrder.IsCanceled,
                                IsClosed = garmentExternalPurchaseOrder.IsClosed,
                                IsIncomeTax = garmentExternalPurchaseOrder.IsIncomeTax,
                                IsOverBudget = false,
                                IsPosted = true,
                                IsUseVat = garmentExternalPurchaseOrder.IsUseVat,
                                LightMedPerspiration = garmentExternalPurchaseOrder.LightMedPerspiration,
                                OrderDate = garmentExternalPurchaseOrder.OrderDate,
                                PaymentDueDays = garmentExternalPurchaseOrder.PaymentDueDays,
                                PaymentMethod = garmentExternalPurchaseOrder.PaymentMethod,
                                PaymentType = garmentExternalPurchaseOrder.PaymentType,
                                PieceLength = garmentExternalPurchaseOrder.PieceLength,
                                QualityStandardType = garmentExternalPurchaseOrder.QualityStandardType,
                                Remark = garmentExternalPurchaseOrder.Remark,
                                Shrinkage = garmentExternalPurchaseOrder.Shrinkage,
                                SupplierCode = garmentExternalPurchaseOrder.SupplierCode,
                                SupplierId = garmentExternalPurchaseOrder.SupplierId,
                                SupplierImport = garmentExternalPurchaseOrder.SupplierImport,
                                SupplierName = garmentExternalPurchaseOrder.SupplierName,
                                Washing = garmentExternalPurchaseOrder.Washing,
                                WetRubbing = garmentExternalPurchaseOrder.WetRubbing,
                                Items = epoItems
                            };

                            suppType = garmentExternalPurchaseOrder.SupplierImport;

                            newEPOList.Add(newEpo);
                        }

                        Correction = new GarmentCorrectionNote
                        {
                            CorrectionDate = garmentUnitExpenditureNote.ExpenditureDate,
                            CorrectionType = "Retur",
                            DOId = garmentDeliveryOrder.Id,
                            DONo = garmentDeliveryOrder.DONo,
                            CurrencyCode = garmentDeliveryOrder.DOCurrencyCode,
                            CurrencyId = (long)garmentDeliveryOrder.DOCurrencyId,
                            IncomeTaxId = (long)garmentDeliveryOrder.IncomeTaxId,
                            IncomeTaxName = garmentDeliveryOrder.IncomeTaxName,
                            IncomeTaxRate = (decimal)garmentDeliveryOrder.IncomeTaxRate,
                            SupplierCode = garmentDeliveryOrder.SupplierCode,
                            SupplierId = garmentDeliveryOrder.SupplierId,
                            SupplierName = garmentDeliveryOrder.SupplierName,
                            UseIncomeTax = (bool)garmentDeliveryOrder.UseIncomeTax,
                            UseVat = (bool)garmentDeliveryOrder.UseVat,
                            TotalCorrection = totalPrice,
                            Items = correctionNoteItems
                        };

                        //await this.garmentReturnCorrectionNoteFacade.Create(Correction, suppType, identityService.Username);
                        #region Correction
                        var epoFacade = new GarmentExternalPurchaseOrderFacades.GarmentExternalPurchaseOrderFacade(serviceProvider, dbContext);
                        var correctionFacade = new GarmentReturnCorrectionNoteFacade(this.serviceProvider, dbContext);

                        EntityExtension.FlagForCreate(Correction, identityService.Username, USER_AGENT);

                        Correction.CorrectionNo = await correctionFacade.GenerateNo(Correction, suppType, 7);
                        //Correction.TotalCorrection = (garmentCorrectionNote.Items.Sum(i => i.PriceTotalAfter)) * (-1);
                        Correction.NKPH = "";
                        Correction.NKPN = "";
                        if (Correction.UseIncomeTax == true)
                        {
                            Correction.NKPH = await correctionFacade.GenerateNKPH(Correction, 7);
                        }
                        if (Correction.UseVat == true)
                        {
                            Correction.NKPN = await correctionFacade.GenerateNKPN(Correction, 7);
                        }

                        garmentDeliveryOrder.IsCorrection = true;

                        EntityExtension.FlagForUpdate(garmentDeliveryOrder, identityService.Username, USER_AGENT);

                        foreach (var item in correctionNoteItems)
                        {
                            item.Quantity = item.Quantity * (-1);
                            item.PriceTotalAfter = item.PriceTotalAfter *(-1);
                            EntityExtension.FlagForCreate(item, identityService.Username, USER_AGENT);

                            var garmentDeliveryOrderDetail = dbContext.GarmentDeliveryOrderDetails.First(d => d.Id == item.DODetailId);
                            var epoDetail = dbContext.GarmentExternalPurchaseOrderItems.First(d => d.Id == garmentDeliveryOrderDetail.EPOItemId);
                            //garmentDeliveryOrderDetail.QuantityCorrection = ((double)item.Quantity * (-1)) + garmentDeliveryOrderDetail.QuantityCorrection;
                            //garmentDeliveryOrderDetail.PriceTotalCorrection = garmentDeliveryOrderDetail.QuantityCorrection * garmentDeliveryOrderDetail.PricePerDealUnitCorrection;
                            garmentDeliveryOrderDetail.ReturQuantity = garmentDeliveryOrderDetail.ReturQuantity + ((double)item.Quantity * (-1));

                            epoDetail.DOQuantity = epoDetail.DOQuantity + (double)item.Quantity;
                            EntityExtension.FlagForUpdate(garmentDeliveryOrderDetail, identityService.Username, USER_AGENT);
                        }
                        dbSetGarmentCorrectionNote.Add(Correction);
                        #endregion

                        #region EPO
                        foreach (var epo in newEPOList)
                        {
                            EntityExtension.FlagForCreate(epo, identityService.Username, USER_AGENT);

                            foreach (var item in epo.Items)
                            {

                                //GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(item.POId));
                                //internalPurchaseOrder.IsPosted = true;

                                //GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                                //if (item.ProductId.ToString() == IPOItem.ProductId)
                                //{

                                //    IPOItem.Status = "Sudah dibuat PO Eksternal";
                                //}

                                //if ((epo.PaymentMethod == "CMT" || epo.PaymentMethod == "FREE FROM BUYER") && (epo.PaymentType == "FREE" || epo.PaymentType == "EX MASTER FREE"))
                                //{
                                //    epo.IsOverBudget = false;
                                //    item.IsOverBudget = false;
                                //    item.UsedBudget = 0;
                                //}
                                //else
                                //{
                                //    var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();

                                //    foreach (var a in ipoItems)
                                //    {
                                //        a.RemainingBudget -= item.UsedBudget;
                                //    }
                                //}

                                EntityExtension.FlagForCreate(item, identityService.Username, USER_AGENT);
                            }
                            dbSetGarmentExternalPurchaseOrder.Add(epo);
                        }

                        #endregion

                        await dbContext.SaveChangesAsync();

                        garmentUnitDeliveryOrder.CorrectionId = Correction.Id;
                        garmentUnitDeliveryOrder.CorrectionNo = Correction.CorrectionNo;

                    }


                    dbSet.Add(garmentUnitExpenditureNote);

                    Created = await dbContext.SaveChangesAsync();

                    transaction.Commit();

                    //var epoFacade = new GarmentExternalPurchaseOrderFacades.GarmentExternalPurchaseOrderFacade(serviceProvider, dbContext);
                    //var correctionFacade = new GarmentReturnCorrectionNoteFacade(this.serviceProvider, dbContext);

                    //if (Correction != null && garmentUnitExpenditureNote.ExpenditureType == "EXTERNAL" && newEPOList.Count > 0)
                    //{
                    //    await correctionFacade.Create(Correction, suppType, identityService.Username);
                    //    garmentUnitDeliveryOrder.CorrectionId = Correction.Id;
                    //    garmentUnitDeliveryOrder.CorrectionNo = Correction.CorrectionNo;


                    //    foreach (var epo in newEPOList)
                    //    {
                    //        await epoFacade.Create(epo, identityService.Username);
                    //    }

                    //    //Created = await dbContext.SaveChangesAsync();
                    //}
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Created;
        }

        

        public async Task<int> Delete(int id)
        {
            int Deleted = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var garmentUnitExpenditureNote = dbSet.Include(m => m.Items).Single(m => m.Id == id);

                    EntityExtension.FlagForDelete(garmentUnitExpenditureNote, identityService.Username, USER_AGENT);

                    var garmentUnitDeliveryOrder = dbSetGarmentUnitDeliveryOrder.FirstOrDefault(d => d.Id == garmentUnitExpenditureNote.UnitDOId);
                    if (garmentUnitDeliveryOrder != null)
                    {
                        EntityExtension.FlagForUpdate(garmentUnitDeliveryOrder, identityService.Username, USER_AGENT);
                        garmentUnitDeliveryOrder.IsUsed = false;
                        

                    }
                    foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
                    {
                        EntityExtension.FlagForDelete(garmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);
                        var garmentUnitDOItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(d => d.Id == garmentUnitExpenditureNoteItem.UnitDOItemId);
                        if (garmentUnitDOItem != null)
                        {
                            garmentUnitDOItem.Quantity = garmentUnitExpenditureNoteItem.Quantity;
                        }

                    }

                    var garmentInventoryDocument = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "IN");
                    dbSetGarmentInventoryDocument.Add(garmentInventoryDocument);

                    foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
                    {
                        var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.FirstOrDefault(s => s.ProductId == garmentUnitExpenditureNoteItem.ProductId && s.StorageId == garmentUnitExpenditureNote.StorageId && s.UomId == garmentUnitExpenditureNoteItem.UomId);
                        
                        var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExisting, "IN");
                        dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                        if (garmentInventorySummaryExisting != null)
                        {
                            EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                            garmentInventorySummaryExisting.Quantity = garmentInventorySummaryExisting.Quantity + (decimal)garmentUnitExpenditureNoteItem.Quantity;
                            garmentInventoryMovement.After = garmentInventorySummaryExisting.Quantity;
                        }
                    }

                    Deleted = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Deleted;
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentUnitExpenditureNote> Query = dbSet;

            Query = Query.Select(m => new GarmentUnitExpenditureNote
            {
                Id = m.Id,
                UENNo = m.UENNo,
                UnitDONo = m.UnitDONo,
                ExpenditureDate = m.ExpenditureDate,
                ExpenditureTo = m.ExpenditureTo,
                ExpenditureType = m.ExpenditureType,
                Items = m.Items.Select(i => new GarmentUnitExpenditureNoteItem
                {
                    Id = i.Id,
                    UENId = i.UENId,
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    RONo = i.RONo,
                    Quantity = i.Quantity,
                    UomId = i.UomId,
                    UomUnit = i.UomUnit,

                }).ToList(),
                CreatedAgent = m.CreatedAgent,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "UENNo", "UnitDONo", "ExpenditureType", "ExpenditureTo", "CreatedBy"
            };

            Query = QueryHelper<GarmentUnitExpenditureNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentUnitExpenditureNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentUnitExpenditureNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentUnitExpenditureNote> pageable = new Pageable<GarmentUnitExpenditureNote>(Query, Page - 1, Size);
            List<GarmentUnitExpenditureNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();
            ListData.AddRange(Data.Select(s => new
            {
                s.Id,
                s.UENNo,
                s.ExpenditureDate,
                s.ExpenditureTo,
                s.ExpenditureType,
                s.UnitDONo,
                s.CreatedAgent,
                s.CreatedBy,
                s.LastModifiedUtc,
                s.Items
            }));

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public GarmentUnitExpenditureNoteViewModel ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                            .Include(m => m.Items)
                            .FirstOrDefault();
            var viewModel = mapper.Map<GarmentUnitExpenditureNoteViewModel>(model);

            return viewModel;
        }

        public async Task<int> Update(int id, GarmentUnitExpenditureNote garmentUnitExpenditureNote)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var itemIsSaveFalse = garmentUnitExpenditureNote.Items.Where(i => i.IsSave == false).ToList();
                    garmentUnitExpenditureNote.Items = garmentUnitExpenditureNote.Items.Where(x => x.IsSave).ToList();

                    var oldGarmentUnitExpenditureNote = dbSet
                        .Include(d => d.Items)
                        .Single(m => m.Id == id);

                    foreach(var uncheck in itemIsSaveFalse)
                    {
                        var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.FirstOrDefault(s => s.ProductId == uncheck.ProductId && s.StorageId == oldGarmentUnitExpenditureNote.StorageId && s.UomId == uncheck.UomId);

                        var garmentInventoryMovement = GenerateGarmentInventoryMovement(oldGarmentUnitExpenditureNote, uncheck, garmentInventorySummaryExisting, "IN");
                        dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                        if (garmentInventorySummaryExisting != null)
                        {
                            EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                            garmentInventorySummaryExisting.Quantity = garmentInventorySummaryExisting.Quantity + (decimal)uncheck.Quantity;
                            garmentInventoryMovement.After = garmentInventorySummaryExisting.Quantity;
                        }
                    }


                    if (garmentUnitExpenditureNote.ExpenditureType == "TRANSFER")
                    {
                        var garmentInventoryDocumentIn = GenerateGarmentInventoryDocument(oldGarmentUnitExpenditureNote, "IN");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentIn);

                        var garmentInventoryDocumentOut = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "OUT");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentOut);

                        var garmentInventoryDocumentInRequest = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "IN");
                        garmentInventoryDocumentInRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                        garmentInventoryDocumentInRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                        garmentInventoryDocumentInRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentInRequest);

                        var garmentInventoryDocumentOutRequest = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "OUT");
                        garmentInventoryDocumentOutRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                        garmentInventoryDocumentOutRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                        garmentInventoryDocumentOutRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentOutRequest);

                        foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
                        {
                            var oldGarmentUnitExpenditureNoteItem = oldGarmentUnitExpenditureNote.Items.Single(i => i.Id == garmentUnitExpenditureNoteItem.Id);

                            //Buat IN untuk gudang yang mengeluarkan
                            var oldGarmentInventorySummaryExisting = dbSetGarmentInventorySummary.Single(s => s.ProductId == oldGarmentUnitExpenditureNoteItem.ProductId && s.StorageId == oldGarmentUnitExpenditureNote.StorageId && s.UomId == oldGarmentUnitExpenditureNoteItem.UomId);

                            var garmentInventoryMovementIn = GenerateGarmentInventoryMovement(oldGarmentUnitExpenditureNote, oldGarmentUnitExpenditureNoteItem, oldGarmentInventorySummaryExisting, "IN");
                            garmentInventoryMovementIn.After = garmentInventoryMovementIn.Before + (decimal)oldGarmentUnitExpenditureNoteItem.Quantity;
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementIn);

                            var garmentInventoryMovementOut = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, oldGarmentInventorySummaryExisting, "OUT");
                            garmentInventoryMovementOut.Before = garmentInventoryMovementIn.After;
                            garmentInventoryMovementOut.After = garmentInventoryMovementOut.Before - (decimal)garmentUnitExpenditureNoteItem.Quantity;
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementOut);

                            if (oldGarmentInventorySummaryExisting != null)
                            {
                                EntityExtension.FlagForUpdate(oldGarmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                                oldGarmentInventorySummaryExisting.Quantity = garmentInventoryMovementOut.After;
                            }

                            //Buat OUT untuk gudang yang mengeluarkan
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitExpenditureNoteItem.ProductId && s.StorageId == garmentUnitExpenditureNote.StorageRequestId && s.UomId == garmentUnitExpenditureNoteItem.UomId);

                            var garmentInventoryMovementInRequest = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExisting, "IN");
                            garmentInventoryMovementInRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                            garmentInventoryMovementInRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                            garmentInventoryMovementInRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                            garmentInventoryMovementInRequest.After = garmentInventoryMovementInRequest.Before + (decimal)garmentUnitExpenditureNoteItem.Quantity;
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementInRequest);

                            var garmentInventoryMovementOutRequest = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExisting, "OUT");
                            garmentInventoryMovementOutRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                            garmentInventoryMovementOutRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                            garmentInventoryMovementOutRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                            garmentInventoryMovementOutRequest.Before = garmentInventoryMovementInRequest.After;
                            garmentInventoryMovementOutRequest.After = garmentInventoryMovementOutRequest.Before - (decimal)garmentUnitExpenditureNoteItem.Quantity;
                            if (garmentInventorySummaryExisting == null || garmentInventorySummaryExisting.Quantity == 0)
                            {
                                garmentInventoryMovementOutRequest.Before = garmentInventoryMovementInRequest.After;
                                garmentInventoryMovementOutRequest.After = garmentInventoryMovementInRequest.Before;
                            }
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementOutRequest);

                            if (garmentInventorySummaryExisting == null)
                            {
                                var garmentInventorySummary = GenerateGarmentInventorySummary(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventoryMovementOutRequest);
                                garmentInventorySummary.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                                garmentInventorySummary.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                                garmentInventorySummary.StorageName = garmentUnitExpenditureNote.StorageRequestName;

                                dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                                garmentInventorySummaryExisting.Quantity = garmentInventoryMovementOutRequest.After;
                            }
                        }
                    }
                    else
                    {
                        var garmentInventoryDocumentIn = GenerateGarmentInventoryDocument(oldGarmentUnitExpenditureNote, "IN");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentIn);

                        var garmentInventoryDocumentOut = GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "OUT");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentOut);

                        foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
                        {
                            var oldGarmentUnitExpenditureNoteItem = oldGarmentUnitExpenditureNote.Items.Single(i => i.Id == garmentUnitExpenditureNoteItem.Id);

                            //Buat IN untuk gudang yang mengeluarkan
                            var oldGarmentInventorySummaryExisting = dbSetGarmentInventorySummary.Single(s => s.ProductId == oldGarmentUnitExpenditureNoteItem.ProductId && s.StorageId == oldGarmentUnitExpenditureNote.StorageId && s.UomId == oldGarmentUnitExpenditureNoteItem.UomId);

                            var garmentInventoryMovementIn = GenerateGarmentInventoryMovement(oldGarmentUnitExpenditureNote, oldGarmentUnitExpenditureNoteItem, oldGarmentInventorySummaryExisting, "IN");
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementIn);
                            
                            var garmentInventoryMovementOut = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, oldGarmentInventorySummaryExisting, "OUT");
                            garmentInventoryMovementOut.Before = garmentInventoryMovementIn.After;
                            garmentInventoryMovementOut.After = garmentInventoryMovementOut.Before -(decimal) garmentUnitExpenditureNoteItem.Quantity;
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementOut);

                            EntityExtension.FlagForUpdate(oldGarmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                            oldGarmentInventorySummaryExisting.Quantity = garmentInventoryMovementOut.After;
                        }
                    }

                    EntityExtension.FlagForUpdate(oldGarmentUnitExpenditureNote, identityService.Username, USER_AGENT);

                    foreach (var oldGarmentUnitExpenditureNoteItem in oldGarmentUnitExpenditureNote.Items)
                    {
                        var newGarmentUnitExpenditureNoteItem = garmentUnitExpenditureNote.Items.FirstOrDefault(i => i.Id == oldGarmentUnitExpenditureNoteItem.Id);
                        if (newGarmentUnitExpenditureNoteItem == null)
                        {
                            var coba = dbContext.GarmentUnitExpenditureNotes.AsNoTracking().FirstOrDefault(d => d.Id == id);
                            coba.Items = itemIsSaveFalse;
                            
                            
                            EntityExtension.FlagForDelete(oldGarmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                            var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == oldGarmentUnitExpenditureNoteItem.UnitDOItemId);
                            garmentUnitDeliveryOrderItem.Quantity = 0;

                            oldGarmentUnitExpenditureNoteItem.DOCurrencyRate = garmentUnitDeliveryOrderItem.DOCurrencyRate;

                            GarmentUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbContext.GarmentUnitReceiptNoteItems.Single(s => s.Id == oldGarmentUnitExpenditureNoteItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)oldGarmentUnitExpenditureNoteItem.Quantity;
                            oldGarmentUnitExpenditureNoteItem.Conversion = garmentUnitReceiptNoteItem.Conversion;
                        }
                        else
                        {
                            EntityExtension.FlagForUpdate(oldGarmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                            var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == oldGarmentUnitExpenditureNoteItem.UnitDOItemId);
                            var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == oldGarmentUnitExpenditureNoteItem.URNItemId);

                            if (garmentUnitDeliveryOrderItem != null && garmentUnitReceiptNoteItem != null)
                            {
                                EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                                garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - ((decimal)oldGarmentUnitExpenditureNoteItem.Quantity - (decimal)newGarmentUnitExpenditureNoteItem.Quantity);
                                garmentUnitDeliveryOrderItem.Quantity = newGarmentUnitExpenditureNoteItem.Quantity;
                            }
                            oldGarmentUnitExpenditureNoteItem.Quantity = garmentUnitExpenditureNote.Items.FirstOrDefault(i => i.Id == oldGarmentUnitExpenditureNoteItem.Id).Quantity;
                            oldGarmentUnitExpenditureNoteItem.DOCurrencyRate = garmentUnitDeliveryOrderItem.DOCurrencyRate;
                            oldGarmentUnitExpenditureNoteItem.Conversion = garmentUnitReceiptNoteItem.Conversion;
                        }
                        var basicPrice = (oldGarmentUnitExpenditureNoteItem.PricePerDealUnit * Math.Round(oldGarmentUnitExpenditureNoteItem.Quantity / (double)oldGarmentUnitExpenditureNoteItem.Conversion, 2) * oldGarmentUnitExpenditureNoteItem.DOCurrencyRate) / (double)oldGarmentUnitExpenditureNoteItem.Conversion;
                        oldGarmentUnitExpenditureNoteItem.BasicPrice = Math.Round((decimal)basicPrice, 4);
                    }

                    //dbSet.Update(garmentUnitExpenditureNote);

                    Updated = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        private GarmentInventorySummary GenerateGarmentInventorySummary(GarmentUnitExpenditureNote garmentUnitExpenditureNote, GarmentUnitExpenditureNoteItem garmentUnitExpenditureNoteItem, GarmentInventoryMovement garmentInventoryMovement)
        {
            var garmentInventorySummary = new GarmentInventorySummary();
            EntityExtension.FlagForCreate(garmentInventorySummary, identityService.Username, USER_AGENT);
            do
            {
                garmentInventorySummary.No = CodeGenerator.Generate();
            }
            while (dbSetGarmentInventorySummary.Any(m => m.No == garmentInventorySummary.No));

            garmentInventorySummary.ProductId = garmentUnitExpenditureNoteItem.ProductId;
            garmentInventorySummary.ProductCode = garmentUnitExpenditureNoteItem.ProductCode;
            garmentInventorySummary.ProductName = garmentUnitExpenditureNoteItem.ProductName;

            garmentInventorySummary.StorageId = garmentUnitExpenditureNote.StorageId;
            garmentInventorySummary.StorageCode = garmentUnitExpenditureNote.StorageCode;
            garmentInventorySummary.StorageName = garmentUnitExpenditureNote.StorageName;

            garmentInventorySummary.Quantity = garmentInventoryMovement.After;

            garmentInventorySummary.UomId = garmentUnitExpenditureNoteItem.UomId;
            garmentInventorySummary.UomUnit = garmentUnitExpenditureNoteItem.UomUnit;

            garmentInventorySummary.StockPlanning = 0;

            return garmentInventorySummary;
        }

        private GarmentInventoryMovement GenerateGarmentInventoryMovement(GarmentUnitExpenditureNote garmentUnitExpenditureNote, GarmentUnitExpenditureNoteItem garmentUnitExpenditureNoteItem, GarmentInventorySummary garmentInventorySummary, string type = "IN")
        {
            var garmentInventoryMovement = new GarmentInventoryMovement();
            EntityExtension.FlagForCreate(garmentInventoryMovement, identityService.Username, USER_AGENT);
            do
            {
                garmentInventoryMovement.No = CodeGenerator.Generate();
            }
            while (dbSetGarmentInventoryMovement.Any(m => m.No == garmentInventoryMovement.No));

            garmentInventoryMovement.Date = garmentInventoryMovement.CreatedUtc;

            garmentInventoryMovement.ReferenceNo = garmentUnitExpenditureNote.UENNo;
            garmentInventoryMovement.ReferenceType = string.Concat("Bon Pengeluaran Unit - ", garmentUnitExpenditureNote.UnitSenderName);

            garmentInventoryMovement.ProductId = garmentUnitExpenditureNoteItem.ProductId;
            garmentInventoryMovement.ProductCode = garmentUnitExpenditureNoteItem.ProductCode;
            garmentInventoryMovement.ProductName = garmentUnitExpenditureNoteItem.ProductName;

            garmentInventoryMovement.Type = (type ?? "").ToUpper() == "IN" ? "IN" : "OUT";

            garmentInventoryMovement.StorageId = garmentUnitExpenditureNote.StorageId;
            garmentInventoryMovement.StorageCode = garmentUnitExpenditureNote.StorageCode;
            garmentInventoryMovement.StorageName = garmentUnitExpenditureNote.StorageName;

            garmentInventoryMovement.StockPlanning = 0;
            if (garmentUnitExpenditureNote.ExpenditureType == "TRANSFER")
            {
                garmentInventoryMovement.Before = garmentInventorySummary == null ? 0 : garmentInventorySummary.Quantity;
                garmentInventoryMovement.Quantity = (decimal)garmentUnitExpenditureNoteItem.Quantity * (type.ToUpper() == "OUT" ? -1 : 1);
                garmentInventoryMovement.After = garmentInventorySummary == null || garmentInventorySummary.Quantity == 0  ? garmentInventoryMovement.Quantity : garmentInventoryMovement.Before + garmentInventoryMovement.Quantity;
            }
            else
            {
                garmentInventoryMovement.Before = garmentInventorySummary == null ? 0 : garmentInventorySummary.Quantity;
                garmentInventoryMovement.Quantity = (decimal)garmentUnitExpenditureNoteItem.Quantity * ((type ?? "").ToUpper() == "OUT" ? -1 : 1);
                garmentInventoryMovement.After = garmentInventoryMovement.Before + garmentInventoryMovement.Quantity;
            }

            garmentInventoryMovement.UomId = garmentUnitExpenditureNoteItem.UomId;
            garmentInventoryMovement.UomUnit = garmentUnitExpenditureNoteItem.UomUnit;

            garmentInventoryMovement.Remark = garmentUnitExpenditureNoteItem.ProductRemark;

            return garmentInventoryMovement;
        }

        private GarmentInventoryDocument GenerateGarmentInventoryDocument(GarmentUnitExpenditureNote garmentUnitExpenditureNote, string type = "IN")
        {
            var garmentInventoryDocument = new GarmentInventoryDocument
            {
                Items = new List<GarmentInventoryDocumentItem>()
            };
            EntityExtension.FlagForCreate(garmentInventoryDocument, identityService.Username, USER_AGENT);
            do
            {
                garmentInventoryDocument.No = CodeGenerator.Generate();
            }
            while (dbSetGarmentInventoryDocument.Any(m => m.No == garmentInventoryDocument.No));

            garmentInventoryDocument.Date = garmentUnitExpenditureNote.ExpenditureDate;
            garmentInventoryDocument.ReferenceNo = garmentUnitExpenditureNote.UENNo;
            garmentInventoryDocument.ReferenceType = string.Concat("Bon Pengeluaran Unit - ", garmentUnitExpenditureNote.UnitSenderName);

            garmentInventoryDocument.Type = (type ?? "").ToUpper() == "IN" ? "IN" : "OUT";

            garmentInventoryDocument.StorageId = garmentUnitExpenditureNote.StorageId;
            garmentInventoryDocument.StorageCode = garmentUnitExpenditureNote.StorageCode;
            garmentInventoryDocument.StorageName = garmentUnitExpenditureNote.StorageName;
            
            garmentInventoryDocument.Remark = "";

            foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
            {

                var garmentInventoryDocumentItem = new GarmentInventoryDocumentItem();
                EntityExtension.FlagForCreate(garmentInventoryDocumentItem, identityService.Username, USER_AGENT);

                garmentInventoryDocumentItem.ProductId = garmentUnitExpenditureNoteItem.ProductId;
                garmentInventoryDocumentItem.ProductCode = garmentUnitExpenditureNoteItem.ProductCode;
                garmentInventoryDocumentItem.ProductName = garmentUnitExpenditureNoteItem.ProductName;

                garmentInventoryDocumentItem.Quantity = (decimal)garmentUnitExpenditureNoteItem.Quantity;

                garmentInventoryDocumentItem.UomId = garmentUnitExpenditureNoteItem.UomId;
                garmentInventoryDocumentItem.UomUnit = garmentUnitExpenditureNoteItem.UomUnit;

                garmentInventoryDocumentItem.ProductRemark = garmentUnitExpenditureNoteItem.ProductRemark;

                garmentInventoryDocument.Items.Add(garmentInventoryDocumentItem);
            }

            return garmentInventoryDocument;
        }

        private async Task<string> GenerateNo(GarmentUnitExpenditureNote garmentUnitExpenditureNote)
        {
            string Year = garmentUnitExpenditureNote.ExpenditureDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("yy");
            string Month = garmentUnitExpenditureNote.ExpenditureDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("MM");
            string Day = garmentUnitExpenditureNote.ExpenditureDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("dd");

            string no = "";
            if (garmentUnitExpenditureNote.ExpenditureType == "PROSES" || garmentUnitExpenditureNote.ExpenditureType == "SAMPLE")// || garmentUnitExpenditureNote.ExpenditureType == "EXTERNAL")
            {
                no = string.Concat("BUK", garmentUnitExpenditureNote.UnitRequestCode, Year, Month, Day);
            }else if (garmentUnitExpenditureNote.ExpenditureType == "TRANSFER" || garmentUnitExpenditureNote.ExpenditureType == "EXTERNAL")
            {
                no = string.Concat("BUK", garmentUnitExpenditureNote.UnitSenderCode, Year, Month, Day);

            }
            int Padding = 3;

            var lastNo = await dbSet.Where(w => w.UENNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.UENNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.UENNo.Replace(no, string.Empty)) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public ReadResponse<object> ReadForGPreparing(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            IQueryable<GarmentUnitExpenditureNote> Query = dbSet
                .Where(x => x.UENNo.Contains(Keyword ?? "") && x.IsPreparing == false && x.Items.Any(i => i.ProductName == "FABRIC"))
                .Select(m => new GarmentUnitExpenditureNote
                {
                    Id = m.Id,
                    UENNo = m.UENNo,
                    ExpenditureDate = m.ExpenditureDate,
                    ExpenditureType = m.ExpenditureType,
                    UnitDOId = m.UnitDOId,
                    UnitDONo = m.UnitDONo,

                    UnitSenderId = m.UnitSenderId,
                    UnitSenderCode = m.UnitSenderCode,
                    UnitSenderName = m.UnitSenderName,
                    UnitRequestId = m.UnitRequestId,
                    UnitRequestCode = m.UnitRequestCode,
                    UnitRequestName = m.UnitRequestName,
                    StorageId = m.StorageId,
                    StorageCode = m.StorageCode,
                    StorageName = m.StorageName,
                    StorageRequestId = m.StorageRequestId,
                    StorageRequestCode = m.StorageRequestCode,
                    StorageRequestName = m.StorageRequestName,
                    IsPreparing = m.IsPreparing,
                    LastModifiedUtc = m.LastModifiedUtc,
                    Items = m.Items.Where(x => x.ProductName == "FABRIC").Select(i => new GarmentUnitExpenditureNoteItem
                    {
                        Id = i.Id,
                        UnitDOItemId = i.UnitDOItemId,
                        URNItemId = i.URNItemId,
                        ProductId = i.ProductId,
                        ProductCode = i.ProductCode,
                        ProductName = i.ProductName,
                        ProductRemark = i.ProductRemark,

                        PRItemId = i.PRItemId,
                        EPOItemId = i.EPOItemId,
                        DODetailId = i.DODetailId,
                        POItemId = i.POItemId,
                        POSerialNumber = i.POSerialNumber,
                        PricePerDealUnit = i.PricePerDealUnit,
                        Quantity = i.Quantity,
                        RONo = i.RONo,
                        UomId = i.UomId,
                        UomUnit = i.UomUnit,
                        FabricType = i.FabricType,
                        DOCurrencyRate = i.DOCurrencyRate,
                        Conversion = i.Conversion,
                        BasicPrice = i.BasicPrice,
                        ReturQuantity = i.ReturQuantity,
                    }).OrderByDescending(i => i.LastModifiedUtc).ToList()
                });

            Query = QueryHelper<GarmentUnitExpenditureNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentUnitExpenditureNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentUnitExpenditureNote> pageable = new Pageable<GarmentUnitExpenditureNote>(Query, Page - 1, Size);
            List<GarmentUnitExpenditureNote> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            //List<GarmentUnitExpenditureNote> DataViewModel = mapper.Map<List<GarmentUnitExpenditureNote>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataModel.Select(s => new
                {
                    s.Id,
                    s.UENNo,
                    s.ExpenditureDate,
                    s.ExpenditureType,
                    s.UnitDOId,
                    s.UnitDONo,
                    s.UnitSenderId,
                    s.UnitSenderCode,
                    s.UnitSenderName,
                    s.StorageId,
                    s.StorageCode,
                    s.StorageName,
                    s.UnitRequestId,
                    s.UnitRequestCode,
                    s.UnitRequestName,
                    s.StorageRequestId,
                    s.StorageRequestCode,
                    s.StorageRequestName,
                    s.CreatedBy,
                    s.LastModifiedUtc,
                    Items = s.Items.Select(i => new
                    {
                        i.Id,
                        i.UnitDOItemId,
                        i.URNItemId,
                        i.DODetailId,
                        i.EPOItemId,
                        i.POItemId,
                        i.PRItemId,
                        i.POSerialNumber,

                        i.ProductId,
                        i.ProductCode,
                        i.ProductName,
                        i.ProductRemark,
                        i.RONo,
                        i.UomId,
                        i.UomUnit,
                        i.PricePerDealUnit,
                        i.FabricType,
                        i.Quantity,
                        i.DOCurrencyRate,
                        i.Conversion,
                        i.BasicPrice,
                        i.ReturQuantity,
                    }).ToList()
                }).ToList()
            );
            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }

        public async Task<int> UpdateIsPreparing(int id, GarmentUnitExpenditureNote garmentUnitExpenditureNote)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldGarmentUnitExpenditureNote = dbSet
                        .Include(d => d.Items)
                        .Single(m => m.Id == id);

                    oldGarmentUnitExpenditureNote.IsPreparing = garmentUnitExpenditureNote.IsPreparing;

                    EntityExtension.FlagForUpdate(oldGarmentUnitExpenditureNote, identityService.Username, USER_AGENT);

                    foreach (var oldGarmentUnitExpenditureNoteItem in oldGarmentUnitExpenditureNote.Items)
                    {
                        EntityExtension.FlagForUpdate(oldGarmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);
                    }

                    //dbSet.Update(garmentUnitExpenditureNote);

                    Updated = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }
    }
}
