using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades
{
    public class PurchaseRequestFacade
    {
        #region DUMMY_DATA
        private List<PurchaseRequest> DUMMY_DATA = new List<PurchaseRequest>()
        {
            new PurchaseRequest()
            {
                Id = 1,
                Active = true,
                BudgetId = "BudgetId-1",
                BudgetCode = "BudgetCode-1",
                BudgetName = "BudgetName-1",
                CategoryId = "CategoryId-1",
                CategoryCode = "CategoryCode-1",
                CategoryName = "CategoryName-1",
                CreatedAgent = "Dummy-1",
                CreatedBy = "Dummy-1",
                CreatedUtc = DateTime.UtcNow,
                Date = DateTimeOffset.UtcNow,
                DeletedAgent = "",
                DeletedBy = "",
                DivisionId = "DivisionId-1",
                DivisionCode = "DivisionCode-1",
                DivisionName = "DivisionName-1",
                ExpectedDeliveryDate = DateTimeOffset.UtcNow,
                Internal = false,
                IsDeleted = false,
                IsPosted = false,
                IsUsed = false,
                LastModifiedAgent = "Dummy-1",
                LastModifiedBy = "Dummy-1",
                LastModifiedUtc = DateTime.UtcNow,
                No = "No-1",
                Remark = "Remark-1",
                Status = Enums.PurchaseRequestStatus.CREATED,
                UId = "8ad231fk1049201da",
                UnitId = "UnitId-1",
                UnitCode = "UnitCode-1",
                UnitName = "UnitName-1",
                Items = new List<PurchaseRequestItem>()
                {
                    new PurchaseRequestItem()
                    {
                        Id = 1,
                        Active = true,
                        IsDeleted = false,
                        PurchaseRequestId = 1,
                        CreatedAgent = "Dummy-1",
                        CreatedBy = "Dummy-1",
                        CreatedUtc = DateTime.UtcNow,
                        LastModifiedAgent = "Dummy-1",
                        LastModifiedBy = "Dummy-1",
                        LastModifiedUtc = DateTime.UtcNow.AddDays(1),
                        DeletedAgent = "",
                        DeletedBy = "",
                        ProductId = "ProductId-1",
                        ProductCode = "ProductCode-1",
                        ProductName = "ProductName-1",
                        Quantity = 10,
                        Remark = "Remark-1",
                        Uom = "MTR"
                    },
                    new PurchaseRequestItem()
                    {
                        Id = 2,
                        Active = true,
                        IsDeleted = false,
                        PurchaseRequestId = 1,
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
                        Remark = "Remark-2",
                        Uom = "PCS"
                    }
                }
            },
            new PurchaseRequest()
            {
                Id = 2,
                Active = true,
                BudgetId = "BudgetId-2",
                BudgetCode = "BudgetCode-2",
                BudgetName = "BudgetName-2",
                CategoryId = "CategoryId-2",
                CategoryCode = "CategoryCode-2",
                CategoryName = "CategoryName-2",
                CreatedAgent = "Dummy-2",
                CreatedBy = "Dummy-2",
                CreatedUtc = DateTime.UtcNow,
                Date = DateTimeOffset.UtcNow,
                DeletedAgent = "",
                DeletedBy = "",
                DivisionId = "DivisionId-2",
                DivisionCode = "DivisionCode-2",
                DivisionName = "DivisionName-2",
                ExpectedDeliveryDate = DateTimeOffset.UtcNow,
                Internal = true,
                IsDeleted = false,
                IsPosted = false,
                IsUsed = false,
                LastModifiedAgent = "Dummy-2",
                LastModifiedBy = "Dummy-2",
                LastModifiedUtc = DateTime.UtcNow,
                No = "No-2",
                Remark = "Remark-2",
                Status = Enums.PurchaseRequestStatus.CREATED,
                UId = "8ad231fk1049201daf32",
                UnitId = "UnitId-2",
                UnitCode = "UnitCode-2",
                UnitName = "UnitName-2",
                Items = new List<PurchaseRequestItem>()
                {
                    new PurchaseRequestItem()
                    {
                        Id = 3,
                        Active = true,
                        IsDeleted = false,
                        PurchaseRequestId = 2,
                        CreatedAgent = "Dummy-3",
                        CreatedBy = "Dummy-3",
                        CreatedUtc = DateTime.UtcNow,
                        LastModifiedAgent = "Dummy-3",
                        LastModifiedBy = "Dummy-3",
                        LastModifiedUtc = DateTime.UtcNow,
                        DeletedAgent = "",
                        DeletedBy = "",
                        ProductId = "ProductId-3",
                        ProductCode = "ProductCode-3",
                        ProductName = "ProductName-3",
                        Quantity = 10,
                        Remark = "Remark-3",
                        Uom = "BUAH"
                    },
                    new PurchaseRequestItem()
                    {
                        Id = 4,
                        Active = true,
                        IsDeleted = false,
                        PurchaseRequestId = 2,
                        CreatedAgent = "Dummy-4",
                        CreatedBy = "Dummy-4",
                        CreatedUtc = DateTime.UtcNow,
                        LastModifiedAgent = "Dummy-4",
                        LastModifiedBy = "Dummy-4",
                        LastModifiedUtc = DateTime.UtcNow,
                        DeletedAgent = "",
                        DeletedBy = "",
                        ProductId = "ProductId-4",
                        ProductCode = "ProductCode-4",
                        ProductName = "ProductName-4",
                        Quantity = 10,
                        Remark = "Remark-4",
                        Uom = "P"
                    }
                }
            }
        };
        #endregion

        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<PurchaseRequest> dbSet;

        public PurchaseRequestFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<PurchaseRequest>();
        }

        //public List<PurchaseRequestViewModel> Read()
        //{
        //    return mapper.Map<List<PurchaseRequestViewModel>>(DUMMY_DATA);
        //}

        public Tuple<List<PurchaseRequest>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<PurchaseRequest> Query = this.dbSet;

            Query = Query.Select(s => new PurchaseRequest
            {
                Id = s.Id,
                UId = s.UId,
                No = s.No,
                Date = s.Date,
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                UnitName = s.UnitName,
                DivisionName = s.DivisionName,
                CategoryName = s.CategoryName,
                IsPosted = s.IsPosted,
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "No", "UnitName", "CategoryName", "DivisionName"
            };

            Query = QueryHelper<PurchaseRequest>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<PurchaseRequest>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<PurchaseRequest>.ConfigureOrder(Query, OrderDictionary);

            Pageable<PurchaseRequest> pageable = new Pageable<PurchaseRequest>(Query, Page - 1, Size);
            List<PurchaseRequest> Data = pageable.Data.ToList<PurchaseRequest>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public PurchaseRequest ReadById(int id)
        {
            var a = this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
            return a;
        }

        //public int Create(PurchaseRequest m)
        //{
        //    int Result = 0;

        //    /* TODO EF Operation */

        //    Result = 1;

        //    return Result;
        //}

        public async Task<int> Create(PurchaseRequest m, string user)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    m.No = await GenerateNo(m);

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                        item.Status = "Belum diterima Pembelian";
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

        public async Task<int> Update(int id, PurchaseRequest purchaseRequest, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .Single(pr => pr.Id == id && !pr.IsDeleted);

                    if (m != null && !id.Equals(purchaseRequest.Id))
                    {

                        EntityExtension.FlagForUpdate(purchaseRequest, user, USER_AGENT);

                        foreach (var item in purchaseRequest.Items)
                        {
                            if (item.Id == 0)
                            {
                                EntityExtension.FlagForCreate(item, user, USER_AGENT);
                            }
                            EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                        }

                        this.dbContext.Update(purchaseRequest);

                        foreach (var item in m.Items)
                        {
                            PurchaseRequestItem purchaseRequestItem = purchaseRequest.Items.FirstOrDefault(i => i.Id.Equals(item.Id));
                            if (purchaseRequestItem == null)
                            {
                                EntityExtension.FlagForDelete(item, user, USER_AGENT);
                                this.dbContext.PurchaseRequestItems.Update(item);
                            }
                        }

                        Updated = await dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception("Invalid Id");
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

                    EntityExtension.FlagForDelete(m, user, USER_AGENT);

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForDelete(item, user, USER_AGENT);
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

        public int PRPost(List<PurchaseRequest> ListPurchaseRequest, string user)
        {
            int Updated = 0;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var Ids = ListPurchaseRequest.Select(d => d.Id).ToList();
                    var listData = this.dbSet
                        .Where(m => Ids.Contains(m.Id) && !m.IsDeleted)
                        .Include(d => d.Items)
                        .ToList();
                    listData.ForEach(m =>
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                        m.IsPosted = true;

                        foreach (var item in m.Items)
                        {
                            EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                        }
                    });

                    Updated = dbContext.SaveChanges();
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

        public int PRUnpost(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                    m.IsPosted = false;

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                    }

                    Updated = dbContext.SaveChanges();
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

        async Task<string> GenerateNo(PurchaseRequest model)
        {
            DateTime Now = DateTime.Now;
            string Year = Now.ToString("yy");
            string Month = Now.ToString("MM");

            string no = $"PR-{model.BudgetCode}-{model.UnitCode}-{model.CategoryCode}-{Year}-{Month}-";
            int Padding = 3;

            var lastNo = await this.dbSet.Where(w => w.No.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.No).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.No.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public Tuple<List<PurchaseRequest>, int, Dictionary<string, string>> ReadModelPosted(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<PurchaseRequest> Query = this.dbSet;

            Query = Query.Select(s => new PurchaseRequest
            {
                Id = s.Id,
                UId = s.UId,
                No = s.No,
                Date = s.Date,
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                UnitName = s.UnitName,
                UnitId = s.UnitId,
                UnitCode = s.UnitCode,
                BudgetCode = s.BudgetCode,
                BudgetId = s.BudgetId,
                BudgetName = s.BudgetName,
                DivisionId = s.DivisionId,
                DivisionCode = s.DivisionCode,
                DivisionName = s.DivisionName,
                CategoryCode = s.CategoryCode,
                CategoryId = s.CategoryId,
                CategoryName = s.CategoryName,
                IsPosted = s.IsPosted,
                Remark = s.Remark,
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc,
                Items = s.Items
                    .Select(
                        q => new PurchaseRequestItem
                        {
                            PurchaseRequestId = q.PurchaseRequestId,
                            ProductId = q.ProductId,
                            ProductCode = q.ProductCode,
                            ProductName = q.ProductName,
                            Uom = q.Uom,
                            UomId = q.UomId,
                            Status = q.Status,
                            Quantity = q.Quantity,
                            Remark = q.Remark
                        })
                    .Where(j => j.PurchaseRequestId.Equals(s.Id))
                    .ToList()
            }).Where(s => s.IsPosted == true);

            List<string> searchAttributes = new List<string>()
            {
                "No", "UnitName", "CategoryName", "DivisionName"
            };

            Query = QueryHelper<PurchaseRequest>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<PurchaseRequest>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<PurchaseRequest>.ConfigureOrder(Query, OrderDictionary);

            Pageable<PurchaseRequest> pageable = new Pageable<PurchaseRequest>(Query, Page - 1, Size);
            List<PurchaseRequest> Data = pageable.Data.ToList<PurchaseRequest>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }
    }
}