using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using System.Net.Http;
using System.Text;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager.CacheData;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class PPHBankExpenditureNoteFacade : IPPHBankExpenditureNoteFacade, IReadByIdable<PPHBankExpenditureNote>
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<PPHBankExpenditureNote> dbSet;
        private readonly DbSet<PurchasingDocumentExpedition> dbSetPurchasingDocumentExpedition;
        private readonly IBankDocumentNumberGenerator bankDocumentNumberGenerator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCacheManager _cacheManager;

        public PPHBankExpenditureNoteFacade(PurchasingDbContext dbContext, IBankDocumentNumberGenerator bankDocumentNumberGenerator, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<PPHBankExpenditureNote>();
            this.dbSetPurchasingDocumentExpedition = dbContext.Set<PurchasingDocumentExpedition>();
            this.bankDocumentNumberGenerator = bankDocumentNumberGenerator;
            _serviceProvider = serviceProvider;
            _cacheManager = (IMemoryCacheManager)serviceProvider.GetService(typeof(IMemoryCacheManager));
        }

        public List<object> GetUnitPaymentOrder(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, string incomeTaxName, double incomeTaxRate, string currency)
        {
            IQueryable<PurchasingDocumentExpedition> Query = this.dbSetPurchasingDocumentExpedition;

            if (dateFrom == null || dateTo == null)
            {
                Query = Query
               .Where(p => p.IsDeleted == false &&
                   p.IncomeTaxName == incomeTaxName &&
                   p.IncomeTaxRate == incomeTaxRate &&
                   p.Currency == currency &&
                   p.IncomeTaxRate != 0 &&
                   p.IsPaidPPH == false && p.Position == ExpeditionPosition.CASHIER_DIVISION
               );
            }
            else
            {
                Query = Query
               .Where(p => p.IsDeleted == false &&
                   p.IncomeTaxName == incomeTaxName &&
                   p.IncomeTaxRate == incomeTaxRate &&
                   p.Currency == currency &&
                   p.IncomeTaxRate != 0 &&
                   p.DueDate.Date >= dateFrom.Value.Date &&
                   p.DueDate.Date <= dateTo.Value.Date &&
                   p.IsPaidPPH == false && p.Position == ExpeditionPosition.CASHIER_DIVISION
               );
            }

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
                    IncomeTax = s.IncomeTax,
                    Vat = s.Vat,
                    TotalPaid = s.TotalPaid,
                    Currency = s.Currency,
                    Items = s.Items.Where(d => d.PurchasingDocumentExpeditionId == s.Id).ToList(),
                    LastModifiedUtc = s.LastModifiedUtc
                });

            List<object> list = new List<object>();
            list.AddRange(
               Query.ToList().Select(s => new
               {
                   Id = s.Id,
                   No = s.UnitPaymentOrderNo,
                   UPODate = s.UPODate,
                   DueDate = s.DueDate,
                   InvoiceNo = s.InvoiceNo,
                   SupplierCode = s.SupplierCode,
                   SupplierName = s.SupplierName,
                   CategoryCode = s.CategoryCode,
                   CategoryName = s.CategoryName,
                   DivisionCode = s.DivisionCode,
                   DivisionName = s.DivisionName,
                   IncomeTax = s.IncomeTax,
                   Vat = s.Vat,
                   TotalPaid = s.TotalPaid,
                   Currency = s.Currency,
                   Items = s.Items,
                   LastModifiedUtc = s.LastModifiedUtc
               }).ToList());

            return list;
        }

        public ReadResponse<object> Read(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            IQueryable<PPHBankExpenditureNote> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "No", "BankAccountName", "Items.UnitPaymentOrderNo"
            };

            Query = QueryHelper<PPHBankExpenditureNote>.ConfigureSearch(Query, searchAttributes, keyword);

            Query = Query
                .Select(s => new PPHBankExpenditureNote
                {
                    Id = s.Id,
                    Date = s.Date,
                    No = s.No,
                    CreatedUtc = s.CreatedUtc,
                    BankAccountName = s.BankAccountName,
                    IncomeTaxName = s.IncomeTaxName,
                    IncomeTaxRate = s.IncomeTaxRate,
                    TotalDPP = s.TotalDPP,
                    TotalIncomeTax = s.TotalIncomeTax,
                    Currency = s.Currency,
                    Items = s.Items.Select(p => new PPHBankExpenditureNoteItem { UnitPaymentOrderNo = p.UnitPaymentOrderNo, PPHBankExpenditureNoteId = p.PPHBankExpenditureNoteId }).Where(p => p.PPHBankExpenditureNoteId == s.Id).ToList(),
                    LastModifiedUtc = s.LastModifiedUtc
                });

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter);
            Query = QueryHelper<PPHBankExpenditureNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            Query = QueryHelper<PPHBankExpenditureNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<PPHBankExpenditureNote> pageable = new Pageable<PPHBankExpenditureNote>(Query, page - 1, size);
            List<PPHBankExpenditureNote> Data = pageable.Data.ToList<PPHBankExpenditureNote>();
            int TotalData = pageable.TotalCount;

            List<object> list = new List<object>();
            list.AddRange(
               Data.Select(s => new
               {
                   Id = s.Id,
                   Date = s.Date,
                   No = s.No,
                   CreatedUtc = s.CreatedUtc,
                   BankAccountName = s.BankAccountName,
                   IncomeTaxName = s.IncomeTaxName,
                   IncomeTaxRate = s.IncomeTaxRate,
                   TotalDPP = s.TotalDPP,
                   TotalIncomeTax = s.TotalIncomeTax,
                   Currency = s.Currency,
                   Items = s.Items.Select(p => new { UnitPaymentOrderNo = p.UnitPaymentOrderNo, PPHBankExpenditureNoteId = p.PPHBankExpenditureNoteId }).Where(p => p.PPHBankExpenditureNoteId == s.Id).ToList(),
                   LastModifiedUtc = s.LastModifiedUtc
               }).ToList()
            );

            return new ReadResponse<object>(list, TotalData, OrderDictionary);
        }

        public async Task<int> Update(int id, PPHBankExpenditureNote model, string username)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForUpdate(model, username, "Facade");
                    dbContext.Entry(model).Property(x => x.Date).IsModified = true;
                    dbContext.Entry(model).Property(x => x.TotalDPP).IsModified = true;
                    dbContext.Entry(model).Property(x => x.TotalIncomeTax).IsModified = true;
                    dbContext.Entry(model).Property(x => x.BGNo).IsModified = true;
                    dbContext.Entry(model).Property(x => x.LastModifiedAgent).IsModified = true;
                    dbContext.Entry(model).Property(x => x.LastModifiedBy).IsModified = true;
                    dbContext.Entry(model).Property(x => x.LastModifiedUtc).IsModified = true;

                    foreach (var item in model.Items)
                    {
                        if (item.Id == 0)
                        {
                            EntityExtension.FlagForCreate(item, username, "Facade");
                            dbContext.PPHBankExpenditureNoteItems.Add(item);

                            PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                            {
                                Id = item.PurchasingDocumentExpeditionId,
                                IsPaidPPH = true,
                                BankExpenditureNotePPHNo = model.No,
                                BankExpenditureNotePPHDate = model.Date
                            };

                            EntityExtension.FlagForUpdate(pde, username, "Facade");
                            //dbContext.Attach(pde);
                            dbContext.Entry(pde).Property(x => x.IsPaidPPH).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHNo).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHDate).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;
                        }
                    }

                    foreach (var item in dbContext.PPHBankExpenditureNoteItems.AsNoTracking().Where(p => p.PPHBankExpenditureNoteId == model.Id))
                    {
                        PPHBankExpenditureNoteItem itemModel = model.Items.FirstOrDefault(prop => prop.Id.Equals(item.Id));

                        if (itemModel == null)
                        {
                            EntityExtension.FlagForDelete(item, username, "Facade");
                            this.dbContext.PPHBankExpenditureNoteItems.Update(item);

                            PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                            {
                                Id = item.PurchasingDocumentExpeditionId,
                                IsPaidPPH = false,
                                BankExpenditureNotePPHDate = null,
                                BankExpenditureNotePPHNo = null
                            };

                            EntityExtension.FlagForUpdate(pde, username, "Facade");
                            //dbContext.Attach(pde);
                            dbContext.Entry(pde).Property(x => x.IsPaidPPH).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHNo).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHDate).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                            dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;
                        }
                    }

                    Updated = await dbContext.SaveChangesAsync();

                    await ReverseJournalTransaction(model.No);
                    await AutoCreateJournalTransaction(model);
                    
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

        private List<IdCOAResult> Units => _cacheManager.Get(MemoryCacheConstant.Units, entry => { return new List<IdCOAResult>(); });
        private List<IdCOAResult> Divisions => _cacheManager.Get(MemoryCacheConstant.Divisions, entry => { return new List<IdCOAResult>(); });
        private List<BankAccountCOAResult> BankAccounts => _cacheManager.Get(MemoryCacheConstant.BankAccounts, entry => { return new List<BankAccountCOAResult>(); });
        private List<IncomeTaxCOAResult> IncomeTaxes => _cacheManager.Get(MemoryCacheConstant.IncomeTaxes, entry => { return new List<IncomeTaxCOAResult>(); });

        private async Task AutoCreateJournalTransaction(PPHBankExpenditureNote model)
        {
            var journalTransactionToPost = new JournalTransaction()
            {
                Date = model.Date,
                Description = "Bon Terima Unit",
                ReferenceNo = model.No,
                Status = "POSTED",
                Items = new List<JournalTransactionItem>()
            };

            int.TryParse(model.BankId, out int bankAccountId);
            var bankAccount = BankAccounts.FirstOrDefault(entity => entity.Id == bankAccountId);

            int.TryParse(model.IncomeTaxId, out int incomeTaxId);
            var incomeTax = IncomeTaxes.FirstOrDefault(entity => entity.Id == incomeTaxId);

            var journalDebitItems = new List<JournalTransactionItem>();
            var journalCreditItems = new List<JournalTransactionItem>();

            journalCreditItems.Add(new JournalTransactionItem()
            {
                COA = new COA()
                {
                    Code = bankAccount.AccountCOA
                },
                Credit = (decimal)model.TotalIncomeTax
            });

            var purchasingDocumentExpeditionIds = model.Items.Select(item => item.PurchasingDocumentExpeditionId).ToList();
            var purchasingDocumentExpeditions = await dbContext.PurchasingDocumentExpeditions.Include(entity => entity.Items).Where(entity => purchasingDocumentExpeditionIds.Contains(entity.Id)).ToListAsync();
            foreach (var item in model.Items)
            {
                var purchasingDocumentExpedition = purchasingDocumentExpeditions.FirstOrDefault(entity => entity.Id == item.PurchasingDocumentExpeditionId);
                var division = Divisions.FirstOrDefault(entity => entity.Code == purchasingDocumentExpedition.DivisionCode);

                journalDebitItems.Add(new JournalTransactionItem()
                {
                    COA = new COA()
                    {
                        Code = $"{incomeTax.COACodeCredit}.{division.COACode}.00"
                    },
                    Debit = (decimal)purchasingDocumentExpedition.IncomeTax
                });
            }

            journalDebitItems = journalDebitItems.GroupBy(grouping => grouping.COA.Code).Select(s => new JournalTransactionItem()
            {
                COA = new COA()
                {
                    Code = s.Key
                },
                Debit = s.Sum(sum => Math.Round(sum.Debit.GetValueOrDefault(), 4)),
                Credit = 0,
                //Remark = string.Join("\n", s.Select(grouped => grouped.Remark).ToList())
            }).ToList();
            journalTransactionToPost.Items.AddRange(journalDebitItems);

            journalCreditItems = journalCreditItems.GroupBy(grouping => grouping.COA.Code).Select(s => new JournalTransactionItem()
            {
                COA = new COA()
                {
                    Code = s.Key
                },
                Debit = 0,
                Credit = s.Sum(sum => Math.Round(sum.Credit.GetValueOrDefault(), 4)),
                //Remark = string.Join("\n", s.Select(grouped => grouped.Remark).ToList())
            }).ToList();
            journalTransactionToPost.Items.AddRange(journalCreditItems);

            if (journalTransactionToPost.Items.Any(item => item.COA.Code.Split(".").FirstOrDefault().Equals("9999")))
                journalTransactionToPost.Status = "DRAFT";

            string journalTransactionUri = "journal-transactions";
            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));
            var response = await httpClient.PostAsync($"{APIEndpoint.Finance}{journalTransactionUri}", new StringContent(JsonConvert.SerializeObject(journalTransactionToPost).ToString(), Encoding.UTF8, General.JsonMediaType));

            response.EnsureSuccessStatusCode();
        }

        private async Task ReverseJournalTransaction(string referenceNo)
        {
            string journalTransactionUri = $"journal-transactions/reverse-transactions/{referenceNo}";
            var httpClient = (IHttpClientService)_serviceProvider.GetService(typeof(IHttpClientService));
            var response = await httpClient.PostAsync($"{APIEndpoint.Finance}{journalTransactionUri}", new StringContent(JsonConvert.SerializeObject(new object()).ToString(), Encoding.UTF8, General.JsonMediaType));

            response.EnsureSuccessStatusCode();
        }

        public async Task<PPHBankExpenditureNote> ReadById(int id)
        {
            return await this.dbContext.PPHBankExpenditureNotes
                .AsNoTracking()
                .Include(p => p.Items)
                    .ThenInclude(p => p.PurchasingDocumentExpedition)
                        .ThenInclude(p => p.Items)
                .Where(d => d.Id.Equals(id) && d.IsDeleted.Equals(false))
                .FirstOrDefaultAsync();
        }

        public async Task<int> Create(PPHBankExpenditureNote model, string username)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(model, username, "Facade");
                    model.No = await bankDocumentNumberGenerator.GenerateDocumentNumber("K", model.BankCode, username);

                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForCreate(item, username, "Facade");

                        PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                        {
                            Id = item.PurchasingDocumentExpeditionId,
                            IsPaidPPH = true,
                            BankExpenditureNotePPHNo = model.No,
                            BankExpenditureNotePPHDate = model.Date
                        };

                        EntityExtension.FlagForUpdate(pde, username, "Facade");
                        //dbContext.Attach(pde);
                        dbContext.Entry(pde).Property(x => x.IsPaidPPH).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHNo).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHDate).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;
                    }

                    this.dbSet.Add(model);
                    Created = await dbContext.SaveChangesAsync();
                    await AutoCreateJournalTransaction(model);
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

        public async Task<int> Delete(int id, string username)
        {
            int Count = 0;

            if (this.dbContext.PPHBankExpenditureNotes.Count(p => p.Id == id && p.IsDeleted == false).Equals(0))
            {
                return 0;
            }

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    PPHBankExpenditureNote PPHBankExpenditureNote = dbContext.PPHBankExpenditureNotes.Single(p => p.Id == id);

                    ICollection<PPHBankExpenditureNoteItem> Items = new List<PPHBankExpenditureNoteItem>(this.dbContext.PPHBankExpenditureNoteItems.Where(p => p.PPHBankExpenditureNoteId.Equals(id)));

                    foreach (PPHBankExpenditureNoteItem item in Items)
                    {
                        EntityExtension.FlagForDelete(item, username, "Facade");
                        this.dbContext.PPHBankExpenditureNoteItems.Update(item);

                        PurchasingDocumentExpedition pde = new PurchasingDocumentExpedition
                        {
                            Id = item.PurchasingDocumentExpeditionId,
                            IsPaidPPH = false,
                            BankExpenditureNotePPHNo = null,
                            BankExpenditureNotePPHDate = null
                        };

                        EntityExtension.FlagForUpdate(pde, username, "Facade");
                        //dbContext.Attach(pde);
                        dbContext.Entry(pde).Property(x => x.IsPaidPPH).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHNo).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.BankExpenditureNotePPHDate).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedAgent).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedBy).IsModified = true;
                        dbContext.Entry(pde).Property(x => x.LastModifiedUtc).IsModified = true;
                    }

                    EntityExtension.FlagForDelete(PPHBankExpenditureNote, username, "Facade");
                    this.dbSet.Update(PPHBankExpenditureNote);
                    Count = await this.dbContext.SaveChangesAsync();

                    await ReverseJournalTransaction(PPHBankExpenditureNote.No);

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
    }
}
