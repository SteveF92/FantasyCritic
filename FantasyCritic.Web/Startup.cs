using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using FantasyCritic.SendGrid;
using FantasyCritic.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.TimeZones;

namespace FantasyCritic.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            int validMinutes = Convert.ToInt32(Configuration["Tokens:ValidMinutes"]);
            var keyString = Configuration["Tokens:Key"];
            var issuer = Configuration["Tokens:Issuer"];
            var audience = Configuration["Tokens:Audience"];

            // Add application services.
            var userStore = new MySQLFantasyCriticUserStore(connectionString);
            var roleStore = new MySQLFantasyCriticRoleStore(connectionString);
            var fantasyCriticRepo = new MySQLFantasyCriticRepo(connectionString, userStore);
            var tokenService = new TokenService(keyString, issuer, audience, validMinutes);
            SendGridEmailSender sendGridEmailSender = new SendGridEmailSender();

            services.AddHttpClient();

            services.AddScoped<IFantasyCriticUserStore>(factory => userStore);
            services.AddScoped<IFantasyCriticRoleStore>(factory => roleStore);
            services.AddScoped<IFantasyCriticRepo>(factory => fantasyCriticRepo);
            services.AddScoped<IUserStore<FantasyCriticUser>>(factory => userStore);
            services.AddScoped<IRoleStore<FantasyCriticRole>>(factory => roleStore);

            services.AddScoped<FantasyCriticUserManager>();
            services.AddScoped<FantasyCriticService>();

            services.AddTransient<IEmailSender>(factory => sendGridEmailSender);
            services.AddTransient<ISMSSender, SMSSender>();
            services.AddTransient<ITokenService>(factory => tokenService);
            services.AddTransient<IClock>(factory => SystemClock.Instance);
            services.AddHttpClient<IOpenCriticService, OpenCriticService>();

            services.AddHttpClient<IOpenCriticService, OpenCriticService>(client =>
            {
                client.BaseAddress = new Uri("https://api.opencritic.com/");
            });


            services.AddIdentity<FantasyCriticUser, FantasyCriticRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 12;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(opt => { opt.ExpireTimeSpan = TimeSpan.FromMinutes(validMinutes); });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString))
                    };
                });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 443;
            });


            // Add framework services.
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
