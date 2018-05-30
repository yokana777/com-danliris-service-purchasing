using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils
{
    public class PurchaseRequestDataUtil
    {
        private PurchaseRequestItemDataUtil purchaseRequestItemDataUtil;
        private readonly IMapper mapper;
        private readonly PurchaseRequestFacade facade;

        public PurchaseRequestDataUtil(PurchaseRequestItemDataUtil purchaseRequestItemDataUtil, IMapper mapper, PurchaseRequestFacade facade)
        {
            this.purchaseRequestItemDataUtil = purchaseRequestItemDataUtil;
            this.mapper = mapper;
            this.facade = facade;
        }

        public PurchaseRequest GetNewData()
        {
            return new PurchaseRequest
            {
                No = "No1",
                Date = DateTimeOffset.Now,
                ExpectedDeliveryDate = DateTimeOffset.Now,
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
                Items = new List<PurchaseRequestItem> { purchaseRequestItemDataUtil.GetNewData() }
            };
        }

        public async Task<PurchaseRequest> GetTestData(string user)
        {
            PurchaseRequest purchaseRequest = GetNewData();

            await facade.Create(purchaseRequest, user);

            return purchaseRequest;
        }

        public PurchaseRequestViewModel GetViewModelTestData()
        {
            PurchaseRequestViewModel viewModel = mapper.Map<PurchaseRequestViewModel>(GetNewData());

            return viewModel;
        }
        public PurchaseRequestViewModel GetViewModelFromModelTestData(PurchaseRequest model)
        {
            PurchaseRequestViewModel viewModel = mapper.Map<PurchaseRequestViewModel>(model);

            return viewModel;
        }
    }
}
