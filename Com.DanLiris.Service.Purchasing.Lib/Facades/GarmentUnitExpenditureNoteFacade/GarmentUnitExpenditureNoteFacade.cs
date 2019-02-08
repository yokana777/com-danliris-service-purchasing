using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInventoryModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel;
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
            mapper = (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public async Task<int> Create(GarmentUnitExpenditureNote garmentUnitExpenditureNote)
        {
            int Created = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(garmentUnitExpenditureNote, identityService.Username, USER_AGENT);
                    garmentUnitExpenditureNote.UENNo = await GenerateNo(garmentUnitExpenditureNote);
                    var garmentUnitDeliveryOrder = dbSetGarmentUnitDeliveryOrder.First(d => d.Id == garmentUnitExpenditureNote.UnitDOId);
                    EntityExtension.FlagForUpdate(garmentUnitDeliveryOrder, identityService.Username, USER_AGENT);
                    garmentUnitDeliveryOrder.IsUsed = true;

                    //garmentUnitExpenditureNote.Items = garmentUnitExpenditureNote.Items.Where(x => x.IsSave).ToList();
                    foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
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

                            }
                        }
                    }

                    var garmentUENIsSaveFalse = garmentUnitExpenditureNote.Items.Where(d => d.IsSave == false).ToList();
                    if (garmentUENIsSaveFalse.Count() > 0)
                    {
                        foreach (var itemFalseIsSave in garmentUENIsSaveFalse)
                        {
                            var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == itemFalseIsSave.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)itemFalseIsSave.Quantity;
                        }
                    }

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
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementRequestIn);

                            var garmentInventoryMovementRequestOut = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExistingRequest, "OUT");
                            garmentInventoryMovementRequestOut.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                            garmentInventoryMovementRequestOut.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                            garmentInventoryMovementRequestOut.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                            if (garmentInventorySummaryExistingRequest == null || garmentInventorySummaryExistingRequest.Quantity == 0)
                            {
                                garmentInventoryMovementRequestOut.Before = garmentInventoryMovementRequestIn.After;
                                garmentInventoryMovementRequestOut.After = garmentInventoryMovementRequestIn.Before;
                            }
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

                    dbSet.Add(garmentUnitExpenditureNote);

                    Created = await dbContext.SaveChangesAsync();
                    transaction.Commit();
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
                    RONo = i.RONo
                }).ToList(),
                CreatedAgent = m.CreatedAgent,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "UENNo", "UnitDONo", "ExpenditureType", "ExpenditureTo", "CreatedAgent"
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
                s.LastModifiedUtc
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
                    var oldGarmentUnitExpenditureNote = dbSet
                        .Include(d => d.Items)
                        .Single(m => m.Id == id);

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
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.Single(s => s.ProductId == garmentUnitExpenditureNoteItem.ProductId && s.StorageId == garmentUnitExpenditureNote.StorageRequestId && s.UomId == garmentUnitExpenditureNoteItem.UomId);

                            var garmentInventoryMovementInRequest = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExisting, "IN");
                            garmentInventoryMovementInRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                            garmentInventoryMovementInRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                            garmentInventoryMovementInRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementInRequest);

                            var garmentInventoryMovementOutRequest = GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventorySummaryExisting, "OUT");
                            garmentInventoryMovementOutRequest.StorageId = garmentUnitExpenditureNote.StorageRequestId;
                            garmentInventoryMovementOutRequest.StorageCode = garmentUnitExpenditureNote.StorageRequestCode;
                            garmentInventoryMovementOutRequest.StorageName = garmentUnitExpenditureNote.StorageRequestName;
                            if (garmentInventorySummaryExisting == null || garmentInventorySummaryExisting.Quantity == 0)
                            {
                                garmentInventoryMovementOutRequest.Before = garmentInventoryMovementInRequest.After;
                                garmentInventoryMovementOutRequest.After = garmentInventoryMovementInRequest.Before;
                            }
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementOutRequest);

                            if (garmentInventorySummaryExisting == null)
                            {
                                var garmentInventorySummary = GenerateGarmentInventorySummary(garmentUnitExpenditureNote, garmentUnitExpenditureNoteItem, garmentInventoryMovementOutRequest);
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
                            EntityExtension.FlagForDelete(oldGarmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                            GarmentUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbContext.GarmentUnitReceiptNoteItems.Single(s => s.Id == oldGarmentUnitExpenditureNoteItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)oldGarmentUnitExpenditureNoteItem.Quantity;
                        }
                        else
                        {
                            EntityExtension.FlagForUpdate(oldGarmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                            var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == oldGarmentUnitExpenditureNoteItem.UnitDOItemId);
                            var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == oldGarmentUnitExpenditureNoteItem.URNItemId);

                            if (garmentUnitDeliveryOrderItem != null && garmentUnitReceiptNoteItem != null)
                            {
                                if (garmentUnitDeliveryOrderItem.Quantity != oldGarmentUnitExpenditureNoteItem.Quantity)
                                {
                                    EntityExtension.FlagForUpdate(garmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);
                                    garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - ((decimal)garmentUnitDeliveryOrderItem.Quantity - (decimal)oldGarmentUnitExpenditureNoteItem.Quantity);
                                }
                            }
                            oldGarmentUnitExpenditureNoteItem.Quantity = garmentUnitExpenditureNote.Items.FirstOrDefault(i => i.Id == oldGarmentUnitExpenditureNoteItem.Id).Quantity;

                        }
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
                garmentInventoryMovement.Quantity = (decimal)garmentUnitExpenditureNoteItem.Quantity;
                garmentInventoryMovement.After = garmentInventorySummary == null || garmentInventorySummary.Quantity == 0  ? garmentInventoryMovement.Quantity : garmentInventoryMovement.Before - garmentInventoryMovement.Quantity;
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
            if (garmentUnitExpenditureNote.ExpenditureType == "PROSES" || garmentUnitExpenditureNote.ExpenditureType == "SAMPLE" || garmentUnitExpenditureNote.ExpenditureType == "EXTERNAL")
            {
                no = string.Concat("BUK", garmentUnitExpenditureNote.UnitRequestCode, Year, Month, Day);
            }else if (garmentUnitExpenditureNote.ExpenditureType == "TRANSFER")
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
    }
}
