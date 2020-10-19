using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition
{
    public class PPHBankExpenditureNoteViewModel : IValidatableObject
    {
        public int Id { get; set; }
        public string No { get; set; }
        public DateTimeOffset? Date { get; set; }
        public bool IsPosted { get; set; }
        public BankViewModel Bank { get; set; }
        public IncomeTaxExpeditionViewModel IncomeTax { get; set; }
        public string Currency { get; set; }
        public string BGNo { get; set; }
        public double TotalDPP { get; set; }
        public double TotalIncomeTax { get; set; }
        public List<UnitPaymentOrderViewModel> PPHBankExpenditureNoteItems { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.Date == null)
            {
                yield return new ValidationResult("Date is required", new List<string> { "Date" });
            }
            else if (this.Date > DateTimeOffset.UtcNow)
            {
                yield return new ValidationResult("Date must be lower or equal than today's date", new List<string> { "Date" });
            }

            if (Bank == null)
            {
                yield return new ValidationResult("Bank is required", new List<string> { "Bank" });
            }

            if (IncomeTax == null)
            {
                yield return new ValidationResult("Income Tax is required", new List<string> { "IncomeTax" });
            }

            if (PPHBankExpenditureNoteItems.Count == 0)
            {
                yield return new ValidationResult("Items is required", new List<string> { "PPHBankExpenditureNoteItems" });

            }
        }

        public PPHBankExpenditureNoteViewModel()
        {    
        }

        public PPHBankExpenditureNoteViewModel(PPHBankExpenditureNote model)
        {
            Id = model.Id;
            No = model.No;
            TotalIncomeTax = model.TotalIncomeTax;
            TotalDPP = model.TotalDPP;
            Date = model.Date;
            IsPosted = model.IsPosted;
            Bank = new BankViewModel()
            {
                _id = model.BankId,
                code = model.BankCode,
                bankName = model.BankName,
                accountName = model.BankAccountName,
                accountNumber = model.BankAccountNumber,
                currency = new CurrencyViewModel()
                {
                    code = model.Currency
                }
            };
            IncomeTax = new IncomeTaxExpeditionViewModel()
            {
                _id = model.IncomeTaxId,
                name = model.IncomeTaxName,
                rate = model.IncomeTaxRate
            };
            BGNo = model.BGNo;
            Currency = model.Currency;
            PPHBankExpenditureNoteItems = new List<UnitPaymentOrderViewModel>();

            foreach(var item in model.Items)
            {
                var m = new UnitPaymentOrderViewModel()
                {
                    Id = item.Id,
                    PurchasingDocumentExpeditionId = item.PurchasingDocumentExpeditionId,
                    No = item.UnitPaymentOrderNo,
                    UPODate = item.PurchasingDocumentExpedition.UPODate,
                    DueDate = item.PurchasingDocumentExpedition.DueDate,
                    InvoiceNo = item.PurchasingDocumentExpedition.InvoiceNo,
                    SupplierCode = item.PurchasingDocumentExpedition.SupplierCode,
                    SupplierName = item.PurchasingDocumentExpedition.SupplierName,
                    DivisionCode = item.PurchasingDocumentExpedition.DivisionCode,
                    DivisionName = item.PurchasingDocumentExpedition.DivisionName,
                    IncomeTax = item.PurchasingDocumentExpedition.IncomeTax,
                    Vat = item.PurchasingDocumentExpedition.Vat,
                    IncomeTaxId = item.PurchasingDocumentExpedition.IncomeTaxId,
                    IncomeTaxName = item.PurchasingDocumentExpedition.IncomeTaxName,
                    IncomeTaxRate = item.PurchasingDocumentExpedition.IncomeTaxRate,
                    TotalPaid = item.PurchasingDocumentExpedition.TotalPaid,
                    Currency = item.PurchasingDocumentExpedition.Currency,
                    Items = new List<UnitPaymentOrderItemViewModel>()
                };

                foreach(var detail in item.PurchasingDocumentExpedition.Items)
                {
                    m.Items.Add(new UnitPaymentOrderItemViewModel()
                    {
                        ProductId = detail.ProductId,
                        ProductCode = detail.ProductCode,
                        ProductName = detail.ProductName,
                        Price = detail.Price,
                        Quantity = detail.Quantity,
                        UnitCode = detail.UnitCode,
                        UnitId = detail.UnitId,
                        UnitName = detail.UnitName,
                        Uom = detail.Uom
                    });
                }

                this.PPHBankExpenditureNoteItems.Add(m);
            }
        }

        public PPHBankExpenditureNote ToModel()
        {
            PPHBankExpenditureNote model = new PPHBankExpenditureNote()
            {
                Id = Id,
                No = No,
                Date = Date.Value,
                TotalIncomeTax = TotalIncomeTax,
                TotalDPP = TotalDPP,
                BankId = Bank._id,
                BankCode = Bank.code,
                BankName = Bank.bankName,
                BankAccountName = Bank.accountName,
                BankAccountNumber = Bank.accountNumber,
                IncomeTaxId = IncomeTax._id,
                IncomeTaxName = IncomeTax.name,
                IncomeTaxRate = IncomeTax.rate,
                BGNo = BGNo,
                Currency = Currency,
                Items = new List<PPHBankExpenditureNoteItem>()
            };

            foreach (var item in PPHBankExpenditureNoteItems)
            {
                model.Items.Add(new PPHBankExpenditureNoteItem()
                {
                    Id = item.Id,
                    PurchasingDocumentExpeditionId = item.PurchasingDocumentExpeditionId,
                    UnitPaymentOrderNo = item.No
                });
            }

            return model;
        }
    }
}
