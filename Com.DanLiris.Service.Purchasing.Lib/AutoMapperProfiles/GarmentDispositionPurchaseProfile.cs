using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDispositionPurchaseModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentDispositionPurchaseProfile : BaseAutoMapperProfile
    {
        public GarmentDispositionPurchaseProfile()
        {
            var map = CreateMap<GarmentDispositionPurchase, FormDto>();
            map.ForAllMembers(opt => opt.Ignore());
            map.ForMember(s => s.Items, opt => opt.MapFrom(d => d.GarmentDispositionPurchaseItems));
            map.ReverseMap();
            

            var mapItem = CreateMap<GarmentDispositionPurchaseItem, FormItemDto>();
            mapItem.ForAllMembers(opt => opt.Ignore());
            mapItem.ForMember(s => s.Details, opt => opt.MapFrom(d => d.GarmentDispositionPurchaseDetails));
            mapItem.ReverseMap();

            var mapDetail = CreateMap<GarmentDispositionPurchaseDetail, FormDetailDto>();
            mapDetail.ForAllMembers(opt => opt.Ignore());
            mapDetail.ReverseMap();


            var mapTable = CreateMap<GarmentDispositionPurchase, DispositionPurchaseTableDto>();
            mapTable
                .ForMember(s => s.Category, opt => opt.MapFrom(d => d.Category))
                .ForMember(s => s.Currency, opt => opt.MapFrom(d => d.CurrencyName))
                .ForMember(s => s.DispositionDate, opt => opt.MapFrom(d => d.CreatedUtc))
                .ForMember(s => s.DispositionNo, opt => opt.MapFrom(d => d.DispositionNo))
                .ForMember(s => s.DueDate, opt => opt.MapFrom(d => d.DueDate))
                .ForMember(s => s.Supplier, opt => opt.MapFrom(d => d.SupplierName));
        }
    }
}
