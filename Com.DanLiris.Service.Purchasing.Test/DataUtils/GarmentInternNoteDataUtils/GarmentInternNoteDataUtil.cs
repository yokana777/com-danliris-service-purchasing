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

        public async Task<GarmentInternNote> GetNewData()
        {
            var datas = Task.Run(() => garmentInvoiceDataUtil.GetTestData("User")).Result;
            Random rnd = new Random();
            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            string nowTicksB = $"{nowTicks}b";
            var invoice =  new GarmentInternNote
            {
                INNo = $"{nowTicksA}",
                INDate = DateTimeOffset.Now,

                SupplierId = datas.SupplierId.ToString(),
                SupplierCode = datas.SupplierCode,
                SupplierName = datas.SupplierName,

                CurrencyRate = 1,
                CurrencyId = datas.CurrencyId.ToString(),
                CurrencyCode = datas.CurrencyCode,

                Remark = "remark",
                Items = new List<GarmentInternNoteItem>
                    {
                        new GarmentInternNoteItem
                        {
                            InvoiceId = datas.Id ,
                            InvoiceDate = datas.InvoiceDate,
                            InvoiceNo = datas.InvoiceNo,
                            TotalAmount = 2000,

                            Details= new List<GarmentInternNoteDetail>
                            {
                                new GarmentInternNoteDetail
                                {
                                    

                                    PaymentMethod = "PaymentMethod",
                                    PaymentDueDate = DateTimeOffset.Now,
                                    PaymentType = "PaymentTyoe",
                                    DOId = 1,
                                    DONo = "dono",
                                    DODate = DateTimeOffset.Now,
                                    RONo = "12343",
                                    PriceTotal = 9000,
                                    InvoiceDetailId = 1,
                                    
                                    ProductId = "12345",
                                    ProductCode = "productcode",
                                    ProductName = "productname",

                                    UnitId = "12345",
                                    UnitCode ="unitcode",
                                    UnitName ="unitname",

                                    UOMId = "12345",
                                    UOMUnit = "ROLL",

                                    Quantity = 40,
                                    PricePerDealUnit = 5000,
                                    POSerialNumber = "PM132434",
                                    PaymentDueDays = 2
                                }
                            }
                        }
                }
            };
            foreach (var item in datas.Items)
            {

                foreach (var detail in item.Details)
                {
                    datas.Items.Add(new GarmentInvoiceDetail){

                    }
                }
            }
            return invoice;
        }
        
        public async Task<GarmentInternNote> GetTestData()
        {
            var data = await GetNewData();
            await facade.Create(data,false, "Unit Test");
            return data;
        }
        
    }
}
