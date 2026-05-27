using LegacyOrderMgmt.Core.Data;
using LegacyOrderMgmt.Core.Services;
using LegacyOrderMgmt.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LegacyOrderMgmt.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Legacy: services.AddMvc() — replaced by AddControllersWithViews() in .NET Core 3.0+
            services.AddMvc();

            // Legacy: verbose EF Core registration with UseInternalServiceProvider
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<OrderDbContext>(options =>
                    options.UseSqlServer(Configuration["ConnectionStrings:OrderDb"])
                           .UseInternalServiceProvider(
                               new ServiceCollection()
                                   .AddEntityFrameworkSqlServer()
                                   .BuildServiceProvider()));

            // Legacy: manual service registration — no extension methods or service modules
            services.AddScoped<OrderService>();
            services.AddScoped<PricingEngine>();
            services.AddScoped<ShippingIntegrationService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<EdiImportService>();
            services.AddScoped<InvoiceService>();
            services.AddScoped<ReportExportService>();

            // Legacy: no health checks registered — invisible to Kubernetes probes
            // Intentionally omitted: services.AddHealthChecks()
        }

        // Legacy: IHostingEnvironment — deprecated in ASP.NET Core 3.0, use IWebHostEnvironment
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Legacy: ServicePointManager TLS configuration at startup
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 20;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Legacy: app.UseMvc() with explicit route template — replaced by endpoint routing in .NET Core 3.0+
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Legacy: no health check endpoint mapped
            // Intentionally omitted: app.UseHealthChecks("/health")
        }
    }
}
