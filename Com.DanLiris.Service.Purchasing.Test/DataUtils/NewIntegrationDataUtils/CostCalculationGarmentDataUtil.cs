using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel.CostCalculationGarment;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.NewIntegrationDataUtils
{
    public class CostCalculationGarmentDataUtil
    {
        public CostCalculationGarmentViewModel GetNewData()
        {
            long nowTicks = DateTimeOffset.Now.Ticks;

            var data = new CostCalculationGarmentViewModel
            {
                RO_Number = $"RO_Number{nowTicks}",
                CostCalculationGarment_Materials = new List<CostCalculationGarment_MaterialViewModel>
                {
                    new CostCalculationGarment_MaterialViewModel
                    {
                        PO_SerialNumber = $"PO_SerialNumber{nowTicks}",
                        Product = new GarmentProductViewModel {
                            Id = 1,
                            Code = $"ProductCode{nowTicks}",
                        },
                        BudgetQuantity = 100,
                        UOMPrice = new UomViewModel {
                            Id = "1",
                            Unit = $"UOMPriceUnit{nowTicks}",
                        },
                        IsPRMaster = true,
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
