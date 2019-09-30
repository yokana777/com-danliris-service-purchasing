using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel.CostCalculationGarment;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class MonitoringROMasterFacade : IMonitoringROMasterFacade
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly PurchasingDbContext dbContext;

        public MonitoringROMasterFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
        }

        private List<MonitoringROMasterViewModel> GetData(long prId)
        {
            var result = (from pri in dbContext.GarmentPurchaseRequestItems
                          join epoi in dbContext.GarmentExternalPurchaseOrderItems on pri.PO_SerialNumber equals epoi.PO_SerialNumber
                          where pri.GarmentPRId == prId && pri.PO_SerialNumber != null
                          select new MonitoringROMasterViewModel
                          {
                              POMaster = pri.PO_SerialNumber,
                              ProductCode = pri.ProductCode,
                              ProductName = pri.ProductName,
                              Quantity = pri.Quantity,
                              UomUnit = pri.UomUnit,
                              DealQuantity = epoi.DealQuantity,
                              DealUomUnit = epoi.DealUomUnit,
                          }).ToList();

            var poMasters = result.Select(s => s.POMaster).ToList();

            var deliveryOrders = (from gdo in dbContext.GarmentDeliveryOrders
                                  join gdoi in dbContext.GarmentDeliveryOrderItems on gdo.Id equals gdoi.GarmentDOId
                                  join gdod in dbContext.GarmentDeliveryOrderDetails on gdoi.Id equals gdod.GarmentDOItemId
                                  where poMasters.Contains(gdod.POSerialNumber)
                                  select new
                                  {
                                      gdo.DONo,
                                      gdo.SupplierName,
                                      gdod.DOQuantity,
                                      gdod.Id,
                                      gdod.POSerialNumber
                                  }).ToList();

            var doDetailIds = deliveryOrders.Select(s => s.Id).ToList();

            var distributions = (from distItem in dbContext.GarmentPOMasterDistributionItems
                                 join distDetail in dbContext.GarmentPOMasterDistributionDetails on distItem.Id equals distDetail.POMasterDistributionItemId
                                 where doDetailIds.Contains(distItem.DODetailId)
                                 select new
                                 {
                                     distItem.DODetailId,
                                     distDetail.RONo,
                                     distDetail.POSerialNumber,
                                     distDetail.Quantity,
                                     distDetail.Conversion,
                                     distDetail.UomUnit
                                 }).ToList();


            if (result != null && result.Count() > 0)
            {
                Parallel.ForEach(result, r =>
                {
                    r.DeliveryOrders = deliveryOrders.Where(w => w.POSerialNumber == r.POMaster).Select(deliveryOrder => new MonitoringROMasterDeliveryOrderViewModel
                    {
                        DONo = deliveryOrder.DONo,
                        SupplierName = deliveryOrder.SupplierName,
                        DOQuantity = deliveryOrder.DOQuantity,
                        Distributions = distributions.Where(w => w.DODetailId == deliveryOrder.Id).Select(dist => new MonitoringROMasterDistributionViewModel
                        {
                            RONo = dist.RONo,
                            POSerialNumber = dist.POSerialNumber,
                            DistributionQuantity = dist.Quantity * (decimal)dist.Conversion,
                            UomUnit = dist.UomUnit
                        }).ToList()
                    }).ToList();
                });
            }

            return result;
        }

        public List<MonitoringROMasterViewModel> GetMonitoring(long prId)
        {
            var data = GetData(prId);
            return data;
        }

        public Tuple<MemoryStream, string> GetExcel(long prId)
        {
            var RONo = dbContext.GarmentPurchaseRequests
                .Where(w => w.Id == prId)
                .Select(s => s.RONo)
                .Single();

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "Column", DataType = typeof(string) });

            List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

            var xls = Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, RONo, mergeCells) }, false);
            return new Tuple<MemoryStream, string>(xls, $"Monitoring RO Master - {RONo}");
        }
    }

    public interface IMonitoringROMasterFacade
    {
        List<MonitoringROMasterViewModel> GetMonitoring(long prId);
        Tuple<MemoryStream, string> GetExcel(long prId);
    }
}
