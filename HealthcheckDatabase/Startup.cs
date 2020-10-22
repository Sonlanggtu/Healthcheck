using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using HealthcheckDatabase.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HealthcheckDatabase.IServies;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthcheckDatabase.Serives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using AspNetCoreRateLimit;
using System.Net.NetworkInformation;
using HealthChecks.UI.Client;

namespace HealthcheckDatabase
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configure the resolvers
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            ///Rate Limit API
            ///// needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));

            //load client rules from appsettings.json
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));

            // inject counter and rules stores
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            // Add framework services.
            //services.AddMvc();

            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //services.AddTransient<IExampleHealthCheckService, ExampleHealthCheckService>();
            services.AddControllersWithViews();
            services.AddRazorPages();


            // services.AddHealthChecks().AddCheck<ExampleHealthCheckService>("HeathyCheckService", null, tags: new[] { "example" });
            //services.AddHealthChecks()
            //    .AddCheck("Example", () => HealthCheckResult.Healthy("Example is OK!"), tags: new[] { "example" });


            // Check Service
            services.AddHealthChecks()
                                    // DB Check Healthy
                                    .AddDbContextCheck<ApplicationDbContext>()
                                    .AddApplicationInsightsPublisher()
                                    // add Service
                                    .AddTypeActivatedCheck<TestHealthCheckWithArgsService>
                                    ("test", failureStatus: HealthStatus.Degraded, tags: new[] { "example" }, args: new object[] {})
                                    // example check
                                    .AddCheck("Foo", () =>
                                        HealthCheckResult.Healthy("Foo is OK!"), tags: new[] { "foo_tag" })
                                    .AddCheck("Bar", () =>
                                        HealthCheckResult.Unhealthy("Bar is unhealthy!"), tags: new[] { "bar_tag" })
                                    .AddCheck("Baz", () =>
                                        HealthCheckResult.Healthy("Baz is OK!"), tags: new[] { "baz_tag" })

                                    // check ping service
                                    .AddCheck("ping", () =>
                                     {
                                         try
                                         {
                                             using (var ping = new Ping())
                                             {
                                                 var reply = ping.Send("asp.net-hacker.rocks");
                                                 if (reply.Status != IPStatus.Success)
                                                 {
                                                     return HealthCheckResult.Unhealthy("Ping is unhealthy");
                                                 }

                                                 if (reply.RoundtripTime > 100)
                                                 {
                                                     return HealthCheckResult.Degraded("Ping is degraded");
                                                 }

                                                 return HealthCheckResult.Healthy("Ping is healthy");
                                             }
                                         }
                                         catch
                                         {
                                             return HealthCheckResult.Unhealthy("Ping is unhealthy");
                                         }
                                     });

            // health check UI
            services.AddHealthChecksUI() //nuget: AspNetCore.HealthChecks.UI
            .AddInMemoryStorage(); //nuget: AspNetCore.HealthChecks.UI.InMemory.Storage

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseClientRateLimiting();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });

                // enpoint health check UI
                //nuget: AspNetCore.HealthChecks.UI
                app.UseHealthChecksUI(options =>
                {
                    options.UIPath = "/healthchecks";
                    options.ApiPath = "/health-ui-api";
                });


                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
