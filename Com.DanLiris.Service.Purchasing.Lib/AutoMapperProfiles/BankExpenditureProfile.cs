using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.BankExpenditureNote;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    class BankExpenditureProfile : BaseAutoMapperProfile
    {
        public BankExpenditureProfile()
        {
            CreateMap<BankExpenditureNoteModel, BankExpenditureNoteViewModel>()
                /* Bank */
                .ForPath(d => d.Bank._id, opt => opt.MapFrom(s => s.BankId))
                .ForPath(d => d.Bank.bankCode, opt => opt.MapFrom(s => s.BankCode))
                .ForPath(d => d.Bank.accountCurrencyId, opt => opt.MapFrom(s => s.BankCurrencyId))
                .ForPath(d => d.Bank.accountName, opt => opt.MapFrom(s => s.BankAccountName))
                .ForPath(d => d.Bank.accountNumber, opt => opt.MapFrom(s => s.BankAccountNumber))
                .ForPath(d => d.Bank.bankName, opt => opt.MapFrom(s => s.BankName))
                .ForPath(d => d.Bank.currency._id, opt => opt.MapFrom(s => s.BankCurrencyId))
                .ForPath(d => d.Bank.currency.code, opt => opt.MapFrom(s => s.BankCurrencyCode))
                .ForPath(d => d.Bank.currency.rate, opt => opt.MapFrom(s => s.BankCurrencyRate))

                /* Supplier */
                .ForPath(d => d.Supplier._id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.Supplier.code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.Supplier.import, opt => opt.MapFrom(s => s.SupplierImport))
                .ForPath(d => d.Supplier.name, opt => opt.MapFrom(s => s.SupplierName))

                .ReverseMap();

            CreateMap<BankExpenditureNoteDetailModel, BankExpenditureNoteDetailViewModel>()
                .ReverseMap();

            CreateMap<BankExpenditureNoteItemModel, BankExpenditureNoteItemViewModel>()
                .ReverseMap();
        }
    }
}
