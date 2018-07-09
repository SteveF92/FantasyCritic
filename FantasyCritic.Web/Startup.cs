using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

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

            // Add application services.
            var userStore = new MySQLFantasyCriticUserStore(connectionString);
            var roleStore = new MySQLFantasyCriticRoleStore(connectionString);
            var fantasyCriticRepo = new MySQLFantasyCriticRepo(connectionString);

            services.AddScoped<IFantasyCriticUserStore>(factory => userStore);
            services.AddScoped<IFantasyCriticRoleStore>(factory => roleStore);
            services.AddScoped<IFantasyCriticRepo>(factory => fantasyCriticRepo);
            services.AddScoped<IUserStore<FantasyCriticUser>>(factory => userStore);
            services.AddScoped<IRoleStore<FantasyCriticRole>>(factory => roleStore);

            services.AddScoped<FantasyCriticUserManager>();
            services.AddScoped<FantasyCriticUserManager>();

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISMSSender, SMSSender>();
            services.AddTransient<IClock>(factory => SystemClock.Instance);

            services.AddIdentity<FantasyCriticUser, FantasyCriticRole>()
                .AddDefaultTokenProviders();

            //Authorization
            int validMinutes = Convert.ToInt32(Configuration["Tokens:ValidMinutes"]);

            services.ConfigureApplicationCookie(opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(validMinutes);
            });

            var keyString = Configuration["Tokens:Key"];
            var issuer = Configuration["Tokens:Issuer"];
            var audience = Configuration["Tokens:Audience"];

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

            // Add framework services.
            services.AddMvc();
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
                app.UseExceptionHandler("/Home/Error");
            }

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
