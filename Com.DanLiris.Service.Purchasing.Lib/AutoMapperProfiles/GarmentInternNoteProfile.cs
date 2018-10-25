using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternNoteViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentInternNoteProfile : Profile
    {
        public GarmentInternNoteProfile()
        {
            CreateMap<GarmentInternNote, GarmentInternNoteViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.inNo, opt => opt.MapFrom(s => s.INNo))
                .ForMember(d => d.remark, opt => opt.MapFrom(s => s.Remark))

                /*Supplier*/
                .ForPath(d => d.supplier.Id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.supplier.Code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.supplier.Name, opt => opt.MapFrom(s => s.SupplierName))

                /*Supplier*/
                .ForPath(d => d.currency.Id, opt => opt.MapFrom(s => s.CurrencyId))
                .ForPath(d => d.currency.Code, opt => opt.MapFrom(s => s.CurrencyCode))
                .ForPath(d => d.currency.Rate, opt => opt.MapFrom(s => s.CurrencyRate))

                .ForMember(d => d.items, opt => opt.MapFrom(s => s.Items))
                .ReverseMap();

            CreateMap<GarmentInternNoteItem, GarmentInternNoteItemViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForPath(d => d.Invoice.Id, opt => opt.MapFrom(s => s.InvoiceId))
                .ForPath(d => d.Invoice.no, opt => opt.MapFrom(s => s.InvoiceNo))
                .ForPath(d => d.Invoice.invoiceDate, opt => opt.MapFrom(s => s.InvoiceDate))
                .ForPath(d => d.Invoice.totalAmount, opt => opt.MapFrom(s => s.TotalAmount))
                .ForMember(d => d.fulfillments, opt => opt.MapFrom(s => s.Details))
                .ReverseMap();

            CreateMap<GarmentInternNoteDetail, GarmentInternNoteFulfillmentViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForPath(d => d.iNDetailId, opt => opt.MapFrom(s => s.INDetailId))
                .ForPath(d => d.purchaseOrder.purchaseRequest.purchaseExternal.no, opt => opt.MapFrom(s => s.EPONo))

                /*Unit*/
                .ForPath(d => d.purchaseOrder.purchaseRequest.purchaseExternal.unit.Id, opt => opt.MapFrom(s => s.UnitId))
                .ForPath(d => d.purchaseOrder.purchaseRequest.purchaseExternal.unit.Code, opt => opt.MapFrom(s => s.UnitCode))

                /*Product*/
                .ForPath(d => d.product.Id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.product.Code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.product.Name, opt => opt.MapFrom(s => s.ProductName))
                
                .ForPath(d => d.PricePerDealUnit, opt => opt.MapFrom(s => s.PricePerDealUnit))
                .ForPath(d => d.PriceTotal, opt => opt.MapFrom(s => s.PriceTotal))
                .ReverseMap();
        }
    }
}
