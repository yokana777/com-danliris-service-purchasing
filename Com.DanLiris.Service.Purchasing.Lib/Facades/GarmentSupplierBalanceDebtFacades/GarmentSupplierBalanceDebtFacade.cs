using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSupplierBalanceDebtModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSupplierBalanceDebtFacades
{
    public class GarmentSupplierBalanceDebtFacade : IBalanceDebtFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentSupplierBalanceDebt> dbSet;
        private readonly DbSet<GarmentDeliveryOrder> dbSetDO;
        private string USER_AGENT = "Facade";

        public GarmentSupplierBalanceDebtFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentSupplierBalanceDebt>();
            this.dbSetDO = dbContext.Set<GarmentDeliveryOrder>();
        }
        //private readonly List<string> Header = new List<string>()
        //{
        //    "Kode Supplier","Supplier", "Import", "Bulan", "Tahun", "Valas", "Mata Uang", "Kurs", "Nilai(IDR)", "Jenis Bahan"
        //};
        //public List<string> CsvHeader => Header;
        //private readonly string[] ImportAllowed = { "True", "False" };
        //private readonly string[] MonthAllowed = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };

        //public async Task<int> UploadData(List<GarmentSupplierBalanceDebt> data, string Username)
        //{
        //    var result = 0;
        //        using (var transaction = dbContext.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                foreach (var item in data)
        //                {
        //                    EntityExtension.FlagForCreate(item, Username, USER_AGENT);
        //                    dbSet.Add(item);
        //                }
        //                result = await dbContext.SaveChangesAsync();
        //                transaction.Commit();
        //            }
        //            catch (Exception e)
        //            {
        //                transaction.Rollback();
        //                throw new Exception(e.Message);
        //            }
        //        }

        //        return result;

        //}

        //public Tuple<bool, List<object>> UploadValidate(ref List<GarmentSupplierBalanceDebtViewModel> Data, List<KeyValuePair<string, StringValues>> Body)
        //{
        //    List<object> ErrorList = new List<object>();
        //    string ErrorMessage;
        //    bool Valid = true;
        //    var dbData = dbSet.ToList();
        //    foreach (GarmentSupplierBalanceDebtViewModel tdebtVM in Data)
        //    {
        //        ErrorMessage = "";
        //        if (string.IsNullOrWhiteSpace(tdebtVM.SupplierCode))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Kode Supplier tidak boleh kosong, ");
        //        }
        //        if (string.IsNullOrWhiteSpace(tdebtVM.SupplierName))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Supplier tidak boleh kosong, ");
        //        }
        //        if (string.IsNullOrWhiteSpace(tdebtVM.CurrencyCode))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Mata Uang tidak boleh kosong, ");
        //        }
        //        if (tdebtVM.CurrencyRate.ToString() == "0" || string.IsNullOrWhiteSpace(tdebtVM.CurrencyRate.ToString()))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Kurs tidak boleh kosong, ");
        //        }
        //        if (string.IsNullOrWhiteSpace(Convert.ToString(tdebtVM.Import)))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Import tidak boleh kosong, ");
        //        }
        //        else if (!ImportAllowed.Any(i => i.Equals(Convert.ToString(tdebtVM.Import), StringComparison.CurrentCultureIgnoreCase)))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Import harus diisi dengan True atau False, ");
        //        }
        //        if (string.IsNullOrWhiteSpace(Convert.ToString(tdebtVM.Month)))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Bulan tidak boleh kosong, ");
        //        }
        //        else if (!MonthAllowed.Any(i => i.Equals(Convert.ToString(tdebtVM.Month), StringComparison.CurrentCultureIgnoreCase)))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Bulan Harus diisi dengan Angka, ");
        //        }
        //        if (string.IsNullOrWhiteSpace(Convert.ToString(tdebtVM.Year)))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Tahun Tidak Boleh Kosong");
        //        }
        //        if (string.IsNullOrWhiteSpace(Convert.ToString(tdebtVM.TotalAmount)))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Valas Tidak Boleh Kosong, Apabila Tidak Ada Saldo Diisi dengan 0 ");
        //        }
        //        if (string.IsNullOrWhiteSpace(Convert.ToString(tdebtVM.TotalAmountIDR)))
        //        {
        //            ErrorMessage = string.Concat(ErrorMessage, "Nilai(IDR) Tidak Boleh Kosong, Apabila Tidak Ada Saldo Diisi dengan 0 ");
        //        }

        //        if (!string.IsNullOrEmpty(ErrorMessage))
        //        {
        //            ErrorMessage = ErrorMessage.Remove(ErrorMessage.Length - 2);
        //            var Error = new ExpandoObject() as IDictionary<string, object>;

        //            Error.Add("Kode Supplier", tdebtVM.SupplierCode);
        //            Error.Add("Supplier", tdebtVM.SupplierName);
        //            Error.Add("Import", tdebtVM.Import);
        //            Error.Add("Bulan", tdebtVM.Year);
        //            Error.Add("Valas", tdebtVM.TotalAmount);
        //            Error.Add("Mata Uang", tdebtVM.CurrencyCode);
        //            Error.Add("Kurs", tdebtVM.CurrencyRate);
        //            Error.Add("Nilai(IDR)", tdebtVM.TotalAmountIDR);
        //            Error.Add("Jenis Bahan", tdebtVM.CodeRequirment);
        //            //Error.Add("Kategori", tdebtVM.CodeRequirment);
        //            Error.Add("Error", ErrorMessage);

        //            ErrorList.Add(Error);
        //        }
        //    }

        //    if (ErrorList.Count > 0)
        //    {
        //        Valid = false;
        //    }

        //    return Tuple.Create(Valid, ErrorList);
        //}

        //public sealed class DebtMap : ClassMap<GarmentSupplierBalanceDebtViewModel>
        //{
        //    public DebtMap()
        //    {
        //        Map(x => x.SupplierCode).Index(0);
        //        Map(x => x.SupplierName).Index(1);
        //        Map(x => x.Import).Index(2).TypeConverter<StringConverter>();
        //        Map(x => x.Month).Index(3);
        //        Map(x => x.Year).Index(4);
        //        Map(x => x.TotalAmount).Index(5);
        //        Map(x => x.CurrencyCode).Index(6);
        //        Map(x => x.CurrencyRate).Index(7);
        //        Map(x => x.TotalAmountIDR).Index(8);
        //        Map(x => x.CodeRequirment).Index(9);
        //    }
        //}

        public Tuple<List<GarmentSupplierBalanceDebt>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentSupplierBalanceDebt> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "SupplierName", "Import"
            };

            Query = QueryHelper<GarmentSupplierBalanceDebt>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSupplierBalanceDebt>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSupplierBalanceDebt>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSupplierBalanceDebt> pageable = new Pageable<GarmentSupplierBalanceDebt>(Query, Page - 1, Size);
            List<GarmentSupplierBalanceDebt> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }
        public async Task<int> Create(GarmentSupplierBalanceDebt m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                        m.TotalValas += item.Valas;
                        m.TotalAmountIDR += item.IDR;


                    }

                    this.dbSet.Add(m);

                    Created = await dbContext.SaveChangesAsync();
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
        public GarmentSupplierBalanceDebt ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public ReadResponse<dynamic> ReadLoader(int Page = 1, int Size = 25, string Order = "{}", int year = 0, string Keyword = null, string Filter = "{}", string Select = "{}", string Search = "[]")
        {
            IQueryable<GarmentDeliveryOrder> Query = dbSetDO;

            List<string> SearchAttributes = JsonConvert.DeserializeObject<List<string>>(Search);
            if (SearchAttributes.Count.Equals(0))
            {
                SearchAttributes = new List<string>() { "DONo" };
            }
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, SearchAttributes, Keyword, SearchWith: "StartsWith");

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Dictionary<string, string> SelectDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Select);
            var SelectedQuery = QueryHelper<GarmentDeliveryOrder>.ConfigureSelect(Query, SelectDictionary);

            int TotalData = SelectedQuery.Count();
            int days = DateTime.DaysInMonth(year, 12);
            DateTimeOffset filter = new DateTimeOffset(new DateTime(year, 12, days));
            SelectedQuery = SelectedQuery.Where("ArrivalDate <= @0", filter);
            //var querydata = SelectedQuery.Where("ArrivalDate <= @0", filter);


            List<dynamic> Data = SelectedQuery
                .Skip((Page - 1) * Size)
                .Take(Size)
                .ToDynamicList();

            return new ReadResponse<dynamic>(Data, TotalData, OrderDictionary);
        }
    }
}
