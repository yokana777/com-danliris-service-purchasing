using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.BankExpenditureNote;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades
{
    public class BankExpenditureNoteFacade : IBankExpenditureNoteFacade, IReadByIdable<BankExpenditureNoteModel>
    {
        private const string V = "Operasional";
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<BankExpenditureNoteModel> dbSet;
        private readonly DbSet<BankExpenditureNoteDetailModel> detailDbSet;
        private readonly IBankDocumentNumberGenerator bankDocumentNumberGenerator;
        public readonly IServiceProvider serviceProvider;

        private readonly string USER_AGENT = "Facade";
        private readonly string CREDITOR_ACCOUNT_URI = "creditor-account/bank-expenditure-note/list";

        public BankExpenditureNoteFacade(PurchasingDbContext dbContext, IBankDocumentNumberGenerator bankDocumentNumberGenerator, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.bankDocumentNumberGenerator = new BankDocumentNumberGenerator(dbContext);
            dbSet = dbContext.Set<BankExpenditureNoteModel>();
            detailDbSet = dbContext.Set<BankExpenditureNoteDetailModel>();
            this.serviceProvider = serviceProvider;
        }

        public async Task<BankExpenditureNoteModel> ReadById(int id)
        {
            return await this.dbContext.BankExpenditureNotes
                .AsNoTracking()
                    .Include(p => p.Details)
                        .ThenInclude(p => p.Items)
                .Where(d => d.Id.Equals(id) && d.IsDeleted.Equals(false))
                .FirstOrDefaultAsync();
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<BankExpenditureNoteModel> Query = this.dbSet;

            Query = Query
                .Select(s => new BankExpenditureNoteModel
                {
                    Id = s.Id,
                    CreatedUtc = s.CreatedUtc,
                    LastModifiedUtc = s.LastModifiedUtc,
                    BankName = s.BankName,
                    BankAccountName = s.BankAccountName,
                    BankAccountNumber = s.BankAccountNumber,
                    DocumentNo = s.DocumentNo,
                    SupplierName = s.SupplierName,
                    GrandTotal = s.GrandTotal,
                    BankCurrencyCode = s.BankCurrencyCode,
                    Details = s.Details.Where(w => w.BankExpenditureNoteId == s.Id).ToList()
                });

            List<string> searchAttributes = new List<string>()
            {
                "DocumentNo", "BankName", "SupplierName","BankCurrencyCode"
            };

            Query = QueryHelper<BankExpenditureNoteModel>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<BankExpenditureNoteModel>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<BankExpenditureNoteModel>.ConfigureOrder(Query, OrderDictionary);

            Pageable<BankExpenditureNoteModel> pageable = new Pageable<BankExpenditureNoteModel>(Query, Page - 1, Size);
            List<BankExpenditureNoteModel> Data = pageable.Data.ToList();

            List<object> list = new List<object>();
            list.AddRange(
               Data.Select(s => new
               {
                   s.Id,
                   s.DocumentNo,
                   s.CreatedUtc,
                   s.BankName,
                   s.BankAccountName,
                   s.BankAccountNumber,
                   s.SupplierName,
                   s.GrandTotal,
                   s.BankCurrencyCode,
                   Details = s.Details.Select(sl => new { sl.SupplierName, sl.UnitPaymentOrderNo }).ToList(),
               }).ToList()
            );

            int TotalData = pageable.TotalCount;

            return new ReadResponse<object>(list, TotalData, OrderDictionary);
        }

        public async Task<int> Update(int id, BankExpenditureNoteModel model, IdentityService identityService)
        {
            int Updated = 0;
            string username = identityService.Username;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForUpdate(model, username, USER_AGENT);
                    dbContext.Entry(model).Property(x => x.GrandTotal).IsModified = true;
                    dbContext.Entry(model).Property(x => x.LastModifiedAgent).IsModified = true;
                    dbContext.Entry(model).Property(x => x.LastModifiedBy).IsModified = true;
                    dbContext.Entry(model).Property(x => x.LastModifiedUtc).IsModified = true;

                    foreach (var detail in model.Details)
                    {
                        if (detail.Id == 0)
                        {
                            EntityExtension.FlagForCreate(detail, username, USER_AGENT);
                            dbContext.BankExpenditureNoteDetails.Add(detail);

                            PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                            {
                                Id = (int)detail.UnitPaymentOrderId,
                                IsPaid = true,
                                BankExpenditureNoteNo = model.DocumentNo,
                                BankExpenditureNoteDate = model.Date
                            };

                            EntityExtension.FlagForUpdate(pde, username, USER_AGENT);
                            //dbContext.Attach(pde);
                            dbContext.Entry(pde).Property(x => x.IsPaid).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNoteNo).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNoteDate).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;

                            foreach (var item in detail.Items)
                            {
                                EntityExtension.FlagForCreate(item, username, USER_AGENT);
                            }
                        }
                    }

                    foreach (var detail in dbContext.BankExpenditureNoteDetails.AsNoTracking().Where(p => p.BankExpenditureNoteId == model.Id))
                    {
                        BankExpenditureNoteDetailModel detailModel = model.Details.FirstOrDefault(prop => prop.Id.Equals(detail.Id));

                        if (detailModel == null)
                        {
                            EntityExtension.FlagForDelete(detail, username, USER_AGENT);

                            foreach (var item in dbContext.BankExpenditureNoteItems.AsNoTracking().Where(p => p.BankExpenditureNoteDetailId == detail.Id))
                            {
                                EntityExtension.FlagForDelete(item, username, USER_AGENT);
                                dbContext.BankExpenditureNoteItems.Update(item);
                            }

                            dbContext.BankExpenditureNoteDetails.Update(detail);

                            PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                            {
                                Id = (int)detail.UnitPaymentOrderId,
                                IsPaid = false,
                                BankExpenditureNoteNo = null,
                                BankExpenditureNoteDate = null
                            };

                            EntityExtension.FlagForUpdate(pde, username, USER_AGENT);
                            //dbContext.Attach(pde);
                            dbContext.Entry(pde).Property(x => x.IsPaid).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNoteNo).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNoteDate).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;
                        }
                    }

                    Updated = await dbContext.SaveChangesAsync();
                    DeleteDailyBankTransaction(model.DocumentNo, identityService);
                    CreateDailyBankTransaction(model, identityService);
                    UpdateCreditorAccount(model, identityService);
                    ReverseJournalTransaction(model.DocumentNo);
                    CreateJournalTransaction(model, identityService);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public async Task<int> Create(BankExpenditureNoteModel model, IdentityService identityService)
        {
            int Created = 0;
            string username = identityService.Username;
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);

                    model.DocumentNo = await bankDocumentNumberGenerator.GenerateDocumentNumber("K", model.BankCode, username);

                    foreach (var detail in model.Details)
                    {
                        EntityExtension.FlagForCreate(detail, username, USER_AGENT);

                        PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                        {
                            Id = (int)detail.UnitPaymentOrderId,
                            IsPaid = true,
                            BankExpenditureNoteNo = model.DocumentNo,
                            BankExpenditureNoteDate = model.Date
                        };

                        EntityExtension.FlagForUpdate(pde, username, USER_AGENT);
                        dbContext.Attach(pde);
                        dbContext.Entry(pde).Property(x => x.IsPaid).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNoteNo).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNoteDate).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;

                        foreach (var item in detail.Items)
                        {
                            EntityExtension.FlagForCreate(item, username, USER_AGENT);
                        }
                    }

                    dbSet.Add(model);
                    Created = await dbContext.SaveChangesAsync();
                    CreateJournalTransaction(model, identityService);
                    CreateDailyBankTransaction(model, identityService);
                    CreateCreditorAccount(model, identityService);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Created;
        }

        private void CreateJournalTransaction(BankExpenditureNoteModel model, IdentityService identityService)
        {
            var items = new List<JournalTransactionItem>();
            foreach (var detail in model.Details)
            {
                var sumDataByUnit = detail.Items.GroupBy(g => g.UnitCode).Select(s => new
                {
                    s.First().UnitCode,
                    Total = s.Sum(sm => sm.Price)
                });

                foreach (var datum in sumDataByUnit)
                {
                    var item = new JournalTransactionItem()
                    {
                        COA = new COA()
                        {
                            Code = COAGenerator.GetDebtCOA(model.SupplierImport, detail.DivisionName, datum.UnitCode)
                        },
                        Debit = datum.Total
                    };

                    items.Add(item);
                }
            }

            items = items.GroupBy(g => g.COA.Code).Select(s => new JournalTransactionItem()
            {
                COA = s.First().COA,
                Debit = s.Sum(sm => sm.Debit)
            }).ToList();

            var bankJournalItem = new JournalTransactionItem()
            {
                COA = new COA()
                {
                    Code = model.BankAccountCOA
                },
                Credit = items.Sum(s => s.Debit)
            };
            items.Add(bankJournalItem);

            var modelToPost = new JournalTransaction()
            {
                Date = DateTimeOffset.Now,
                Description = "Bukti Pengeluaran Bank",
                ReferenceNo = model.DocumentNo,
                Items = items
            };

            string journalTransactionUri = "journal-transactions";
            //var httpClient = new HttpClientService(identityService);
            var httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.PostAsync($"{APIEndpoint.Finance}{journalTransactionUri}", new StringContent(JsonConvert.SerializeObject(modelToPost).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
            response.EnsureSuccessStatusCode();
        }

        private void ReverseJournalTransaction(string referenceNo)
        {
            string journalTransactionUri = $"journal-transactions/reverse-transactions/{referenceNo}";
            var httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.PostAsync($"{APIEndpoint.Finance}{journalTransactionUri}", new StringContent(JsonConvert.SerializeObject(new object()).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> Delete(int Id, IdentityService identityService)
        {
            int Count = 0;
            string username = identityService.Username;

            if (dbSet.Count(p => p.Id == Id && p.IsDeleted == false).Equals(0))
            {
                return 0;
            }

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    BankExpenditureNoteModel bankExpenditureNote = dbContext.BankExpenditureNotes.Single(p => p.Id == Id);

                    ICollection<BankExpenditureNoteDetailModel> Details = new List<BankExpenditureNoteDetailModel>(dbContext.BankExpenditureNoteDetails.Where(p => p.BankExpenditureNoteId.Equals(Id)));

                    foreach (var detail in Details)
                    {
                        ICollection<BankExpenditureNoteItemModel> Items = new List<BankExpenditureNoteItemModel>(dbContext.BankExpenditureNoteItems.Where(p => p.BankExpenditureNoteDetailId.Equals(detail.Id)));

                        foreach (var item in Items)
                        {
                            EntityExtension.FlagForDelete(item, username, USER_AGENT);
                            dbContext.BankExpenditureNoteItems.Update(item);
                        }

                        EntityExtension.FlagForDelete(detail, username, USER_AGENT);
                        dbContext.BankExpenditureNoteDetails.Update(detail);

                        PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                        {
                            Id = (int)detail.UnitPaymentOrderId,
                            IsPaid = false,
                            BankExpenditureNoteNo = null,
                            BankExpenditureNoteDate = null
                        };

                        EntityExtension.FlagForUpdate(pde, username, USER_AGENT);
                        //dbContext.Attach(pde);
                        dbContext.Entry(pde).Property(x => x.IsPaid).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNoteNo).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNoteDate).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;
                    }

                    EntityExtension.FlagForDelete(bankExpenditureNote, username, USER_AGENT);
                    dbSet.Update(bankExpenditureNote);
                    Count = await dbContext.SaveChangesAsync();
                    DeleteDailyBankTransaction(bankExpenditureNote.DocumentNo, identityService);
                    DeleteCreditorAccount(bankExpenditureNote, identityService);
                    ReverseJournalTransaction(bankExpenditureNote.DocumentNo);
                    transaction.Commit();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Count;
        }

        public ReadResponse<object> GetAllByPosition(int Page, int Size, string Order, string Keyword, string Filter)
        {
            IQueryable<PurchasingDocumentExpedition> Query = dbContext.PurchasingDocumentExpeditions;

            Query = Query
                .Select(s => new PurchasingDocumentExpedition
                {
                    Id = s.Id,
                    UnitPaymentOrderNo = s.UnitPaymentOrderNo,
                    UPODate = s.UPODate,
                    DueDate = s.DueDate,
                    InvoiceNo = s.InvoiceNo,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    CategoryCode = s.CategoryCode,
                    CategoryName = s.CategoryName,
                    DivisionCode = s.DivisionCode,
                    DivisionName = s.DivisionName,
                    TotalPaid = s.TotalPaid,
                    Currency = s.Currency,
                    Position = s.Position,
                    VerifyDate = s.VerifyDate,
                    IncomeTax = s.IncomeTax,
                    Vat = s.Vat,
                    IsPaid = s.IsPaid,
                    PaymentMethod = s.PaymentMethod,
                    Items = s.Items.Where(w => w.PurchasingDocumentExpeditionId == s.Id).ToList(),
                    LastModifiedUtc = s.LastModifiedUtc
                });

            List<string> searchAttributes = new List<string>()
            {
                "UnitPaymentOrderNo", "SupplierName", "DivisionName", "SupplierCode", "InvoiceNo"
            };

            Query = QueryHelper<PurchasingDocumentExpedition>.ConfigureSearch(Query, searchAttributes, Keyword);

            if (Filter.Contains("verificationFilter"))
            {
                Filter = "{}";
                List<ExpeditionPosition> positions = new List<ExpeditionPosition> { ExpeditionPosition.SEND_TO_PURCHASING_DIVISION, ExpeditionPosition.SEND_TO_ACCOUNTING_DIVISION, ExpeditionPosition.SEND_TO_CASHIER_DIVISION };
                Query = Query.Where(p => positions.Contains(p.Position));
            }

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<PurchasingDocumentExpedition>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<PurchasingDocumentExpedition>.ConfigureOrder(Query, OrderDictionary);

            Pageable<PurchasingDocumentExpedition> pageable = new Pageable<PurchasingDocumentExpedition>(Query, Page - 1, Size);
            List<PurchasingDocumentExpedition> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> list = new List<object>();
            list.AddRange(Data.Select(s => new
            {
                UnitPaymentOrderId = s.Id,
                s.UnitPaymentOrderNo,
                s.UPODate,
                s.DueDate,
                s.InvoiceNo,
                s.SupplierCode,
                s.SupplierName,
                s.CategoryCode,
                s.CategoryName,
                s.DivisionCode,
                s.DivisionName,
                s.Vat,
                s.IncomeTax,
                s.IsPaid,
                TotalPaid = s.TotalPaid - s.IncomeTax,
                s.Currency,
                s.PaymentMethod,
                Items = s.Items.Select(sl => new
                {
                    UnitPaymentOrderItemId = sl.Id,
                    sl.UnitId,
                    sl.UnitCode,
                    sl.UnitName,
                    sl.ProductId,
                    sl.ProductCode,
                    sl.ProductName,
                    sl.Quantity,
                    sl.Uom,
                    sl.Price
                }).ToList()
            }));

            return new ReadResponse<object>(list, TotalData, OrderDictionary);
        }

        public ReadResponse<object> GetReport(int Size, int Page, string DocumentNo, string UnitPaymentOrderNo, string InvoiceNo, string SupplierCode, string DivisionCode, string PaymentMethod, DateTimeOffset? DateFrom, DateTimeOffset? DateTo, int Offset)
        {
            IQueryable<BankExpenditureNoteReportViewModel> Query;

            if (DateFrom == null || DateTo == null)
            {
                Query = (from a in dbContext.BankExpenditureNotes
                         join b in dbContext.BankExpenditureNoteDetails on a.Id equals b.BankExpenditureNoteId
                         join c in dbContext.PurchasingDocumentExpeditions on b.UnitPaymentOrderId equals c.Id
                         where c.InvoiceNo == (InvoiceNo ?? c.InvoiceNo)
                            && c.SupplierCode == (SupplierCode ?? c.SupplierCode)
                            && c.UnitPaymentOrderNo == (UnitPaymentOrderNo ?? c.UnitPaymentOrderNo)
                            && c.DivisionCode == (DivisionCode ?? c.DivisionCode)
                            && !c.PaymentMethod.ToUpper().Equals("CASH")
                            && c.IsPaid
                            && c.PaymentMethod == (PaymentMethod ?? c.PaymentMethod)
                         where a.DocumentNo == (DocumentNo ?? a.DocumentNo)
                         orderby a.DocumentNo
                         select new BankExpenditureNoteReportViewModel
                         {
                             DocumentNo = a.DocumentNo,
                             Currency = a.BankCurrencyCode,
                             Date = a.Date,
                             SupplierName = c.SupplierName,
                             CategoryName = c.CategoryName == null ? "-" : c.CategoryName,
                             DivisionName = c.DivisionName,
                             PaymentMethod = c.PaymentMethod,
                             UnitPaymentOrderNo = b.UnitPaymentOrderNo,
                             BankName = string.Concat(a.BankAccountName, " - ", a.BankName, " - ", a.BankAccountNumber, " - ", a.BankCurrencyCode),
                             DPP = c.TotalPaid - c.Vat,
                             VAT = c.Vat,
                             TotalPaid = c.TotalPaid,
                             InvoiceNumber = c.InvoiceNo
                         }
                      );
            }
            else
            {
                Query = (from a in dbContext.BankExpenditureNotes
                         join b in dbContext.BankExpenditureNoteDetails on a.Id equals b.BankExpenditureNoteId
                         join c in dbContext.PurchasingDocumentExpeditions on b.UnitPaymentOrderId equals c.Id
                         where c.InvoiceNo == (InvoiceNo ?? c.InvoiceNo)
                            && c.SupplierCode == (SupplierCode ?? c.SupplierCode)
                            && c.UnitPaymentOrderNo == (UnitPaymentOrderNo ?? c.UnitPaymentOrderNo)
                            && c.DivisionCode == (DivisionCode ?? c.DivisionCode)
                            && !c.PaymentMethod.ToUpper().Equals("CASH")
                            && c.IsPaid
                            && c.PaymentMethod == (PaymentMethod ?? c.PaymentMethod)
                         where a.DocumentNo == (DocumentNo ?? a.DocumentNo) && a.Date.AddHours(Offset).Date >= DateFrom.Value.Date && a.Date.AddHours(Offset).Date <= DateTo.Value.Date
                         orderby a.DocumentNo
                         select new BankExpenditureNoteReportViewModel
                         {
                             DocumentNo = a.DocumentNo,
                             Currency = a.BankCurrencyCode,
                             Date = a.Date,
                             SupplierName = c.SupplierName,
                             CategoryName = c.CategoryName == null ? "-" : c.CategoryName,
                             DivisionName = c.DivisionName,
                             PaymentMethod = c.PaymentMethod,
                             UnitPaymentOrderNo = b.UnitPaymentOrderNo,
                             BankName = string.Concat(a.BankAccountName, " - ", a.BankName, " - ", a.BankAccountNumber, " - ", a.BankCurrencyCode),
                             DPP = c.TotalPaid - c.Vat,
                             VAT = c.Vat,
                             TotalPaid = c.TotalPaid,
                             InvoiceNumber = c.InvoiceNo
                         }
                      );
            }

            Pageable<BankExpenditureNoteReportViewModel> pageable = new Pageable<BankExpenditureNoteReportViewModel>(Query, Page - 1, Size);
            List<object> data = pageable.Data.ToList<object>();

            return new ReadResponse<object>(data, pageable.TotalCount, new Dictionary<string, string>());
        }

        public void CreateDailyBankTransaction(BankExpenditureNoteModel model, IdentityService identityService)
        {
            DailyBankTransactionViewModel modelToPost = new DailyBankTransactionViewModel()
            {
                Bank = new AccountBankViewModel()
                {
                    _id = model.BankId,
                    code = model.BankCode,
                    accountName = model.BankAccountName,
                    accountNumber = model.BankAccountNumber,
                    bankCode = model.BankCode,
                    bankName = model.BankName,
                    currency = new CurrencyViewModel()
                    {
                        code = model.BankCurrencyCode,
                        _id = model.BankCurrencyId,
                    }
                },
                Date = model.Date,
                Nominal = model.GrandTotal,
                ReferenceNo = model.DocumentNo,
                ReferenceType = "Bayar Hutang",
                SourceType = "Operasional",
                Status = "OUT",
                Supplier = new SupplierViewModel()
                {
                    _id = model.SupplierId,
                    code = model.SupplierCode,
                    name = model.SupplierName
                }
            };

            string dailyBankTransactionUri = "daily-bank-transactions";
            //var httpClient = new HttpClientService(identityService);
            var httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.PostAsync($"{APIEndpoint.Finance}{dailyBankTransactionUri}", new StringContent(JsonConvert.SerializeObject(modelToPost).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
            response.EnsureSuccessStatusCode();
        }

        public void DeleteDailyBankTransaction(string documentNo, IdentityService identityService)
        {
            string dailyBankTransactionUri = "daily-bank-transactions/by-reference-no/";
            //var httpClient = new HttpClientService(identityService);
            var httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.DeleteAsync($"{APIEndpoint.Finance}{dailyBankTransactionUri}{documentNo}").Result;
            response.EnsureSuccessStatusCode();
        }

        private void CreateCreditorAccount(BankExpenditureNoteModel model, IdentityService identityService)
        {
            List<CreditorAccountViewModel> postedData = new List<CreditorAccountViewModel>();
            foreach (var item in model.Details)
            {
                CreditorAccountViewModel viewModel = new CreditorAccountViewModel()
                {
                    Code = model.DocumentNo,
                    Date = model.Date,
                    Id = (int)model.Id,
                    InvoiceNo = item.InvoiceNo,
                    Mutation = item.TotalPaid,
                    SupplierCode = model.SupplierCode,
                    SupplierName = model.SupplierName
                };
                postedData.Add(viewModel);
            }


            var httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.PostAsync($"{APIEndpoint.Finance}{CREDITOR_ACCOUNT_URI}", new StringContent(JsonConvert.SerializeObject(postedData).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
            response.EnsureSuccessStatusCode();
        }

        private void UpdateCreditorAccount(BankExpenditureNoteModel model, IdentityService identityService)
        {
            List<CreditorAccountViewModel> postedData = new List<CreditorAccountViewModel>();
            foreach (var item in model.Details)
            {
                CreditorAccountViewModel viewModel = new CreditorAccountViewModel()
                {
                    Code = model.DocumentNo,
                    Date = model.Date,
                    Id = (int)model.Id,
                    InvoiceNo = item.InvoiceNo,
                    Mutation = item.TotalPaid,
                    SupplierCode = model.SupplierCode,
                    SupplierName = model.SupplierName
                };
                postedData.Add(viewModel);
            }


            var httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.PutAsync($"{APIEndpoint.Finance}{CREDITOR_ACCOUNT_URI}", new StringContent(JsonConvert.SerializeObject(postedData).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
            response.EnsureSuccessStatusCode();

        }

        private void DeleteCreditorAccount(BankExpenditureNoteModel model, IdentityService identityService)
        {
            var httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.DeleteAsync($"{APIEndpoint.Finance}{CREDITOR_ACCOUNT_URI}/{model.DocumentNo}").Result;
            response.EnsureSuccessStatusCode();

        }

        public List<ExpenditureInfo> GetByPeriod(int month, int year, int timeoffset)
        {
            return dbSet.Where(w => w.Date.AddHours(timeoffset).Month.Equals(month) && w.Date.AddHours(timeoffset).Year.Equals(year)).Select(s => new ExpenditureInfo() { DocumentNo = s.DocumentNo, BankName = s.BankName, BGCheckNumber = s.BGCheckNumber }).ToList();
        }
    }

    public class ExpenditureInfo
    {
        public string DocumentNo { get; set; }
        public string BankName { get; set; }
        public string BGCheckNumber { get; set; }
    }
}
