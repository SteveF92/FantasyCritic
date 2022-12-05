using System.Reflection;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Dapper.NodaTime;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNetUtilities;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using FantasyCritic.Lib.Discord.Models;

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

        var fantasyCriticSettings = configuration.GetSection("FantasyCriticSettings").Get<FantasyCriticSettings>()!;
        ulong? devDiscordServerID = configuration.GetValue<ulong?>("DevDiscordServerId");

        var fantasyCriticDiscordConfiguration = new FantasyCriticDiscordConfiguration(configuration.GetValue<string>("BotToken")!, fantasyCriticSettings.BaseAddress, true, devDiscordServerID);

        var repositoryConfiguration = new RepositoryConfiguration(configuration["ConnectionString"]!, SystemClock.Instance);

        _serviceProvider = new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton(fantasyCriticSettings)
            .AddSingleton(fantasyCriticDiscordConfiguration)
            .AddSingleton(_socketConfig)
            .AddTransient<IClock>(_ => SystemClock.Instance)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<DiscordBotService>()
            .AddSingleton(repositoryConfiguration)
            .AddScoped<IDiscordRepo, MySQLDiscordRepo>()
            .AddScoped<IDiscordSupplementalDataRepo, MySQLDiscordSupplementalDataRepo>()
            .AddScoped<DiscordPushService>()
            .AddScoped<IMasterGameRepo, MySQLMasterGameRepo>()
            .AddScoped<IFantasyCriticUserStore, MySQLFantasyCriticUserStore>()
            .AddScoped<IReadOnlyFantasyCriticUserStore, MySQLFantasyCriticUserStore>()
            .AddScoped<IUserStore<FantasyCriticUser>, MySQLFantasyCriticUserStore>()
            .AddScoped<IFantasyCriticRepo, MySQLFantasyCriticRepo>()
            .AddScoped<InterLeagueService>()
            .AddScoped<GameSearchingService>()
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
