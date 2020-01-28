using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.Moonlay.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades
{
    public class GarmentPurchaseRequestItemFacade : IGarmentPurchaseRequestItemFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentPurchaseRequestItem> dbSet;

        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        public GarmentPurchaseRequestItemFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentPurchaseRequestItem>();
        }

        public async Task<int> Patch(string id, JsonPatchDocument<GarmentPurchaseRequestItem> jsonPatch)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var IDs = JsonConvert.DeserializeObject<List<long>>(id);
                    foreach (var ID in IDs)
                    {
                        var data = dbSet.Where(d => d.Id == ID)
                            .Single();

                        EntityExtension.FlagForUpdate(data, identityService.Username, USER_AGENT);

                        jsonPatch.ApplyTo(data);
                    }

                    Updated = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return Updated;
        }
    }
}
