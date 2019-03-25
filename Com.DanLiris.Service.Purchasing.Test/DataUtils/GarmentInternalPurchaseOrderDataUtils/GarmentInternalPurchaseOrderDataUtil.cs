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

        public async Task<List<GarmentInternalPurchaseOrder>> GetNewData()
        {
            return await Task.Run(() => garmentPurchaseRequestDataUtil.GetTestDataByTags());
        }

        public async Task<List<GarmentInternalPurchaseOrder>> GetTestData()
        {
            var data = await GetNewData();
            await facade.CreateMultiple(data, "Unit Test");
            return data;
        }

        public async Task<List<GarmentInternalPurchaseOrder>> GetTestDataByTags()
        {
            var testData = await GetTestData();
            var data = await GetNewData();
            await facade.CreateMultiple(data, "Unit Test");
            return facade.ReadByTags("accessories", null, DateTimeOffset.MinValue, DateTimeOffset.MinValue, "Unit Test");
        }

    }
}
