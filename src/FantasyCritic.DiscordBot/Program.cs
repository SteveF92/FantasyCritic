using System.Reflection;
using FantasyCritic.Discord.Bot;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Dapper.NodaTime;
using FantasyCritic.Discord.Models;
using FantasyCritic.Discord.Utilities;
using FantasyCritic.Lib.Services;
using Serilog;
using Serilog.Events;

namespace FantasyCritic.DiscordBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        ConfigureLogging();
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        var section = configuration.GetSection("DiscordSettings");
        var discordSettings = section.Get<DiscordSettings>();

        // Bot Dependencies
        var botToken = configuration["BotToken"];
        var repositoryConfiguration = new RepositoryConfiguration(configuration["ConnectionString"], SystemClock.Instance);
        var baseAddress = configuration["BaseAddress"];
        var userStore = new MySQLFantasyCriticUserStore(repositoryConfiguration);
        var masterGameRepo = new MySQLMasterGameRepo(repositoryConfiguration, userStore);
        var fantasyCriticRepo = new MySQLFantasyCriticRepo(repositoryConfiguration, userStore, masterGameRepo);
        var clock = SystemClock.Instance;
        var interLeagueService = new InterLeagueService(fantasyCriticRepo, masterGameRepo, clock);
        var gameSearchingService = new GameSearchingService(interLeagueService, clock);
        var discordRepo = new MySQLDiscordRepo(repositoryConfiguration, fantasyCriticRepo);
        var parameterParser = new DiscordParameterParser();
        var discordFormatter = new DiscordFormatter(discordSettings);

        DapperNodaTimeSetup.Register();

        var discordBotService = new DiscordBotService(botToken,
            fantasyCriticRepo,
            discordRepo,
            clock,
            gameSearchingService,
            parameterParser,
            discordFormatter,
            discordSettings,
            baseAddress);
        await discordBotService.InitializeBot();

        await Task.Delay(-1);
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
