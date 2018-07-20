using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
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
using System.IO;
using System.Data;
using System.Globalization;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO
{
    public class InternalPurchaseOrderFacade 
    {
        #region Dummy Data
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
                        PRItemId = 1,
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
                        PRItemId = 2,
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
                        PRItemId = 1,
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
                        PRItemId = 2,
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
        #endregion
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
            return this.dbSet
                .Select(s => new InternalPurchaseOrder
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
                    LastModifiedUtc = s.LastModifiedUtc,
                    PRId = s.PRId,
                    Remark = s.Remark,
                    Items = s.Items
                    .Select(
                        q => new InternalPurchaseOrderItem
                        {
                            Id = q.Id,
                            POId = q.POId,
                            PRItemId = q.PRItemId,
                            ProductId = q.ProductId,
                            ProductName = q.ProductName,
                            ProductCode = q.ProductCode,
                            UomId = q.UomId,
                            UomUnit = q.UomUnit,
                            Quantity = q.Quantity,
                            ProductRemark = q.ProductRemark,
                            Status = q.Status
                        }
                    )
                    .Where(j => j.POId.Equals(s.Id) && j.Quantity != 0)
                    .ToList()
                })
                .Where(p => p.Id == id)
                .FirstOrDefault();
        }

        async Task<string> GeneratePONo(InternalPurchaseOrder model)
        {
            DateTime Now = DateTime.Now;
            string Year = Now.ToString("yy");
            string Month = Now.ToString("MM");
            string YearCreated = model.CreatedUtc.ToString("yy");
            string MonthCreated = model.CreatedUtc.ToString("MM");
            var lastInternalPurchaseNo = new InternalPurchaseOrder();
            string internalPurchaseDate = Year + Month;
            string CreatedDate = YearCreated + MonthCreated;
            string internalPurchaseNo = "PO" + model.UnitCode + internalPurchaseDate;
            //string Check_internalPurchaseNo = "PO" + model.UnitCode + internalPurchaseNo;

            if (model.PONo != null && internalPurchaseDate == CreatedDate)
            {
                lastInternalPurchaseNo = await this.dbSet.Where(w => w.PONo.StartsWith(model.PONo)).OrderByDescending(o => o.PONo).FirstOrDefaultAsync();
            }
            else
            {
                lastInternalPurchaseNo = await this.dbSet.Where(w => w.PONo.StartsWith(internalPurchaseNo)).OrderByDescending(o => o.PONo).FirstOrDefaultAsync();
            }
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
                    foreach (var item in m.Items)
                    {
                        item.Status = "PO Internal belum diorder";
                        PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == item.PRItemId);
                        purchaseRequestItem.Status = "Sudah diterima Pembelian";
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
                LastModifiedUtc = s.LastModifiedUtc,
                PRId= s.PRId,
                Remark = s.Remark,
                Items = s.Items
                    .Select(
                        q => new InternalPurchaseOrderItem
                        {
                            Id = q.Id,
                            POId = q.POId,
                            PRItemId = q.PRItemId,
                            ProductId = q.ProductId,
                            ProductName = q.ProductName,
                            ProductCode = q.ProductCode,
                            UomId = q.UomId,
                            UomUnit = q.UomUnit,
                            Quantity = q.Quantity,
                            ProductRemark = q.ProductRemark,
                            Status = q.Status
                        }
                    )
                    .Where(j => j.POId.Equals(s.Id))
                    .ToList()
            });

            List<string> searchAttributes = new List<string>()
            {
                "PRNo", "CreatedBy", "UnitName", "CategoryName", "DivisionName"
            };

            Query = QueryHelper<InternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

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
                        var n = this.dbContext.InternalPurchaseOrderItems
                        .Count(pr => pr.PRItemId == item.PRItemId && !pr.IsDeleted);
                        if (n == 1)
                        {
                            PurchaseRequestItem purchaseRequestItem = this.dbContext.PurchaseRequestItems.FirstOrDefault(s => s.Id == item.PRItemId);
                            purchaseRequestItem.Status = "Belum diterima Pembelian";
                        }
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

        public InternalPurchaseOrder ReadByIdforSplit(int id)
        {
            return this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
        }

        public async Task<int> Split(int id, InternalPurchaseOrder internalPurchaseOrder, string user)
        {
            int Splitted = 0;
            InternalPurchaseOrder UpdateData = ReadByIdforSplit(id);
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .Single(pr => pr.Id == id && !pr.IsDeleted);

                    if (m != null)
                    {
                        
                        EntityExtension.FlagForUpdate(UpdateData, user, "Facade");
                        EntityExtension.FlagForCreate(internalPurchaseOrder, user, "Facade");                        

                        foreach (var itemUpdate in UpdateData.Items)
                        {
                            foreach (var item in internalPurchaseOrder.Items)
                            {
                                if (item.ProductId == itemUpdate.ProductId)
                                {
                                    if (item.Quantity <= itemUpdate.Quantity)
                                    {
                                        EntityExtension.FlagForUpdate(itemUpdate, user, "Facade");
                                        itemUpdate.Quantity = itemUpdate.Quantity - item.Quantity;
                                        EntityExtension.FlagForCreate(item, user, "Facade");
                                        item.Id = 0;
                                        item.POId = 0;
                                    }
                                }
                            }
                        }
                        internalPurchaseOrder.Id = 0;
                        internalPurchaseOrder.PONo = await this.GeneratePONo(UpdateData);
                        internalPurchaseOrder.Active = UpdateData.Active;
                        internalPurchaseOrder.BudgetCode = UpdateData.BudgetCode;
                        internalPurchaseOrder.BudgetId = UpdateData.BudgetId;
                        internalPurchaseOrder.BudgetName = UpdateData.BudgetName;
                        internalPurchaseOrder.CategoryCode = UpdateData.CategoryCode;
                        internalPurchaseOrder.CategoryId = UpdateData.CategoryId;
                        internalPurchaseOrder.CategoryName = UpdateData.CategoryName;
                        internalPurchaseOrder.DivisionCode = UpdateData.DivisionCode;
                        internalPurchaseOrder.DivisionId = UpdateData.DivisionId;
                        internalPurchaseOrder.UId = UpdateData.UId;
                        internalPurchaseOrder.UnitCode = UpdateData.UnitCode;
                        internalPurchaseOrder.UnitId = UpdateData.UnitId;

                        this.dbContext.InternalPurchaseOrders.Add(internalPurchaseOrder);
                        this.dbContext.Update(UpdateData);
                        Splitted = await dbContext.SaveChangesAsync();

                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }
            return Splitted;
        }

        public IQueryable<InternalPurchaseOrderReportViewModel> GetReportQuery(string unitId, string categoryId, string staff, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = (from a in dbContext.InternalPurchaseOrders
                         join i in dbContext.InternalPurchaseOrderItems on a.Id equals i.POId 
                         where a.IsDeleted == false
                             && i.IsDeleted == false
                             && a.UnitId == (string.IsNullOrWhiteSpace(unitId) ? a.UnitId : unitId)
                             && a.CategoryId == (string.IsNullOrWhiteSpace(categoryId) ? a.CategoryId : categoryId)
                             && a.CreatedBy == (string.IsNullOrWhiteSpace(staff) ? a.CreatedBy : staff)
                             && a.PRDate.AddHours(offset).Date >= DateFrom.Date
                             && a.PRDate.AddHours(offset).Date <= DateTo.Date
                         select new InternalPurchaseOrderReportViewModel
                         {
                             prNo = a.PRNo,
                             category = a.CategoryName,
                             unit = a.DivisionName + " - " + a.UnitName,
                             prDate = a.PRDate,
                             expectedDeliveryDatePO = a.ExpectedDeliveryDate == null ? new DateTime(1970, 1, 1) : a.ExpectedDeliveryDate,
                             poStatus = i.Status,
                             productName = i.ProductName,
                             uom = i.UomUnit,
                             quantity = i.Quantity,
                             staff=a.CreatedBy,
                             LastModifiedUtc = i.LastModifiedUtc
                         });
            return Query;
        }

        public Tuple<List<InternalPurchaseOrderReportViewModel>, int> GetReport(string unitId, string categoryId, string staff, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetReportQuery( unitId, categoryId, staff,  dateFrom, dateTo, offset);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            }
            

            Pageable<InternalPurchaseOrderReportViewModel> pageable = new Pageable<InternalPurchaseOrderReportViewModel>(Query, page - 1, size);
            List<InternalPurchaseOrderReportViewModel> Data = pageable.Data.ToList<InternalPurchaseOrderReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcel(string unitId, string categoryId, string staff, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery(unitId, categoryId, staff,  dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal diminta datang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Status", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Staff", DataType = typeof(String) });
            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", 0, "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string date = item.prDate == null ? "-" : item.prDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poDate = item.expectedDeliveryDatePO == new DateTime(1970, 1, 1) ? "-" : item.expectedDeliveryDatePO.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    result.Rows.Add(index,date, item.prNo, item.category, item.productName,item.quantity, item.uom, poDate, item.unit, item.poStatus, item.staff);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
    }
}
