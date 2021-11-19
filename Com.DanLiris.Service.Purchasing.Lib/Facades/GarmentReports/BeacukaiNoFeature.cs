using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System.Net.Http;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Newtonsoft.Json;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class BeacukaiNoFeature : IBeacukaiNoFeature
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSet;

        public BeacukaiNoFeature(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentDeliveryOrder>();
        }

        //public Tuple<List<BeacukaiNoFeatureViewModel>, int> GetBeacukaiNoReport(string filter, string keyword)
        //{
            //var Query = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset);
            //Query = Query.OrderByDescending(x => x.SupplierName).ThenBy(x => x.Dono);
            //List<BeacukaiNoFeatureViewModel> Data = GetBeacukaiNo(filter, keyword);
            //Data = Data.OrderByDescending(x => x.KodeBarang).ToList();
            //int TotalData = Data.Count();
            //return Tuple.Create(Data, Data.Count());
        //}

        public List<BeacukaiNoFeatureViewModel> GetBeacukaiNo(string filter, string keyword)
        {
            var Query = filter == "BCNo" ? from a in dbContext.GarmentBeacukais
                                           join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                                           join c in dbContext.GarmentDeliveryOrders on b.GarmentDOId equals c.Id
                                           join d in dbContext.GarmentDeliveryOrderItems on c.Id equals d.GarmentDOId
                                           join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                                           where a.BeacukaiNo == keyword
                                           //&& a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false
                                           //&& e.IsDeleted == false
                                           select new BeacukaiNoFeatureViewModel
                                           {
                                               BCType = a.CustomsType,
                                               BCDate = a.BeacukaiDate.DateTime,
                                               ProductCode = e.ProductCode,
                                               PO = e.POSerialNumber,
                                               BCNo = a.BeacukaiNo
                                           }
                         : filter == "PONo" ? from a in dbContext.GarmentBeacukais
                                              join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                                              join c in dbContext.GarmentDeliveryOrders on b.GarmentDOId equals c.Id
                                              join d in dbContext.GarmentDeliveryOrderItems on c.Id equals d.GarmentDOId
                                              join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                                              where e.POSerialNumber == keyword
                                              //&& a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false
                                              //&& e.IsDeleted == false
                                              select new BeacukaiNoFeatureViewModel
                                              {
                                                  BCType = a.CustomsType,
                                                  BCDate = a.BeacukaiDate.DateTime,
                                                  ProductCode = e.ProductCode,
                                                  PO = e.POSerialNumber,
                                                  DONo = c.DONo,
                                                  QtyBC = e.SmallQuantity,
                                                  BCNo = a.BeacukaiNo
                                              } :
                                              //from a in dbContext.GarmentBeacukais
                                              //join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                                              from c in dbContext.GarmentDeliveryOrders
                                              join d in dbContext.GarmentDeliveryOrderItems on c.Id equals d.GarmentDOId
                                              join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                                              where e.RONo == keyword
                                              //&& a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false
                                              //&& e.IsDeleted == false
                                              select new BeacukaiNoFeatureViewModel
                                              {
                                                  //BCType = a.CustomsType,
                                                  //BCDate = a.BeacukaiDate.DateTime,
                                                  ProductCode = e.ProductCode,
                                                  PO = e.POSerialNumber,
                                                  DONo = c.DONo,
                                                  QtyBC = e.SmallQuantity,
                                              };

            var ProductCode = string.Join(",", Query.Select(x => x.ProductCode).Distinct().ToList());

            var Code = GetProductCode(ProductCode);

            var Query2 = from a in Query
                         join b in Code on a.ProductCode equals b.Code into Codes
                         from code in Codes.DefaultIfEmpty()
                         select new BeacukaiNoFeatureViewModel
                         {
                             BCType = a.BCType,
                             BCDate = a.BCDate,
                             ProductCode = a.ProductCode,
                             PO = a.PO,
                             DONo = a.DONo,
                             QtyBC = a.QtyBC,
                             Composition = code != null ? code.Composition : "-",
                             Construction = code != null ? code.Const : "-",
                             BCNo = a.BCNo,
                         };

            return Query2.ToList();
        }

        private List<GarmentProductViewModel> GetProductCode(string codes)
        {
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));

            var httpContent = new StringContent(JsonConvert.SerializeObject(codes), Encoding.UTF8, "application/json");

            var garmentProductionUri = APIEndpoint.Core + $"master/garmentProducts/byCode";
            var httpResponse = httpClient.SendAsync(HttpMethod.Get, garmentProductionUri, httpContent).Result;

            List<GarmentProductViewModel> viewModel = new List<GarmentProductViewModel>();

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = httpResponse.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

                viewModel = JsonConvert.DeserializeObject<List<GarmentProductViewModel>>(result.GetValueOrDefault("data").ToString());

            }

            return viewModel;

        }
    }
}
