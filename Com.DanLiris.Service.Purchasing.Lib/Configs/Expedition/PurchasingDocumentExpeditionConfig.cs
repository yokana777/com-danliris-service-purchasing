using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Com.DanLiris.Service.Purchasing.Lib.Configs.Expedition
{
    public class PurchasingDocumentExpeditionConfig : IEntityTypeConfiguration<PurchasingDocumentExpedition>
    {
        public void Configure(EntityTypeBuilder<PurchasingDocumentExpedition> builder)
        {
            builder.Property(p => p.UnitPaymentOrderNo).HasMaxLength(255);
        }
    }
}
