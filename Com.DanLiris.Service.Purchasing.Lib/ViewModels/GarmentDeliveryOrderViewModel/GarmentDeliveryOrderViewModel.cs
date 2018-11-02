using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel
{
    public class GarmentDeliveryOrderViewModel : BaseViewModel, IValidatableObject
    {
        public string doNo { get; set; }
        public DateTimeOffset? doDate { get; set; }
        public DateTimeOffset? arrivalDate { get; set; }

        public SupplierViewModel supplier { get; set; }

        public string shipmentType { get; set; }
        public string shipmentNo { get; set; }

        public string remark { get; set; }
        public bool isClosed { get; set; }
        public bool isCustoms { get; set; }
        public bool isInvoice { get; set; }
        public long customsId { get; set; }
        public string billNo { get; set; }
        public string paymentBill { get; set; }
        public double totalAmount { get; set; }
        public List<GarmentDeliveryOrderItemViewModel> items { get; set; }

        //public List<long> unitReceiptNoteIds { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(doNo))
            {
                yield return new ValidationResult("DoNo is required", new List<string> { "doNo" });
            }
            else
            {
                PurchasingDbContext purchasingDbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
                if (purchasingDbContext.GarmentDeliveryOrders.Where(DO => DO.DONo.Equals(doNo) && DO.Id != Id && DO.DODate.ToOffset((new TimeSpan(7, 0, 0))) == doDate && DO.SupplierId == supplier.Id && DO.ArrivalDate.ToOffset((new TimeSpan(7, 0, 0))) == arrivalDate).Count() > 0)
                {
                    yield return new ValidationResult("DoNo is already exist", new List<string> { "doNo" });
                }
            }
            if (arrivalDate.Equals(DateTimeOffset.MinValue) || arrivalDate == null)
            {
                yield return new ValidationResult("ArrivalDate is required", new List<string> { "arrivalDate" });
            }
            if (doDate.Equals(DateTimeOffset.MinValue) || doDate == null)
            {
                yield return new ValidationResult("DoDate is required", new List<string> { "doDate" });
            }
            if (arrivalDate != null && doDate > arrivalDate)
            {
                yield return new ValidationResult("DoDate is greater than ArrivalDate", new List<string> { "doDate" });
            }
            if (supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "supplier" });
            }
            if (shipmentNo == null)
            {
                yield return new ValidationResult("ShipmentNo is required", new List<string> { "shipmentNo" });
            }


            int itemErrorCount = 0;
            int detailErrorCount = 0;

            if (this.items == null || items.Count <= 0)
            {
                yield return new ValidationResult("PurchaseOrderExternal is required", new List<string> { "itemscount" });
            }
            else
            {
                string itemError = "[";

                foreach (var item in items)
                {
                    itemError += "{";

                    if (item.purchaseOrderExternal == null)
                    {
                        itemErrorCount++;
                        itemError += "purchaseOrderExternal: 'No PurchaseOrderExternal selected', ";
                    }
                    else if (item.fulfillments == null || item.fulfillments.Count.Equals(0))
                    {
                        itemErrorCount++;
                        itemError += "fulfillmentscount: 'PurchaseRequest is required', ";
                    }
                    else
                    {
                        string detailError = "[";

                        foreach (var detail in item.fulfillments)
                        {
                            detailError += "{";

                            if (detail.conversion == 0)
                            {
                                detailErrorCount++;
                                detailError += "conversion: 'Conversion can not 0', ";
                            }

                            detailError += "}, ";
                        }

                        detailError += "]";

                        if (detailErrorCount > 0)
                        {
                            itemErrorCount++;
                            itemError += $"fulfillments: {detailError}, ";
                        }
                    }


                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "items" });
            }
        }
    }
}
