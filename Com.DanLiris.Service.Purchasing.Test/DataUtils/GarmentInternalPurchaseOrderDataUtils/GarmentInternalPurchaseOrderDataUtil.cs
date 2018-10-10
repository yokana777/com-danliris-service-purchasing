using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils
{
    public class GarmentInternalPurchaseOrderDataUtil
    {
        private readonly GarmentInternalPurchaseOrderFacade facade;
        private readonly GarmentPurchaseRequestDataUtil garmentPurchaseRequestDataUtil;

        public GarmentInternalPurchaseOrderDataUtil(GarmentInternalPurchaseOrderFacade facade, GarmentPurchaseRequestDataUtil garmentPurchaseRequestDataUtil)
        {
            this.facade = facade;
            this.garmentPurchaseRequestDataUtil = garmentPurchaseRequestDataUtil;
        }

        public List<GarmentInternalPurchaseOrder> GetNewData()
        {
            return Task.Run(() => garmentPurchaseRequestDataUtil.GetTestDataByTags()).Result;
        }

        public async Task<List<GarmentInternalPurchaseOrder>> GetTestData()
        {
            var data = GetNewData();
            await facade.CreateMultiple(data, "Unit Test");
            return data;
        }

    }
}
