using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class VBRequestPOExternalService : IVBRequestPOExternalService
    {
        private readonly PurchasingDbContext _dbContext;

        public VBRequestPOExternalService(PurchasingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<POExternalDto> ReadPOExternal(string keyword, string division, string currencyCode)
        {
            var result = new List<POExternalDto>();

            if (division.ToUpper() == "GARMENT")
            {
                var query = _dbContext.GarmentExternalPurchaseOrders.Include(entity => entity.Items).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(entity => entity.EPONo.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(currencyCode))
                {
                    query = query.Where(entity => entity.CurrencyCode == currencyCode);
                }

                var queryResult = query.Take(10).ToList();

                var epoIdAndPOIds = queryResult.SelectMany(element => element.Items).Select(element => new EPOIdAndPOId() { EPOId = element.GarmentEPOId, POId = element.POId }).ToList();
                var poIds = epoIdAndPOIds.Select(element => element.POId).ToList();
                var purchaseOrders = _dbContext.GarmentInternalPurchaseOrders.Where(entity => poIds.Contains(entity.Id)).ToList();
                //var internalPOs = _dbContext

                result = queryResult.Select(entity => new POExternalDto(entity, purchaseOrders)).ToList();
            }
            else
            {
                var query = _dbContext.ExternalPurchaseOrders.Include(entity => entity.Items).ThenInclude(entity => entity.Details).AsQueryable();
                
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(entity => entity.EPONo.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(currencyCode))
                {
                    query = query.Where(entity => entity.CurrencyCode == currencyCode);
                }

                var queryResult = query.Take(10).ToList();
                result = queryResult.Select(entity => new POExternalDto(entity)).ToList();
            }

            return result;
        }
    }

    public class EPOIdAndPOId
    {
        public long EPOId { get; set; }
        public long POId { get; set; }
    }
}
