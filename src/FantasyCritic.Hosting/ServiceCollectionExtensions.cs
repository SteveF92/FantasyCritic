using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNetUtilities;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.AWS;
using FantasyCritic.EmailTemplates;
using FantasyCritic.Lib.BackgroundServices;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using FantasyCritic.Postmark;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace FantasyCritic.Hosting;

public static class ServiceCollectionExtensions
{

    /// <summary>
    /// The clock, the connection string and every Dapper repository: the layer that needs nothing but the
    /// database. <see cref="AddFantasyCriticCore"/> calls this, so a repository is still registered in exactly
    /// one place. A host that wants the database and nothing else calls it directly instead — the command line
    /// tool does, so that a deploy's `worker-should-pull` does not depend on the Discord, Patreon and email
    /// configuration it would never use.
    /// </summary>
    public static IServiceCollection AddFantasyCriticRepositories(this IServiceCollection services, ConnectionStringsOptions connectionStrings)
    {
        IClock clock = SystemClock.Instance;

        services.AddTransient<IClock>(_ => clock);
        services.AddSingleton(new RepositoryConfiguration(connectionStrings.DefaultConnection, clock));

        services.AddScoped<IFantasyCriticUserStore, MySQLFantasyCriticUserStore>();
        services.AddScoped<IReadOnlyFantasyCriticUserStore, MySQLFantasyCriticUserStore>();
        services.AddScoped<IFantasyCriticRoleStore, MySQLFantasyCriticRoleStore>();
        services.AddScoped<IUserStore<FantasyCriticUser>, MySQLFantasyCriticUserStore>();
        services.AddScoped<IRoleStore<FantasyCriticRole>, MySQLFantasyCriticRoleStore>();

        services.AddScoped<IMasterGameRepo, MySQLMasterGameRepo>();
        services.AddScoped<IFantasyCriticRepo, MySQLFantasyCriticRepo>();
        services.AddScoped<ICombinedDataRepo, MySQLCombinedDataRepo>();
        services.AddScoped<IRoyaleRepo, MySQLRoyaleRepo>();
        services.AddScoped<IConferenceRepo, MySQLConferenceRepo>();
        services.AddScoped<IPatreonTokensRepo, MySQLPatreonTokensRepo>();
        services.AddScoped<IDiscordRepo, MySQLDiscordRepo>();
        services.AddScoped<IDailyStatsRepo, MySQLDailyStatsRepo>();
        services.AddScoped<IJobRepo, MySQLJobRepo>();

        return services;
    }

    /// <summary>
    /// Configuration, repositories and domain services — everything needed to talk to the database
    /// and run game logic, with no web, email or admin concerns. Web layers ASP.NET Identity,
    /// authentication and its controllers on top; the bot layers the Discord gateway on top.
    /// </summary>
    public static IServiceCollection AddFantasyCriticCore(this IServiceCollection services, ConnectionStringsOptions connectionStrings,
        DiscordOptions discord, string baseAddress, IHostEnvironment environment)
    {
        services.AddHttpClient();
        services.AddFantasyCriticRepositories(connectionStrings);

        //Configuration objects
        services.AddSingleton(new EnvironmentConfiguration(baseAddress, environment.IsProduction(), IntegrationTestMode: false));
        services.AddSingleton(new FantasyCriticDiscordConfiguration(discord.BotToken, baseAddress, environment.IsDevelopment()));

        //Domain services
        services.AddScoped<IHypeFactorService, HypeFactorService>();
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
        services.AddScoped<AllTimeStatsService>();

        //The push half of the Discord integration. It opens its own gateway connection in its
        //constructor, so it must be a singleton no matter which host it is running in.
        services.AddSingleton<IDiscordFormatter, DiscordFormatter>();
        services.AddSingleton<DiscordPushService>();

        return services;
    }

    /// <summary>
    /// <see cref="AdminService"/> and the external systems behind it: OpenCritic, GG, Patreon and RDS snapshots.
    /// Registered by Web, which calls it from controller actions, and by the worker, whose job handlers
    /// call it. Needs <see cref="AddFantasyCriticCore"/> and <see cref="AddFantasyCriticIdentityCore"/>
    /// (or Web's own Identity registration).
    /// </summary>
    public static IServiceCollection AddFantasyCriticAdminServices(this IServiceCollection services, AwsOptions aws, OpenCriticOptions openCritic,
        PatreonOptions patreon)
    {
        services.AddFantasyCriticPatreon(patreon);
        services.AddScoped<AdminService>();
        services.AddScoped<IRDSManager>(_ => new RDSManager(aws.RdsInstanceName));

        services.AddHttpClient<IOpenCriticService, OpenCriticService>(client =>
        {
            client.BaseAddress = new Uri("https://opencritic-api.p.rapidapi.com/");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", openCritic.ApiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "opencritic-api.p.rapidapi.com");
        });
        services.AddHttpClient<IGGService, GGService>(client =>
        {
            client.BaseAddress = new Uri("https://api.ggapp.io/");
        });

        return services;
    }

    /// <summary>
    /// Private so that it is only ever registered once, by <see cref="AddFantasyCriticAdminServices"/>:
    /// AdminService is why a host needs Patreon at all.
    /// </summary>
    private static IServiceCollection AddFantasyCriticPatreon(this IServiceCollection services, PatreonOptions patreon)
    {
        //As the base type: Web passes its PatreonAuthOptions, which also carries the login secret.
        services.AddSingleton<PatreonOptions>(patreon);
        services.AddScoped<PatreonService>();
        return services;
    }

    /// <summary>
    /// Outbound email: Razor templates rendered and sent through Postmark. Registered by Web and by the
    /// worker, which sends the public bidding emails.
    /// </summary>
    public static IServiceCollection AddFantasyCriticEmail(this IServiceCollection services, PostmarkOptions postmark)
    {
        services.AddRazorTemplating();
        services.AddScoped<IEmailBuilder, RazorEmailBuilder>();
        services.AddScoped<IEmailSender>(_ => new PostmarkEmailSender(postmark.ApiKey, "admin@fantasycritic.games"));
        services.AddScoped<EmailSendingService>();

        return services;
    }

    /// <summary>
    /// The ASP.NET Identity infrastructure <see cref="FantasyCriticUserManager"/> needs — password
    /// hasher, validators, normalizer, <c>IdentityOptions</c>. For hosts that are not the web app;
    /// Web gets these from its own <c>AddIdentity</c> call, which also wires up sign-in and cookies.
    /// </summary>
    public static IServiceCollection AddFantasyCriticIdentityCore(this IServiceCollection services)
    {
        services.AddIdentityCore<FantasyCriticUser>(FantasyCriticIdentityOptions.Configure)
            .AddRoles<FantasyCriticRole>()
            .AddUserManager<FantasyCriticUserManager>()
            .AddRoleManager<FantasyCriticRoleManager>();

        return services;
    }

    /// <summary>
    /// The command-handling half of the Discord integration: the gateway client, the interaction
    /// framework, and the hosted service that connects them. Only the bot process registers these —
    /// running them in the web app as well would handle every slash command twice.
    /// </summary>
    public static IServiceCollection AddFantasyCriticDiscordBot(this IServiceCollection services)
    {
        services.AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
        });

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<DiscordBotService>();
        services.AddSingleton<RoleHandler>();
        services.AddHostedService<DiscordHostedService>();

        return services;
    }
}
