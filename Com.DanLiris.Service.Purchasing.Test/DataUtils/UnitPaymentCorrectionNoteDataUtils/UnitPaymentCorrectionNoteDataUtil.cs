using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentCorrectionNoteDataUtils
{
    public class UnitPaymentCorrectionNoteDataUtil
    {
        private UnitPaymentOrderDataUtil unitPaymentOrderDataUtil;
        private readonly UnitPaymentQuantityCorrectionNoteFacade facade;

        public UnitPaymentCorrectionNoteDataUtil(UnitPaymentOrderDataUtil unitPaymentOrderDataUtil, UnitPaymentQuantityCorrectionNoteFacade facade)
        {
            this.unitPaymentOrderDataUtil = unitPaymentOrderDataUtil;
            this.facade = facade;
        }

        public UnitPaymentCorrectionNote GetNewData()
        {
            Lib.Models.UnitPaymentOrderModel.UnitPaymentOrder unitPaymentOrder = Task.Run(() => this.unitPaymentOrderDataUtil.GetTestData()).Result;

            List<UnitPaymentCorrectionNoteItem> unitPaymentCorrectionNoteItem = new List<UnitPaymentCorrectionNoteItem>();
            foreach (var item in unitPaymentOrder.Items)
            {
                foreach (var detail in item.Details)
                {
                    unitPaymentCorrectionNoteItem.Add(new UnitPaymentCorrectionNoteItem
                    {
                        UPODetailId = detail.Id,
                        URNNo = item.URNNo,
                        EPONo = detail.EPONo,
                        PRId = detail.PRId,
                        PRNo = detail.PRNo,
                        PRDetailId = detail.PRItemId,
                        ProductId = detail.ProductId,
                        ProductCode = detail.ProductCode,
                        ProductName = detail.ProductName,
                        UomId = detail.UomId,
                        UomUnit = detail.UomUnit,
                        PricePerDealUnitBefore = (long)detail.PricePerDealUnitCorrection,
                        PriceTotalBefore = (long)detail.PriceTotalCorrection
                    });
                }
            }

            UnitPaymentCorrectionNote unitPaymentCorrectionNote = new UnitPaymentCorrectionNote
            {
                DivisionId = "DivisionId",
                DivisionCode = "DivisionCode",
                DivisionName = "DivisionName",

                SupplierId = "SupplierId",
                SupplierCode = "SupplierCode",
                SupplierName = "SupplierName",

                UPCNo = "18-06-G-NKI-001",
                UPOId = 1,

                UPONo = "18-06-G-NKI-001",

                CorrectionDate = new DateTimeOffset(),

                CorrectionType = null,

                InvoiceCorrectionDate = new DateTimeOffset(),
                InvoiceCorrectionNo = "123456",

                useVat = true,
                VatTaxCorrectionDate = new DateTimeOffset(),
                VatTaxCorrectionNo = null,

                useIncomeTax = true,
                IncomeTaxCorrectionName = null,
                IncomeTaxCorrectionNo = null,

                ReleaseOrderNoteNo = "123456",
                ReturNoteNo = "",

                CategoryId = "CategoryId ",
                CategoryCode = "CategoryCode",
                CategoryName = "CategoryName",

                Remark = null,

                DueDate = new DateTimeOffset(), // ???

                Items = unitPaymentCorrectionNoteItem
            };
            return unitPaymentCorrectionNote;
        }

        public async Task<UnitPaymentCorrectionNote> GetTestData()
        {
            var data = GetNewData();
            await facade.Create(data, "Unit Test", 7);
            return data;
        }
    }
}
