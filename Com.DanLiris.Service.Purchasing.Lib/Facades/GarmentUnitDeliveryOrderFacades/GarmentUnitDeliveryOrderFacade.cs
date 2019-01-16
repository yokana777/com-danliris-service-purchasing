using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderFacades
{
    public class GarmentUnitDeliveryOrderFacade : IGarmentUnitDeliveryOrder
    {
        private string USER_AGENT = "Facade";
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentUnitDeliveryOrder> dbSet;
        private readonly IMapper mapper;

        public GarmentUnitDeliveryOrderFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.serviceProvider = serviceProvider;
            dbSet = dbContext.Set<GarmentUnitDeliveryOrder>();
            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public Tuple<List<GarmentUnitDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentUnitDeliveryOrder> Query = dbSet;

            Query = Query.Select(m => new GarmentUnitDeliveryOrder
            {
                Id = m.Id,
                UnitDONo = m.UnitDONo,
                UnitDODate = m.UnitDODate,
                UnitDOType = m.UnitDOType,
                UnitRequestCode = m.UnitRequestCode,
                UnitRequestName = m.UnitRequestName,
                UnitSenderCode = m.UnitSenderCode,
                UnitSenderName = m.UnitSenderName,
                StorageName = m.StorageName,
                StorageCode = m.StorageCode,
                StorageRequestCode = m.StorageRequestCode,
                StorageRequestName = m.StorageRequestName,
                IsUsed = m.IsUsed,
                RONo = m.RONo,
                Article = m.Article,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "UnitDONo", "RONo", "UnitDOType", "Article","UnitDODate","UnitRequestName","StorageName"
            };

            Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentUnitDeliveryOrder> pageable = new Pageable<GarmentUnitDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentUnitDeliveryOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentUnitDeliveryOrder ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }
        public async Task<int> Create(GarmentUnitDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    m.UnitDONo = await GenerateNo(m, clientTimeZoneOffset);

                    foreach (var item in m.Items)
                    {
                        GarmentUnitReceiptNoteItem garmentUnitReceiptNote = this.dbContext.GarmentUnitReceiptNoteItems.FirstOrDefault(s => s.Id == item.URNItemId);
                        if (garmentUnitReceiptNote != null)
                            garmentUnitReceiptNote.OrderQuantity = garmentUnitReceiptNote.OrderQuantity + (decimal)item.Quantity;
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                    }

                    this.dbSet.Add(m);

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

        public int Delete(int id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                                .Include(m => m.Items)
                                .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);
                    foreach (var item in model.Items)
                    {
                        GarmentUnitReceiptNoteItem garmentUnitReceiptNote = this.dbContext.GarmentUnitReceiptNoteItems.FirstOrDefault(s => s.Id == item.URNItemId);
                        if (garmentUnitReceiptNote != null)
                            garmentUnitReceiptNote.OrderQuantity = garmentUnitReceiptNote.OrderQuantity - (decimal)item.Quantity;
                        EntityExtension.FlagForDelete(item, username, USER_AGENT);
                    }

                    Deleted = dbContext.SaveChanges();
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

        public Task<int> Update(int id, GarmentUnitDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            throw new NotImplementedException();
        }

        async Task<string> GenerateNo(GarmentUnitDeliveryOrder model, int clientTimeZoneOffset)
        {
            GarmentUnitReceiptNote garmentUnitReceiptNote = (from data in dbContext.GarmentUnitReceiptNotes select data).FirstOrDefault();
            DateTimeOffset dateTimeOffsetNow = garmentUnitReceiptNote.ReceiptDate;
            string Month = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM");
            string Year = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy");
            string Day = dateTimeOffsetNow.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd");

            string no = string.Concat("DO",garmentUnitReceiptNote.UnitCode,Year,Month,Day);
            int Padding = 4;

            var lastNo = await this.dbSet.Where(w => w.UnitDONo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.UnitDONo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                //int lastNoNumber = Int32.Parse(lastNo.INNo.Replace(no, "")) + 1;
                int.TryParse(lastNo.UnitDONo.Replace(no, ""), out int lastno1);
                int lastNoNumber = lastno1 + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public ReadResponse<object> ReadForUnitExpenditureNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            IQueryable<GarmentUnitDeliveryOrder> Query = dbSet
                .Select(m => new GarmentUnitDeliveryOrder
                {
                    Id = m.Id,
                    UnitDONo = m.UnitDONo,
                    UnitDOType = m.UnitDOType,
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
                    IsUsed = m.IsUsed,
                    LastModifiedUtc = m.LastModifiedUtc,
                    Items = m.Items.Select(i => new GarmentUnitDeliveryOrderItem
                    {
                        Id = i.Id,
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
                        URNItemId = i.URNItemId,
                        UomId = i.UomId,
                        UomUnit = i.UomUnit,
                        FabricType = i.FabricType,
                        DesignColor = i.DesignColor
                    }).ToList()
                });

            Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentUnitDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentUnitDeliveryOrder> pageable = new Pageable<GarmentUnitDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentUnitDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentUnitDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentUnitDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.UnitDONo,
                    s.UnitDOType,
                    s.IsUsed,
                    s.Storage,
                    s.StorageRequest,
                    s.UnitRequest,
                    s.UnitSender,
                    s.CreatedBy,
                    s.LastModifiedUtc,
                    Items = s.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.ProductCode,
                        i.ProductName,
                        i.ProductRemark,
                        i.Quantity,
                        i.DODetailId,
                        i.EPOItemId,
                        i.FabricType,
                        i.PricePerDealUnit,
                        i.POSerialNumber,
                        i.POItemId,
                        i.PRItemId,
                        i.UomId,
                        i.UomUnit,
                        i.RONo,
                        i.URNItemId,
                        i.DesignColor,
                        Buyer = new
                        {
                            Id = dbContext.GarmentInternalPurchaseOrders.Where(m => m.Items.Any(k => k.Id == i.POItemId)).Select(m => m.BuyerId).FirstOrDefault(),
                            Code = dbContext.GarmentInternalPurchaseOrders.Where(m => m.Items.Any(k => k.Id == i.POItemId)).Select(m => m.BuyerCode).FirstOrDefault()
                        },
                    }).ToList()
                }).ToList()
            );
            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }
    }
}
