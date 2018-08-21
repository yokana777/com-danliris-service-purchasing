using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Serializers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.PurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.InternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using Xunit;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.BankExpenditureNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade.Reports;

namespace Com.DanLiris.Service.Purchasing.Test
{
    public class ServiceProviderFixture
    {
        public IServiceProvider ServiceProvider { get; private set; }

        private void RegisterEndpoints(IConfigurationRoot Configuration)
        {
            APIEndpoint.Purchasing = Configuration.GetValue<string>(Constant.PURCHASING_ENDPOINT) ?? Configuration[Constant.PURCHASING_ENDPOINT];
        }

        private void RegisterSerializationProvider()
        {
            BsonSerializer.RegisterSerializationProvider(new SerializationProvider());
        }

        private void RegisterClassMap()
        {
            ClassMap<UnitReceiptNoteViewModel>.Register();
            ClassMap<UnitReceiptNoteItemViewModel>.Register();
            ClassMap<UnitViewModel>.Register();
            ClassMap<DivisionViewModel>.Register();
            ClassMap<CategoryViewModel>.Register();
            ClassMap<ProductViewModel>.Register();
            ClassMap<UomViewModel>.Register();
            ClassMap<PurchaseOrderViewModel>.Register();
            ClassMap<SupplierViewModel>.Register();
            ClassMap<UnitPaymentOrderUnpaidViewModel>.Register();
        }

        public ServiceProviderFixture()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(Constant.SECRET, "DANLIRISTESTENVIRONMENT"),
                    new KeyValuePair<string, string>("ASPNETCORE_ENVIRONMENT", "Test"),
                    new KeyValuePair<string, string>(Constant.DEFAULT_CONNECTION, "Server=localhost,1401;Database=com.danliris.db.purchasing.service.test;User Id=sa;Password=Standar123.;MultipleActiveResultSets=True;"),
                    new KeyValuePair<string, string>(Constant.MONGODB_CONNECTION, "mongodb://localhost:27017/admin")
                })
                .Build();

            RegisterEndpoints(configuration);
            string connectionString = configuration.GetConnectionString(Constant.DEFAULT_CONNECTION) ?? configuration[Constant.DEFAULT_CONNECTION];

            this.ServiceProvider = new ServiceCollection()
                .AddDbContext<PurchasingDbContext>((serviceProvider, options) =>
                {
                    options.UseSqlServer(connectionString);
                }, ServiceLifetime.Transient)
                .AddTransient<PurchasingDocumentExpeditionFacade>()
                .AddTransient<PurchasingDocumentExpeditionReportFacade>()
                .AddTransient<ImportPurchasingBookReportFacade>()
                .AddTransient<LocalPurchasingBookReportFacade>()
                .AddTransient<SendToVerificationDataUtil>()

                .AddTransient<UnitPaymentOrderUnpaidReportFacade>()
                .AddTransient<UnitPaymentOrderNotVerifiedReportFacade>()
                .AddTransient<PurchasingDocumentAcceptanceDataUtil>()
                .AddTransient<UnitPaymentOrderUnpaidReportDataUtil>()
                .AddTransient<UnitReceiptNoteImportFalseBsonDataUtil>()

                .AddTransient<PurchaseRequestFacade>()
                .AddTransient<PurchaseRequestDataUtil>()
                .AddTransient<PurchaseRequestItemDataUtil>()


                .AddTransient<PurchaseOrderMonitoringAllFacade>()

                .AddTransient<InternalPurchaseOrderFacade>()
                .AddTransient<InternalPurchaseOrderDataUtil>()
                .AddTransient<InternalPurchaseOrderItemDataUtil>()

                .AddTransient<ExternalPurchaseOrderFacade>()
                .AddTransient<ExternalPurchaseOrderDataUtil>()
                .AddTransient<ExternalPurchaseOrderItemDataUtil>()
                .AddTransient<ExternalPurchaseOrderDetailDataUtil>()

                .AddTransient<DeliveryOrderFacade>()
                .AddTransient<DeliveryOrderDataUtil>()
                .AddTransient<DeliveryOrderItemDataUtil>()
                .AddTransient<DeliveryOrderDetailDataUtil>()

                .AddTransient<BankExpenditureNoteFacade>()
                .AddTransient<BankExpenditureNoteDataUtil>()

                .AddTransient<UnitReceiptNoteFacade>()
                .AddTransient<UnitReceiptNoteDataUtil>()
                .AddTransient<UnitReceiptNoteItemDataUtil>()
				.AddTransient<TotalPurchaseFacade>()
				.AddSingleton<IHttpClientService, HttpClientTestService>()
                .AddSingleton<IdentityService>()
                .BuildServiceProvider();

            RegisterSerializationProvider();
            RegisterClassMap();
            MongoDbContext.connectionString = configuration.GetConnectionString(Constant.MONGODB_CONNECTION) ?? configuration[Constant.MONGODB_CONNECTION];

            PurchasingDbContext dbContext = ServiceProvider.GetService<PurchasingDbContext>();
            dbContext.Database.Migrate();
        }     
    }

    [CollectionDefinition("ServiceProviderFixture Collection")]
    public class ServiceProviderFixtureCollection : ICollectionFixture<ServiceProviderFixture>
    {
    }
}
