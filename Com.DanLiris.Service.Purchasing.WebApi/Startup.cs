using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Serializers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
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
        }

        private void RegisterFacades(IServiceCollection services)
        {
            services
                .AddTransient<PurchasingDocumentExpeditionFacade>()
                .AddTransient<BankExpenditureNoteFacade>()
                .AddTransient<PurchasingDocumentExpeditionReportFacade>()
                .AddTransient<UnitPaymentOrderNotVerifiedReportFacade>()
                .AddTransient<PurchaseRequestFacade>()
                .AddTransient<DeliveryOrderFacade>()
                .AddTransient<ImportPurchasingBookReportFacade>()
                .AddTransient<LocalPurchasingBookReportFacade>()
                .AddTransient<InternalPurchaseOrderFacade>()
                .AddTransient<ExternalPurchaseOrderFacade>();
        }

        private void RegisterServices(IServiceCollection services, bool isTest)
        {
            services
                .AddScoped<IdentityService>()
                .AddScoped<ValidateService>();

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
        }

        #endregion Register

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString(Constant.DEFAULT_CONNECTION) ?? Configuration[Constant.DEFAULT_CONNECTION];
            string env = Configuration.GetValue<string>(Constant.ASPNETCORE_ENVIRONMENT);

            /* Register */
            services.AddDbContext<PurchasingDbContext>(options => options.UseSqlServer(connectionString));
            RegisterEndpoints();
            RegisterFacades(services);
            RegisterServices(services, env.Equals("Test"));
            services.AddAutoMapper();

            RegisterSerializationProvider();
            RegisterClassMap();
            MongoDbContext.connectionString = Configuration.GetConnectionString(Constant.MONGODB_CONNECTION) ?? Configuration[Constant.MONGODB_CONNECTION];

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
                context.Database.Migrate();
            }

            app.UseAuthentication();
            app.UseCors(PURCHASING_POLICITY);
            app.UseMvc();
        }
    }
}