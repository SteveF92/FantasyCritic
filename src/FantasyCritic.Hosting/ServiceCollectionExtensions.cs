using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNetUtilities;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.BackgroundServices;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace FantasyCritic.Hosting;

public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Configuration, repositories and domain services — everything needed to talk to the database
    /// and run game logic, with no web, email or admin concerns. Web layers ASP.NET Identity,
    /// authentication and its controllers on top; the bot layers the Discord gateway on top.
    /// </summary>
    public static IServiceCollection AddFantasyCriticCore(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var baseAddress = configuration["BaseAddress"]!;
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var discordBotToken = configuration["BotToken"]!;

        IClock clock = SystemClock.Instance;

        services.AddHttpClient();
        services.AddTransient<IClock>(_ => clock);

        //Configuration objects
        services.AddSingleton(new RepositoryConfiguration(connectionString, clock));
        services.AddSingleton(new PatreonConfig(configuration["Authentication:Patreon:ClientId"]!, configuration["PatreonService:CampaignID"]!));
        services.AddSingleton(new EnvironmentConfiguration(baseAddress, environment.IsProduction()));
        services.AddSingleton(new FantasyCriticDiscordConfiguration(discordBotToken, baseAddress, environment.IsDevelopment(), configuration.GetValue<ulong?>("DevDiscordServerId")));

        //MySQL repos
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

        //Domain services
        services.AddScoped<PatreonService>();
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
    public static IServiceCollection AddFantasyCriticDiscordBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
        });
        services.AddSingleton(new FantasyCriticSettings
        {
            BaseAddress = configuration["BaseAddress"]!
        });

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<DiscordBotService>();
        services.AddSingleton<RoleHandler>();
        services.AddHostedService<DiscordHostedService>();

        return services;
    }
}
