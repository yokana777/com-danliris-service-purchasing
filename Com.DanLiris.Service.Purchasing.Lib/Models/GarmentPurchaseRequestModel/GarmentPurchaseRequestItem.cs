using Com.Moonlay.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel
{
    public class GarmentPurchaseRequestItem : StandardEntity<long>
    {
        [MaxLength(255)]
        public string PO_SerialNumber { get; set; }

        [MaxLength(255)]
        public string ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(1000)]
        public string ProductName { get; set; }

        public long Quantity { get; set; }
        public long BudgetPrice { get; set; }


        [MaxLength(255)]
        public string UomId { get; set; }
        [MaxLength(255)]
        public string UomUnit { get; set; }

        [MaxLength(255)]
        public string CategoryId { get; set; }
        [MaxLength(1000)]
        public string CategoryName { get; set; }

        public string ProductRemark { get; set; }

        public string Status { get; set; }

        public virtual long GarmentPRId { get; set; }
        [ForeignKey("GarmentPRId")]
        public virtual GarmentPurchaseRequest GarmentPurchaseRequest { get; set; }
    }
}
