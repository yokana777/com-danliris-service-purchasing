using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class PurchaseRequestProfile : Profile
    {
        public PurchaseRequestProfile()
        {
            CreateMap<PurchaseRequestItem, UomViewModel>()
                .ForMember(d => d.unit, opt => opt.MapFrom(s => s.Uom));

            CreateMap<PurchaseRequestItem, ProductViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.ProductId))
                .ForMember(d => d.code, opt => opt.MapFrom(s => s.ProductCode))
                .ForMember(d => d.name, opt => opt.MapFrom(s => s.ProductName))
                .ForMember(d => d.uom, opt => opt.MapFrom(s => Mapper.Map<PurchaseRequestItem, UomViewModel>(s)));

            CreateMap<PurchaseRequestItem, PurchaseRequestItemViewModel>()
                .ForMember(d => d.product, opt => opt.MapFrom(s => Mapper.Map<PurchaseRequestItem, ProductViewModel>(s)));

            CreateMap<PurchaseRequest, BudgetViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.BudgetId))
                .ForMember(d => d.code, opt => opt.MapFrom(s => s.BudgetCode))
                .ForMember(d => d.name, opt => opt.MapFrom(s => s.BudgetName));

            CreateMap<PurchaseRequest, DivisionViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.DivisionId))
                .ForMember(d => d.code, opt => opt.MapFrom(s => s.DivisionCode))
                .ForMember(d => d.name, opt => opt.MapFrom(s => s.DivisionName));

            CreateMap<PurchaseRequest, UnitViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.UnitId))
                .ForMember(d => d.code, opt => opt.MapFrom(s => s.UnitCode))
                .ForMember(d => d.name, opt => opt.MapFrom(s => s.UnitName))
                .ForMember(d => d.division, opt => opt.MapFrom(s => Mapper.Map<PurchaseRequest, DivisionViewModel>(s)));

            CreateMap<PurchaseRequest, CategoryViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.CategoryId))
                .ForMember(d => d.code, opt => opt.MapFrom(s => s.CategoryCode))
                .ForMember(d => d.name, opt => opt.MapFrom(s => s.CategoryName));

            CreateMap<PurchaseRequest, PurchaseRequestViewModel>()
                .ForMember(d => d.budget, opt => opt.MapFrom(s => Mapper.Map<PurchaseRequest, BudgetViewModel>(s)))
                .ForMember(d => d.unit, opt => opt.MapFrom(s => Mapper.Map<PurchaseRequest, UnitViewModel>(s)))
                .ForMember(d => d.category, opt => opt.MapFrom(s => Mapper.Map<PurchaseRequest, CategoryViewModel>(s)))
                .ForMember(d => d.items, opt => opt.MapFrom(s => Mapper.Map<ICollection<PurchaseRequestItem>, List<PurchaseRequestItemViewModel>>(s.Items)));
        }
    }
}
