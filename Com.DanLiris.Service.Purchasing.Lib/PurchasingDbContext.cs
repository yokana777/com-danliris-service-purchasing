using Com.DanLiris.Service.Purchasing.Lib.Configs.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.Moonlay.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankDocumentNumber;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchasingDispositionModel;

namespace Com.DanLiris.Service.Purchasing.Lib
{
    public class PurchasingDbContext : StandardDbContext
    {
        public PurchasingDbContext(DbContextOptions<PurchasingDbContext> options) : base(options)
        {
        }

        public DbSet<PurchasingDocumentExpedition> PurchasingDocumentExpeditions { get; set; }
        public DbSet<PurchasingDocumentExpeditionItem> PurchasingDocumentExpeditionItems { get; set; }
        public DbSet<PPHBankExpenditureNote> PPHBankExpenditureNotes { get; set; }
        public DbSet<PPHBankExpenditureNoteItem> PPHBankExpenditureNoteItems { get; set; }


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

        public DbSet<UnitPaymentOrder> UnitPaymentOrders { get; set; }
        public DbSet<UnitPaymentOrderItem> UnitPaymentOrderItems { get; set; }
        public DbSet<UnitPaymentOrderDetail> UnitPaymentOrderDetails { get; set; }

        public DbSet<BankDocumentNumber> BankDocumentNumbers { get; set; }

        public DbSet<UnitPaymentCorrectionNote> UnitPaymentCorrectionNotes { get; set; }
        public DbSet<UnitPaymentCorrectionNoteItem> UnitPaymentCorrectionNoteItems { get; set; }

        public DbSet<GarmentPurchaseRequest> GarmentPurchaseRequests { get; set; }
        public DbSet<GarmentPurchaseRequestItem> GarmentPurchaseRequestItems { get; set; }

        public DbSet<GarmentInternalPurchaseOrder> GarmentInternalPurchaseOrders { get; set; }
        public DbSet<GarmentInternalPurchaseOrderItem> GarmentInternalPurchaseOrderItems { get; set; }

        public DbSet<GarmentExternalPurchaseOrder> GarmentExternalPurchaseOrders { get; set; }
        public DbSet<GarmentExternalPurchaseOrderItem> GarmentExternalPurchaseOrderItems { get; set; }

        public DbSet<GarmentDeliveryOrder> GarmentDeliveryOrders { get; set; }
        public DbSet<GarmentDeliveryOrderItem> GarmentDeliveryOrderItems { get; set; }
        public DbSet<GarmentDeliveryOrderDetail> GarmentDeliveryOrderDetails { get; set; }
        public DbSet<GarmentInvoice> GarmentInvoices { get; set; }
        public DbSet<GarmentInvoiceItem> GarmentInvoiceItems { get; set; }
        public DbSet<GarmentInvoiceDetail> GarmentInvoiceDetails { get; set; }
        public DbSet<GarmentInternNote> GarmentInternNotes { get; set; }
        public DbSet<GarmentInternNoteItem> GarmentInternNoteItems { get; set; }
        public DbSet<GarmentInternNoteDetail> GarmentInternNoteDetails { get; set; }
        public DbSet<PurchasingDisposition> PurchasingDispositions { get; set; }
        public DbSet<PurchasingDispositionItem> PurchasingDispositionItems { get; set; }
        public DbSet<PurchasingDispositionDetail> PurchasingDispositionDetails { get; set; }

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
