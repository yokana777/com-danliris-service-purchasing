using Com.DanLiris.Service.Purchasing.Lib.Configs.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.Moonlay.Data.EntityFrameworkCore;
using Com.Moonlay.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;

namespace Com.DanLiris.Service.Purchasing.Lib
{
    public class PurchasingDbContext : StandardDbContext
    {
        public PurchasingDbContext(DbContextOptions<PurchasingDbContext> options) : base(options)
        {
        }

        public DbSet<PurchasingDocumentExpedition> PurchasingDocumentExpeditions { get; set; }
        public DbSet<PurchasingDocumentExpeditionItem> PurchasingDocumentExpeditionItems { get; set; }
        public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
        public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }

        public DbSet<InternalPurchaseOrder> InternalPurchaseOrders { get; set; }
        public DbSet<InternalPurchaseOrderItem> InternalPurchaseOrderItems { get; set; }

        public DbSet<ExternalPurchaseOrder> ExternalPurchaseOrders { get; set; }
        public DbSet<ExternalPurchaseOrderItem> ExternalPurchaseOrderItems { get; set; }
        public DbSet<ExternalPurchaseOrderDetail> ExternalPurchaseOrderDetails { get; set; }

        public DbSet<BankExpenditureNoteModel> BankExpenditureNotes { get; set; }
        public DbSet<BankExpenditureNoteItemModel> BankExpenditureNoteItems { get; set; }
        public DbSet<BankExpenditureNoteDetailModel> BankExpenditureNoteDetails { get; set; }

        public DbSet<UnitReceiptNote> UnitReceiptNotes { get; set; }
        public DbSet<UnitReceiptNoteItem> UnitReceiptNoteItems { get; set; }

        public DbSet<DeliveryOrder> DeliveryOrders { get; set; }
        public DbSet<DeliveryOrderItem> DeliveryOrderItems { get; set; }
        public DbSet<DeliveryOrderDetail> DeliveryOrderDetails { get; set; }
        public object PPHBankExpenditureNoteItems { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PurchasingDocumentExpeditionConfig());

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
