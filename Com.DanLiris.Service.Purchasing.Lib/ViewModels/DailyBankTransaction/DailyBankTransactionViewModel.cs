using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.DailyBankTransaction
{
    public class DailyBankTransactionViewModel : BaseViewModel, IValidatableObject
    {
        public AccountBankViewModel Bank { get; set; }
        public string Code { get; set; }
        public BuyerViewModel Buyer { get; set; }
        public DateTimeOffset? Date { get; set; }
        public double? Nominal { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceType { get; set; }
        public string Remark { get; set; }
        public string SourceType { get; set; }
        public string Status { get; set; }
        public SupplierViewModel Supplier { get; set; }
        public double? AfterNominal { get; set; }
        public double? BeforeNominal { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Bank == null || string.IsNullOrWhiteSpace(Bank._id))
            {
                yield return new ValidationResult("Bank harus diisi", new List<string> { "Bank" });
            }

            if (Date == null)
            {
                yield return new ValidationResult("Tanggal harus diisi", new List<string> { "Date" });
            }
            else if (Date > DateTimeOffset.Now)
            {
                yield return new ValidationResult("Tanggal harus lebih kecil dari atau sama dengan tanggal sekarang", new List<string> { "Date" });
            }

            if (string.IsNullOrWhiteSpace(ReferenceNo))
            {
                yield return new ValidationResult("No. Referensi harus diisi", new List<string> { "ReferenceNo" });
            }

            if (string.IsNullOrWhiteSpace(ReferenceType))
            {
                yield return new ValidationResult("Jenis Referensi harus diisi", new List<string> { "ReferenceType" });
            }

            if (string.IsNullOrWhiteSpace(Status) || (!Status.ToUpper().Equals("IN") && !Status.ToUpper().Equals("OUT")))
            {
                yield return new ValidationResult("Status harus diisi", new List<string> { "Status" });
            }
            else
            {
                switch (Status.ToUpper())
                {
                    case "IN":
                        if (!string.IsNullOrWhiteSpace(SourceType) && (SourceType.ToUpper().Equals("OPERASIONAL") || SourceType.ToUpper().Equals("INVESTASI") || SourceType.ToUpper().Equals("PENDANAAN")))
                        {
                            if (Buyer == null || string.IsNullOrWhiteSpace(Buyer._id))
                                if (SourceType.ToUpper().Equals("OPERASIONAL"))
                                {
                                    yield return new ValidationResult("Buyer harus diisi", new List<string> { "Buyer" });
                                }
                                else
                                {
                                    yield return new ValidationResult("Dari harus diisi", new List<string> { "Buyer" });
                                }
                        }
                        break;
                    case "OUT":
                        if (!string.IsNullOrWhiteSpace(SourceType) && (SourceType.ToUpper().Equals("OPERASIONAL") || SourceType.ToUpper().Equals("INVESTASI") || SourceType.ToUpper().Equals("PENDANAAN")))
                        {
                            if (Supplier == null || string.IsNullOrWhiteSpace(Supplier._id))
                                if (SourceType.ToUpper().Equals("OPERASIONAL"))
                                {
                                    yield return new ValidationResult("Supplier harus diisi", new List<string> { "Supplier" });
                                }
                                else
                                {
                                    yield return new ValidationResult("Ke harus diisi", new List<string> { "Supplier" });
                                }
                        }
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(SourceType) || (!SourceType.ToUpper().Equals("OPERASIONAL") && !SourceType.ToUpper().Equals("INVESTASI") && !SourceType.ToUpper().Equals("PENDANAAN")))
            {
                yield return new ValidationResult("Jenis Sumber harus diisi", new List<string> { "SourceType" });
            }

            if (Nominal <= 0)
            {
                yield return new ValidationResult("Nominal harus lebih besar dari 0", new List<string> { "Nominal" });
            }
        }
    }
}
