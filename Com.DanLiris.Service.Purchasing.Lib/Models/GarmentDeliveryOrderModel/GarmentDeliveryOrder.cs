using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel
{
    public class GarmentDeliveryOrder : BaseModel
    {
        [MaxLength(255)]
        public string DONo { get; set; }
        public DateTimeOffset DODate { get; set; }
        public DateTimeOffset ArrivalDate { get; set; }

        /* Supplier */
        [MaxLength(255)]
        public long SupplierId { get; set; }
        [MaxLength(255)]
        public string SupplierCode { get; set; }
        [MaxLength(1000)]
        public string SupplierName { get; set; }

        public string ShipmentType { get; set; }
        public string ShipmentNo { get; set; }

        public string Remark { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCustoms { get; set; }
        public bool IsInvoice { get; set; }
        public long CustomsId { get; set; }
        [MaxLength(50)]
        public string BillNo { get; set; }
        [MaxLength(50)]
        public string PaymentBill { get; set; }
        public double TotalQuantity { get; set; }
        public double TotalAmount { get; set; }

        public virtual ICollection<GarmentDeliveryOrderItem> Items { get; set; }
    }
}
