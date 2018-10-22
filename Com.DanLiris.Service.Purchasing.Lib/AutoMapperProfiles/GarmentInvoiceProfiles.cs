using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentInvoiceProfiles : Profile
    {
        public GarmentInvoiceProfiles()
        {
            CreateMap<GarmentInvoice, GarmentInvoiceViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.InvoiceNo, opt => opt.MapFrom(s => s.InvoiceNo))
                .ForMember(d => d.InvoiceDate, opt => opt.MapFrom(s => s.InvoiceDate))
                .ForMember(d => d.IsPayTax, opt => opt.MapFrom(s => s.IsPayTax))
                .ForMember(d => d.UseVat, opt => opt.MapFrom(s => s.UseVat))
                .ForMember(d => d.UseIncomeTax, opt => opt.MapFrom(s => s.UseIncomeTax))
                .ForMember(d => d.VatNo, opt => opt.MapFrom(s => s.VatNo))
                .ForMember(d => d.VatDate, opt => opt.MapFrom(s => s.VatDate))
                .ForMember(d => d.IncomeTaxNo, opt => opt.MapFrom(s => s.IncomeTaxNo))
                .ForMember(d => d.IncomeTaxDate, opt => opt.MapFrom(s => s.IncomeTaxDate))
                /*Supplier*/
                .ForPath(d => d.Suppliers._id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.Suppliers.code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.Suppliers.name, opt => opt.MapFrom(s => s.SupplierName))
                .ReverseMap();

            CreateMap<GarmentInvoiceItem, GarmentInvoiceItemViewModel>()
               .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
               .ForPath(d => d.DeliveryOrders.Id, opt => opt.MapFrom(s => s.DeliveryOrderId))
               .ForPath(d => d.DeliveryOrders.doNo, opt => opt.MapFrom(s => s.DeliveryOrderNo))
               .ForPath(d => d.DeliveryOrders.doDate, opt => opt.MapFrom(s => s.DODate))
               .ForPath(d => d.DeliveryOrders.arrivalDate, opt => opt.MapFrom(s => s.ArrivalDate))
               .ForPath(d => d.DeliveryOrders.totalAmount, opt => opt.MapFrom(s => s.TotalAmount))
               .ReverseMap();

            CreateMap<GarmentInvoiceDetail, GarmentInvoiceDetailViewModel>()
              .ForPath(d => d.EPOId, opt => opt.MapFrom(s => s.EPOId))
              .ForPath(d => d.EPONo, opt => opt.MapFrom(s => s.EPONo))
              .ForPath(d => d.IPOId, opt => opt.MapFrom(s => s.IPOId))
              .ForPath(d => d.IPONo, opt => opt.MapFrom(s => s.IPONo))
              .ForPath(d => d.Products._id, opt => opt.MapFrom(s => s.ProductId))
              .ForPath(d => d.Products.code, opt => opt.MapFrom(s => s.ProductCode))
              .ForPath(d => d.Uoms._id, opt => opt.MapFrom(s => s.UomId))
              .ForPath(d => d.Uoms.unit, opt => opt.MapFrom(s => s.UomUnit))
              .ForPath(d => d.DOQuantity, opt => opt.MapFrom(s => s.DOQuantity))
              .ForPath(d => d.PricePerDealUnit, opt => opt.MapFrom(s => s.PricePerDealUnit))
              .ReverseMap();
        }
    }
}
