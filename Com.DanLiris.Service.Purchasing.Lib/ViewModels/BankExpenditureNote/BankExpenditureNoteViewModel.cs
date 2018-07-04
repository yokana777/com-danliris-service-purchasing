﻿using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.BankExpenditureNote
{
    public class BankExpenditureNoteViewModel : BaseViewModel, IValidatableObject
    {
        public AccountBankViewModel Bank { get; set; }
        public string BGCheckNumber { get; set; }
        public DateTimeOffset? Date { get; set; }
        public List<BankExpenditureNoteDetailViewModel> Details { get; set; }
        public string DocumentNo { get; set; }
        public SupplierViewModel Supplier { get; set; }
        public double GrandTotal { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Details == null || Details.Count.Equals(0))
            {
                yield return new ValidationResult("Minimal 1 Surat Perintah Bayar", new List<string> { "Details" });
            }

            if (Bank == null)
            {
                yield return new ValidationResult("Bank harus diisi", new List<string> { "Bank" });
            }

            if (Date == null)
            {
                yield return new ValidationResult("Tanggal harus diisi", new List<string> { "Date" });
            }

            if (string.IsNullOrWhiteSpace(BGCheckNumber))
            {
                yield return new ValidationResult("Cek/BG harus diisi", new List<string> { "BGCheckNumber" });
            }
        }
    }
}