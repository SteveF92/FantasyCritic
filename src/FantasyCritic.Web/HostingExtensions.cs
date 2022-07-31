using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using NodaTime.Serialization.JsonNet;
using IEmailSender = FantasyCritic.Lib.Interfaces.IEmailSender;

namespace FantasyCritic.Web;

public static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, string awsRegion, ConfigurationStoreSet configuration)
    {
        var services = builder.Services;
        var environment = builder.Environment;

        Log.Information($"Startup: Running in {environment} mode.");

        var rdsInstanceName = configuration.AssertConfigValue("AWS:rdsInstanceName");
        var awsBucket = configuration.AssertConfigValue("AWS:bucket");
        var mailgunAPIKey = configuration.AssertConfigValue("Mailgun:apiKey");
        var baseAddress = configuration.AssertConfigValue("BaseAddress");
        var duendeLicense = configuration.AssertConfigValue("IdentityServer:License");

        var rootFolder = configuration.AssertConfigValue("LinuxRootFolder");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            rootFolder = configuration.AssertConfigValue("RootFolder");
        }

        var identityConfig = new IdentityConfig(configuration.AssertConfigValue("IdentityServer:FCBotSecret"), configuration.AssertConfigValue("IdentityServer:CertificateKey"));

        IClock clock = SystemClock.Instance;

        // Add application services.
        services.AddHttpClient();
        services.AddTransient<IClock>(factory => clock);

        //MySQL Repos
        string connectionString = configuration.AssertConnectionString("DefaultConnection");

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
            configuration.AssertConfigValue("PatreonService:AccessToken"),
            configuration.AssertConfigValue("PatreonService:RefreshToken"),
            configuration.AssertConfigValue("Authentication:Patreon:ClientId"),
            configuration.AssertConfigValue("PatreonService:CampaignID")
        ));

        services.AddScoped<EmailSendingServiceConfiguration>(_ => new EmailSendingServiceConfiguration(baseAddress, environment.IsProduction()));

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
        if (environment.IsProduction() || environment.IsStaging())
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

        if (environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                identityServerBuilder.AddSigningCredential($"CN={identityConfig.KeyName}");
            }
            else
            {
                var certPath = configuration.AssertConfigValue("IdentityServer:CertificatePath");
                var certPassword = configuration.AssertConfigValue("IdentityServer:CertificatePassword");
                var rsaCert = new X509Certificate2(certPath, certPassword);
                identityServerBuilder.AddSigningCredential(rsaCert);
            }
        }

        var authenticationBuilder = services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "FantasyCriticCookie";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true; // the cookie would be re-issued on any request half way through the ExpireTimeSpan
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;

            })
            .AddLocalApi(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.ExpectedScope = FantasyCriticScopes.ReadScope.Name;
            });

        if (environment.IsProduction())
        {
            authenticationBuilder
                .AddGoogle(options =>
                {
                    options.ClientId = configuration.AssertConfigValue("Authentication:Google:ClientId");
                    options.ClientSecret = configuration.AssertConfigValue("Authentication:Google:ClientSecret");
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                    microsoftOptions.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                    microsoftOptions.ClientId = configuration.AssertConfigValue("Authentication:Microsoft:ClientId");
                    microsoftOptions.ClientSecret = configuration.AssertConfigValue("Authentication:Microsoft:ClientSecret");
                })
                .AddTwitch(options =>
                {
                    options.ClientId = configuration.AssertConfigValue("Authentication:Twitch:ClientId");
                    options.ClientSecret = configuration.AssertConfigValue("Authentication:Twitch:ClientSecret");
                })
                .AddPatreon(options =>
                {
                    options.ClientId = configuration.AssertConfigValue("Authentication:Patreon:ClientId");
                    options.ClientSecret = configuration.AssertConfigValue("Authentication:Patreon:ClientSecret");
                })
                .AddDiscord(options =>
                {
                    options.ClientId = configuration.AssertConfigValue("Authentication:Discord:ClientId");
                    options.ClientSecret = configuration.AssertConfigValue("Authentication:Discord:ClientSecret");
                });
        }

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

        if (environment.IsDevelopment())
        {
            // Only in Development, used for debugging
            services.AddTransient<IAuthorizationHandler, FantasyCriticAuthorizationHandler>();
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        var env = app.Environment;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            app.UseForwardedHeaders();
        }

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

        app.UseStaticFiles();

        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();
        app.MapHub<UpdateHub>("/updatehub");

        app.MapFallbackToFile("index.html");

        return app;
    }
}
