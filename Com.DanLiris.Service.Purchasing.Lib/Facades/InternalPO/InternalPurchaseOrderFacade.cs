using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO
{
    public class InternalPurchaseOrderFacade 
    {
        private List<InternalPurchaseOrder> DUMMY_DATA = new List<InternalPurchaseOrder>()
        {
            new InternalPurchaseOrder()
            {
                Id = 1,
                Active = true,
                PONo = "ABC123",
                IsoNo = "",
                PRId = "PurchaseRequestId-1",
                PRNo = "PurchaseRequestNo-1",
                PRDate = DateTimeOffset.UtcNow,
                ExpectedDeliveryDate = DateTimeOffset.UtcNow,
                BudgetId = "BudgetId-1",
                BudgetCode = "BudgetCode-1",
                BudgetName = "BudgetName-1",
                CategoryId = "CategoryId-1",
                CategoryCode = "CategoryCode-1",
                CategoryName = "CategoryName-1",
                CreatedAgent = "Dummy-1",
                CreatedBy = "Dummy-1",
                CreatedUtc = DateTime.UtcNow,
                DeletedAgent = "",
                DeletedBy = "",
                DivisionId = "DivisionId-1",
                DivisionCode = "DivisionCode-1",
                DivisionName = "DivisionName-1",
                IsDeleted = false,
                IsPosted = false,
                IsClosed = false,
                LastModifiedAgent = "Dummy-1",
                LastModifiedBy = "Dummy-1",
                LastModifiedUtc = DateTime.UtcNow,
                Remark = "Remark-1",
                Status = "",
                UId = "8ad231fk1049201da",
                UnitId = "UnitId-1",
                UnitCode = "UnitCode-1",
                UnitName = "UnitName-1",
                Items = new List<InternalPurchaseOrderItem>()
                {
                    new InternalPurchaseOrderItem()
                    {
                        Id = 1,
                        Active = true,
                        IsDeleted = false,
                        POId = 1,
                        PRItemId = "PurchaseRequestItem-1",
                        CreatedAgent = "Dummy-1",
                        CreatedBy = "Dummy-1",
                        CreatedUtc = DateTime.UtcNow,
                        LastModifiedAgent = "Dummy-1",
                        LastModifiedBy = "Dummy-1",
                        LastModifiedUtc = DateTime.UtcNow,
                        DeletedAgent = "",
                        DeletedBy = "",
                        ProductId = "ProductId-1",
                        ProductCode = "ProductCode-1",
                        ProductName = "ProductName-1",
                        Quantity = 10,
                        ProductRemark = "Remark-1",
                        UomId = "58662db1f28e81002db4b234",
                        UomUnit = "KWT",
                        Status = "",

                    },
                    new InternalPurchaseOrderItem()
                    {
                        Id = 2,
                        Active = true,
                        IsDeleted = false,
                        POId = 1,
                        PRItemId = "PurchaseRequestItem-1",
                        CreatedAgent = "Dummy-1",
                        CreatedBy = "Dummy-1",
                        CreatedUtc = DateTime.UtcNow,
                        LastModifiedAgent = "Dummy-1",
                        LastModifiedBy = "Dummy-1",
                        LastModifiedUtc = DateTime.UtcNow,
                        DeletedAgent = "",
                        DeletedBy = "",
                        ProductId = "ProductId-2",
                        ProductCode = "ProductCode-2",
                        ProductName = "ProductName-2",
                        Quantity = 10,
                        ProductRemark = "Remark-2",
                        UomId = "5869471df28e81002db4d332",
                        UomUnit = "PIL",
                        Status = "",
                    }
                }
            },
            new InternalPurchaseOrder()
            {
                Id = 2,
                Active = true,
                PONo = "ABC123",
                IsoNo = "",
                PRId = "PurchaseRequestId-2",
                PRNo = "PurchaseRequestNo-2",
                PRDate = DateTimeOffset.UtcNow,
                ExpectedDeliveryDate = DateTimeOffset.UtcNow,
                BudgetId = "BudgetId-2",
                BudgetCode = "BudgetCode-2",
                BudgetName = "BudgetName-2",
                CategoryId = "CategoryId-2",
                CategoryCode = "CategoryCode-2",
                CategoryName = "CategoryName-2",
                CreatedAgent = "Dummy-1",
                CreatedBy = "Dummy-1",
                CreatedUtc = DateTime.UtcNow,
                DeletedAgent = "",
                DeletedBy = "",
                DivisionId = "DivisionId-2",
                DivisionCode = "DivisionCode-2",
                DivisionName = "DivisionName-2",
                IsDeleted = false,
                IsPosted = false,
                IsClosed = false,
                LastModifiedAgent = "Dummy-1",
                LastModifiedBy = "Dummy-1",
                LastModifiedUtc = DateTime.UtcNow,
                Remark = "Remark-2",
                Status = "",
                UId = "8ad231fk1049201da",
                UnitId = "UnitId-2",
                UnitCode = "UnitCode-2",
                UnitName = "UnitName-2",
                Items = new List<InternalPurchaseOrderItem>()
                {
                    new InternalPurchaseOrderItem()
                    {
                        Id = 3,
                        Active = true,
                        IsDeleted = false,
                        POId = 2,
                        PRItemId = "PurchaseRequestItem-2",
                        CreatedAgent = "Dummy-1",
                        CreatedBy = "Dummy-1",
                        CreatedUtc = DateTime.UtcNow,
                        LastModifiedAgent = "Dummy-1",
                        LastModifiedBy = "Dummy-1",
                        LastModifiedUtc = DateTime.UtcNow,
                        DeletedAgent = "",
                        DeletedBy = "",
                        ProductId = "ProductId-2",
                        ProductCode = "ProductCode-2",
                        ProductName = "ProductName-2",
                        Quantity = 10,
                        ProductRemark = "Remark-2",
                        UomId = "58662db1f28e81002db4b234",
                        UomUnit = "KWT",
                        Status = "",
                    },
                    new InternalPurchaseOrderItem()
                    {
                        Id = 3,
                        Active = true,
                        IsDeleted = false,
                        POId = 2,
                        PRItemId = "PurchaseRequestItem-2",
                        CreatedAgent = "Dummy-1",
                        CreatedBy = "Dummy-1",
                        CreatedUtc = DateTime.UtcNow,
                        LastModifiedAgent = "Dummy-1",
                        LastModifiedBy = "Dummy-1",
                        LastModifiedUtc = DateTime.UtcNow,
                        DeletedAgent = "",
                        DeletedBy = "",
                        ProductId = "ProductId-2",
                        ProductCode = "ProductCode-2",
                        ProductName = "ProductName-2",
                        Quantity = 10,
                        ProductRemark = "Remark-2",
                        UomId = "5869471df28e81002db4d332",
                        UomUnit = "PIL",
                        Status = "",
                    }
                }
            }
        };

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<InternalPurchaseOrder> dbSet;

        public InternalPurchaseOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<InternalPurchaseOrder>();
        }

        //public List<InternalPurchaseOrderViewModel> Read()
        //{
        //    return mapper.Map<List<InternalPurchaseOrderViewModel>>(DUMMY_DATA);
        //}

        public InternalPurchaseOrder ReadById(int id)
        {
            return this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
        }

        async Task<string> GeneratePONo(InternalPurchaseOrder model)
        {
            DateTime Now = DateTime.Now;
            string Year = Now.ToString("yy");
            string Month = Now.ToString("MM");

            string internalPurchaseNo = Year + Month;

            var lastInternalPurchaseNo = await this.dbSet.Where(w => w.PONo.StartsWith(internalPurchaseNo)).OrderByDescending(o => o.PONo).FirstOrDefaultAsync();

            int Padding = 5;

            if (lastInternalPurchaseNo == null)
            {
                return internalPurchaseNo + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNo = Int32.Parse(lastInternalPurchaseNo.PONo.Replace(internalPurchaseNo, "")) + 1;
                return internalPurchaseNo + lastNo.ToString().PadLeft(Padding, '0');
            }
        }

        public async Task<int> Create(InternalPurchaseOrder m, string user)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, "Facade");
                    m.PONo = await this.GeneratePONo(m);
                    m.PONo = "PO" + m.UnitCode + m.PONo;
                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, "Facade");
                    }

                    this.dbContext.InternalPurchaseOrders.Add(m);
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

        public Tuple<List<InternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<InternalPurchaseOrder> Query = this.dbSet;
            //IQueryable<PurchaseRequest> Query = DUMMY_DATA.AsQueryable();

            Query = Query.Select(s => new InternalPurchaseOrder
            {
                Id = s.Id,
                UId = s.UId,
                PONo = s.PONo,
                PRNo = s.PRNo,
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                UnitName = s.UnitName,
                DivisionName = s.DivisionName,
                CategoryName = s.CategoryName,
                IsPosted = s.IsPosted,
                CreatedBy = s.CreatedBy,
                PRDate = s.PRDate,
                LastModifiedUtc = s.LastModifiedUtc
                //Items = s.Items
                //    .Select(
                //        q => new InternalPurchaseOrderItem
                //        {
                //            Id = q.Id,
                //            POId = q.POId,
                //            PRItemId = q.PRItemId,
                //            ProductId =q.ProductId,
                //            ProductName = q.ProductName,
                //            ProductCode = q.ProductCode,
                //            UomId = q.UomId,
                //            UomUnit = q.UomUnit,
                //            Quantity = q.Quantity,
                //            ProductRemark = q.ProductRemark,
                //            Status = q.Status
                //        }
                //    )
                //    .Where(j => j.POId.Equals(s.Id))
                //    .ToList()
            });

            if (Keyword != null)
            {
                Query = Query.Where("UnitName.Contains(@0)", Keyword);
            }

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<InternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<InternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<InternalPurchaseOrder> pageable = new Pageable<InternalPurchaseOrder>(Query, Page - 1, Size);
            List<InternalPurchaseOrder> Data = pageable.Data.ToList<InternalPurchaseOrder>();
            int TotalData = pageable.TotalCount;

            //var newData = mapper.Map<List<InternalPurchaseOrderViewModel>>(Data);

            //List<object> list = new List<object>();
            //list.AddRange(
            //    newData.AsQueryable().Select(s => new
            //    {
            //        s._id,
            //        s.prNo,
            //        s.poNo,
            //        s.expectedDeliveryDate,
            //        unit = new
            //        {
            //            division = new { s.unit.division.name },
            //            s.unit.name
            //        },
            //        category = new { s.category.name },
            //        s.isPosted,
            //    }).ToList()
            //);

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public async Task<int> Update(int id, InternalPurchaseOrder internalPurchaseOrder, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .Single(pr => pr.Id == id && !pr.IsDeleted);

                    if (m != null)
                    {

                        EntityExtension.FlagForUpdate(internalPurchaseOrder, user, "Facade");

                        foreach (var item in internalPurchaseOrder.Items)
                        {
                            EntityExtension.FlagForUpdate(item, user, "Facade");
                        }

                        this.dbContext.Update(internalPurchaseOrder);
                        Updated = await dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception("Error while updating data");
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public int Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(m, user, "Facade");

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForDelete(item, user, "Facade");
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

        //public async Task<int> Split(int id, InternalPurchaseOrder internalPurchaseOrder, string user)
        //{
        //    int Splitted = 0;

        //    using (var transaction = this.dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var m = this.dbSet.AsNoTracking()
        //                .Include(d => d.Items)
        //                .Single(pr => pr.Id == id && !pr.IsDeleted);

        //            if (m != null)
        //            {

        //                EntityExtension.FlagForUpdate(internalPurchaseOrder, user, "Facade");

        //                foreach (var item in internalPurchaseOrder.Items)
        //                {
        //                    EntityExtension.FlagForUpdate(item, user, "Facade");
        //                }

        //                this.dbContext.Update(internalPurchaseOrder);
        //                Splitted = await dbContext.SaveChangesAsync();
        //                transaction.Commit();
        //            }
        //            else
        //            {
        //                throw new Exception("Error while updating data");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            transaction.Rollback();
        //            throw new Exception(e.Message);
        //        }
        //    }

        //    return Splitted;
        //}
    }
}
