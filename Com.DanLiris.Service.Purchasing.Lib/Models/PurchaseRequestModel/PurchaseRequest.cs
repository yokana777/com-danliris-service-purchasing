using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
            CategoryCode = mongoPurchaseRequest.category.name;
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
