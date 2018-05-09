using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.Moonlay.NetCore.Lib.Service;
using System;

namespace Com.DanLiris.Service.Purchasing.Lib.Services.Expedition
{
    public class PurchasingDocumentExpeditionService : StandardEntityService<PurchasingDbContext, PurchasingDocumentExpedition>
    {
        private string AGENT = "Service";

        public PurchasingDocumentExpeditionService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override void OnCreating(PurchasingDocumentExpedition model)
        {
            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));

            base.OnCreating(model);
            model._CreatedAgent = AGENT;
            model._CreatedBy = identityService.Username;
            model._LastModifiedAgent = AGENT;
            model._LastModifiedBy = identityService.Username;
        }

        public override void OnUpdating(int id, PurchasingDocumentExpedition model)
        {
            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));

            base.OnUpdating(id, model);
            model._LastModifiedAgent = AGENT;
            model._LastModifiedBy = identityService.Username;
        }

        public override void OnDeleting(PurchasingDocumentExpedition model)
        {
            IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));

            base.OnDeleting(model);
            model._DeletedAgent = AGENT;
            model._DeletedBy = identityService.Username;
        }
    }
}
