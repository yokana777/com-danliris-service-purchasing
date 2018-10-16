using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentDeliveryOrderProfile : Profile
    {
        public GarmentDeliveryOrderProfile()
        {
            CreateMap<GarmentDeliveryOrder, GarmentDeliveryOrderViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.doNo, opt => opt.MapFrom(s => s.DONo))
                .ForMember(d => d.doDate, opt => opt.MapFrom(s => s.DODate))
                .ForMember(d => d.arrivalDate, opt => opt.MapFrom(s => s.ArrivalDate))

                /*Supplier*/
                .ForPath(d => d.supplier._id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.supplier.code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.supplier.name, opt => opt.MapFrom(s => s.SupplierName))

                .ForPath(d => d.shipmentNo, opt => opt.MapFrom(s => s.ShipmentNo))
                .ForPath(d => d.shipmentType, opt => opt.MapFrom(s => s.ShipmentType))

                .ForPath(d => d.remark, opt => opt.MapFrom(s => s.Remark))
                .ForPath(d => d.isClosed, opt => opt.MapFrom(s => s.IsClosed))
                .ForPath(d => d.isCustoms, opt => opt.MapFrom(s => s.IsCustoms))
                .ForPath(d => d.isInvoice, opt => opt.MapFrom(s => s.IsInvoice))

                .ForPath(d => d.customsId, opt => opt.MapFrom(s => s.CustomsId))
                .ForPath(d => d.billNo, opt => opt.MapFrom(s => s.BillNo))
                .ForPath(d => d.paymentBill, opt => opt.MapFrom(s => s.PaymentBill))

                .ForMember(d => d.items, opt => opt.MapFrom(s => s.Items))
                .ReverseMap();

            CreateMap<GarmentDeliveryOrderItem, GarmentDeliveryOrderItemViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForPath(d => d.purchaseOrderExternal._id, opt => opt.MapFrom(s => s.EPOId))
                .ForPath(d => d.purchaseOrderExternal.no, opt => opt.MapFrom(s => s.EPONo))
                .ForMember(d => d.fulfillments, opt => opt.MapFrom(s => s.Details))
                .ReverseMap();

            CreateMap<GarmentDeliveryOrderDetail, GarmentDeliveryOrderFulfillmentViewModel>()
                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForPath(d => d.purchaseOrder.purchaseRequest._id, opt => opt.MapFrom(s => s.PRId))
                .ForPath(d => d.purchaseOrder.purchaseRequest.no, opt => opt.MapFrom(s => s.PRNo))

                /*Unit*/
                .ForPath(d => d.purchaseOrder.purchaseRequest.unit._id, opt => opt.MapFrom(s => s.UnitId))
                .ForPath(d => d.purchaseOrder.purchaseRequest.unit.code, opt => opt.MapFrom(s => s.UnitCode))

                /*Product*/
                .ForPath(d => d.product._id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.product.code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.product.name, opt => opt.MapFrom(s => s.ProductName))
                .ForPath(d => d.remark, opt => opt.MapFrom(s => s.ProductRemark))
                .ForPath(d => d.doQuantity, opt => opt.MapFrom(s => s.DOQuantity))
                .ForPath(d => d.dealQuantity, opt => opt.MapFrom(s => s.DealQuantity))

                /*UOM*/
                .ForPath(d => d.purchaseOrderUom._id, opt => opt.MapFrom(s => s.UomId))
                .ForPath(d => d.purchaseOrderUom.unit, opt => opt.MapFrom(s => s.UomUnit))

                .ForPath(d => d.smallQuantity, opt => opt.MapFrom(s => s.SmallQuantity))

                /*SmallUOM*/
                .ForPath(d => d.smallUom._id, opt => opt.MapFrom(s => s.SmallUomId))
                .ForPath(d => d.smallUom.unit, opt => opt.MapFrom(s => s.SmallUomUnit))

                /*Currency*/
                .ForPath(d => d.currency._id, opt => opt.MapFrom(s => s.CurrencyId))
                .ForPath(d => d.currency.code, opt => opt.MapFrom(s => s.CurrencyCode))

                .ForPath(d => d.isClosed, opt => opt.MapFrom(s => s.IsClosed))

                .ForPath(d => d.PricePerDealUnit, opt => opt.MapFrom(s => s.PricePerDealUnit))
                .ForPath(d => d.PriceTotal, opt => opt.MapFrom(s => s.PriceTotal))
                .ReverseMap();
        }
    }
}
