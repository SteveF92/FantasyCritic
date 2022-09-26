using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
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
using FantasyCritic.Web.BackgroundServices;
using FantasyCritic.Web.Filters;
using FantasyCritic.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using NodaTime.Serialization.JsonNet;
using CacheControlHeaderValue = Microsoft.Net.Http.Headers.CacheControlHeaderValue;
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
        var openCriticAPIKey = configuration.AssertConfigValue("OpenCritic:apiKey");
        var baseAddress = configuration.AssertConfigValue("BaseAddress");
        var discordBotToken = configuration.AssertConfigValue("Discord:BotToken");

        var rootFolder = configuration.AssertConfigValue("LinuxRootFolder");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            rootFolder = configuration.AssertConfigValue("RootFolder");
        }

        IClock clock = SystemClock.Instance;

        // Add application services.
        services.AddHttpClient();
        services.AddTransient<IClock>(_ => clock);

        //MySQL Repos
        string connectionString = configuration.AssertConnectionString("DefaultConnection");

        services.AddSingleton<RepositoryConfiguration>(_ => new RepositoryConfiguration(connectionString, clock));
        services.AddSingleton<EmailSendingServiceConfiguration>(_ => new EmailSendingServiceConfiguration(baseAddress, environment.IsProduction()));
        services.AddSingleton<DiscordConfiguration>(_ => new DiscordConfiguration(discordBotToken));

        AdminServiceConfiguration adminServiceConfiguration = new AdminServiceConfiguration(true);
        if (environment.IsProduction() || environment.IsStaging())
        {
            adminServiceConfiguration = new AdminServiceConfiguration(false);
        }
        services.AddSingleton<AdminServiceConfiguration>(_ => adminServiceConfiguration);

        services.AddScoped<IFantasyCriticUserStore, MySQLFantasyCriticUserStore>();
        services.AddScoped<IReadOnlyFantasyCriticUserStore, MySQLFantasyCriticUserStore>();
        services.AddScoped<IFantasyCriticRoleStore, MySQLFantasyCriticRoleStore>();
        services.AddScoped<IUserStore<FantasyCriticUser>, MySQLFantasyCriticUserStore>();
        services.AddScoped<IRoleStore<FantasyCriticRole>, MySQLFantasyCriticRoleStore>();

        services.AddScoped<IMasterGameRepo, MySQLMasterGameRepo>();
        services.AddScoped<IFantasyCriticRepo, MySQLFantasyCriticRepo>();
        services.AddScoped<IRoyaleRepo, MySQLRoyaleRepo>();

        services.AddScoped<PatreonService>(_ => new PatreonService(
            configuration.AssertConfigValue("PatreonService:AccessToken"),
            configuration.AssertConfigValue("PatreonService:RefreshToken"),
            configuration.AssertConfigValue("Authentication:Patreon:ClientId"),
            configuration.AssertConfigValue("PatreonService:CampaignID")
        ));

        var tempFolder = Path.Combine(rootFolder, "Temp");
        services.AddScoped<IHypeFactorService>(_ => new LambdaHypeFactorService(awsRegion, awsBucket, tempFolder));
        services.AddScoped<IRDSManager>(_ => new RDSManager(rdsInstanceName));
        services.AddScoped<FantasyCriticUserManager>();
        services.AddScoped<FantasyCriticRoleManager>();
        services.AddScoped<GameAcquisitionService>();
        services.AddScoped<LeagueMemberService>();
        services.AddScoped<PublisherService>();
        services.AddScoped<InterLeagueService>();
        services.AddScoped<DraftService>();
        services.AddScoped<GameSearchingService>();
        services.AddScoped<TradeService>();
        services.AddScoped<FantasyCriticService>();
        services.AddScoped<RoyaleService>();
        services.AddScoped<EmailSendingService>();
        services.AddScoped<DiscordPushService>();
        services.AddScoped<DiscordRequestService>();

        services.AddScoped<IEmailSender>(_ => new MailGunEmailSender("fantasycritic.games", mailgunAPIKey, "noreply@fantasycritic.games", "Fantasy Critic"));

        services.AddScoped<AdminService>();

        services.AddHttpClient<IOpenCriticService, OpenCriticService>(client =>
        {
            client.BaseAddress = new Uri("https://opencritic-api.p.rapidapi.com/");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", openCriticAPIKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "opencritic-api.p.rapidapi.com");
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
        services.AddSingleton<IScheduledTask, GrantSuperDropsTask>();
        services.AddSingleton<IScheduledTask, ExpireTradesTask>();
        services.AddScheduler((_, args) =>
        {
            args.SetObserved();
        });

        //Hosted Services
        services.AddHostedService<DiscordHostedService>();

        services.AddAuthorization(options =>
        {
            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
            policyBuilder.RequireAuthenticatedUser();

            var basicUserPolicy = policyBuilder.Build();

            options.AddPolicy("BasicUser", basicUserPolicy);
            options.DefaultPolicy = basicUserPolicy;

            options.AddPolicy("PlusUser", policy =>
            {
                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole("PlusUser");
            });

            options.AddPolicy("Admin", policy =>
            {
                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            });

            options.AddPolicy("FactChecker", policy =>
            {
                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole("FactChecker");
            });

            options.AddPolicy("Write", policy =>
            {
                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
                policy.RequireAuthenticatedUser();
            });
        });

        services.AddIdentity<FantasyCriticUser, FantasyCriticRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                const string letters = "abcdefghijklmnopqrstuvwxyz";
                const string numbers = "0123456789";
                const string specials = "-._@+ ";
                options.User.AllowedUserNameCharacters = letters + letters.ToUpper() + numbers + specials;
            })
            .AddSignInManager<FantasyCriticSignInManager>()
            .AddUserManager<FantasyCriticUserManager>()
            .AddRoleManager<FantasyCriticRoleManager>()
            .AddDefaultTokenProviders();

        var authenticationBuilder = services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "FantasyCriticCookie";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration =
                    true; // the cookie would be re-issued on any request half way through the ExpireTimeSpan
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
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

        services.AddSingleton<IXmlRepository, MySQLXmlRepository>();
        var serviceProvider = services.BuildServiceProvider();
        services.AddDataProtection()
            .AddKeyManagementOptions(options => options.XmlRepository = serviceProvider.GetService<IXmlRepository>());

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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !env.IsDevelopment())
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

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();
        app.MapHub<UpdateHub>("/updatehub");

        app.MapFallbackToFile("index.html");

        return app;
    }
}
