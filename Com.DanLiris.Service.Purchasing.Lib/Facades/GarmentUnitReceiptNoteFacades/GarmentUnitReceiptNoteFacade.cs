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

            mapper = (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentUnitReceiptNote> Query = dbSet;

            Query = Query.Select(m => new GarmentUnitReceiptNote
            {
                Id = m.Id,
                URNNo = m.URNNo,
                UnitName = m.UnitName,
                ReceiptDate = m.ReceiptDate,
                SupplierName = m.SupplierName,
                DONo = m.DONo,
                Items = m.Items.Select(i => new GarmentUnitReceiptNoteItem
                {
                    Id = i.Id,
                    RONo = i.RONo
                }).ToList(),
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "URNNo", "UnitName", "SupplierName", "DONo"
            };

            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentUnitReceiptNote>.ConfigureFilter(Query, FilterDictionary);

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
                Unit = new { Name = s.UnitName },
                s.ReceiptDate,
                Supplier = new { Name = s.SupplierName },
                s.DONo,
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

                    foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    {
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

                        var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == garmentUnitReceiptNoteItem.UomId);

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
                            var oldGarmentInventorySummary = dbSetGarmentInventorySummary.Single(s => s.ProductId == oldGarmentUnitReceiptNoteItem.ProductId && s.StorageId == oldGarmentUnitReceiptNote.StorageId && s.UomId == oldGarmentUnitReceiptNoteItem.UomId);

                            var garmentInventoryMovementOut = GenerateGarmentInventoryMovement(oldGarmentUnitReceiptNote, oldGarmentUnitReceiptNoteItem, oldGarmentInventorySummary, "OUT");
                            dbSetGarmentInventoryMovement.Add(garmentInventoryMovementOut);

                            EntityExtension.FlagForUpdate(oldGarmentInventorySummary, identityService.Username, USER_AGENT);
                            oldGarmentInventorySummary.Quantity = garmentInventoryMovementOut.After;

                            // Buat IN untuk Gudang yang baru
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == oldGarmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == oldGarmentUnitReceiptNoteItem.UomId);

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

        public async Task<int> Delete(int id)
        {
            int Deleted = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var garmentUnitReceiptNote = dbSet.Include(m => m.Items).Single(m => m.Id == id);

                    EntityExtension.FlagForDelete(garmentUnitReceiptNote, identityService.Username, USER_AGENT);

                    foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    {
                        EntityExtension.FlagForDelete(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);

                        var garmentDeliveryOrderDetail = dbSetGarmentDeliveryOrderDetail.First(d => d.Id == garmentUnitReceiptNoteItem.DODetailId);
                        EntityExtension.FlagForUpdate(garmentDeliveryOrderDetail, identityService.Username, USER_AGENT);
                        garmentDeliveryOrderDetail.ReceiptQuantity = (double)((decimal)garmentDeliveryOrderDetail.ReceiptQuantity - garmentUnitReceiptNoteItem.ReceiptQuantity);

                        var garmentExternalPurchaseOrderItem = dbSetGarmentExternalPurchaseOrderItems.First(d => d.Id == garmentUnitReceiptNoteItem.EPOItemId);
                        EntityExtension.FlagForUpdate(garmentExternalPurchaseOrderItem, identityService.Username, USER_AGENT);
                        garmentExternalPurchaseOrderItem.ReceiptQuantity = (double)((decimal)garmentExternalPurchaseOrderItem.ReceiptQuantity - garmentUnitReceiptNoteItem.ReceiptQuantity);
                    }

                    if (garmentUnitReceiptNote.IsStorage)
                    {
                        var garmentInventoryDocument = GenerateGarmentInventoryDocument(garmentUnitReceiptNote, "OUT");
                        dbSetGarmentInventoryDocument.Add(garmentInventoryDocument);

                        foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                        {
                            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.FirstOrDefault(s => s.ProductId == garmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == garmentUnitReceiptNoteItem.UomId);

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
            while (dbSetGarmentInventorySummary.Any(m => m.No == garmentInventoryMovement.No));

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
                    y.SmallUomId,
                    y.SmallUomUnit,
                    y.DesignColor,
                    y.POSerialNumber,
                    y.PricePerDealUnit,
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
                    x.Items.Any(i => i.RONo.Contains((Keyword ?? "").Trim()) && (hasRONoFilter ? (i.RONo != RONo) : true) && (isPROSES && (i.RONo.EndsWith("S") || i.RONo.EndsWith("M")) ? false : true))
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
                    Article = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == y.EPOItemId).Select(d => d.Article).FirstOrDefault()
                })).ToList();
            List<object> result = new List<object>(readForUnitDO);
            return result;
        }
    }
}
