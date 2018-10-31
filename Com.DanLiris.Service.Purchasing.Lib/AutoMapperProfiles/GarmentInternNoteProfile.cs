using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel;
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
                .ForMember(d => d.inDate, opt => opt.MapFrom(s => s.INDate))
                .ForMember(d => d.remark, opt => opt.MapFrom(s => s.Remark))
                .ForMember(d => d.invoiceNoteNo, opt => opt.MapFrom(s => s.InvoiceNoteNo))
                .ForMember(d => d.hasUnitReceiptNote, opt => opt.MapFrom(s => s.HasUnitReceiptNote))
                
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
                .ForPath(d => d.garmentInvoice.Id, opt => opt.MapFrom(s => s.InvoiceId))
                .ForPath(d => d.garmentInvoice.invoiceNo, opt => opt.MapFrom(s => s.InvoiceNo))
                .ForPath(d => d.garmentInvoice.invoiceDate, opt => opt.MapFrom(s => s.InvoiceDate))
                .ForPath(d => d.garmentInvoice.isPayTax, opt => opt.MapFrom(s => s.IsPayTax))
                .ForPath(d => d.totalAmount, opt => opt.MapFrom(s => s.TotalAmount))

                .ForMember(d => d.detail, opt => opt.MapFrom(s => s.Details))
                .ReverseMap();

            CreateMap<GarmentInternNoteDetail, GarmentInternNoteDetailViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForPath(d => d.ePOId, opt => opt.MapFrom(s => s.EPOId))
                .ForPath(d => d.ePONo, opt => opt.MapFrom(s => s.EPONo))
                
                .ForPath(d => d.dOId, opt => opt.MapFrom(s => s.DOId))
                .ForPath(d => d.dONo, opt => opt.MapFrom(s => s.DONo))
                .ForPath(d => d.dODate, opt => opt.MapFrom(s => s.DODate))
                .ForPath(d => d.pricePerDealUnit, opt => opt.MapFrom(s => s.PricePerDealUnit))
                .ForPath(d => d.priceTotal, opt => opt.MapFrom(s => s.PriceTotal))
                .ForPath(d => d.poSerialNumber, opt => opt.MapFrom(s => s.POSerialNumber))
                .ForPath(d => d.paymentType, opt => opt.MapFrom(s => s.PaymentType))
                .ForPath(d => d.termOfPayment, opt => opt.MapFrom(s => s.TermOfPayment))
                .ForPath(d => d.paymentDueDays, opt => opt.MapFrom(s => s.PaymentDueDays))

                /*Unit*/
                .ForPath(d => d.unit.Id, opt => opt.MapFrom(s => s.UnitId))
                .ForPath(d => d.unit.Code, opt => opt.MapFrom(s => s.UnitCode))

                /*Product*/
                .ForPath(d => d.product.Id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.product.Code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.product.Name, opt => opt.MapFrom(s => s.ProductName))
                
                .ForPath(d => d.pricePerDealUnit, opt => opt.MapFrom(s => s.PricePerDealUnit))
                .ForPath(d => d.priceTotal, opt => opt.MapFrom(s => s.PriceTotal))
                .ReverseMap();
        }
    }
}
