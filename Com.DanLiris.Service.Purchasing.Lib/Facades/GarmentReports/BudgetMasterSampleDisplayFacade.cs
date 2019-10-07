using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class BudgetMasterSampleDisplayFacade : IBudgetMasterSampleDisplayFacade
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly PurchasingDbContext dbContext;

        public BudgetMasterSampleDisplayFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
        }

        private List<BudgetMasterSampleDisplayViewModel> GetData(long prId)
        {
            var Query = dbContext.GarmentPurchaseRequestItems
                .Where(i => i.GarmentPRId == prId);

            var result = Query.Select(i => new BudgetMasterSampleDisplayViewModel
            {
                ProductCode = i.ProductCode,
                Remark = i.ProductRemark,
                Quantity = i.Quantity * i.PriceConversion,
                Uom = i.PriceUomUnit,
                Price = i.BudgetPrice,
                POSerialNumber = i.PO_SerialNumber
            });

            return result.ToList();
        }

        public List<BudgetMasterSampleDisplayViewModel> GetMonitoring(long prId)
        {
            var data = GetData(prId);
            return data;
        }

        public Tuple<MemoryStream, string> GetExcel(long prId)
        {
            var garmentpurchaseRequest = dbContext.GarmentPurchaseRequests
                .Where(w => w.Id == prId)
                .Select(s => new {
                    s.RONo,
                    s.PRType
                })
                .Single();

            var data = GetData(prId);

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "Urut", DataType = typeof(int) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Quantity", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor PO", DataType = typeof(string) });

            foreach (var i in Enumerable.Range(0, (data ?? new List<BudgetMasterSampleDisplayViewModel>()).Count()))
            {
                var d = data[i];
                result.Rows.Add(i + 1, d.ProductCode, d.Remark, d.Quantity, d.Uom, d.Price, d.Quantity * d.Price, d.POSerialNumber);
            }

            var excel = Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, garmentpurchaseRequest.RONo) }, false);

            return new Tuple<MemoryStream, string>(excel, string.Concat("Display Budget ", garmentpurchaseRequest.PRType, " - ", garmentpurchaseRequest.RONo));
        }
    }

    public interface IBudgetMasterSampleDisplayFacade
    {
        List<BudgetMasterSampleDisplayViewModel> GetMonitoring(long prId);
        Tuple<MemoryStream, string> GetExcel(long prId);
    }
}
