using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNetUtilities;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.AWS;
using FantasyCritic.EmailTemplates;
using FantasyCritic.Lib.BackgroundServices;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Scheduling;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
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
using Microsoft.Extensions.Configuration;
using FantasyCritic.Postmark;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Discord.EventHandlers;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Web;

public static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfigurationRoot configuration)
    {
        var services = builder.Services;
        var environment = builder.Environment;

        Log.Information($"Startup: Running in {environment} mode.");

        var rdsInstanceName = configuration["AWS:rdsInstanceName"]!;
        var awsBucket = configuration["AWS:bucket"]!;
        var postmarkAPIKey = configuration["Postmark:apiKey"]!;
        var openCriticAPIKey = configuration["OpenCritic:apiKey"]!;
        var baseAddress = configuration["BaseAddress"]!;
        var discordBotToken = configuration["BotToken"]!;

        var rootFolder = configuration["LinuxRootFolder"]!;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            rootFolder = configuration["RootFolder"]!;
        }

        IClock clock = SystemClock.Instance;

        // Add application services.
        services.AddHttpClient();
        services.AddTransient<IClock>(_ => clock);

        //MySQL Repos
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        var repoConfiguration = new RepositoryConfiguration(connectionString, clock);
        var patreonConfig = new PatreonConfig(configuration["Authentication:Patreon:ClientId"]!, configuration["PatreonService:CampaignID"]!);
        var emailSendingConfig = new EnvironmentConfiguration(baseAddress, environment.IsProduction());
        var discordConfiguration = new FantasyCriticDiscordConfiguration(discordBotToken, baseAddress, environment.IsDevelopment(), configuration.GetValue<ulong?>("DevDiscordServerId"));
        services.AddSingleton<RepositoryConfiguration>(_ => repoConfiguration);
        services.AddSingleton<PatreonConfig>(_ => patreonConfig);
        services.AddSingleton<EnvironmentConfiguration>(_ => emailSendingConfig);
        services.AddSingleton<FantasyCriticDiscordConfiguration>(_ => discordConfiguration);
        services.AddSingleton<IDiscordFormatter, DiscordFormatter>();
        services.AddSingleton<DiscordPushService>();

        services.AddScoped<IFantasyCriticUserStore, MySQLFantasyCriticUserStore>();
        services.AddScoped<IReadOnlyFantasyCriticUserStore, MySQLFantasyCriticUserStore>();
        services.AddScoped<IFantasyCriticRoleStore, MySQLFantasyCriticRoleStore>();
        services.AddScoped<IUserStore<FantasyCriticUser>, MySQLFantasyCriticUserStore>();
        services.AddScoped<IRoleStore<FantasyCriticRole>, MySQLFantasyCriticRoleStore>();

        services.AddScoped<IMasterGameRepo, MySQLMasterGameRepo>();
        services.AddScoped<IFantasyCriticRepo, MySQLFantasyCriticRepo>();
        services.AddScoped<IRoyaleRepo, MySQLRoyaleRepo>();
        services.AddScoped<IConferenceRepo, MySQLConferenceRepo>();
        services.AddScoped<IPatreonTokensRepo, MySQLPatreonTokensRepo>();
        services.AddScoped<IDiscordRepo, MySQLDiscordRepo>();
        services.AddScoped<IEmailBuilder, RazorEmailBuilder>();
        services.AddScoped<SelectMenuExecutedHandler>();

        services.AddScoped<PatreonService>();

        if (environment.IsProduction() || environment.IsStaging())
        {
            var tempFolder = Path.Combine(rootFolder, "Temp");
            services.AddScoped<IExternalHypeFactorService>(_ => new LambdaHypeFactorService(configuration["AWS:region"]!, awsBucket, tempFolder));
            services.AddScoped<IHypeFactorService, HypeFactorService>();
        }
        else
        {
            services.AddScoped<IHypeFactorService>(_ => new DefaultHypeFactorService());
        }

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
        services.AddScoped<ConferenceService>();
        services.AddScoped<EmailSendingService>();

        //Email Services
        //services.AddScoped<IEmailSender>(_ => new SESEmailSender(configuration["AWS:region"], "noreply@fantasycritic.games"));
        //services.AddScoped<IEmailSender>(_ => new MailGunEmailSender("fantasycritic.games", mailgunAPIKey, "noreply@fantasycritic.games", "Fantasy Critic"));
        services.AddScoped<IEmailSender>(_ => new PostmarkEmailSender(postmarkAPIKey, "noreply@fantasycritic.games"));

        if (environment.IsProduction() || environment.IsStaging())
        {
            var tempFolder = Path.Combine(rootFolder, "Temp");
            services.AddScoped<IExternalHypeFactorService>(_ => new LambdaHypeFactorService(configuration["AWS:region"]!, awsBucket, tempFolder));
        }
        else
        {
            services.AddScoped<IHypeFactorService>(_ => new DefaultHypeFactorService());
        }

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

        if (!environment.IsDevelopment())
        {
            //Add scheduled tasks & scheduler
            services.AddSingleton<IScheduledTask, RefreshDataTask>();
            services.AddSingleton<IScheduledTask, TimeFlagsTask>();
            services.AddSingleton<IScheduledTask, PatreonUpdateTask>();
            services.AddSingleton<IScheduledTask, PublicBiddingNotificationTask>();
            services.AddSingleton<IScheduledTask, ProcessSpecialAuctionsTask>();
            services.AddSingleton<IScheduledTask, GrantSuperDropsTask>();
            services.AddSingleton<IScheduledTask, ExpireTradesTask>();
            services.AddSingleton<IScheduledTask, GameReleaseNotificationTask>();
            services.AddScheduler((_, args) =>
            {
                args.SetObserved();
            });
        }

        if (!string.IsNullOrWhiteSpace(discordBotToken) && discordBotToken != "secret")
        {
            //Discord request service
            DiscordSocketConfig socketConfig = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true,
            };
            var fantasyCriticSettings = new FantasyCriticSettings
            {
                BaseAddress = baseAddress
            };
            services.AddSingleton(socketConfig);
            services.AddSingleton(fantasyCriticSettings);
            services.AddScoped<DiscordSocketClient>();
            services.AddScoped(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
            services.AddScoped<DiscordBotService>();
            services.AddHostedService<DiscordHostedService>();
        }

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
        });

        services.AddIdentity<FantasyCriticUser, FantasyCriticRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                const string letters = "abcdefghijklmnopqrstuvwxyz";
                const string numbers = "0123456789";
                const string specials = "-._@+ ";
                options.User.AllowedUserNameCharacters = letters + letters.ToUpper() + numbers + specials;
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 5;
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
                    options.ClientId = configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                    microsoftOptions.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                    microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
                    microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
                })
                .AddTwitch(options =>
                {
                    options.ClientId = configuration["Authentication:Twitch:ClientId"]!;
                    options.ClientSecret = configuration["Authentication:Twitch:ClientSecret"]!;
                })
                .AddPatreon(options =>
                {
                    options.ClientId = configuration["Authentication:Patreon:ClientId"]!;
                    options.ClientSecret = configuration["Authentication:Patreon:ClientSecret"]!;
                })
                .AddDiscord(options =>
                {
                    options.ClientId = configuration["Authentication:Discord:ClientId"]!;
                    options.ClientSecret = configuration["Authentication:Discord:ClientSecret"]!;
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

        services.AddRazorTemplating();

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
