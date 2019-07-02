using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade.Reports;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentBeacukaiFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentCorrectionNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitReceiptNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.MonitoringUnitReceiptFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.PurchasingDispositionFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitExpenditureNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Serializers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Serialization;
using System.Text;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacade.Reports;
using Com.DanLiris.Service.Purchasing.Lib.Facades.PurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentUnitDeliveryOrderReturFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReceiptCorrectionFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.MonitoringCentralBillReceptionFacades;

namespace Com.DanLiris.Service.Purchasing.WebApi
{
    public class Startup
    {
        /* Hard Code */
        private string[] EXPOSED_HEADERS = new string[] { "Content-Disposition", "api-version", "content-length", "content-md5", "content-type", "date", "request-id", "response-time" };
        private string PURCHASING_POLICITY = "PurchasingPolicy";

        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #region Register

        private void RegisterEndpoints()
        {
            APIEndpoint.Purchasing = Configuration.GetValue<string>(Constant.PURCHASING_ENDPOINT) ?? Configuration[Constant.PURCHASING_ENDPOINT];
            APIEndpoint.Core = Configuration.GetValue<string>(Constant.CORE_ENDPOINT) ?? Configuration[Constant.CORE_ENDPOINT];
            APIEndpoint.Inventory = Configuration.GetValue<string>(Constant.INVENTORY_ENDPOINT) ?? Configuration[Constant.INVENTORY_ENDPOINT];
            APIEndpoint.Finance = Configuration.GetValue<string>(Constant.FINANCE_ENDPOINT) ?? Configuration[Constant.FINANCE_ENDPOINT];
            APIEndpoint.CustomsReport = Configuration.GetValue<string>(Constant.CUSTOMSREPORT_ENDPOINT) ?? Configuration[Constant.FINANCE_ENDPOINT];
        }

        private void RegisterFacades(IServiceCollection services)
        {
            services
                .AddTransient<PurchasingDocumentExpeditionFacade>()
                .AddTransient<IBankExpenditureNoteFacade, BankExpenditureNoteFacade>()
                .AddTransient<IBankDocumentNumberGenerator, BankDocumentNumberGenerator>()
                .AddTransient<PurchasingDocumentExpeditionReportFacade>()
                .AddTransient<IPPHBankExpenditureNoteFacade, PPHBankExpenditureNoteFacade>()
                .AddTransient<IPPHBankExpenditureNoteReportFacade, PPHBankExpenditureNoteReportFacade>()
                .AddTransient<IUnitPaymentOrderPaidStatusReportFacade, UnitPaymentOrderPaidStatusReportFacade>()
                .AddTransient<IUnitPaymentOrderUnpaidReportFacade, UnitPaymentOrderUnpaidReportFacade>()
                .AddTransient<UnitPaymentOrderNotVerifiedReportFacade>()
                .AddTransient<PurchaseRequestFacade>()
                .AddTransient<DeliveryOrderFacade>()
                .AddTransient<ImportPurchasingBookReportFacade>()
                .AddTransient<LocalPurchasingBookReportFacade>()
                .AddTransient<InternalPurchaseOrderFacade>()
                .AddTransient<ExternalPurchaseOrderFacade>()
                .AddTransient<MonitoringPriceFacade>()
                .AddTransient<IUnitReceiptNoteFacade, UnitReceiptNoteFacade>()
                .AddTransient<TotalPurchaseFacade>()
                .AddTransient<PurchaseRequestGenerateDataFacade>()
                .AddTransient<ExternalPurchaseOrderGenerateDataFacade>()
                .AddTransient<UnitReceiptNoteGenerateDataFacade>()
                .AddTransient<IUnitPaymentOrderFacade, UnitPaymentOrderFacade>()
                .AddTransient<IUnitPaymentQuantityCorrectionNoteFacade, UnitPaymentQuantityCorrectionNoteFacade>()
                .AddTransient<IUnitPaymentPriceCorrectionNoteFacade, UnitPaymentPriceCorrectionNoteFacade>()
                .AddTransient<PurchaseOrderMonitoringAllFacade>()
                .AddTransient<IGarmentPurchaseRequestFacade, GarmentPurchaseRequestFacade>()
                .AddTransient<IGarmentInternalPurchaseOrderFacade, GarmentInternalPurchaseOrderFacade>()
                .AddTransient<IGarmentTotalPurchaseOrderFacade, TotalGarmentPurchaseFacade>()
                .AddTransient<IGarmentInvoice, GarmentInvoiceFacade>()
                .AddTransient<IGarmentInternNoteFacade, GarmentInternNoteFacades>()
                .AddTransient<IGarmentExternalPurchaseOrderFacade, GarmentExternalPurchaseOrderFacade>()
                .AddTransient<IGarmentDeliveryOrderFacade, GarmentDeliveryOrderFacade>()
                .AddTransient<IPurchasingDispositionFacade, PurchasingDispositionFacade>()
                .AddTransient<IGarmentCorrectionNotePriceFacade, GarmentCorrectionNotePriceFacade>()
                .AddTransient<IGarmentCorrectionNoteQuantityFacade, GarmentCorrectionNoteQuantityFacade>()
                .AddTransient<IGarmentBeacukaiFacade, GarmentBeacukaiFacade>()
                .AddTransient<IPurchasingDispositionFacade, PurchasingDispositionFacade>()
                .AddTransient<IGarmentCorrectionNotePriceFacade, GarmentCorrectionNotePriceFacade>()
                .AddTransient<IGarmentUnitReceiptNoteFacade, GarmentUnitReceiptNoteFacade>()
                .AddTransient<IMonitoringUnitReceiptAllFacade, MonitoringUnitReceiptAllFacade>()
                .AddTransient<IGarmentUnitDeliveryOrderFacade, GarmentUnitDeliveryOrderFacade>()
                .AddTransient<IGarmentUnitExpenditureNoteFacade, GarmentUnitExpenditureNoteFacade>()
                .AddTransient<IGarmentReturnCorrectionNoteFacade, GarmentReturnCorrectionNoteFacade>()
                .AddTransient<IGarmentUnitDeliveryOrderReturFacade, GarmentUnitDeliveryOrderReturFacade>()
                .AddTransient<IMonitoringCentralBillReceptionFacade, MonitoringCentralBillReceptionFacade>()
                .AddTransient<IGarmentReceiptCorrectionFacade, GarmentReceiptCorrectionFacade>();

        }

