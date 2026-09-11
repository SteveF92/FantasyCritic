using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using FantasyCritic.AWS;
using FantasyCritic.EmailTemplates;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Scheduling;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using FantasyCritic.Postmark;
using FantasyCritic.Web.Authorization;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.OpenApi;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime.Serialization.SystemTextJson;
using NSwag;
using Serilog;
using CacheControlHeaderValue = Microsoft.Net.Http.Headers.CacheControlHeaderValue;
using IEmailSender = FantasyCritic.Lib.Interfaces.IEmailSender;

namespace FantasyCritic.Web;

public static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfigurationRoot configuration)
    {
        var services = builder.Services;
        var environment = builder.Environment;

        Log.Information($"Startup: Running in {environment} mode.");

        var rdsInstanceName = configuration["AWS:rdsInstanceName"]!;
        var postmarkAPIKey = configuration["Postmark:apiKey"]!;
        var openCriticAPIKey = configuration["OpenCritic:apiKey"]!;

        //Repositories, domain services and the Discord push service. Shared with the bot process
        //(and, later, the Hangfire worker) so the three hosts cannot drift apart.
        services.AddFantasyCriticCore(configuration, environment);

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: [DatabaseHealthCheck.ReadyTag]);

        //Read once at startup: the RELEASE file cannot change without a new deploy, which restarts the process.
        services.AddSingleton<BuildInfo>(_ => BuildInfoReader.Read(environment.ContentRootPath));

        //Web-only services
        services.AddScoped<IEmailBuilder, RazorEmailBuilder>();
        services.AddScoped<IRDSManager>(_ => new RDSManager(rdsInstanceName));
        services.AddScoped<EmailSendingService>();

        //Email Services
        //services.AddScoped<IEmailSender>(_ => new SESEmailSender(configuration["AWS:region"], "noreply@fantasycritic.games"));
        //services.AddScoped<IEmailSender>(_ => new MailGunEmailSender("fantasycritic.games", mailgunAPIKey, "noreply@fantasycritic.games", "Fantasy Critic"));
        services.AddScoped<IEmailSender>(_ => new PostmarkEmailSender(postmarkAPIKey, "admin@fantasycritic.games"));

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
            services.AddSingleton<IScheduledTask, ReleasingThisWeekNotificationTask>();
            services.AddScheduler((_, args) =>
            {
                args.SetObserved();
            });
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

            options.AddPolicy("FactCheckerOrAdmin", policy =>
            {
                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole("FactChecker", "Admin");
            });

            options.AddPolicy("ActionRunner", policy =>
            {
                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole("ActionRunner");
            });
        });

        services.AddIdentity<FantasyCriticUser, FantasyCriticRole>(FantasyCriticIdentityOptions.Configure)
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
                options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
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
            .SetApplicationName($"FantasyCritic-{environment.EnvironmentName}")
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
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            })
            .AddControllersAsServices();

        services.AddRazorPages();
        services.AddSignalR();

        if (environment.IsDevelopment())
        {
            // Only in Development, used for debugging
            services.AddTransient<IAuthorizationHandler, FantasyCriticAuthorizationDebugHandler>();
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
        services.AddSession();
        services.AddOpenApiDocument(settings =>
        {
            settings.Title = "Fantasy Critic API";
            settings.DocumentName = "FantasyCriticAPI";
            settings.OperationProcessors.Add(new FantasyCriticOperationProcessor());
            settings.SchemaSettings.SchemaProcessors.Add(new RequireNonNullablePropertiesSchemaProcessor());
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        var environmentConfig = app.Services.GetService<EnvironmentConfiguration>()!;

        var env = app.Environment;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !env.IsDevelopment())
        {
            app.UseForwardedHeaders();
        }

        // Health probes run before HTTPS redirection so that a plain-HTTP request from the
        // deploy script (or, later, an ALB target group) gets an answer instead of a 307.
        // /health is liveness only: it runs no checks and just proves the process is serving.
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        // /health/ready additionally proves the database is reachable.
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(DatabaseHealthCheck.ReadyTag)
        });

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

        // Add OpenAPI/Swagger middlewares
        app.UseOpenApi(settings =>
        {
            settings.PostProcess = (document, httpRequest) =>
            {
                document.Servers.Clear();
                document.Servers.Add(new OpenApiServer { Url = environmentConfig.BaseAddress });
            };
        });

        app.UseSwaggerUi();

        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRewriter(new RewriteOptions()
            .AddRedirectToWww()
        );

        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var request = httpContext.Request;

                if (request.Path.StartsWithSegments("/Account"))
                {
                    return;
                }

                foreach (var (key, value) in request.Query)
                {
                    diagnosticContext.Set($"Param_{key}", value.ToString());
                }

                foreach (var (key, value) in httpContext.GetRouteData().Values)
                {
                    if (key is "controller" or "action" or "page" || value is null)
                    {
                        continue;
                    }

                    diagnosticContext.Set($"Route_{key}", value.ToString());
                }
            };
        });

        app.UseRouting();

        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        // The assets under /assets are safe to cache for a long time.
        app.Use(async (context, next) =>
        {
            // Routing has already run, so an /assets path that did not match a static asset
            // is the SPA fallback and must keep its no-store headers.
            var isStaticAsset = context.GetEndpoint()?.Metadata.GetMetadata<StaticAssetDescriptor>() is not null;
            if (isStaticAsset && context.Request.Path.StartsWithSegments("/assets"))
            {
                context.Response.OnStarting(() =>
                {
                    if (context.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                    }

                    return Task.CompletedTask;
                });
            }

            await next();
        });

        // Using MapStaticAssets instead of UseStaticFiles lets us send compressed responses for our static files.
        app.MapStaticAssets();

        app.MapControllers();
        app.MapRazorPages();
        app.MapHub<UpdateHub>("/updatehub");

        app.MapFallbackToFile("index.html", new StaticFileOptions()
        {
            OnPrepareResponse = (context) =>
            {
                // Prevent caching for HTML files
                var headers = context.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };
                context.Context.Response.Headers["Pragma"] = "no-cache";
                context.Context.Response.Headers["Expires"] = "0";
            }
        });

        return app;
    }
}
