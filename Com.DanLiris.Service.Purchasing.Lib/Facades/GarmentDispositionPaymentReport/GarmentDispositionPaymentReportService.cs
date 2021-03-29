using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDispositionPaymentReport
{
    public class GarmentDispositionPaymentReportService : IGarmentDispositionPaymentReportService
    {
        private readonly PurchasingDbContext _dbContext;

        public GarmentDispositionPaymentReportService(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetService<PurchasingDbContext>();
        }

        public List<GarmentDispositionPaymentReportDto> GetReportByDate(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var dispositionQuery = _dbContext.GarmentDispositionPurchases.Where(entity => entity.CreatedUtc >= startDate && entity.CreatedUtc <= endDate);
            var dispositionItemQuery = _dbContext.GarmentDispositionPurchaseItems.AsQueryable();
            var dispositionDetailQuery = _dbContext.GarmentDispositionPurchaseDetailss.AsQueryable();
            var externalPurchaseOrderQuery = _dbContext.GarmentExternalPurchaseOrders.AsQueryable();
            var deliveryOrderItemQuery = _dbContext.GarmentDeliveryOrderItems.AsQueryable();
            var deliveryOrderQuery = _dbContext.GarmentDeliveryOrders.AsQueryable();
            var deliveryOrderDetailQuery = _dbContext.GarmentDeliveryOrderDetails.AsQueryable();
            var customsQuery = _dbContext.GarmentBeacukais.AsQueryable();
            var customsItemsQuery = _dbContext.GarmentBeacukaiItems.AsQueryable();
            var unitReceiptNoteItemQuery = _dbContext.GarmentUnitReceiptNoteItems.AsQueryable();
            var unitReceiptNoteQuery = _dbContext.GarmentUnitReceiptNotes.AsQueryable();
            var internalNoteQuery = _dbContext.GarmentInternNotes.AsQueryable();
            var internalNoteItemQuery = _dbContext.GarmentInternNoteItems.AsQueryable();
            var internalNoteDetailQuery = _dbContext.GarmentInternNoteDetails.AsQueryable();


            var query = from disposition in dispositionQuery

                        join item in dispositionItemQuery on disposition.Id equals item.GarmentDispositionPurchaseId into dispositionItems
                        from dispositionItem in dispositionItems.DefaultIfEmpty()


                        join detail in dispositionDetailQuery on dispositionItem.Id equals detail.GarmentDispositionPurchaseItemId into dispositionDetails
                        from dispositionDetail in dispositionDetails.DefaultIfEmpty()

                        join externalPO in externalPurchaseOrderQuery on dispositionItem.EPOId equals externalPO.Id into dispositionItemEPOs
                        from dispositionItemEPO in dispositionItemEPOs.DefaultIfEmpty()

                        join deliveryOrderItem in deliveryOrderItemQuery on dispositionItem.EPOId equals deliveryOrderItem.EPOId into dispositionItemDOItems
                        from dispositionItemDOItem in dispositionItemDOItems.DefaultIfEmpty()

                        join deliveryOrder in deliveryOrderQuery on dispositionItemDOItem.GarmentDOId equals deliveryOrder.Id into dispositionItemDOs
                        from dispositionItemDO in dispositionItemDOs.DefaultIfEmpty()

                        join deliveryOrderDetail in deliveryOrderDetailQuery on dispositionItemDOItem.Id equals deliveryOrderDetail.GarmentDOItemId into dispositionItemDODetails
                        from dispositionItemDODetail in dispositionItemDODetails.DefaultIfEmpty()

                        join customItem in customsItemsQuery on dispositionItemDO.Id equals customItem.GarmentDOId into dispositionItemCustomItems
                        from dispositionItemCustomItem in dispositionItemCustomItems.DefaultIfEmpty()

                        join custom in customsQuery on dispositionItemCustomItem.BeacukaiId equals custom.Id into dispositionItemCustoms
                        from dispositionItemCustom in dispositionItemCustoms.DefaultIfEmpty()

                        join unitReceiptNoteItem in unitReceiptNoteItemQuery on dispositionItemDODetail.Id equals unitReceiptNoteItem.DODetailId into dispositionItemURNItems
                        from dispositionItemURNItem in dispositionItemURNItems.DefaultIfEmpty()

                        join unitReceiptNote in unitReceiptNoteQuery on dispositionItemURNItem.URNId equals unitReceiptNote.Id into dispositionItemURNs
                        from dispositionItemURN in dispositionItemURNs.DefaultIfEmpty()

                        join internalNoteDetail in internalNoteDetailQuery on dispositionItemDO.Id equals internalNoteDetail.DOId into dispositionItemINDetails
                        from dispositionItemINDetail in dispositionItemINDetails.DefaultIfEmpty()

                        join internalNoteItem in internalNoteItemQuery on dispositionItemINDetail.GarmentItemINId equals internalNoteItem.Id into dispositionItemINItems
                        from dispositionItemINItem in dispositionItemINItems.DefaultIfEmpty()

                        join internalNote in internalNoteQuery on dispositionItemINItem.GarmentINId equals internalNote.Id into dispositionItemINs
                        from dispositionItemIN in dispositionItemINs.DefaultIfEmpty()

                        select new
                        {
                            disposition.Id,
                            disposition.DispositionNo,
                            disposition.CreatedUtc,
                            disposition.DueDate,
                            disposition.InvoiceProformaNo,
                            disposition.SupplierId,
                            disposition.SupplierCode,
                            disposition.SupplierName,
                            disposition.SupplierIsImport,
                            disposition.CurrencyId,
                            dispositionItem.CurrencyRate,
                            dispositionItem.CurrencyCode,
                            disposition.Dpp,
                            disposition.VAT,
                            disposition.IncomeTax,
                            disposition.OtherCost,
                            disposition.Amount,
                            dispositionItem.DispositionQuantityPaid,
                            EPOId = dispositionItemDOItem != null ? dispositionItemDOItem.EPOId : 0,
                            EPONo = dispositionItemDOItem != null ? dispositionItemDOItem.EPONo : "",
                            DOId = dispositionItemDO != null ? dispositionItemDO.Id : 0,
                            DONo = dispositionItemDO != null ? dispositionItemDO.DONo : "",
                            BillNo = dispositionItemDO != null ? dispositionItemDO.BillNo : "",
                            PaymentBill = dispositionItemDO != null ? dispositionItemDO.PaymentBill : "",
                            DOQuantity = dispositionItemDODetail != null ? dispositionItemDODetail.DOQuantity :0,
                            CustomNoteNo = dispositionItemCustom != null ? dispositionItemCustom.BeacukaiNo : "",
                            CustomNoteId = dispositionItemCustom != null ? dispositionItemCustom.Id : 0,
                            CustomNoteDate = dispositionItemCustom != null ? (DateTimeOffset?)dispositionItemCustom.BeacukaiDate : null,
                            UnitReceiptNoteId = dispositionItemURN != null ? dispositionItemURN.Id : 0,
                            UnitReceiptNoteNo = dispositionItemURN != null ? dispositionItemURN.URNNo : "",
                            InternalNoteId = dispositionItemIN != null ? dispositionItemIN.Id : 0,
                            InternalNoteNo = dispositionItemIN != null ? dispositionItemIN.INNo : "",
                            InternalNoteDate = dispositionItemIN != null ? (DateTimeOffset?)dispositionItemIN.INDate : null,
                        };

            var result = query.ToList();

            return result.Select(element => new GarmentDispositionPaymentReportDto(element.Id, element.DispositionNo, element.CreatedUtc, element.DueDate, element.InvoiceProformaNo, element.SupplierId, element.SupplierCode, element.SupplierName, element.CurrencyId, element.CurrencyCode, element.CurrencyRate, element.Dpp, element.VAT, element.IncomeTax, element.OtherCost, element.Amount, 0, "", "", (int)element.EPOId, element.EPONo, element.DispositionQuantityPaid, (int)element.DOId, element.DONo, element.DOQuantity, element.PaymentBill, element.BillNo, (int)element.CustomNoteId, element.CustomNoteNo, element.CustomNoteDate, (int)element.UnitReceiptNoteId, element.UnitReceiptNoteNo, (int)element.InternalNoteId, element.InternalNoteNo, element.InternalNoteDate)).ToList();
        }

        public List<GarmentDispositionPaymentReportDto> GetReportByDispositionIds(List<int> dispositionIds)
        {
            var dispositionQuery = _dbContext.GarmentDispositionPurchases.Where(entity => dispositionIds.Contains(entity.Id));
            var dispositionItemQuery = _dbContext.GarmentDispositionPurchaseItems.AsQueryable();
            var dispositionDetailQuery = _dbContext.GarmentDispositionPurchaseDetailss.AsQueryable();
            var externalPurchaseOrderQuery = _dbContext.GarmentExternalPurchaseOrders.AsQueryable();
            var deliveryOrderItemQuery = _dbContext.GarmentDeliveryOrderItems.AsQueryable();
            var deliveryOrderQuery = _dbContext.GarmentDeliveryOrders.AsQueryable();
            var deliveryOrderDetailQuery = _dbContext.GarmentDeliveryOrderDetails.AsQueryable();
            var customsQuery = _dbContext.GarmentBeacukais.AsQueryable();
            var customsItemsQuery = _dbContext.GarmentBeacukaiItems.AsQueryable();
            var unitReceiptNoteItemQuery = _dbContext.GarmentUnitReceiptNoteItems.AsQueryable();
            var unitReceiptNoteQuery = _dbContext.GarmentUnitReceiptNotes.AsQueryable();
            var internalNoteQuery = _dbContext.GarmentInternNotes.AsQueryable();
            var internalNoteItemQuery = _dbContext.GarmentInternNoteItems.AsQueryable();
            var internalNoteDetailQuery = _dbContext.GarmentInternNoteDetails.AsQueryable();


            var query = from disposition in dispositionQuery

                        join item in dispositionItemQuery on disposition.Id equals item.GarmentDispositionPurchaseId into dispositionItems
                        from dispositionItem in dispositionItems.DefaultIfEmpty()


                        join detail in dispositionDetailQuery on dispositionItem.Id equals detail.GarmentDispositionPurchaseItemId into dispositionDetails
                        from dispositionDetail in dispositionDetails.DefaultIfEmpty()

                        join externalPO in externalPurchaseOrderQuery on dispositionItem.EPOId equals externalPO.Id into dispositionItemEPOs
                        from dispositionItemEPO in dispositionItemEPOs.DefaultIfEmpty()

                        join deliveryOrderItem in deliveryOrderItemQuery on dispositionItem.EPOId equals deliveryOrderItem.EPOId into dispositionItemDOItems
                        from dispositionItemDOItem in dispositionItemDOItems.DefaultIfEmpty()

                        join deliveryOrder in deliveryOrderQuery on dispositionItemDOItem.GarmentDOId equals deliveryOrder.Id into dispositionItemDOs
                        from dispositionItemDO in dispositionItemDOs.DefaultIfEmpty()

                        join deliveryOrderDetail in deliveryOrderDetailQuery on dispositionItemDOItem.Id equals deliveryOrderDetail.GarmentDOItemId into dispositionItemDODetails
                        from dispositionItemDODetail in dispositionItemDODetails.DefaultIfEmpty()

                        join customItem in customsItemsQuery on dispositionItemDO.Id equals customItem.GarmentDOId into dispositionItemCustomItems
                        from dispositionItemCustomItem in dispositionItemCustomItems.DefaultIfEmpty()

                        join custom in customsQuery on dispositionItemCustomItem.BeacukaiId equals custom.Id into dispositionItemCustoms
                        from dispositionItemCustom in dispositionItemCustoms.DefaultIfEmpty()

                        join unitReceiptNoteItem in unitReceiptNoteItemQuery on dispositionItemDODetail.Id equals unitReceiptNoteItem.DODetailId into dispositionItemURNItems
                        from dispositionItemURNItem in dispositionItemURNItems.DefaultIfEmpty()

                        join unitReceiptNote in unitReceiptNoteQuery on dispositionItemURNItem.URNId equals unitReceiptNote.Id into dispositionItemURNs
                        from dispositionItemURN in dispositionItemURNs.DefaultIfEmpty()

                        join internalNoteDetail in internalNoteDetailQuery on dispositionItemDO.Id equals internalNoteDetail.DOId into dispositionItemINDetails
                        from dispositionItemINDetail in dispositionItemINDetails.DefaultIfEmpty()

                        join internalNoteItem in internalNoteItemQuery on dispositionItemINDetail.GarmentItemINId equals internalNoteItem.Id into dispositionItemINItems
                        from dispositionItemINItem in dispositionItemINItems.DefaultIfEmpty()

                        join internalNote in internalNoteQuery on dispositionItemINItem.GarmentINId equals internalNote.Id into dispositionItemINs
                        from dispositionItemIN in dispositionItemINs.DefaultIfEmpty()

                        select new
                        {
                            disposition.Id,
                            disposition.DispositionNo,
                            disposition.CreatedUtc,
                            disposition.DueDate,
                            disposition.InvoiceProformaNo,
                            disposition.SupplierId,
                            disposition.SupplierCode,
                            disposition.SupplierName,
                            disposition.SupplierIsImport,
                            disposition.CurrencyId,
                            dispositionItem.CurrencyRate,
                            dispositionItem.CurrencyCode,
                            disposition.Dpp,
                            disposition.VAT,
                            disposition.IncomeTax,
                            disposition.OtherCost,
                            disposition.Amount,
                            dispositionItem.DispositionQuantityPaid,
                            EPOId = dispositionItemDOItem != null ? dispositionItemDOItem.EPOId : 0,
                            EPONo = dispositionItemDOItem != null ? dispositionItemDOItem.EPONo : "",
                            DOId = dispositionItemDO != null ? dispositionItemDO.Id : 0,
                            DONo = dispositionItemDO != null ? dispositionItemDO.DONo : "",
                            BillNo = dispositionItemDO != null ? dispositionItemDO.BillNo : "",
                            PaymentBill = dispositionItemDO != null ? dispositionItemDO.PaymentBill : "",
                            DOQuantity = dispositionItemDODetail != null ? dispositionItemDODetail.DOQuantity : 0,
                            CustomNoteNo = dispositionItemCustom != null ? dispositionItemCustom.BeacukaiNo : "",
                            CustomNoteId = dispositionItemCustom != null ? dispositionItemCustom.Id : 0,
                            CustomNoteDate = dispositionItemCustom != null ? (DateTimeOffset?)dispositionItemCustom.BeacukaiDate : null,
                            UnitReceiptNoteId = dispositionItemURN != null ? dispositionItemURN.Id : 0,
                            UnitReceiptNoteNo = dispositionItemURN != null ? dispositionItemURN.URNNo : "",
                            InternalNoteId = dispositionItemIN != null ? dispositionItemIN.Id : 0,
                            InternalNoteNo = dispositionItemIN != null ? dispositionItemIN.INNo : "",
                            InternalNoteDate = dispositionItemIN != null ? (DateTimeOffset?)dispositionItemIN.INDate : null,
                        };

            var result = query.ToList();

            return result.Select(element => new GarmentDispositionPaymentReportDto(element.Id, element.DispositionNo, element.CreatedUtc, element.DueDate, element.InvoiceProformaNo, element.SupplierId, element.SupplierCode, element.SupplierName, element.CurrencyId, element.CurrencyCode, element.CurrencyRate, element.Dpp, element.VAT, element.IncomeTax, element.OtherCost, element.Amount, 0, "", "", (int)element.EPOId, element.EPONo, element.DispositionQuantityPaid, (int)element.DOId, element.DONo, element.DOQuantity, element.PaymentBill, element.BillNo, (int)element.CustomNoteId, element.CustomNoteNo, element.CustomNoteDate, (int)element.UnitReceiptNoteId, element.UnitReceiptNoteNo, (int)element.InternalNoteId, element.InternalNoteNo, element.InternalNoteDate)).ToList();
        }
    }
}
