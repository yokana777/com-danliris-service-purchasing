using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInventoryModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels;
using Com.DanLiris.Service.Purchasing.Lib.PDFTemplates.GarmentUnitReceiptNotePDFTemplates;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using System.Data;
using System.Globalization;
using System.Net.Http;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades
{
    public class GarmentUnitReceiptNoteFacade : IGarmentUnitReceiptNoteFacade
    {
        private readonly string USER_AGENT = "Facade";

        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentUnitReceiptNote> dbSet;
        private readonly DbSet<GarmentDeliveryOrderDetail> dbSetGarmentDeliveryOrderDetail;
        private readonly DbSet<GarmentExternalPurchaseOrderItem> dbSetGarmentExternalPurchaseOrderItems;
        private readonly DbSet<GarmentInternalPurchaseOrderItem> dbSetGarmentInternalPurchaseOrderItems;
        private readonly DbSet<GarmentInventoryDocument> dbSetGarmentInventoryDocument;
        private readonly DbSet<GarmentInventoryMovement> dbSetGarmentInventoryMovement;
        private readonly DbSet<GarmentInventorySummary> dbSetGarmentInventorySummary;
        private readonly DbSet<GarmentDeliveryOrder> dbsetGarmentDeliveryOrder;

        private readonly IMapper mapper;

        public GarmentUnitReceiptNoteFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentUnitReceiptNote>();
            dbSetGarmentDeliveryOrderDetail = dbContext.Set<GarmentDeliveryOrderDetail>();
            dbSetGarmentExternalPurchaseOrderItems = dbContext.Set<GarmentExternalPurchaseOrderItem>();
            dbSetGarmentInternalPurchaseOrderItems = dbContext.Set<GarmentInternalPurchaseOrderItem>();
            dbSetGarmentInventoryDocument = dbContext.Set<GarmentInventoryDocument>();
            dbSetGarmentInventoryMovement = dbContext.Set<GarmentInventoryMovement>();
            dbSetGarmentInventorySummary = dbContext.Set<GarmentInventorySummary>();
            dbsetGarmentDeliveryOrder = dbContext.Set<GarmentDeliveryOrder>();

            mapper = (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentUnitReceiptNote> Query = dbSet;
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureFilter(Query, FilterDictionary);

            Query = Query.Select(m => new GarmentUnitReceiptNote
            {
                Id = m.Id,
                URNNo = m.URNNo,
                UnitCode=m.UnitCode,
                UnitId=m.UnitId,
                UnitName = m.UnitName,
                ReceiptDate = m.ReceiptDate,
                SupplierName = m.SupplierName,
                DONo = m.DONo,
                DOId=m.DOId,
                StorageName=m.StorageName,
                StorageId=m.StorageId,
                StorageCode=m.StorageCode,
                Items = m.Items.Select(i => new GarmentUnitReceiptNoteItem
                {
                    Id = i.Id,
                    RONo = i.RONo,
                    ProductCode=i.ProductCode,
                    ProductId = i.ProductId,
                    ProductName=i.ProductName,
                    ProductRemark=i.ProductRemark,
                    OrderQuantity=i.OrderQuantity,
                    ReceiptQuantity=i.ReceiptQuantity,
                    SmallQuantity=i.SmallQuantity,
                    UomId=i.UomId,
                    UomUnit=i.UomUnit,
                    Conversion=i.Conversion,
                    DODetailId=i.DODetailId,
                    EPOItemId=i.EPOItemId,
                    POItemId=i.POItemId,
                    PRItemId=i.PRItemId,
                    POSerialNumber=i.POSerialNumber,
                    SmallUomId=i.SmallUomId,
                    SmallUomUnit=i.SmallUomUnit,
                    PricePerDealUnit=i.PricePerDealUnit,
                    DesignColor=i.DesignColor,
                    ReceiptCorrection=i.ReceiptCorrection,
                    CorrectionConversion=i.CorrectionConversion
                }).ToList(),
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "URNNo", "UnitName", "SupplierName", "DONo"
            };

            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentUnitReceiptNote> pageable = new Pageable<GarmentUnitReceiptNote>(Query, Page - 1, Size);
            List<GarmentUnitReceiptNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();
            ListData.AddRange(Data.Select(s => new
            {
                s.Id,
                s.URNNo,
                s.DOId,
                Unit = new { Name = s.UnitName, Id=s.UnitId, Code=s.UnitCode },
                Storage= new {name=s.StorageName, _id=s.StorageId, code=s.StorageCode},
                s.ReceiptDate,
                Supplier = new { Name = s.SupplierName },
                s.DONo,
                Items=new List<GarmentUnitReceiptNoteItem>(s.Items),
                s.CreatedBy,
                s.LastModifiedUtc
            }));

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public GarmentUnitReceiptNoteViewModel ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                            .Include(m => m.Items)
                            .FirstOrDefault();
            var viewModel = mapper.Map<GarmentUnitReceiptNoteViewModel>(model);

            viewModel.IsInvoice = dbContext.GarmentDeliveryOrders.Where(gdo => gdo.Id == viewModel.DOId).Select(gdo => gdo.IsInvoice).FirstOrDefault();

            foreach (var item in viewModel.Items)
            {
                item.Buyer = new BuyerViewModel
                {
                    Name = dbContext.GarmentPurchaseRequests.Where(m => m.Id == item.PRId).Select(m => m.BuyerName).FirstOrDefault()
                };
                item.Article = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == item.EPOItemId).Select(m => m.Article).FirstOrDefault();
            }

            return viewModel;
        }

        public MemoryStream GeneratePdf(GarmentUnitReceiptNoteViewModel garmentUnitReceiptNote)
        {
            return GarmentUnitReceiptNotePDFTemplate.GeneratePdfTemplate(serviceProvider, garmentUnitReceiptNote);
        }

        public async Task<int> Create(GarmentUnitReceiptNote garmentUnitReceiptNote)
        {
            int Created = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(garmentUnitReceiptNote, identityService.Username, USER_AGENT);
                    garmentUnitReceiptNote.URNNo = await GenerateNo(garmentUnitReceiptNote);
                    garmentUnitReceiptNote.IsStorage = true;

                    if (garmentUnitReceiptNote.URNType == "PEMBELIAN")
                    {
                        var garmentDeliveryOrder = dbsetGarmentDeliveryOrder.First(d => d.Id == garmentUnitReceiptNote.DOId);
                        garmentUnitReceiptNote.DOCurrencyRate = garmentDeliveryOrder.DOCurrencyRate;
                    }
                    else if(garmentUnitReceiptNote.URNType == "PROSES")
                    {
                        await UpdateDR(garmentUnitReceiptNote.DRId, true);
                    }
                    

                    foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    {
                        garmentUnitReceiptNoteItem.CorrectionConversion = garmentUnitReceiptNoteItem.Conversion;
                        EntityExtension.FlagForCreate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                        garmentUnitReceiptNoteItem.ReceiptCorrection = garmentUnitReceiptNoteItem.ReceiptQuantity;

                        var garmentDeliveryOrderDetail = dbSetGarmentDeliveryOrderDetail.First(d => d.Id == garmentUnitReceiptNoteItem.DODetailId);
                        EntityExtension.FlagForUpdate(garmentDeliveryOrderDetail, identityService.Username, USER_AGENT);
                        garmentDeliveryOrderDetail.ReceiptQuantity = (double)((decimal)garmentDeliveryOrderDetail.ReceiptQuantity + garmentUnitReceiptNoteItem.ReceiptQuantity);

                        var garmentExternalPurchaseOrderItem = dbSetGarmentExternalPurchaseOrderItems.First(d => d.Id == garmentUnitReceiptNoteItem.EPOItemId);
                        EntityExtension.FlagForUpdate(garmentExternalPurchaseOrderItem, identityService.Username, USER_AGENT);
                        garmentExternalPurchaseOrderItem.ReceiptQuantity = (double)((decimal)garmentExternalPurchaseOrderItem.ReceiptQuantity + garmentUnitReceiptNoteItem.ReceiptQuantity);

                        var garmentInternalPurchaseOrderItem = dbSetGarmentInternalPurchaseOrderItems.First(d => d.Id == garmentUnitReceiptNoteItem.POItemId);
                        EntityExtension.FlagForUpdate(garmentInternalPurchaseOrderItem, identityService.Username, USER_AGENT);
                        garmentInternalPurchaseOrderItem.Status = "Barang sudah diterima Unit";

                        var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == garmentUnitReceiptNoteItem.SmallUomId);

                        var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUnitReceiptNote, garmentUnitReceiptNoteItem, garmentInventorySummaryExisting);
                        dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                        if (garmentInventorySummaryExisting == null)
                        {
                            var garmentInventorySummary = GenerateGarmentInventorySummary(garmentUnitReceiptNote, garmentUnitReceiptNoteItem, garmentInventoryMovement);
                            dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                        }
                        else
                        {
                            EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                            garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                        }

                        await dbContext.SaveChangesAsync();
                    }

                    var garmentInventoryDocument = GenerateGarmentInventoryDocument(garmentUnitReceiptNote);
                    dbSetGarmentInventoryDocument.Add(garmentInventoryDocument);

                    dbSet.Add(garmentUnitReceiptNote);

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

        private async Task UpdateDR(string DRId, bool isUsed)
        {
            string drUri ="delivery-returns";
            IHttpClientService httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));

            var response = httpClient.GetAsync($"{APIEndpoint.GarmentProduction}{drUri}/{DRId}").Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                GarmentDeliveryReturnViewModel viewModel = JsonConvert.DeserializeObject< GarmentDeliveryReturnViewModel>(result.GetValueOrDefault("data").ToString());
                viewModel.IsUsed= isUsed;
                foreach(var item in viewModel.Items)
                {
                    item.QuantityUENItem = item.Quantity + 1;
                    item.RemainingQuantityPreparingItem= item.Quantity + 1;
                    item.IsSave = true;
                }

                //var httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
                var response2 = httpClient.PutAsync($"{APIEndpoint.GarmentProduction}{drUri}/{DRId}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
                var content2 = response2.Content.ReadAsStringAsync().Result;
                response2.EnsureSuccessStatusCode();
            }
            
        }

        public async Task<int> Update(int id, GarmentUnitReceiptNote garmentUnitReceiptNote)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldGarmentUnitReceiptNote = dbSet
                        .Include(d => d.Items)
                        .Single(m => m.Id == id);

                    // Gudang berubah
                    if (garmentUnitReceiptNote.StorageId != oldGarmentUnitReceiptNote.StorageId)
                    {
                        foreach (var oldGarmentUnitReceiptNoteItem in oldGarmentUnitReceiptNote.Items)
                        {
                            // Buat OUT untuk Gudang yang lama
                            var oldGarmentInventorySummary = dbSetGarmentInventorySummary.Single(s => s.ProductId == oldGarmentUnitReceiptNoteItem.ProductId && s.StorageId == oldGarmentUnitReceiptNote.StorageId && s.UomId == oldGarmentUnitReceiptNoteItem.SmallUomId);

                            var garmentInventoryMovementOut = GenerateGarmentInventoryMovement(oldGarmentUnitReceiptNote, oldGarmentUnitReceiptNoteItem, oldGarmentInventorySummary, "OUT");
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementOut);

                            EntityExtension.FlagForUpdate(oldGarmentInventorySummary, identityService.Username, USER_AGENT);
                            oldGarmentInventorySummary.Quantity = garmentInventoryMovementOut.After;

                            // Buat IN untuk Gudang yang baru
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == oldGarmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == oldGarmentUnitReceiptNoteItem.SmallUomId);

                            var garmentInventoryMovementIn = GenerateGarmentInventoryMovement(garmentUnitReceiptNote, oldGarmentUnitReceiptNoteItem, garmentInventorySummaryExisting, "IN");
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementIn);

                            if (garmentInventorySummaryExisting == null)
                            {
                                var garmentInventorySummary = GenerateGarmentInventorySummary(garmentUnitReceiptNote, oldGarmentUnitReceiptNoteItem, garmentInventoryMovementIn);
                                dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                                garmentInventorySummaryExisting.Quantity = garmentInventoryMovementIn.After;
                            }

                            await dbContext.SaveChangesAsync();
                        }

                        var garmentInventoryDocumentOut = GenerateGarmentInventoryDocument(oldGarmentUnitReceiptNote, "OUT");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentOut);

                        var garmentInventoryDocumentIn = GenerateGarmentInventoryDocument(garmentUnitReceiptNote, "IN");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentIn);

                        oldGarmentUnitReceiptNote.StorageId = garmentUnitReceiptNote.StorageId;
                        oldGarmentUnitReceiptNote.StorageCode = garmentUnitReceiptNote.StorageCode;
                        oldGarmentUnitReceiptNote.StorageName = garmentUnitReceiptNote.StorageName;
                    }

                    EntityExtension.FlagForUpdate(oldGarmentUnitReceiptNote, identityService.Username, USER_AGENT);
                    foreach (var oldGarmentUnitReceiptNoteItem in oldGarmentUnitReceiptNote.Items)
                    {
                        EntityExtension.FlagForUpdate(oldGarmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                    }
                    oldGarmentUnitReceiptNote.Remark = garmentUnitReceiptNote.Remark;

                    var garmentDeliveryOrder = dbsetGarmentDeliveryOrder.First(d => d.Id == garmentUnitReceiptNote.DOId);
                    garmentUnitReceiptNote.DOCurrencyRate = garmentDeliveryOrder.DOCurrencyRate;

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

        public async Task<int> Delete(int id, string deletedReason)
        {
            int Deleted = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var garmentUnitReceiptNote = dbSet.Include(m => m.Items).Single(m => m.Id == id);

                    garmentUnitReceiptNote.DeletedReason = deletedReason;
                    EntityExtension.FlagForDelete(garmentUnitReceiptNote, identityService.Username, USER_AGENT);

                    if (garmentUnitReceiptNote.URNType == "PROSES")
                    {
                        await UpdateDR(garmentUnitReceiptNote.DRId, false);
                    }

                    foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    {
                        EntityExtension.FlagForDelete(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);

                        var garmentDeliveryOrderDetail = dbSetGarmentDeliveryOrderDetail.First(d => d.Id == garmentUnitReceiptNoteItem.DODetailId);
                        EntityExtension.FlagForUpdate(garmentDeliveryOrderDetail, identityService.Username, USER_AGENT);
                        garmentDeliveryOrderDetail.ReceiptQuantity = (double)((decimal)garmentDeliveryOrderDetail.ReceiptQuantity - garmentUnitReceiptNoteItem.ReceiptQuantity);

                        var garmentExternalPurchaseOrderItem = dbSetGarmentExternalPurchaseOrderItems.First(d => d.Id == garmentUnitReceiptNoteItem.EPOItemId);
                        EntityExtension.FlagForUpdate(garmentExternalPurchaseOrderItem, identityService.Username, USER_AGENT);
                        garmentExternalPurchaseOrderItem.ReceiptQuantity = (double)((decimal)garmentExternalPurchaseOrderItem.ReceiptQuantity - garmentUnitReceiptNoteItem.ReceiptQuantity);

                        if(garmentExternalPurchaseOrderItem.ReceiptQuantity == 0)
                        {
                            var garmentInternalPurchaseOrderItem = dbSetGarmentInternalPurchaseOrderItems.First(d => d.Id == garmentUnitReceiptNoteItem.POItemId);
                            if (garmentExternalPurchaseOrderItem.DOQuantity>0 && garmentExternalPurchaseOrderItem.DOQuantity < garmentExternalPurchaseOrderItem.DealQuantity)
                            {
                                garmentInternalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                            } else if(garmentExternalPurchaseOrderItem.DOQuantity>0 && garmentExternalPurchaseOrderItem.DOQuantity >= garmentExternalPurchaseOrderItem.DealQuantity)
                            {
                                garmentInternalPurchaseOrderItem.Status = "Barang sudah datang semua";
                            }
                        }
                    }

                    if (garmentUnitReceiptNote.IsStorage)
                    {
                        var garmentInventoryDocument = GenerateGarmentInventoryDocument(garmentUnitReceiptNote, "OUT");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocument);

                        foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                        {
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == garmentUnitReceiptNoteItem.SmallUomId);

                            var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUnitReceiptNote, garmentUnitReceiptNoteItem, garmentInventorySummaryExisting, "OUT");
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                            if (garmentInventorySummaryExisting != null)
                            {
                                EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                                garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                            }
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

        private GarmentInventorySummary GenerateGarmentInventorySummary(GarmentUnitReceiptNote garmentUnitReceiptNote, GarmentUnitReceiptNoteItem garmentUnitReceiptNoteItem, GarmentInventoryMovement garmentInventoryMovement)
        {
            var garmentInventorySummary = new GarmentInventorySummary();
            EntityExtension.FlagForCreate(garmentInventorySummary, identityService.Username, USER_AGENT);
            do
            {
                garmentInventorySummary.No = CodeGenerator.Generate();
            }
            while (dbSetGarmentInventorySummary.Any(m => m.No == garmentInventorySummary.No));

            garmentInventorySummary.ProductId = garmentUnitReceiptNoteItem.ProductId;
            garmentInventorySummary.ProductCode = garmentUnitReceiptNoteItem.ProductCode;
            garmentInventorySummary.ProductName = garmentUnitReceiptNoteItem.ProductName;

            garmentInventorySummary.StorageId = garmentUnitReceiptNote.StorageId;
            garmentInventorySummary.StorageCode = garmentUnitReceiptNote.StorageCode;
            garmentInventorySummary.StorageName = garmentUnitReceiptNote.StorageName;

            garmentInventorySummary.Quantity = garmentInventoryMovement.After;

            garmentInventorySummary.UomId = garmentUnitReceiptNoteItem.SmallUomId;
            garmentInventorySummary.UomUnit = garmentUnitReceiptNoteItem.SmallUomUnit;

            garmentInventorySummary.StockPlanning = 0;

            return garmentInventorySummary;
        }

        private GarmentInventoryMovement GenerateGarmentInventoryMovement(GarmentUnitReceiptNote garmentUnitReceiptNote, GarmentUnitReceiptNoteItem garmentUnitReceiptNoteItem, GarmentInventorySummary garmentInventorySummary, string type = "IN")
        {
            var garmentInventoryMovement = new GarmentInventoryMovement();
            EntityExtension.FlagForCreate(garmentInventoryMovement, identityService.Username, USER_AGENT);
            do
            {
                garmentInventoryMovement.No = CodeGenerator.Generate();
            }
            while (dbSetGarmentInventoryMovement.Any(m => m.No == garmentInventoryMovement.No));

            garmentInventoryMovement.Date = garmentInventoryMovement.CreatedUtc;

            garmentInventoryMovement.ReferenceNo = garmentUnitReceiptNote.URNNo;
            garmentInventoryMovement.ReferenceType = string.Concat("Bon Terima Unit - ", garmentUnitReceiptNote.UnitName);

            garmentInventoryMovement.ProductId = garmentUnitReceiptNoteItem.ProductId;
            garmentInventoryMovement.ProductCode = garmentUnitReceiptNoteItem.ProductCode;
            garmentInventoryMovement.ProductName = garmentUnitReceiptNoteItem.ProductName;

            garmentInventoryMovement.StorageId = garmentUnitReceiptNote.StorageId;
            garmentInventoryMovement.StorageCode = garmentUnitReceiptNote.StorageCode;
            garmentInventoryMovement.StorageName = garmentUnitReceiptNote.StorageName;

            garmentInventoryMovement.StockPlanning = 0;

            garmentInventoryMovement.Before = garmentInventorySummary == null ? 0 : garmentInventorySummary.Quantity;
            garmentInventoryMovement.Quantity = garmentUnitReceiptNoteItem.SmallQuantity * ((type ?? "").ToUpper() == "OUT" ? -1 : 1);
            garmentInventoryMovement.After = garmentInventoryMovement.Before + garmentInventoryMovement.Quantity;

            garmentInventoryMovement.UomId = garmentUnitReceiptNoteItem.SmallUomId;
            garmentInventoryMovement.UomUnit = garmentUnitReceiptNoteItem.SmallUomUnit;

            garmentInventoryMovement.Remark = garmentUnitReceiptNoteItem.ProductRemark;

            garmentInventoryMovement.Type = (type ?? "").ToUpper() == "IN" ? "IN" : "OUT";

            return garmentInventoryMovement;
        }

        private GarmentInventoryDocument GenerateGarmentInventoryDocument(GarmentUnitReceiptNote garmentUnitReceiptNote, string type = "IN")
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

            garmentInventoryDocument.ReferenceNo = garmentUnitReceiptNote.URNNo;
            garmentInventoryDocument.ReferenceType = string.Concat("Bon Terima Unit - ", garmentUnitReceiptNote.UnitName);

            garmentInventoryDocument.Type = (type ?? "").ToUpper() == "IN" ? "IN" : "OUT";

            garmentInventoryDocument.StorageId = garmentUnitReceiptNote.StorageId;
            garmentInventoryDocument.StorageCode = garmentUnitReceiptNote.StorageCode;
            garmentInventoryDocument.StorageName = garmentUnitReceiptNote.StorageName;

            garmentInventoryDocument.Remark = garmentUnitReceiptNote.Remark;

            foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
            {
                var garmentInventoryDocumentItem = new GarmentInventoryDocumentItem();
                EntityExtension.FlagForCreate(garmentInventoryDocumentItem, identityService.Username, USER_AGENT);

                garmentInventoryDocumentItem.ProductId = garmentUnitReceiptNoteItem.ProductId;
                garmentInventoryDocumentItem.ProductCode = garmentUnitReceiptNoteItem.ProductCode;
                garmentInventoryDocumentItem.ProductName = garmentUnitReceiptNoteItem.ProductName;

                garmentInventoryDocumentItem.Quantity = garmentUnitReceiptNoteItem.SmallQuantity;

                garmentInventoryDocumentItem.UomId = garmentUnitReceiptNoteItem.SmallUomId;
                garmentInventoryDocumentItem.UomUnit = garmentUnitReceiptNoteItem.SmallUomUnit;

                garmentInventoryDocumentItem.ProductRemark = garmentUnitReceiptNoteItem.ProductRemark;

                garmentInventoryDocument.Items.Add(garmentInventoryDocumentItem);
            }

            return garmentInventoryDocument;
        }

        private async Task<string> GenerateNo(GarmentUnitReceiptNote garmentUnitReceiptNote)
        {
            string Year = garmentUnitReceiptNote.ReceiptDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("yy");
            string Month = garmentUnitReceiptNote.ReceiptDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("MM");
            string Day = garmentUnitReceiptNote.ReceiptDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("dd");

            string no = string.Concat("BUM", garmentUnitReceiptNote.UnitCode, Year, Month, Day);
            int Padding = 3;

            var lastNo = await dbSet.Where(w => w.URNNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.URNNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.URNNo.Replace(no, string.Empty)) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public List<object> ReadForUnitDO(string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentUnitReceiptNote> Query = dbSet;
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long unitId = 0;
            long storageId = 0;
            bool hasUnitFilter = FilterDictionary.ContainsKey("UnitId") && long.TryParse(FilterDictionary["UnitId"], out unitId);
            bool hasStorageFilter = FilterDictionary.ContainsKey("StorageId") && long.TryParse(FilterDictionary["StorageId"], out storageId);
            bool isPROSES = FilterDictionary.ContainsKey("Type") && FilterDictionary["Type"] == "PROSES";

            var readForUnitDO = Query.Where(x => 
                    (!hasUnitFilter ? true : x.UnitId == unitId) &&
                    (!hasStorageFilter ? true : x.StorageId == storageId) &&
                    x.IsDeleted == false &&
                    x.Items.Any(i => i.RONo.Contains((Keyword ?? "").Trim()) && (isPROSES && (i.RONo.EndsWith("S") || i.RONo.EndsWith("M")) ? false : true))
                )
                .SelectMany(x => x.Items
                .Where(i => i.RONo.Contains((Keyword ?? "").Trim()) && (isPROSES && (i.RONo.EndsWith("S") || i.RONo.EndsWith("M")) ? false : true))
                .Select(y => new
                {
                    x.URNNo,
                    y.URNId,
                    y.Id,
                    y.RONo,
                    y.DODetailId,
                    y.EPOItemId,
                    y.POItemId,
                    y.PRItemId,
                    y.ProductId,
                    y.ProductName,
                    y.ProductCode,
                    y.ProductRemark,
                    y.OrderQuantity,
                    y.SmallQuantity,
                    y.SmallUomId,
                    y.SmallUomUnit,
                    y.DesignColor,
                    y.POSerialNumber,
                    y.PricePerDealUnit,
                    y.ReceiptCorrection,
                    y.Conversion,
                    y.CorrectionConversion,
                    Article = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == y.EPOItemId).Select(d => d.Article).FirstOrDefault()
                })).ToList();
            var coba = readForUnitDO.GroupBy(g => g.RONo);
            var test = coba.Select(c => new
            {
                Article = c.Select(s => s.Article).FirstOrDefault(),
                RONo = c.Key,
                Items = c.ToList()
            });
            List<object> result = new List<object>(test);
            return result;
        }

        public List<object> ReadForUnitDOHeader(string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentUnitReceiptNote> Query = dbSet;
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long unitId = 0;
            long storageId = 0;
            bool hasUnitFilter = FilterDictionary.ContainsKey("UnitId") && long.TryParse(FilterDictionary["UnitId"], out unitId);
            bool isPROSES = FilterDictionary.ContainsKey("Type") && FilterDictionary["Type"] == "PROSES";
            bool hasRONoFilter = FilterDictionary.ContainsKey("RONo");
            bool hasStorageFilter = FilterDictionary.ContainsKey("StorageId") && long.TryParse(FilterDictionary["StorageId"], out storageId);
            string RONo = hasRONoFilter ? (FilterDictionary["RONo"] ?? "").Trim() : "";

            var readForUnitDO = Query.Where(x =>
                    (!hasUnitFilter ? true : x.UnitId == unitId) &&
                    (!hasStorageFilter ? true : x.StorageId == storageId) &&
                    x.IsDeleted == false &&
                    x.Items.Any(i => i.RONo.Contains((Keyword ?? "").Trim()) && (hasRONoFilter ? (i.RONo != RONo) : true))
                )
                .SelectMany(x => x.Items.Select(y => new
                {
                    x.URNNo,
                    y.URNId,
                    y.Id,
                    y.RONo,
                    y.DODetailId,
                    y.EPOItemId,
                    y.POItemId,
                    y.PRItemId,
                    y.ProductId,
                    y.ProductName,
                    y.ProductCode,
                    y.ProductRemark,
                    y.OrderQuantity,
                    y.SmallQuantity,
                    y.DesignColor,
                    y.SmallUomId,
                    y.SmallUomUnit,
                    y.POSerialNumber,
                    y.PricePerDealUnit,
                    y.ReceiptCorrection,
                    y.Conversion,
                    y.CorrectionConversion,
                    Article = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == y.EPOItemId).Select(d => d.Article).FirstOrDefault()
                })).ToList();
            List<object> result = new List<object>(readForUnitDO);
            return result;
        }

        public ReadResponse<object> ReadURNItem(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentUnitReceiptNote> Query = dbSet;
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureFilter(Query, FilterDictionary);


            List<string> searchAttributes = new List<string>()
            {
                "URNNo", "UnitName", "SupplierName", "DONo"
            };

            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureSearch(Query, searchAttributes, Keyword);



            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentUnitReceiptNote> pageable = new Pageable<GarmentUnitReceiptNote>(Query, Page - 1, Size);
            List<GarmentUnitReceiptNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            var data= Query.SelectMany(x => x.Items.Select(y => new
            {
                x.DOId,
                x.DONo,
                x.URNNo,
                y.URNId,
                y.Id,
                y.RONo,
                y.DODetailId,
                y.EPOItemId,
                y.POItemId,
                y.PRItemId,
                y.ProductId,
                y.ProductName,
                y.ProductCode,
                y.ProductRemark,
                y.OrderQuantity,
                y.SmallQuantity,
                y.DesignColor,
                y.SmallUomId,
                y.SmallUomUnit,
                y.POSerialNumber,
                y.PricePerDealUnit,
                x.DOCurrencyRate,
                y.Conversion,
                y.UomUnit,
                y.UomId,
                y.ReceiptCorrection,
                y.CorrectionConversion,
                Article = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == y.EPOItemId).Select(d => d.Article).FirstOrDefault()
            })).ToList();

            List<object> ListData = new List<object>(data);

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        #region Flow Detail Penerimaan 
        public IQueryable<FlowDetailPenerimaanViewModels> GetReportQueryFlow(DateTime? dateFrom, DateTime? dateTo, string unit, string category, int offset)

        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            var Status = new[] { "" };
            switch (category)
            {
                case "Bahan Baku":
                    Status = new[] { "FABRIC", "SUBKON" };
                    break;
                case "Bahan Pendukung":
                    Status = new[] { "APPLICATION", "BADGES", "BUNGEE CORD", "BUCKLE", "BENANG HANGTAG", "BUTTON", "COLLAR BONE", "CARE LABEL",
                    "DRAWSTRING", "ELASTIC", "EMBROIDERY", "LAIN-LAIN", "GROSS GRAIN", "GESPER", "HOOK & BAR", "HOOK & EYE",
                    "INTERLINING", "KNACKS", "ID LABEL", "LABEL", "LACE", "MESS GUSSET", "METAL IGOT", "NECK LABEL",
                    "PL PACKING", "PADDING", "PEN KRAH", "POLYCORD", "PLISKET", "POLYWOSHER", "PIPING", "PULLER","QUILTING","RIBBON","RIB","RING","STRAPPING BAND",
                    "SLEEVE HEADER", "SIZE LABEL", "SAMPLE MATERIAL", "SHOULDER PAD", "SPONGE FOAM", "SPINDLE", "STOPPER", "SEWING THREAD","TAPE / DRYTEX","TRIMMING GROOMET","TASSEL","VELCRO","VITTER BAND",
                    "WADDING", "WAPPEN", "WRAPBAND", "WASH", "ZIPPER","PROCESS",
                    };
                    break;
                case "Bahan Embalase":
                    Status = new[] { "ATTENTION NAME", "POLYBAG", "BACK CB", "BENANG KENUR", "BELT", "BIS NAME","BEARING STAMP","BUTTERFLY","CABLE TIES",
                        "COLLAR CB", "CUFF STUD", "CLIPS", "DOCUMENT", "DOLL", "LAIN - LAIN","FOAM HANGER","FELT","GADGET","GLUE","GARMENT",
                        "HANDLING", "HANGER", "HOOK", "HEAT TRANSFER", "ISOLASI", "INNER BOX","STAMPED INK","INSERT TAG","KLEM SENG","KARET GELANG","LACKBAND",
                        "LICENSE SEAL", "LOOP", "INSERT CD/LAYER", "MACHINE", "MOULD", "METAL SLIDER","OUTER BOX","PLASTIC COLLAR","PIN","PLASTIC","PALLET",
                        "PAPER", "PRINT", "TALI", "SILICA BAG", "SHAVING", "SILICA GEL","GARMENT SAMPLE","SHIPPING MARK","STUDS TRANSFER","SEAL TAG","STICKER",
                        "STAMP", "STRING", "STATIONARY", "SWATCH CARD", "SIZE CHIP", "TAG","GARMENT TEST","TIE / DASI","TISSUE PAPER","TIGER TAIL",
                    };
                    break;
                default:
                    Status = new[] { "" };
                    break;
            }

            List<FlowDetailPenerimaanViewModels> Data = new List<FlowDetailPenerimaanViewModels>();

            var Query = (from a in dbContext.GarmentUnitReceiptNotes
                         join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                         join c in dbContext.GarmentInternalPurchaseOrders on b.POId equals c.Id
                         join d in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals d.Id
                         join e in dbContext.GarmentDeliveryOrderItems on d.GarmentDOItemId equals e.Id
                         join f in dbContext.GarmentDeliveryOrders on e.GarmentDOId equals f.Id


                         where a.IsDeleted == false
                            && b.IsDeleted == false
                            && c.IsDeleted == false
                            && d.IsDeleted == false
                            && e.IsDeleted == false
                            && f.IsDeleted == false
                            && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                            && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                            && a.UnitCode == (string.IsNullOrWhiteSpace(unit) ? a.UnitCode : unit)
                            && (category == "Bahan Baku" ? Status.Contains(b.ProductName) : (category == "Bahan Pendukung" ? Status.Contains(b.ProductName) : (category == "Bahan Embalase" ? Status.Contains(b.ProductName) : b.ProductName == b.ProductName)))

                         select new FlowDetailPenerimaanViewModels
                         {
                             kdbarang = b.ProductCode,
                             nmbarang = b.ProductName,
                             nopo = b.POSerialNumber,
                             keterangan = b.ProductRemark,
                             noro = b.RONo,
                             artikel = c.Article,
                             kdbuyer = c.BuyerCode,
                             nobukti = a.URNNo,
                             tanggal = a.CreatedUtc,
                             jumlahbeli = d.DOQuantity,
                             satuanbeli = d.SmallUomUnit,
                             jumlahterima = decimal.ToDouble(b.ReceiptQuantity),
                             satuanterima = b.SmallUomUnit,
                             jumlah = f.DOCurrencyRate.GetValueOrDefault() * decimal.ToDouble(b.PricePerDealUnit) * decimal.ToDouble(b.ReceiptQuantity),



                         });
            var index = 1;
            foreach (var item in Query)
            {

                Data.Add(
                       new FlowDetailPenerimaanViewModels
                       {

                           no = index++,
                           kdbarang = item.kdbarang,
                           nmbarang = item.nmbarang,
                           nopo = item.nopo,
                           keterangan = item.keterangan,
                           noro = item.noro,
                           artikel = item.artikel,
                           kdbuyer = item.kdbuyer,
                           asal = "Pembelian Eksternal",
                           nobukti = item.nobukti,
                           tanggal = item.tanggal,
                           jumlahbeli = item.jumlahbeli,
                           satuanbeli = item.satuanbeli,
                           jumlahterima = item.jumlahterima,
                           satuanterima = item.satuanterima,
                           jumlah = item.jumlah,


                       });

            }

            return Data.AsQueryable();
        }


        public Tuple<List<FlowDetailPenerimaanViewModels>, int> GetReportFlow(DateTime? dateFrom, DateTime? dateTo, string unit, string category, int page, int size, string Order, int offset)

        {
            var Query = GetReportQueryFlow(dateFrom, dateTo, unit, category, offset);


            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderBy(b => b.no).ThenBy(b => b.no);
            }


            Pageable<FlowDetailPenerimaanViewModels> pageable = new Pageable<FlowDetailPenerimaanViewModels>(Query, page - 1, size);
            List<FlowDetailPenerimaanViewModels> Data = pageable.Data.ToList<FlowDetailPenerimaanViewModels>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }


        public MemoryStream GenerateExcelLow(DateTime? dateFrom, DateTime? dateTo, string unit, string category, int offset)
        {
            var Query = GetReportQueryFlow(dateFrom, dateTo, unit, category, offset);

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No R/O", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Artikel", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Buyer", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Asal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Bukti", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Beli", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Beli", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Terima", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Terima", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah", DataType = typeof(String) });


            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            if (Query.ToArray().Count() == 0)
            {
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            }
            else
            {
                int index = 0;
                foreach (FlowDetailPenerimaanViewModels data in Query)
                {
                    index++;
                    string tgl = data.tanggal == null ? "-" : data.tanggal.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    result.Rows.Add(index, data.kdbarang, data.nmbarang, data.nopo, data.keterangan, data.noro, data.artikel, data.kdbuyer, data.asal, data.nobukti, tgl, data.jumlahbeli, data.satuanbeli, data.jumlahterima, data.satuanterima, data.jumlah);

                }

            }

            return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
        }

        #endregion
    }
}
