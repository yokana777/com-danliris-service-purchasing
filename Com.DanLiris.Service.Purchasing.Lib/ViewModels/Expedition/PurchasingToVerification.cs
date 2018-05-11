using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition
{
    public class PurchasingToVerificationViewModel : BaseViewModel, IValidatableObject
    {
        public DateTimeOffset? SubmissionDate { get; set; }
        public List<UnitPaymentOrder> UnitPaymentOrders { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.SubmissionDate == null)
            {
                yield return new ValidationResult("Submission Date is required", new List<string> { "SubmissionDate" });
            }
            else if (this.SubmissionDate > DateTimeOffset.UtcNow)
            {
                yield return new ValidationResult("Submission Date must be lower or equal than today's date", new List<string> { "SubmissionDate" });
            }

            if (this.UnitPaymentOrders.Count.Equals(0))
            {
                yield return new ValidationResult("Unit Payment Orders is required", new List<string> { "UnitPaymentOrdersCollection" });
            }
            else
            {
                int Count = 0;
                string error = "[";

                foreach (UnitPaymentOrder unitPaymentOrder in UnitPaymentOrders)
                {
                    if (string.IsNullOrWhiteSpace(unitPaymentOrder.No))
                    {
                        Count++;
                        error += "{ UnitPaymentOrder: 'Unit Payment Order is required' }, ";
                    }
                    else if (UnitPaymentOrders.Count(prop => prop.No == unitPaymentOrder.No) > 1)
                    {
                        Count++;
                        error += "{ UnitPaymentOrder: 'Unit Payment Order must be unique' }, ";
                    }
                    else
                    {
                        error += "{},";
                    }
                }

                error += "]";

                if (Count > 0)
                {
                    yield return new ValidationResult(error, new List<string> { "UnitPaymentOrders" });
                }
            }
        }

        public override object ToModel()
        {
            List<PurchasingDocumentExpedition> list = new List<PurchasingDocumentExpedition>();

            foreach (UnitPaymentOrder unitPaymentOrder in this.UnitPaymentOrders)
            {
                list.Add(new PurchasingDocumentExpedition()
                {
                    SendToVerificationDivisionDate = (DateTimeOffset)this.SubmissionDate,
                    UnitPaymentOrderNo = unitPaymentOrder.No,
                    Supplier = unitPaymentOrder.Supplier,
                    Division = unitPaymentOrder.Division
                });
            }

            return list;
        }
    }
}
