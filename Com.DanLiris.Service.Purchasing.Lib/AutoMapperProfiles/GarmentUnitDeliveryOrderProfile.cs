using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentUnitDeliveryOrderProfile : Profile
    {
        public GarmentUnitDeliveryOrderProfile()
        {
            CreateMap<GarmentUnitDeliveryOrder, GarmentUnitDeliveryOrderViewModel>()
                .ForPath(d => d.UnitRequest.Id, opt => opt.MapFrom(s => s.UnitRequestId))
                .ForPath(d => d.UnitRequest.Code, opt => opt.MapFrom(s => s.UnitSenderCode))
                .ForPath(d => d.UnitRequest.Name, opt => opt.MapFrom(s => s.UnitRequestName))

                .ForPath(d => d.UnitSender.Id, opt => opt.MapFrom(s => s.UnitSenderId))
                .ForPath(d => d.UnitSender.Code, opt => opt.MapFrom(s => s.UnitSenderCode))
                .ForPath(d => d.UnitSender.Name, opt => opt.MapFrom(s => s.UnitSenderName))

                .ForPath(d => d.Storage._id, opt => opt.MapFrom(s => s.StorageId))
                .ForPath(d => d.Storage.code, opt => opt.MapFrom(s => s.StorageCode))
                .ForPath(d => d.Storage.name, opt => opt.MapFrom(s => s.StorageName))

                .ReverseMap();

            CreateMap<GarmentUnitDeliveryOrderItem, GarmentUnitDeliveryOrderItemViewModel>()
                .ForPath(d => d.Product.Id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.Product.Code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.Product.Name, opt => opt.MapFrom(s => s.ProductName))
                .ForPath(d => d.Product.Remark, opt => opt.MapFrom(s => s.ProductRemark))

                .ForPath(d => d.Uom.Id, opt => opt.MapFrom(s => s.UomId))
                .ForPath(d => d.Uom.Unit, opt => opt.MapFrom(s => s.UomUnit))

                .ReverseMap();
        }
    }
}
