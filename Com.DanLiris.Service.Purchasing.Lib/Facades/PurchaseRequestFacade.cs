using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades
{
    public class PurchaseRequestFacade
    {
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
                        LastModifiedUtc = DateTime.UtcNow,
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

        public List<PurchaseRequest> Read()
        {
            return DUMMY_DATA;
        }

        public PurchaseRequest ReadById(int id)
        {
            return DUMMY_DATA.Single(p => p.Id == id);
        }

        public int Create(PurchaseRequest m)
        {
            int Result = 0;

            /* TODO EF Operation */

            Result = 1;

            return Result;
        }
    }
}
