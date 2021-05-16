using Antiques_Auction_WebApp.Services;
using Antiques_Auction_WebApp.Data;
using Antiques_Auction_WebApp.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Antiques_Auction_WebApp.MailService;
using System;
using Antiques_Auction_WebApp.Helpers;

namespace Antiques_Auction_WebApp
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
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));
            services.AddSingleton<IDatabaseSettings>(x => x.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            services.AddSingleton<AntiqueItemService>();
            services.AddSingleton<BidService>();
            services.AddSingleton<AutoBidConfigService>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<EmailService>();
            services.AddSingleton<IConfiguration>(Configuration);
            
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc()
                .AddSessionStateTempDataProvider();
            services.AddSession();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddAutoMapper(typeof(Startup));
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddSignalR();
            services.AddTransient<IEmailSender, MailKitEmailSender>();
            services.Configure<MailKitEmailSenderOptions>(options =>
            {
                options.HostAddress = Configuration["ExternalProviders:MailKit:SMTP:Address"];
                options.HostPort = Convert.ToInt32(Configuration["ExternalProviders:MailKit:SMTP:Port"]);
                options.HostUsername = Configuration["ExternalProviders:MailKit:SMTP:Account"];
                options.HostPassword = Configuration["ExternalProviders:MailKit:SMTP:Password"];
                options.SenderEmail = Configuration["ExternalProviders:MailKit:SMTP:SenderEmail"];
                options.SenderName = Configuration["ExternalProviders:MailKit:SMTP:SenderName"];
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<BidHub>("/bidHub");
            });
        }
    }
}
