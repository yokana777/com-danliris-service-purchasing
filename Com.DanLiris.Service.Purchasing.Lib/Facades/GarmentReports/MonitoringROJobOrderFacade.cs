using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel.CostCalculationGarment;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

        private async Task<Tuple<List<MonitoringROJobOrderViewModel>, CostCalculationGarmentViewModel>> GetData(long costCalculationId) {

            CostCalculationGarmentViewModel costCalculationGarmentViewModel = await GetCostCalculation(costCalculationId);

            if (costCalculationGarmentViewModel.CostCalculationGarment_Materials != null && costCalculationGarmentViewModel.CostCalculationGarment_Materials.Count() > 0)
            {
                HashSet<long> productIds = costCalculationGarmentViewModel.CostCalculationGarment_Materials.Select(m => m.Product.Id).ToHashSet();
                Dictionary<long, string> productNames = await GetProducts(productIds);

                var result = costCalculationGarmentViewModel.CostCalculationGarment_Materials.Select(m =>
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
                                        SupplierName = poDist.SupplierName,
                                        OverUsageReason = poDistDetail.OverUsageReason
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

                return new Tuple<List<MonitoringROJobOrderViewModel>, CostCalculationGarmentViewModel>(result, costCalculationGarmentViewModel);
            }

            throw new Exception("Tidak ada Product");
        }

        public async Task<List<MonitoringROJobOrderViewModel>> GetMonitoring(long costCalculationId)
        {
            var data = await GetData(costCalculationId);
            return data.Item1;
        }

        public async Task<Tuple<MemoryStream, string>> GetExcel(long costCalculationId)
        {
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "PO Serial Number", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Budget Quantity", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Beli", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Status", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO Master", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "PO Master", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Pembagian PO", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Ket. Kelebihan Barang", DataType = typeof(string) });

            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            var data = await GetData(costCalculationId);

            int rowPosition = 2;

            foreach (var d in data.Item1)
            {
                if (d.Items != null && d.Items.Count() > 0)
                {
                    var firstMergedRowPosition = rowPosition;
                    var lastMergedRowPosition = rowPosition;
                    foreach (var i in d.Items)
                    {
                        result.Rows.Add(d.POSerialNumber, d.ProductCode, d.ProductName, d.BudgetQuantity, d.UomPriceUnit, d.Status, i.ROMaster, i.POMaster, i.DistributionQuantity, i.UomCCUnit, i.DONo, i.SupplierName, i.OverUsageReason);
                        lastMergedRowPosition = rowPosition++;
                    }
                    foreach (var col in new[] { "A", "B", "C", "D", "E", "F" })
                    {
                        if (firstMergedRowPosition != lastMergedRowPosition)
                        {
                            mergeCells.Add(($"{col}{firstMergedRowPosition}:{col}{lastMergedRowPosition}", col == "D" ? ExcelHorizontalAlignment.Right : ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Bottom));
                        }
                    }
                }
                else
                {
                    result.Rows.Add(d.POSerialNumber, d.ProductCode, d.ProductName, d.BudgetQuantity, d.UomPriceUnit, d.Status, "", "", null, "", "", "", "");
                    rowPosition++;
                }
            }

            var xls = Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "RO Job Order", mergeCells) }, false);
            return new Tuple<MemoryStream, string>(xls, $"Monitoring RO Job Order - {data.Item2.RO_Number}");
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
        Task<Tuple<MemoryStream, string>> GetExcel(long costCalculationId);
    }
}
