using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel.CostCalculationGarment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class MonitoringROJobOrderFacade : IMonitoringROJobOrderFacade
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly PurchasingDbContext dbContext;

        public MonitoringROJobOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
        }

        public async Task<List<MonitoringROJobOrderViewModel>> GetMonitoring(long costCalculationId)
        {

            CostCalculationGarmentViewModel costCalculationGarmentViewModel = await GetCostCalculation(costCalculationId);
            if (costCalculationGarmentViewModel.CostCalculationGarment_Materials != null)
            {
                HashSet<long> productIds = costCalculationGarmentViewModel.CostCalculationGarment_Materials.Select(m => m.Product.Id).ToHashSet();
                Dictionary<long, string> productNames = await GetProducts(productIds);

                return costCalculationGarmentViewModel.CostCalculationGarment_Materials.Select(m =>
                {
                    List<MonitoringROJobOrderItemViewModel> garmentPOMasterDistributions = new List<MonitoringROJobOrderItemViewModel>();
                    if (m.IsPRMaster.GetValueOrDefault())
                    {
                        var Query = from poDistDetail in dbContext.GarmentPOMasterDistributionDetails
                                    join poDistItem in dbContext.GarmentPOMasterDistributionItems on poDistDetail.POMasterDistributionItemId equals poDistItem.Id
                                    join poDist in dbContext.GarmentPOMasterDistributions on poDistItem.POMasterDistributionId equals poDist.Id
                                    join doDetail in dbContext.GarmentDeliveryOrderDetails on poDistItem.DODetailId equals doDetail.Id
                                    where poDistDetail.POSerialNumber == m.PO_SerialNumber
                                    select new MonitoringROJobOrderItemViewModel
                                    {
                                        ROMaster = doDetail.RONo,
                                        POMaster = doDetail.POSerialNumber,
                                        DistributionQuantity = poDistDetail.Quantity,
                                        Conversion = poDistDetail.Conversion,
                                        UomCCUnit = poDistDetail.UomCCUnit,
                                        DONo = poDist.DONo,
                                        SupplierName = poDist.SupplierName
                                    };
                        garmentPOMasterDistributions = Query.ToList();
                    }

                    return new MonitoringROJobOrderViewModel
                    {
                        POSerialNumber = m.PO_SerialNumber,
                        ProductCode = m.Product.Code,
                        ProductName = productNames.GetValueOrDefault(m.Product.Id),
                        BudgetQuantity = m.BudgetQuantity,
                        UomPriceUnit = m.UOMPrice.Unit,
                        Status = m.IsPRMaster.GetValueOrDefault() ? "MASTER" : "JOB ORDER",
                        Items = garmentPOMasterDistributions
                    };
                }).ToList();
            }
            throw new Exception("Tidak ada Product");
        }

        private async Task<CostCalculationGarmentViewModel> GetCostCalculation(long costCalculationId)
        {
            var httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));
            var response = await httpClient.GetAsync($"{APIEndpoint.Sales}cost-calculation-garments/{costCalculationId}");
            var content = await response.Content.ReadAsStringAsync();

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content) ?? new Dictionary<string, object>();
            if (response.IsSuccessStatusCode)
            {
                CostCalculationGarmentViewModel data = JsonConvert.DeserializeObject<CostCalculationGarmentViewModel>(result.GetValueOrDefault("data").ToString());
                return data;
            }
            else
            {
                throw new Exception(string.Concat("Failed Get CostCalculation : ", (string)result.GetValueOrDefault("error") ?? "- ", ". Message : ", (string)result.GetValueOrDefault("message") ?? "- ", ". Status : ", response.StatusCode, "."));
            }
        }

        private async Task<Dictionary<long, string>> GetProducts(HashSet<long> productIds)
        {
            var param = string.Join('&', productIds.Select(id => $"garmentProductList[]={id}"));
            var httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));
            var response = await httpClient.GetAsync($"{APIEndpoint.Core}master/garmentProducts/byId?{param}");
            var content = await response.Content.ReadAsStringAsync();

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content) ?? new Dictionary<string, object>();
            if (response.IsSuccessStatusCode)
            {
                List<GarmentProductViewModel> data = JsonConvert.DeserializeObject<List<GarmentProductViewModel>>(result.GetValueOrDefault("data").ToString());
                return data.ToDictionary(k => k.Id, v => v.Name);
            }
            else
            {
                throw new Exception(string.Concat("Failed Get Products : ", (string)result.GetValueOrDefault("error") ?? "- ", ". Message : ", (string)result.GetValueOrDefault("message") ?? "- ", ". Status : ", response.StatusCode, "."));
            }
        }
    }

    public interface IMonitoringROJobOrderFacade
    {
        Task<List<MonitoringROJobOrderViewModel>> GetMonitoring(long costCalculationId);
    }
}
