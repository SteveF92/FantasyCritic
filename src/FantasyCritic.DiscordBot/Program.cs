using System.Reflection;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Dapper.NodaTime;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Discord.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.Bot;

namespace FantasyCritic.DiscordBot;

public class Program
{
    private readonly ServiceProvider _serviceProvider;
    private readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
        AlwaysDownloadUsers = true,
    };

    public Program()
    {
        ConfigureLogging();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        var discordSettings = configuration.GetSection("DiscordSettings").Get<DiscordSettings>();
        var fantasyCriticSettings = configuration.GetSection("FantasyCriticSettings").Get<FantasyCriticSettings>();

        var repositoryConfiguration = new RepositoryConfiguration(configuration["ConnectionString"], SystemClock.Instance);

        _serviceProvider = new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton(fantasyCriticSettings)
            .AddSingleton(discordSettings)
            .AddSingleton(_socketConfig)
            .AddTransient<IClock>(_ => SystemClock.Instance)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<DiscordBotService>()
            .AddSingleton(repositoryConfiguration)
            .AddScoped<IMasterGameRepo, MySQLMasterGameRepo>()
            .AddScoped<IFantasyCriticUserStore, MySQLFantasyCriticUserStore>()
            .AddScoped<IReadOnlyFantasyCriticUserStore, MySQLFantasyCriticUserStore>()
            .AddScoped<IUserStore<FantasyCriticUser>, MySQLFantasyCriticUserStore>()
            .AddScoped<IFantasyCriticRepo, MySQLFantasyCriticRepo>()
            .AddScoped<InterLeagueService>()
            .AddScoped<GameSearchingService>()
            .AddScoped<IDiscordRepo, MySQLDiscordRepo>()
            .AddScoped<IDiscordParameterParser, DiscordParameterParser>()
            .AddScoped<IDiscordFormatter, DiscordFormatter>()
            .BuildServiceProvider();
    }

    public static async Task Main(string[] args)
    {
        DapperNodaTimeSetup.Register();
        await new Program().RunAsync();
    }

    public async Task RunAsync()
    {
        await _serviceProvider
            .GetRequiredService<DiscordBotService>()
            .InitializeBotAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
    }
}
