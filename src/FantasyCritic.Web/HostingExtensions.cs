using System.IO;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Scheduling;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using FantasyCritic.Mailgun;
using FantasyCritic.MySQL;
using FantasyCritic.Web.AuthorizationHandlers;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using NLog;
using NodaTime.Serialization.JsonNet;
using VueCliMiddleware;
using IEmailSender = FantasyCritic.Lib.Interfaces.IEmailSender;

namespace FantasyCritic.Web;

public static class HostingExtensions
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static string GetSPAPath(IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            return "ClientApp";
        }

        return "ClientApp/dist";
    }
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var env = builder.Environment;
        if (env.IsDevelopment())
        {
            _logger.Info("Startup: Running in Development mode.");
        }
        else
        {
            _logger.Info("Startup: Running in Production mode.");
        }

        var configuration = builder.Configuration;
        var services = builder.Services;
        IClock clock = SystemClock.Instance;
        var rdsInstanceName = configuration["AWS:rdsInstanceName"];
        var awsRegion = configuration["AWS:region"];
        var awsBucket = configuration["AWS:bucket"];
        var mailgunAPIKey = configuration["Mailgun:apiKey"];
        var baseAddress = configuration["BaseAddress"];
        var rootFolder = configuration["RootFolder"];
        var duendeLicense = configuration["IdentityServer:License"];

        var identityConfig = new IdentityConfig(configuration["IdentityServer:FCBotSecret"], configuration["IdentityServer:CertificateKey"]);

        // Add application services.
        services.AddHttpClient();
        services.AddTransient<IClock>(factory => clock);

        //MySQL Repos
        string connectionString = configuration.GetConnectionString("DefaultConnection");

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
            configuration["PatreonService:AccessToken"],
            configuration["PatreonService:RefreshToken"],
            configuration["Authentication:Patreon:ClientId"],
            configuration["PatreonService:CampaignID"]
        ));

        services.AddScoped<EmailSendingServiceConfiguration>(_ => new EmailSendingServiceConfiguration(baseAddress, env.IsProduction()));

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
        services.AddScoped<TradeService>();
        services.AddScoped<FantasyCriticService>();
        services.AddScoped<RoyaleService>();
        services.AddScoped<EmailSendingService>();

        services.AddScoped<IEmailSender>(factory => new MailGunEmailSender("fantasycritic.games", mailgunAPIKey, "noreply@fantasycritic.games", "Fantasy Critic"));

        AdminServiceConfiguration adminServiceConfiguration = new AdminServiceConfiguration(true);
        if (env.IsProduction() || env.IsStaging())
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
        services.AddSingleton<IScheduledTask, ProcessSpecialAuctionsTask>();
        services.AddScheduler((sender, args) =>
        {
            args.SetObserved();
        });

        var schemes = new[] { IdentityConstants.ApplicationScheme, JwtBearerDefaults.AuthenticationScheme };
        services.AddAuthorization(options =>
        {
            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.AddAuthenticationSchemes(schemes);
            policyBuilder.RequireAuthenticatedUser();
            policyBuilder.RequireClaim("scope", FantasyCriticScopes.ReadScope.Name);

            var basicUserPolicy = policyBuilder.Build();

            options.AddPolicy("BasicUser", basicUserPolicy);
            options.DefaultPolicy = basicUserPolicy;

            options.AddPolicy("PlusUser", policy =>
            {
                policy.AddAuthenticationSchemes(schemes);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", FantasyCriticScopes.ReadScope.Name);
                policy.RequireRole("PlusUser");
            });

            options.AddPolicy("Admin", policy =>
            {
                policy.AddAuthenticationSchemes(schemes);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", FantasyCriticScopes.ReadScope.Name);
                policy.RequireClaim("scope", FantasyCriticScopes.WriteScope.Name);
                policy.RequireRole("Admin");
            });

            options.AddPolicy("Write", policy =>
            {
                policy.AddAuthenticationSchemes(schemes);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", FantasyCriticScopes.ReadScope.Name);
                policy.RequireClaim("scope", FantasyCriticScopes.WriteScope.Name);
            });
        });

        services.AddIdentity<FantasyCriticUser, FantasyCriticRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                var letters = "abcdefghijklmnopqrstuvwxyz";
                var numbers = "0123456789";
                var specials = "-._@+ ";
                options.User.AllowedUserNameCharacters = letters + letters.ToUpper() + numbers + specials;
            })
            .AddSignInManager<FantasyCriticSignInManager>()
            .AddUserManager<FantasyCriticUserManager>()
            .AddRoleManager<FantasyCriticRoleManager>()
            .AddDefaultTokenProviders();

        var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.LicenseKey = duendeLicense;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
                options.UserInteraction.ConsentUrl = "/Account/Consent";
            })
            .AddPersistedGrantStore<MySQLPersistedGrantStore>()
            .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
            .AddInMemoryApiScopes(IdentityConfig.APIScopes)
            .AddInMemoryApiResources(IdentityConfig.APIResources)
            .AddInMemoryClients(identityConfig.Clients)
            .AddAspNetIdentity<FantasyCriticUser>();

        if (env.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            identityServerBuilder.AddSigningCredential($"CN={identityConfig.KeyName}");
        }

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "FantasyCriticCookie";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true; // the cookie would be re-issued on any request half way through the ExpireTimeSpan
            })
            .AddLocalApi(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.ExpectedScope = FantasyCriticScopes.ReadScope.Name;
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            })
            .AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                microsoftOptions.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
            })
            .AddTwitch(options =>
            {
                options.ClientId = configuration["Authentication:Twitch:ClientId"];
                options.ClientSecret = configuration["Authentication:Twitch:ClientSecret"];
            })
            .AddPatreon(options =>
            {
                options.ClientId = configuration["Authentication:Patreon:ClientId"];
                options.ClientSecret = configuration["Authentication:Patreon:ClientSecret"];
            })
            .AddDiscord(options =>
            {
                options.ClientId = configuration["Authentication:Discord:ClientId"];
                options.ClientSecret = configuration["Authentication:Discord:ClientSecret"];
            });

        var keysFolder = Path.Combine(rootFolder, "Keys");
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

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

        services.AddRazorPages();
        services.AddSignalR();

        // In production, the Vue files will be served from this directory
        services.AddSpaStaticFiles(staticFileOptions =>
        {
            staticFileOptions.RootPath = GetSPAPath(env);
        });


        if (env.IsDevelopment())
        {
            // Only in Development, used for debugging
            services.AddTransient<IAuthorizationHandler, FantasyCriticAuthorizationHandler>();
        }

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        var env = app.Environment;
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
        app.UseIdentityServer();
        app.UseAuthorization();

        //This works
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
            endpoints.MapHub<UpdateHub>("/updatehub");
        });

        //This does not
        //app.MapControllers();
        //app.MapRazorPages();
        //app.MapHub<UpdateHub>("/updatehub");

        var spaPath = GetSPAPath(env);
        var spaStaticFileOptions = new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(env.ContentRootPath, spaPath))
        };

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = spaPath;

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

        return app;
    }
}
