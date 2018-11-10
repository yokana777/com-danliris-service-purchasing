using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchasingDispositionViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class PurchasingDispositionProfile : Profile
    {
        public PurchasingDispositionProfile()
        {
            CreateMap<PurchasingDisposition, PurchasingDispositionViewModel>()
                .ForPath(d => d.Supplier.Id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.Supplier.Code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.Supplier.Name, opt => opt.MapFrom(s => s.SupplierName))
                .ForPath(d => d.Currency.Id, opt => opt.MapFrom(s => s.CurrencyId))
                .ForPath(d => d.Currency.Code, opt => opt.MapFrom(s => s.CurrencyCode))
                .ForPath(d => d.Currency.Rate, opt => opt.MapFrom(s => s.CurrencyRate))
                .ReverseMap();

            CreateMap<PurchasingDispositionItem, PurchasingDispositionItemViewModel>()
                .ForPath(d => d.Unit.Id, opt => opt.MapFrom(s => s.UnitId))
                .ForPath(d => d.Unit.Code, opt => opt.MapFrom(s => s.UnitCode))
                .ForPath(d => d.Unit.Name, opt => opt.MapFrom(s => s.UnitName))
                .ForPath(d => d.Unit.Division.Id, opt => opt.MapFrom(s => s.DivisionId))
                .ForPath(d => d.Unit.Division.Code, opt => opt.MapFrom(s => s.DivisionCode))
                .ForPath(d => d.Unit.Division.Name, opt => opt.MapFrom(s => s.DivisionName))
                .ForPath(d => d.IncomeTax.Id, opt => opt.MapFrom(s => s.IncomeTaxId))
                .ForPath(d => d.IncomeTax.Name, opt => opt.MapFrom(s => s.IncomeTaxName))
                .ForPath(d => d.IncomeTax.Rate, opt => opt.MapFrom(s => s.IncomeTaxRate))
                .ReverseMap();

            CreateMap<PurchasingDispositionDetail, PurchasingDispositionDetailViewModel>()
                .ForPath(d => d.Product.Id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.Product.Code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.Product.Name, opt => opt.MapFrom(s => s.ProductName))
                .ForPath(d => d.DealUom.Id, opt => opt.MapFrom(s => s.DealUomId))
                .ForPath(d => d.DealUom.Unit, opt => opt.MapFrom(s => s.DealUomUnit))
                .ForPath(d => d.Category.Id, opt => opt.MapFrom(s => s.CategoryId))
                .ForPath(d => d.Category.Code, opt => opt.MapFrom(s => s.CategoryCode))
                .ForPath(d => d.Category.Name, opt => opt.MapFrom(s => s.CategoryName))
                .ReverseMap();
        }
    }
}
