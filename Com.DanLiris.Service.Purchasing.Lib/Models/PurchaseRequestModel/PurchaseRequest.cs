using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel
{
    public class PurchaseRequest : BaseModel
    {
        public PurchaseRequest()
        {
        }

        public PurchaseRequest(PurchaseRequestMongo mongoPurchaseRequest)
        {
            Active = mongoPurchaseRequest._active;
            BudgetCode = mongoPurchaseRequest.budget.code;
            BudgetName = mongoPurchaseRequest.budget.name;
            CategoryCode = mongoPurchaseRequest.category.code;
            CategoryName = mongoPurchaseRequest.category.name;
            CreatedAgent = mongoPurchaseRequest._createAgent;
            CreatedBy = mongoPurchaseRequest._createdBy;
            CreatedUtc = mongoPurchaseRequest._createdDate;
            Date = mongoPurchaseRequest.date;
            DeletedAgent = mongoPurchaseRequest._deleted ? mongoPurchaseRequest._updateAgent : "";
            DeletedBy = mongoPurchaseRequest._deleted ? mongoPurchaseRequest._updatedBy : "";
            DeletedUtc = mongoPurchaseRequest._deleted ?  mongoPurchaseRequest._updatedDate : DateTime.MinValue;
            DivisionCode = mongoPurchaseRequest.unit.division.code;
            DivisionName = mongoPurchaseRequest.unit.division.name;
            ExpectedDeliveryDate = mongoPurchaseRequest.expectedDeliveryDate;
            Internal = mongoPurchaseRequest.@internal;
            IsDeleted = mongoPurchaseRequest._deleted;
            IsPosted = mongoPurchaseRequest.isPosted;
            IsUsed = mongoPurchaseRequest.isUsed;
            Items = mongoPurchaseRequest.items.Select(mongoPurchaseRequestItem => new PurchaseRequestItem(mongoPurchaseRequestItem)).ToList();
            LastModifiedAgent = mongoPurchaseRequest._updateAgent;
            LastModifiedBy = mongoPurchaseRequest._updatedBy;
            LastModifiedUtc = mongoPurchaseRequest._updatedDate;
            No = mongoPurchaseRequest.no;
            Remark = mongoPurchaseRequest.remark;
            Status = (PurchaseRequestStatus)mongoPurchaseRequest.status.value;
            UId = mongoPurchaseRequest._id.ToString();
            UnitCode = mongoPurchaseRequest.unit.code;
            UnitName = mongoPurchaseRequest.unit.name;
        }

        [MaxLength(255)]
        public string No { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset ExpectedDeliveryDate { get; set; }

        /* Budget */
        [MaxLength(255)]
        public string BudgetId { get; set; }
        [MaxLength(255)]
        public string BudgetCode { get; set; }
        [MaxLength(1000)]
        public string BudgetName { get; set; }

        /* Unit */
        [MaxLength(255)]
        public string UnitId { get; set; }
        [MaxLength(255)]
        public string UnitCode { get; set; }
        [MaxLength(1000)]
        public string UnitName { get; set; }

        /* Division */
        [MaxLength(255)]
        public string DivisionId { get; set; }
        [MaxLength(255)]
        public string DivisionCode { get; set; }
        [MaxLength(1000)]
        public string DivisionName { get; set; }

        /* Category */
        [MaxLength(255)]
        public string CategoryId { get; set; }
        [MaxLength(255)]
        public string CategoryCode { get; set; }
        [MaxLength(1000)]
        public string CategoryName { get; set; }

        public bool IsPosted { get; set; }
        public bool IsUsed { get; set; }
        public string Remark { get; set; }
        public bool Internal { get; set; }
        public PurchaseRequestStatus Status { get; set; }

        public virtual ICollection<PurchaseRequestItem> Items { get; set; }
    }
}
