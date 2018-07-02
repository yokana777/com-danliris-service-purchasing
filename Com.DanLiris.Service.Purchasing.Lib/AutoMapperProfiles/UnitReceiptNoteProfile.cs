using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class UnitReceiptNoteProfile : BaseAutoMapperProfile
    {
        public UnitReceiptNoteProfile()
        {
            CreateMap<UnitReceiptNoteItem, UnitReceiptNoteItemViewModel>()
                .ForPath(d => d.product._id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.product.code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.product.name, opt => opt.MapFrom(s => s.ProductName))
                .ForPath(d => d.product.uom.unit, opt => opt.MapFrom(s => s.Uom))
                .ForPath(d => d.product.uom._id, opt => opt.MapFrom(s => s.UomId))
                .ForPath(d => d.deliveredQuantity, opt => opt.MapFrom(s => s.ReceiptQuantity))
                .ReverseMap();

            CreateMap<UnitReceiptNote, UnitReceiptNoteViewModel>()
                .ForPath(d => d._id, opt => opt.MapFrom(s => s.Id))
                /* Unit */
                .ForPath(d => d.unit._id, opt => opt.MapFrom(s => s.UnitId))
                .ForPath(d => d.unit.code, opt => opt.MapFrom(s => s.UnitCode))
                .ForPath(d => d.unit.name, opt => opt.MapFrom(s => s.UnitName))
                /* Division */
                .ForPath(d => d.unit.division._id, opt => opt.MapFrom(s => s.DivisionId))
                .ForPath(d => d.unit.division.code, opt => opt.MapFrom(s => s.DivisionCode))
                .ForPath(d => d.unit.division.name, opt => opt.MapFrom(s => s.DivisionName))
                /* Supplier */
                .ForPath(d => d.supplier._id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.supplier.code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.supplier.name, opt => opt.MapFrom(s => s.SupplierName))
                .ForPath(d => d.no, opt => opt.MapFrom(s => s.URNNo))
                .ForPath(d => d.date, opt => opt.MapFrom(s => s.ReceiptDate))
                .ForMember(d => d.items, opt => opt.MapFrom(s => s.Items))
                .ReverseMap();
        }
    }
}
