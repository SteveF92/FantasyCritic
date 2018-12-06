using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using FantasyCritic.SendGrid;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using SystemClock = NodaTime.SystemClock;

namespace FantasyCritic.Web
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
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            int validMinutes = Convert.ToInt32(Configuration["Tokens:ValidMinutes"]);
            var keyString = Configuration["Tokens:Key"];
            var issuer = Configuration["Tokens:Issuer"];
            var audience = Configuration["Tokens:Audience"];
            IClock clock = SystemClock.Instance;

            // Add application services.
            var userStore = new MySQLFantasyCriticUserStore(connectionString, clock);
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
            services.AddScoped<FantasyCriticRoleManager>();
            services.AddScoped<FantasyCriticService>();

            services.AddTransient<IEmailSender>(factory => sendGridEmailSender);
            services.AddTransient<ISMSSender, SMSSender>();
            services.AddTransient<ITokenService>(factory => tokenService);
            services.AddTransient<IClock>(factory => clock);
            services.AddHttpClient<IOpenCriticService, OpenCriticService>();

            services.AddHttpClient<IOpenCriticService, OpenCriticService>(client =>
            {
                client.BaseAddress = new Uri("https://api.opencritic.com/");
            });

            services.AddIdentity<FantasyCriticUser, FantasyCriticRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(validMinutes);
                opt.Events.OnRedirectToAccessDenied = ReplaceRedirector(HttpStatusCode.Forbidden, opt.Events.OnRedirectToAccessDenied);
                opt.Events.OnRedirectToLogin = ReplaceRedirector(HttpStatusCode.Unauthorized, opt.Events.OnRedirectToLogin);
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString))
                    };

                    // We have to hook the OnMessageReceived event in order to
                    // allow the JWT authentication handler to read the access
                    // token from the query string when a WebSocket or 
                    // Server-Sent Events request comes in.
                    cfg.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/updatehub")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
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

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Webpack initialization with hot-reload.
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseRewriter(new RewriteOptions()
                .AddRedirectToWww()
                .AddRedirectToHttps()
            );

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<UpdateHub>("/updatehub");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new {controller = "Home", action = "Index"});
            });
        }

        private static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceRedirector(HttpStatusCode statusCode, Func<RedirectContext<CookieAuthenticationOptions>, Task> existingRedirector)
        {
            return context => {
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    return existingRedirector(context);
                }
                context.Response.StatusCode = (int)statusCode;
                return Task.CompletedTask;
            };
        }
            
    }
}
