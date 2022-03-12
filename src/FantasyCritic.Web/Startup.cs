using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VueCliMiddleware;
using Microsoft.AspNetCore.SpaServices;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dapper.NodaTime;
using Duende.IdentityServer;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Scheduling;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Statistics;
using FantasyCritic.Mailgun;
using FantasyCritic.MySQL;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using NLog;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using FantasyCritic.Lib.Patreon;

namespace FantasyCritic.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _spaPath;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;


            if (_env.IsDevelopment())
            {
                _logger.Info("Startup: Running in Development mode.");
                _spaPath = "ClientApp";
            }
            else
            {
                _logger.Info("Startup: Running in Production mode.");
                _spaPath = "ClientApp/dist";
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IClock clock = NodaTime.SystemClock.Instance;
            var rdsInstanceName = Configuration["AWS:rdsInstanceName"];
            var awsRegion = Configuration["AWS:region"];
            var awsBucket = Configuration["AWS:bucket"];
            var mailgunAPIKey = Configuration["Mailgun:apiKey"];
            var baseAddress = Configuration["BaseAddress"];
            var jwtSecret = Configuration["Authentication:JWTSecret"];
            var duendeLicense = Configuration["IdentityServer:License"];

            var identityConfig = new IdentityConfig(Configuration["IdentityServer:MainSecret"],
                Configuration["IdentityServer:FCBotSecret"], Configuration["IdentityServer:CertificateKey"], _env.IsProduction());

            // Add application services.
            services.AddHttpClient();
            services.AddTransient<IClock>(factory => clock);

            //MySQL Repos
            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            var userStore = new MySQLFantasyCriticUserStore(connectionString, clock);
            var roleStore = new MySQLFantasyCriticRoleStore(connectionString);
            services.AddScoped<IFantasyCriticUserStore>(factory => userStore);
            services.AddScoped<IFantasyCriticRoleStore>(factory => roleStore);
            services.AddScoped<RepositoryConfiguration>(factory => new RepositoryConfiguration(connectionString, clock));
            services.AddScoped<IUserStore<FantasyCriticUser>, MySQLFantasyCriticUserStore>(factory => userStore);
            services.AddScoped<IRoleStore<FantasyCriticRole>, MySQLFantasyCriticRoleStore>(factory => roleStore);

            services.AddScoped<IMasterGameRepo>(factory => new MySQLMasterGameRepo(connectionString, userStore));
            services.AddScoped<IFantasyCriticRepo>(factory => new MySQLFantasyCriticRepo(connectionString, userStore, new MySQLMasterGameRepo(connectionString, userStore)));
            services.AddScoped<IRoyaleRepo>(factory => new MySQLRoyaleRepo(connectionString, userStore, new MySQLMasterGameRepo(connectionString, userStore),
                new MySQLFantasyCriticRepo(connectionString, userStore, new MySQLMasterGameRepo(connectionString, userStore))));

            services.AddScoped<PatreonService>(factory => new PatreonService(
                Configuration["PatreonService:AccessToken"],
                Configuration["PatreonService:RefreshToken"],
                Configuration["Authentication:Patreon:ClientId"],
                Configuration["PatreonService:CampaignID"]
                ));

            services.AddScoped<EmailSendingServiceConfiguration>(_ => new EmailSendingServiceConfiguration(baseAddress));

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
            services.AddScoped<EmailSendingService>();


            services.AddScoped<IEmailSender>(factory => new MailGunEmailSender("fantasycritic.games", mailgunAPIKey, "noreply@fantasycritic.games", "Fantasy Critic"));

            AdminServiceConfiguration adminServiceConfiguration = new AdminServiceConfiguration(true);
            if (_env.IsProduction() || _env.IsStaging())
            {
                adminServiceConfiguration = new AdminServiceConfiguration(false);
            }
            services.AddScoped<AdminServiceConfiguration>(_ => adminServiceConfiguration);
            services.AddScoped<AdminService>();

            services.AddHttpClient<IOpenCriticService, OpenCriticService>(client =>
            {
                client.BaseAddress = new Uri("https://api.opencritic.com/api/");
            });
            services.AddHttpClient<IGGService, GGService>(client =>
            {
                client.BaseAddress = new Uri("https://api.ggapp.io/");
            });

            //Add scheduled tasks & scheduler
            services.AddSingleton<IScheduledTask, RefreshDataTask>();
            services.AddSingleton<IScheduledTask, TimeFlagsTask>();
            services.AddSingleton<IScheduledTask, PatreonUpdateTask>();
            services.AddSingleton<IScheduledTask, EmailSendingTask>();
            services.AddScheduler((sender, args) =>
            {
                args.SetObserved();
            });

            services.AddAuthorization(ctx =>
            {
                ctx.AddPolicy("Api", policy =>
                {
                    policy.AddAuthenticationSchemes(
                        IdentityServerConstants.DefaultCookieAuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "FantasyCritic.WebAPI");
                });
            });

            services.AddAuthentication()
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = baseAddress;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = false,

                    ValidTypes = new[] { "at+jwt" },

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                };
            });

            services.AddIdentity<FantasyCriticUser, FantasyCriticRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    var letters = "abcdefghijklmnopqrstuvwxyz";
                    var numbers = "0123456789";
                    var specials = "-._@+ ";
                    options.User.AllowedUserNameCharacters = letters + letters.ToUpper() + numbers + specials;
                })
                .AddDefaultUI()
                .AddUserManager<FantasyCriticUserManager>()
                .AddRoleManager<FantasyCriticRoleManager>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
                {
                    options.LicenseKey = duendeLicense;
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;
                })
                .AddPersistedGrantStore<MySQLPersistedGrantStore>()
                .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
                .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
                .AddInMemoryClients(identityConfig.Clients)
                .AddAspNetIdentity<FantasyCriticUser>();

            if (_env.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                builder.AddSigningCredential($"CN={identityConfig.KeyName}");
            }

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "FantasyCriticCookie";
                    options.LoginPath = "/Identity/Account/Login";
                    options.LogoutPath = "/Identity/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true; // the cookie would be re-issued on any request half way through the ExpireTimeSpan
                })
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                    microsoftOptions.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                    microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                })
                .AddTwitch(options =>
                {
                    options.ClientId = Configuration["Authentication:Twitch:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Twitch:ClientSecret"];
                })
                .AddPatreon(options =>
                {
                    options.ClientId = Configuration["Authentication:Patreon:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Patreon:ClientSecret"];
                })
                .AddDiscord(options =>
                {
                    options.ClientId = Configuration["Authentication:Discord:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Discord:ClientSecret"];
                })
                .AddIdentityServerJwt();

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\FantasyCritic\Keys\"));

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

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });

            services.AddRazorPages();

            services.AddSignalR();

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = _spaPath;
            });

            DapperNodaTimeSetup.Register();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = Arguments.TryGetOptions(System.Environment.GetCommandLineArgs(), false, out string mode, out ushort port, out bool https);

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

            app.UseHttpsRedirection();
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
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
                endpoints.MapHub<UpdateHub>("/updatehub");
            });

            var spaStaticFileOptions = new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(System.IO.Path.Combine(env.ContentRootPath, _spaPath))
            };

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = _spaPath;

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
                    spa.Options.DefaultPageStaticFileOptions = spaStaticFileOptions;
                }
            });
        }
    }
}
