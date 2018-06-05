using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils
{
    public class InternalPurchaseOrderDataUtil
    {
        private InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil;
        private readonly InternalPurchaseOrderFacade facade;

        public InternalPurchaseOrderDataUtil(InternalPurchaseOrderItemDataUtil internalPurchaseOrderItemDataUtil, InternalPurchaseOrderFacade facade)
        {
            this.internalPurchaseOrderItemDataUtil = internalPurchaseOrderItemDataUtil;
            this.facade = facade;
        }

        public InternalPurchaseOrder GetNewData()
        {
            return new InternalPurchaseOrder
            {
                IsoNo = "",
                PRId = "PurchaseRequestId-1",
                PRNo = "PurchaseRequestNo-1",
                PRDate = DateTimeOffset.UtcNow,
                ExpectedDeliveryDate = DateTimeOffset.UtcNow,
                BudgetId = "BudgetId",
                BudgetCode = "BudgetCode",
                BudgetName = "BudgetName",
                UnitId = "UnitId",
                UnitCode = "UnitCode",
                UnitName = "UnitName",
                DivisionId = "DivisionId",
                DivisionCode = "DivisionCode",
                DivisionName = "DivisionName",
                CategoryId = "CategoryId",
                CategoryCode = "CategoryCode",
                CategoryName = "CategoryName",
                Remark = "Remark",
                Items = new List<InternalPurchaseOrderItem> { internalPurchaseOrderItemDataUtil.GetNewData() }
            };
        }

        public InternalPurchaseOrderViewModel GetNewDataViewModel()
        {
            return new InternalPurchaseOrderViewModel
            {
                prId = "PurchaseRequestId-1",
                prNo = "PurchaseRequestNo",
                prDate = DateTimeOffset.Now,
                expectedDeliveryDate = DateTimeOffset.Now,
                budget = new BudgetViewModel
                {
                    _id = "BudgetId",
                    code = "BudgetCode",
                    name = "BudgetName",
                },
                unit = new UnitViewModel
                {
                    _id = "UnitId",
                    code = "UnitCode",
                    name = "UnitName",
                    division = new DivisionViewModel
                    {
                        _id = "DivisionId",
                        code = "DivisionCode",
                        name = "DivisionName",
                    }
                },
                category = new CategoryViewModel
                {
                    _id = "CategoryId",
                    code = "CategoryCode",
                    name = "CategoryName",
                },
                remark = "Remark",
                items = new List<InternalPurchaseOrderItemViewModel> { internalPurchaseOrderItemDataUtil.GetNewDataViewModel() }
            };
        }

        public async Task<InternalPurchaseOrder> GetTestData(string user)
        {
            InternalPurchaseOrder internalPurchaseOrder = GetNewData();

            await facade.Create(internalPurchaseOrder, user);

            return internalPurchaseOrder;
        }

        //public PurchaseRequestViewModel GetViewModelTestData()
        //{
        //    PurchaseRequestViewModel viewModel = mapper.Map<PurchaseRequestViewModel>(GetNewData());

        //    return viewModel;
        //}
        //public PurchaseRequestViewModel GetViewModelFromModelTestData(PurchaseRequest model)
        //{
        //    PurchaseRequestViewModel viewModel = mapper.Map<PurchaseRequestViewModel>(model);

        //    return viewModel;
        //}
    }
}
