using Com.DanLiris.Service.Purchasing.Lib.Facades.PurchasingDispositionFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchasingDispositionDataUtils
{
    public class PurchasingDispositionDataUtil
    {
        private readonly PurchasingDispositionFacade facade;
        private readonly ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil;

        public PurchasingDispositionDataUtil(PurchasingDispositionFacade facade, ExternalPurchaseOrderDataUtil externalPurchaseOrderDataUtil)
        {
            this.facade = facade;
            this.externalPurchaseOrderDataUtil = externalPurchaseOrderDataUtil;
        }

        public  PurchasingDisposition GetNewData()
        {
            var datas = Task.Run(() =>  externalPurchaseOrderDataUtil.GetTestData("unit-test")).Result;
            var itemData = datas.Items;
            ExternalPurchaseOrderDetail detailData= new ExternalPurchaseOrderDetail();
            ExternalPurchaseOrderItem itemdata = new ExternalPurchaseOrderItem();
            foreach (var item in itemData)
            {
                itemdata = item;break;
            }
            foreach(var detail in itemdata.Details)
            {
                detailData = detail;break;
            }
            return new PurchasingDisposition
            {
                SupplierId = "1",
                SupplierCode = "Supplier1",
                SupplierName = "supplier1",

                Bank="Bank",
                Amount=1000,
                Calculation="axb+c",
                //InvoiceNo="test",
                ConfirmationOrderNo="test",
                //Investation = "test",

                PaymentDueDate=new DateTimeOffset(),
                ProformaNo="aaa",
                PaymentMethod="Test",

                Remark = "Remark1",
                
                

                Items = new List<PurchasingDispositionItem>
                {
                    new PurchasingDispositionItem
                    {
                       EPOId=datas.Id.ToString(),
                       EPONo=datas.EPONo,
                       IncomeTaxId="1",
                       IncomeTaxName="tax",
                       IncomeTaxRate=1,
                       UseIncomeTax=true,
                       UseVat=true,
                       Details=new List<PurchasingDispositionDetail>
                       {
                            new PurchasingDispositionDetail
                            {
                                //EPODetailId=detailData.Id.ToString(),
                                CategoryCode="test",
                                CategoryId="1",
                                CategoryName="test",
                                DealQuantity=10,
                                PaidQuantity=1000,
                                DealUomId="1",
                                DealUomUnit="test",
                                PaidPrice=1000,
                                PricePerDealUnit=100,
                                PriceTotal=10000,
                                PRId="1",
                                PRNo="test",
                                ProductCode="test",
                                ProductName="test",
                                ProductId="1",
                                   UnitName="test",
                                   UnitCode="test",
                                   UnitId="1",

                            }
                       }
                    }
                }
            };
        }

       

        public async Task<PurchasingDisposition> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data, "Unit Test",7);
            return data;
        }

    }
}
