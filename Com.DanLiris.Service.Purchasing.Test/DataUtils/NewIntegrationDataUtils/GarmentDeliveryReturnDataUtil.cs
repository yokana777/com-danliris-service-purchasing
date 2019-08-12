using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.NewIntegrationDataUtils
{
    public class GarmentDeliveryReturnDataUtil
    {
        public GarmentDeliveryReturnViewModel GetNewData()
        {
            long nowTicks = DateTimeOffset.Now.Ticks;

            var data = new GarmentDeliveryReturnViewModel
            {
                DRNo = $"DRNO{nowTicks}",
                RONo = $"RONO{nowTicks}",
                Items= new List<GarmentDeliveryReturnItemViewModel>
                {
                    new GarmentDeliveryReturnItemViewModel
                    {
                        Quantity=1,
                        IsSave=true,
                    }
                }
            };
            return data;
        }

        public Dictionary<string, object> GetResultFormatterOk()
        {
            var data = GetNewData();

            Dictionary<string, object> result =
                new ResultFormatter("1.0", General.OK_STATUS_CODE, General.OK_MESSAGE)
                .Ok(data);

            return result;
        }

        public string GetResultFormatterOkString()
        {
            var result = GetResultFormatterOk();

            return JsonConvert.SerializeObject(result);
        }
    }
}
