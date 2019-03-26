using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils
{

    public class ExternalPurchaseOrderDataUtil
    {
        private InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil;
        private ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil;
        private readonly ExternalPurchaseOrderFacade facade;

        public ExternalPurchaseOrderDataUtil(ExternalPurchaseOrderFacade facade, InternalPurchaseOrderDataUtil internalPurchaseOrderDataUtil, ExternalPurchaseOrderItemDataUtil externalPurchaseOrderItemDataUtil)
        {
            this.facade = facade;
            this.internalPurchaseOrderDataUtil = internalPurchaseOrderDataUtil;
            this.externalPurchaseOrderItemDataUtil = externalPurchaseOrderItemDataUtil;
        }

        public async Task<ExternalPurchaseOrder> GetNewData(string user)
        {
            InternalPurchaseOrder internalPurchaseOrder = await internalPurchaseOrderDataUtil.GetTestData(user);
            //List<ExternalPurchaseOrderDetail> detail = new List<ExternalPurchaseOrderDetail>();
            //foreach (var POdetail in internalPurchaseOrder.Items)
            //{
            //    detail.Add(new ExternalPurchaseOrderDetail
            //    {
            //        POItemId = POdetail.Id,
            //        PRItemId = Convert.ToInt64(POdetail.PRItemId),
            //        ProductId = "ProductId",
            //        ProductCode = "ProductCode",
            //        ProductName = "ProductName",
            //        DefaultQuantity = 10,
            //        DealUomId = "UomId",
            //        DealUomUnit = "Uom",
            //        ProductRemark = "Remark",
            //        PriceBeforeTax = 1000,
            //        PricePerDealUnit = 200,
            //        DealQuantity = POdetail.Quantity
            //    });
            //}
            //List<ExternalPurchaseOrderItem> items = new List<ExternalPurchaseOrderItem>();
            //items.Add(new ExternalPurchaseOrderItem
            //{
            //    POId = internalPurchaseOrder.Id,
            //    PRId = Convert.ToInt64(internalPurchaseOrder.PRId),
            //    PONo = internalPurchaseOrder.PONo,
            //    PRNo = internalPurchaseOrder.PRNo,
            //    UnitCode = "unitcode",
            //    UnitName = "unit",
            //    UnitId = "unitId",
            //    Details = detail
            //});

            return new ExternalPurchaseOrder
            {
                CurrencyCode = "CurrencyCode",
                CurrencyId = "CurrencyId",
                CurrencyRate = 0.5,
                UnitId = "UnitId",
                UnitCode = "UnitCode",
                UnitName = "UnitName",
                DivisionId = "DivisionId",
                DivisionCode = "DivisionCode",
                DivisionName = "DivisionName",
                FreightCostBy = "test",
                DeliveryDate = DateTime.Now.AddDays(1),
                OrderDate = DateTime.Now,
                SupplierCode = "sup",
                SupplierId = "supId",
                IncomeTaxName = "Final",
                IncomeTaxRate = "1.5",
                UseIncomeTax = true,
                IncomeTaxBy="Supplier",
                SupplierName = "Supplier",
                PaymentMethod = "test",
                Remark = "Remark",
                Items = new List<ExternalPurchaseOrderItem> { externalPurchaseOrderItemDataUtil.GetNewData(internalPurchaseOrder) }
            };
        }

        public async Task<ExternalPurchaseOrderViewModel> GetNewDataViewModel(string user)
        {
            InternalPurchaseOrder internalPurchaseOrder = await internalPurchaseOrderDataUtil.GetTestData(user);
            

            return new ExternalPurchaseOrderViewModel
            {
                unit = new UnitViewModel
                {
                    _id = internalPurchaseOrder.UnitId,
                    code = internalPurchaseOrder.UnitCode,
                    name = internalPurchaseOrder.UnitName,
                    division = new DivisionViewModel
                    {
                        _id = internalPurchaseOrder.DivisionId,
                        code = internalPurchaseOrder.DivisionCode,
                        name = internalPurchaseOrder.DivisionName,
                    }
                },
                currency= new CurrencyViewModel
                {
                    code = "CurrencyCode",
                    _id = "CurrencyId",
                    rate = 0.5,
                },
                freightCostBy = "test",
                deliveryDate = DateTime.Now.AddDays(1),
                orderDate = DateTime.Now,
                supplier= new SupplierViewModel
                {
                    code = "sup",
                    _id = "supId",
                    name = "Supplier",
                },
                paymentMethod = "test",
                remark = "Remark",
                items = new List<ExternalPurchaseOrderItemViewModel> { externalPurchaseOrderItemDataUtil.GetNewDataViewModel(internalPurchaseOrder) }
            };
        }

        public async Task<ExternalPurchaseOrder> GetTestData(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = await GetNewData(user);

            await facade.Create(externalPurchaseOrder, user,7);

            return externalPurchaseOrder;
        }

        public async Task<ExternalPurchaseOrder> GetTestData2(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = await GetNewData(user);

            await facade.Create(externalPurchaseOrder, user, 7);
            externalPurchaseOrder.CreatedUtc = externalPurchaseOrder.CreatedUtc.AddDays(10);
            await facade.Update(Convert.ToInt32(externalPurchaseOrder.Id), externalPurchaseOrder, user);

            return externalPurchaseOrder;
        }

        public async Task<ExternalPurchaseOrder> GetTestData3(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = await GetNewData(user);

            await facade.Create(externalPurchaseOrder, user, 7);
            externalPurchaseOrder.CreatedUtc = externalPurchaseOrder.CreatedUtc.AddDays(16);
            await facade.Update(Convert.ToInt32(externalPurchaseOrder.Id), externalPurchaseOrder, user);

            return externalPurchaseOrder;
        }

        public async Task<ExternalPurchaseOrder> GetTestData4(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = await GetNewData(user);

            await facade.Create(externalPurchaseOrder, user, 7);
            externalPurchaseOrder.CreatedUtc = externalPurchaseOrder.CreatedUtc.AddDays(-40);
            await facade.Update(Convert.ToInt32(externalPurchaseOrder.Id), externalPurchaseOrder, user);

            return externalPurchaseOrder;
        }

        public async Task<ExternalPurchaseOrder> GetTestData5(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = await GetNewData(user);

            await facade.Create(externalPurchaseOrder, user, 7);
            externalPurchaseOrder.CreatedUtc = externalPurchaseOrder.CreatedUtc.AddDays(-70);
            await facade.Update(Convert.ToInt32(externalPurchaseOrder.Id), externalPurchaseOrder, user);

            return externalPurchaseOrder;
        }

        public async Task<ExternalPurchaseOrder> GetTestDataUnused(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = await GetNewData(user);
            externalPurchaseOrder.IsPosted = true;
            foreach (var item in externalPurchaseOrder.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.DOQuantity = 0;
                    detail.DealQuantity = 2;
                }
            }

            await facade.Create(externalPurchaseOrder, user,7);

            return externalPurchaseOrder;
        }

        public async Task<ExternalPurchaseOrder> GetTestDataMP(string user)
        {
            ExternalPurchaseOrder externalPurchaseOrder = await GetNewData(user);
            externalPurchaseOrder.IsPosted = true;
            foreach (var item in externalPurchaseOrder.Items)
            {
                foreach (var detail in item.Details)
                {
                    detail.ProductName = "KERTAS BURAM";
                }
            }

            await facade.Create(externalPurchaseOrder, user, 7);

            return externalPurchaseOrder;
        }
    }
}
    

