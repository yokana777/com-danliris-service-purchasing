using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Services.Expedition;
using Com.Moonlay.NetCore.Lib;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class PurchasingDocumentExpeditionFacade : IReadable, IDeleteable
    {
        public readonly PurchasingDocumentExpeditionService purchasingDocumentExpeditionService;

        public PurchasingDocumentExpeditionFacade(PurchasingDocumentExpeditionService purchasingDocumentExpeditionService)
        {
            this.purchasingDocumentExpeditionService = purchasingDocumentExpeditionService;
        }

        public Tuple<List<object>, int, Dictionary<string, string>> Read(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            IQueryable<PurchasingDocumentExpedition> Query = this.purchasingDocumentExpeditionService.DbSet;

            Query = Query
                .Select(s => new PurchasingDocumentExpedition
                {
                    Id = s.Id,
                    UnitPaymentOrderNo = s.UnitPaymentOrderNo,
                    Supplier = s.Supplier,
                    Division = s.Division,
                    Position = s.Position,
                    _LastModifiedUtc = s._LastModifiedUtc
                });

            List<string> searchAttributes = new List<string>()
            {
                "UnitPaymentOrderNo", "Supplier", "Division"
            };

            Query = QueryHelper<PurchasingDocumentExpedition>.ConfigureSearch(Query, searchAttributes, keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter);
            Query = QueryHelper<PurchasingDocumentExpedition>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            Query = QueryHelper<PurchasingDocumentExpedition>.ConfigureOrder(Query, OrderDictionary);

            Pageable<PurchasingDocumentExpedition> pageable = new Pageable<PurchasingDocumentExpedition>(Query, page - 1, size);
            List<PurchasingDocumentExpedition> Data = pageable.Data.ToList<PurchasingDocumentExpedition>();
            int TotalData = pageable.TotalCount;

            List<object> list = new List<object>();
            list.AddRange(
               Data.Select(s => new
               {
                   Id = s.Id,
                   UnitPaymentOrderNo = s.UnitPaymentOrderNo,
                   Supplier = s.Supplier,
                   Division = s.Division,
                   _LastModifiedUtc = s._LastModifiedUtc
               }).ToList()
            );

            return Tuple.Create(list, TotalData, OrderDictionary);
        }

        public async Task<int> Delete(int id)
        {
            int Count = 0;

            if (!this.purchasingDocumentExpeditionService.IsExists(id))
            {
                return 0;
            }

            using (var transaction = this.purchasingDocumentExpeditionService.DbContext.Database.BeginTransaction())
            {
                try
                {
                    PurchasingDocumentExpedition purchasingDocumentExpedition = purchasingDocumentExpeditionService.DbSet.AsNoTracking().Single(p => p.Id == id);
                    Count = await this.purchasingDocumentExpeditionService.DeleteAsync(id);
                    UpdateUnitPaymentOrderPosition(new List<string>() { purchasingDocumentExpedition.UnitPaymentOrderNo }, ExpeditionPosition.PURCHASING_DIVISION);

                    transaction.Commit();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Count;
        }

        public async Task<int> SendToVerification(object list, string username)
        {
            int Created = 0;

            using (var transaction = this.purchasingDocumentExpeditionService.DbContext.Database.BeginTransaction())
            {
                try
                {
                    List<string> unitPaymentOrders = new List<string>();

                    foreach (PurchasingDocumentExpedition purchasingDocumentExpedition in (List<PurchasingDocumentExpedition>)list)
                    {
                        unitPaymentOrders.Add(purchasingDocumentExpedition.UnitPaymentOrderNo);
                        purchasingDocumentExpedition.Position = ExpeditionPosition.SEND_TO_VERIFICATION_DIVISION;
                        purchasingDocumentExpedition.Active = true;
                        purchasingDocumentExpedition.SendToVerificationDivisionBy = username;
                        Created += await this.purchasingDocumentExpeditionService.CreateAsync(purchasingDocumentExpedition);
                    }

                    UpdateUnitPaymentOrderPosition(unitPaymentOrders, ExpeditionPosition.SEND_TO_VERIFICATION_DIVISION);

                    transaction.Commit();
                }
                catch (ServiceValidationExeption e)
                {
                    transaction.Rollback();
                    throw new ServiceValidationExeption(e.ValidationContext, e.ValidationResults);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Created;
        }

        public void UpdateUnitPaymentOrderPosition(List<string> unitPaymentOrders, ExpeditionPosition position)
        {
            string unitPaymentOrderUri = "unit-payment-orders/update/position";

            var data = new
            {
                position = position,
                unitPaymentOrders = unitPaymentOrders
            };

            HttpClientService httpClient = (HttpClientService)this.purchasingDocumentExpeditionService.ServiceProvider.GetService(typeof(HttpClientService));
            var response = httpClient.PutAsync($"{APIEndpoint.Purchasing}{unitPaymentOrderUri}", new StringContent(JsonConvert.SerializeObject(data).ToString(), Encoding.UTF8, General.JsonMediaType)).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
