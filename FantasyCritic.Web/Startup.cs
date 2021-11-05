using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueCliMiddleware;
using Microsoft.AspNetCore.SpaServices;
using System.Net;
using System.Text;
using Dapper.NodaTime;
using FantasyCritic.AWS;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Scheduling;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Statistics;
using FantasyCritic.Mailgun;
using FantasyCritic.MySQL;
using FantasyCritic.RDS;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using NodaTime;
using NodaTime.Serialization.JsonNet;

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
            int validMinutes = Convert.ToInt32(Configuration["Tokens:ValidMinutes"]);
            var keyString = Configuration["Tokens:Key"];
            var issuer = Configuration["Tokens:Issuer"];
            var audience = Configuration["Tokens:Audience"];
            IClock clock = NodaTime.SystemClock.Instance;
            var rdsInstanceName = Configuration["AWS:rdsInstanceName"];
            var awsRegion = Configuration["AWS:region"];
            var awsBucket = Configuration["AWS:bucket"];
            var mailgunAPIKey = Configuration["Mailgun:apiKey"];

            // Add application services.

            var tokenService = new TokenService(keyString, issuer, audience, validMinutes);

            services.AddHttpClient();

            //MySQL Repos
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            var userStore = new MySQLFantasyCriticUserStore(connectionString, clock);
            var roleStore = new MySQLFantasyCriticRoleStore(connectionString);
            services.AddScoped<IFantasyCriticUserStore>(factory => userStore);
            services.AddScoped<IFantasyCriticRoleStore>(factory => roleStore);
            services.AddScoped<IMasterGameRepo>(factory => new MySQLMasterGameRepo(connectionString, userStore));
            services.AddScoped<IFantasyCriticRepo>(factory => new MySQLFantasyCriticRepo(connectionString, userStore, new MySQLMasterGameRepo(connectionString, userStore)));
            services.AddScoped<IRoyaleRepo>(factory => new MySQLRoyaleRepo(connectionString, userStore, new MySQLMasterGameRepo(connectionString, userStore),
                new MySQLFantasyCriticRepo(connectionString, userStore, new MySQLMasterGameRepo(connectionString, userStore))));
            services.AddScoped<IUserStore<FantasyCriticUser>>(factory => userStore);
            services.AddScoped<IRoleStore<FantasyCriticRole>>(factory => roleStore);

            services.AddScoped<IHypeFactorService>(factory => new LambdaHypeFactorService(awsRegion, awsBucket));
            services.AddScoped<IRDSManager>(factory => new RDSManager(rdsInstanceName));
            services.AddScoped<FantasyCriticUserManager>();
            services.AddScoped<FantasyCriticRoleManager>();
            services.AddScoped<GameAcquisitionService>();
            services.AddScoped<LeagueMemberService>();
            services.AddScoped<PublisherService>();
            services.AddScoped<InterLeagueService>();
            services.AddScoped<DraftService>();
            services.AddScoped<GameSearchingService>();
            services.AddScoped<ActionProcessingService>();
            services.AddScoped<FantasyCriticService>();
            services.AddScoped<RoyaleService>();

            services.AddTransient<IEmailSender>(factory => new MailGunEmailSender("fantasycritic.games", mailgunAPIKey, "noreply@fantasycritic.games"));
            services.AddTransient<ISMSSender, SMSSender>();
            services.AddTransient<ITokenService>(factory => tokenService);
            services.AddTransient<IClock>(factory => clock);
            services.AddHttpClient<IOpenCriticService, OpenCriticService>();

            services.AddHttpClient<IOpenCriticService, OpenCriticService>(client =>
            {
                client.BaseAddress = new Uri("https://api.opencritic.com/api/");
            });

            services.AddScoped<AdminService>();

            //Add scheduled tasks & scheduler
            services.AddSingleton<IScheduledTask, RefreshDataTask>();
            services.AddSingleton<IScheduledTask, TimeFlagsTask>();
            services.AddScheduler((sender, args) =>
            {
                args.SetObserved();
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

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });

            services.AddSignalR();

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            DapperNodaTimeSetup.Register();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = CommandLine.Arguments.TryGetOptions(System.Environment.GetCommandLineArgs(), false, out string mode, out ushort port, out bool https);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (https)
            {
                app.UseHttpsRedirection();
            }

            app.UseRewriter(new RewriteOptions()
                .AddRedirectToWww()
            );

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = (context) =>
                {
                    var headers = context.Context.Response.GetTypedHeaders();

                    headers.CacheControl = new CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(365)
                    };
                }
            });
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                // Add MapRazorPages if the app uses Razor Pages. Since Endpoint Routing includes support for many frameworks, adding Razor Pages is now opt -in.
                // endpoints.MapRazorPages();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                endpoints.MapHub<UpdateHub>("/updatehub");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    // run npm process with client app
                    if (mode == "start")
                    {
                        spa.UseVueCli(npmScript: "serve", port: port, forceKill: true, https: https);
                    }

                    // if you just prefer to proxy requests from client app, use proxy to SPA dev server instead,
                    // app should be already running before starting a .NET client:
                    // run npm process with client app
                    if (mode == "attach")
                    {
                        spa.UseProxyToSpaDevelopmentServer($"{(https ? "https" : "http")}://localhost:{port}"); // your Vue app port
                    }
                }
                else
                {
                    spa.Options.SourcePath = "dist";
                }
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
