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
using System.Data;
using System.Globalization;
using System.IO;
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

        public MemoryStream GenerateExcel(string bankId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int offset)
        {
            var Query = GetQuery(bankId, dateFrom, dateTo);

            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Referensi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Referensi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Currency", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Debit", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kredit", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "After", DataType = typeof(double) });

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", 0, 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                var previous = new DailyBankTransactionModel();
                foreach (var item in Query)
                {
                    result.Rows.Add(item.Date.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID")), item.Remark, item.ReferenceNo, item.ReferenceType, item.AccountBankCurrencyCode, item.Status.ToUpper().Equals("IN") ? item.Nominal : 0, item.Status.ToUpper().Equals("OUT") ? item.Nominal : 0, item.AfterNominal);
                    previous = item;
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Mutasi") }, true);
        }

        public ReadResponse GetReport(string bankId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            IQueryable<DailyBankTransactionModel> Query = GetQuery(bankId, dateFrom, dateTo);

            var Test = Query.ToList();
            List<object> Result = Query.Cast<object>().ToList();

            Dictionary<string, string> order = new Dictionary<string, string>();

            return new ReadResponse(Result, Result.Count, order);
        }

        private IQueryable<DailyBankTransactionModel> GetQuery(string bankId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            DateTimeOffset DateFrom = dateFrom == null ? dateTo == null ? DateTimeOffset.Now.AddDays(-30) : dateTo.Value.AddDays(-30) : dateFrom.Value;
            DateTimeOffset DateTo = dateTo == null ? dateFrom == null ? DateTimeOffset.Now : dateFrom.Value.AddDays(DateTimeOffset.Now.Subtract(dateFrom.Value).TotalDays) : dateTo.Value;

            var Query = (from transaction in _DbContext.DailyBankTransactions
                     where
                     transaction.IsDeleted == false
                     && string.IsNullOrWhiteSpace(bankId) ? true : bankId.Equals(transaction.AccountBankId)
                     && transaction.Date >= DateFrom
                     && transaction.Date <= DateTo
                     orderby transaction.Date, transaction.CreatedUtc
                     select new DailyBankTransactionModel
                     {
                         Id = transaction.Id,
                         Date = transaction.Date,
                         Remark = $"{transaction.BuyerName}\n{transaction.Remark}",
                         ReferenceNo = transaction.ReferenceNo,
                         ReferenceType = transaction.ReferenceType,
                         AccountBankCurrencyCode = transaction.AccountBankCurrencyCode,
                         BeforeNominal = transaction.BeforeNominal,
                         AfterNominal = transaction.AfterNominal,
                         Nominal = transaction.Nominal,
                         Status = transaction.Status,
                     });

            return Query;
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
