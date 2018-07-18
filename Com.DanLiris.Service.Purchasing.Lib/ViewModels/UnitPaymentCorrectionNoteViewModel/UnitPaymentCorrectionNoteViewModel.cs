using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Microsoft.EntityFrameworkCore;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel
{
    public class UnitPaymentCorrectionNoteViewModel : BaseViewModel, IValidatableObject
    {
        public string uPCNo { get; set; }
        public DateTimeOffset correctionDate { get; set; }
        public string correctionType { get; set; }
        public long uPOId { get; set; }
        public string uPONo { get; set; }
        public SupplierViewModel supplier { get; set; }
        public DivisionViewModel division { get; set; }
        public CategoryViewModel category { get; set; }
        public string invoiceCorrectionNo { get; set; }
        public DateTimeOffset? invoiceCorrectionDate { get; set; }
        public bool useVat { get; set; }
        public string vatTaxCorrectionNo { get; set; }
        public DateTimeOffset? vatTaxCorrectionDate { get; set; }
        public bool useIncomeTax { get; set; }
        public string incomeTaxCorrectionNo { get; set; }
        public string incomeTaxCorrectionName { get; set; }
        public string releaseOrderNoteNo { get; set; }
        public DateTimeOffset dueDate { get; set; }
        public string remark { get; set; }
        public string returNoteNo { get; set; }
        public List<UnitPaymentCorrectionNoteItemViewModel> items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            PurchasingDbContext dbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
            //InternalPurchaseOrder a =
            if (this.uPONo == null)
            {
                yield return new ValidationResult("No. Surat Perintah Bayar is required", new List<string> { "uPOId" });
            }
            if (this.releaseOrderNoteNo == null)
            {
                yield return new ValidationResult("No. Bon Keluar is required", new List<string> { "releaseOrderNoteNo" });
            }
        }
    }
}
