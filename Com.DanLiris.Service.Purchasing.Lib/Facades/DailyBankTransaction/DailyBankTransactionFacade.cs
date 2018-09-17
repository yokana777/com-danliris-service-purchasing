using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.DailyBankTransaction
{
    public class DailyBankTransactionFacade : IDailyBankTransactionFacade, IReadByIdable<DailyBankTransactionModel>
    {
        private readonly PurchasingDbContext _DbContext;
        private readonly DbSet<DailyBankTransactionModel> _DbSet;

        public DailyBankTransactionFacade(PurchasingDbContext dbContext)
        {
            _DbContext = dbContext;
            _DbSet = dbContext.Set<DailyBankTransactionModel>();
        }

        public async Task<int> Create(DailyBankTransactionModel model, string username)
        {
            int Created = 0;

            using (var transaction = _DbContext.Database.BeginTransaction())
            {
                try
                {
                    do
                    {
                        model.Code = CodeGenerator.Generate();
                    }
                    while (_DbSet.Any(d => d.Code.Equals(model.Code)));

                    var previousDocument = await _DbSet.Where(w => w.AccountBankId.Equals(model.AccountBankId)).OrderByDescending(o => o.CreatedUtc).FirstOrDefaultAsync();

                    if (previousDocument != null)
                    {
                        model.BeforeNominal = previousDocument.AfterNominal;
                        if (model.Status.ToUpper().Equals("IN"))
                        {
                            model.AfterNominal = model.BeforeNominal + model.Nominal;
                        }
                        else if (model.Status.ToUpper().Equals("OUT"))
                        {
                            model.AfterNominal = model.BeforeNominal - model.Nominal;
                        }
                    }
                    else
                    {
                        if (model.Status.ToUpper().Equals("IN"))
                        {
                            model.AfterNominal += model.Nominal;
                        }
                        else if (model.Status.ToUpper().Equals("OUT"))
                        {
                            model.AfterNominal -= model.Nominal;
                        }
                    }

                    EntityExtension.FlagForCreate(model, username, "Facade");

                    _DbSet.Add(model);
                    Created = await _DbContext.SaveChangesAsync();
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

        public ReadResponse Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<DailyBankTransactionModel> Query = _DbSet;

            Query = Query
                .Select(s => new DailyBankTransactionModel
                {
                    Id = s.Id,
                    CreatedUtc = s.CreatedUtc,
                    Code = s.Code,
                    LastModifiedUtc = s.LastModifiedUtc,
                    AccountBankName = s.AccountBankName,
                    AccountBankAccountName = s.AccountBankAccountName,
                    AccountBankAccountNumber = s.AccountBankAccountNumber,
                    AccountBankCode = s.AccountBankCode,
                    AccountBankCurrencyCode = s.AccountBankCurrencyCode,
                    AccountBankCurrencyId = s.AccountBankCurrencyId,
                    AccountBankCurrencySymbol = s.AccountBankCurrencySymbol,
                    AccountBankId = s.AccountBankId,
                    Date = s.Date,
                    ReferenceNo = s.ReferenceNo,
                    ReferenceType = s.ReferenceType,
                    Status = s.Status,
                    SourceType = s.SourceType
                });

            List<string> searchAttributes = new List<string>()
            {
                "Code", "ReferenceNo", "ReferenceType","AccountBankName", "AccountBankCurrencyCode", "Status", "SourceType"
            };

            Query = QueryHelper<DailyBankTransactionModel>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<DailyBankTransactionModel>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<DailyBankTransactionModel>.ConfigureOrder(Query, OrderDictionary);

            Pageable<DailyBankTransactionModel> pageable = new Pageable<DailyBankTransactionModel>(Query, Page - 1, Size);
            List<DailyBankTransactionModel> Data = pageable.Data.ToList();

            List<object> list = new List<object>();
            list.AddRange(
               Data.Select(s => new
               {
                   s.Id,
                   s.CreatedUtc,
                   s.Code,
                   s.LastModifiedUtc,
                   s.AccountBankName,
                   s.AccountBankAccountName,
                   s.AccountBankAccountNumber,
                   s.AccountBankCode,
                   s.AccountBankCurrencyCode,
                   s.AccountBankCurrencyId,
                   s.AccountBankCurrencySymbol,
                   s.AccountBankId,
                   s.Date,
                   s.ReferenceNo,
                   s.ReferenceType,
                   s.Status,
                   s.SourceType
               }).ToList()
            );

            int TotalData = pageable.TotalCount;

            return new ReadResponse(list, TotalData, OrderDictionary);
        }

        public async Task<DailyBankTransactionModel> ReadById(int id)
        {
            return await _DbSet.Where(w => w.Id.Equals(id)).FirstOrDefaultAsync();
        }
    }
}
