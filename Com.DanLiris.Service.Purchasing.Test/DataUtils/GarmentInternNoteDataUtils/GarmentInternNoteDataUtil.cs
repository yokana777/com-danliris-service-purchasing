using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInvoiceDataUtils;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternNoteDataUtils
{
    public class GarmentInternNoteDataUtil
    {
        private readonly GarmentInvoiceDataUtil garmentInvoiceDataUtil;
        private readonly GarmentInternNoteFacades facade;
        public GarmentInternNoteDataUtil(GarmentInvoiceDataUtil garmentInvoiceDataUtil, GarmentInternNoteFacades facade)
        {
            this.garmentInvoiceDataUtil = garmentInvoiceDataUtil;
            this.facade = facade;
        }

        public GarmentInternNote GetNewData()
        {
            var datas = Task.Run(() => garmentInvoiceDataUtil.GetTestData("User")).Result;
            var internNote =  new GarmentInternNote
            {
                INNo = "IN12345L",
                INDate = DateTimeOffset.Now,

                SupplierId = datas.SupplierId.ToString(),
                SupplierCode = datas.SupplierCode,
                SupplierName = datas.SupplierName,

                CurrencyRate = 1,
                CurrencyId = datas.CurrencyId.ToString(),
                CurrencyCode = datas.CurrencyCode,

                Remark = "remark",
                Items = new List<GarmentInternNoteItem>()
            };
            foreach (var item in datas.Items)
            {
                var InternNoteItem = new GarmentInternNoteItem
                {
                    InvoiceId = datas.Id,
                    InvoiceNo = datas.InvoiceNo,
                    InvoiceDate = datas.InvoiceDate,
                    TotalAmount = 20000
                };
                foreach (var detail in item.Details)
                {
                    var InternNoteDetail = new GarmentInternNoteDetail
                    {
                        EPOId = detail.EPOId,
                        EPONo = detail.EPONo,
                        POSerialNumber = detail.POSerialNumber,
                        RONo = detail.RONo,
                        PricePerDealUnit = detail.PricePerDealUnit,
                        Quantity = (long)detail.DOQuantity,
                        ProductId = detail.ProductId.ToString(),
                        ProductCode = detail.ProductCode,
                        UOMId = detail.UomId.ToString(),
                        UOMUnit = detail.UomUnit,
                        DOId = item.DeliveryOrderId,
                        DONo = item.DeliveryOrderNo,
                        DODate = item.DODate,
                        PaymentMethod = item.PaymentMethod,
                        PaymentType = item.PaymentType,
                    };
                }
            }
            return internNote;
        }
        
        public async Task<GarmentInternNote> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data,false, "Unit Test");
            return data;
        }
        
    }
}
