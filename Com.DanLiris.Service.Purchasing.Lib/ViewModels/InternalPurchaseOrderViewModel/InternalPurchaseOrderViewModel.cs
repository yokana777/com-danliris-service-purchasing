using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Microsoft.EntityFrameworkCore;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel
{
    public class InternalPurchaseOrderViewModel : BaseViewModel, IValidatableObject
    {
        public string poNo { get; set; }
        public string isoNo { get; set; }
        public string prId { get; set; }
        public string prNo { get; set; }
        public DateTimeOffset prDate { get; set; }
        public DateTimeOffset expectedDeliveryDate { get; set; }
        public BudgetViewModel budget { get; set; }
        public DivisionViewModel division { get; set; }
        public UnitViewModel unit { get; set; }
        public CategoryViewModel category { get; set; }
        public string remark { get; set; }
        public bool isPosted { get; set; }
        public bool isClosed { get; set; }
        public string status { get; set; }
        public List<InternalPurchaseOrderItemViewModel> items { get; set; }

        

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            PurchasingDbContext dbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
            //InternalPurchaseOrder a =
            if (this.prNo == null)
            {
                yield return new ValidationResult("No. PR is required", new List<string> { "prNo" });
            }
            if (this.items.Count.Equals(0))
            {
                yield return new ValidationResult("Items is required", new List<string> { "itemscount" });
            }
            //InternalPurchaseOrder NewData = dbContext.InternalPurchaseOrders.Include(p => p.Items).FirstOrDefault(p => p.PONo == this.poNo);
            //var n = dbContext.InternalPurchaseOrders.Count(pr => pr.PRNo == prNo && !pr.IsDeleted);
            if(this.poNo != null)
            {
                InternalPurchaseOrder NewData = dbContext.InternalPurchaseOrders.Include(p => p.Items).FirstOrDefault(p => p.PONo == this.poNo);
                var n = dbContext.InternalPurchaseOrders.Count(pr => pr.PRNo == prNo && !pr.IsDeleted);
                foreach (var itemCreate in NewData.Items)
                {
                    foreach (InternalPurchaseOrderItemViewModel Item in items)
                    {
                        if (itemCreate.Quantity == Item.quantity)
                        {
                            yield return new ValidationResult("Data belum ada yang diubah", new List<string> { "itemscount" });
                        }
                        if (Item.quantity > itemCreate.Quantity)
                        {
                            yield return new ValidationResult("Jumlah tidak boleh lebih dari (Quantity)", new List<string> { "itemscount" });
                        }
                    }
                }
            }
        }
    }
}