        private void RegisterServices(IServiceCollection services, bool isTest)
        {
            services
                .AddScoped<IdentityService>()
                .AddScoped<ValidateService>()
                .AddScoped<IValidateService, ValidateService>();

            if (isTest == false)
            {
                services.AddScoped<IHttpClientService, HttpClientService>();
            }
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
            ClassMap<UnitPaymentCorrectionNoteViewModel>.Register();
        }

        #endregion Register

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString(Constant.DEFAULT_CONNECTION) ?? Configuration[Constant.DEFAULT_CONNECTION];
            string env = Configuration.GetValue<string>(Constant.ASPNETCORE_ENVIRONMENT);
            APIEndpoint.ConnectionString = Configuration.GetConnectionString("DefaultConnection") ?? Configuration["DefaultConnection"];

            /* Register */
            services.AddDbContext<PurchasingDbContext>(options => options.UseSqlServer(connectionString));
            RegisterEndpoints();
            RegisterFacades(services);
            RegisterServices(services, env.Equals("Test"));
            services.AddAutoMapper();

            RegisterSerializationProvider();
            RegisterClassMap();
            MongoDbContext.connectionString = Configuration.GetConnectionString(Constant.MONGODB_CONNECTION) ?? Configuration[Constant.MONGODB_CONNECTION];


            /* Versioning */
            services.AddApiVersioning(options => { options.DefaultApiVersion = new ApiVersion(1, 0); });

            /* Authentication */
            string Secret = Configuration.GetValue<string>(Constant.SECRET) ?? Configuration[Constant.SECRET];
            SymmetricSecurityKey Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = false,
                        IssuerSigningKey = Key
                    };
                });

            /* CORS */
            services.AddCors(options => options.AddPolicy(PURCHASING_POLICITY, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithExposedHeaders(EXPOSED_HEADERS);
            }));

            /* API */
            services
               .AddMvcCore()
               .AddAuthorization()
               .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
               .AddJsonFormatters();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /* Update Database */
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                PurchasingDbContext context = serviceScope.ServiceProvider.GetService<PurchasingDbContext>();
                context.Database.SetCommandTimeout(10 * 60 * 1000);
                context.Database.Migrate();
            }

            app.UseAuthentication();
            app.UseCors(PURCHASING_POLICITY);
            app.UseMvc();
        }
    }
}
