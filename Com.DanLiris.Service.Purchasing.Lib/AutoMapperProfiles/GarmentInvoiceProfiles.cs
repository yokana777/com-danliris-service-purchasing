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
                .ForMember(d => d.invoiceNo, opt => opt.MapFrom(s => s.InvoiceNo))
                .ForMember(d => d.invoiceDate, opt => opt.MapFrom(s => s.InvoiceDate))
                .ForMember(d => d.isPayTax, opt => opt.MapFrom(s => s.IsPayTax))
                .ForMember(d => d.useVat, opt => opt.MapFrom(s => s.UseVat))
                .ForMember(d => d.useIncomeTax, opt => opt.MapFrom(s => s.UseIncomeTax))
                .ForMember(d => d.vatNo, opt => opt.MapFrom(s => s.VatNo))
                .ForMember(d => d.vatDate, opt => opt.MapFrom(s => s.VatDate))
                .ForMember(d => d.incomeTaxNo, opt => opt.MapFrom(s => s.IncomeTaxNo))
                .ForMember(d => d.incomeTaxDate, opt => opt.MapFrom(s => s.IncomeTaxDate))
                /*Supplier*/
                .ForPath(d => d.supplier.Id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.supplier.Code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.supplier.Name, opt => opt.MapFrom(s => s.SupplierName))
                .ReverseMap();

            CreateMap<GarmentInvoiceItem, GarmentInvoiceItemViewModel>()
               .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
               .ForPath(d => d.deliveryOrder.Id, opt => opt.MapFrom(s => s.DeliveryOrderId))
               .ForPath(d => d.deliveryOrder.doNo, opt => opt.MapFrom(s => s.DeliveryOrderNo))
               .ForPath(d => d.deliveryOrder.doDate, opt => opt.MapFrom(s => s.DODate))
               .ForPath(d => d.deliveryOrder.arrivalDate, opt => opt.MapFrom(s => s.ArrivalDate))
               .ForPath(d => d.deliveryOrder.totalAmount, opt => opt.MapFrom(s => s.TotalAmount))
               .ReverseMap();

            CreateMap<GarmentInvoiceDetail, GarmentInvoiceDetailViewModel>()
              //.ForPath(d => d.ePOId, opt => opt.MapFrom(s => s.EPOId))
              //.ForPath(d => d.ePONo, opt => opt.MapFrom(s => s.EPONo))
              //.ForPath(d => d.iPOId, opt => opt.MapFrom(s => s.IPOId))
              //.ForPath(d => d.iPONo, opt => opt.MapFrom(s => s.IPONo))
              .ForPath(d => d.product.Id, opt => opt.MapFrom(s => s.ProductId))
              .ForPath(d => d.product.Code, opt => opt.MapFrom(s => s.ProductCode))
              .ForPath(d => d.uoms.Id, opt => opt.MapFrom(s => s.UomId))
              .ForPath(d => d.uoms.Unit, opt => opt.MapFrom(s => s.UomUnit))
              .ForPath(d => d.dOQuantity, opt => opt.MapFrom(s => s.DOQuantity))
              .ForPath(d => d.pricePerDealUnit, opt => opt.MapFrom(s => s.PricePerDealUnit))
              .ReverseMap();
        }
    }
}
