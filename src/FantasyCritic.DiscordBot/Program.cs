using System.Reflection;
using FantasyCritic.Discord.Bot;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Dapper.NodaTime;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Discord.Models;
using FantasyCritic.Discord.Utilities;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
namespace FantasyCritic.DiscordBot;

public class Program
{
    private static ServiceProvider _serviceProvider;
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

        var repositoryConfiguration = new RepositoryConfiguration(configuration["ConnectionString"], SystemClock.Instance);

        _serviceProvider = new ServiceCollection()
            .AddSingleton(configuration)
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

        // Bot Dependencies
        //var botToken = configuration["BotToken"];
        //var baseAddress = configuration["BaseAddress"];
        //var userStore = new MySQLFantasyCriticUserStore(repositoryConfiguration);
        // var masterGameRepo = new MySQLMasterGameRepo(repositoryConfiguration, userStore);
        //var fantasyCriticRepo = new MySQLFantasyCriticRepo(repositoryConfiguration, userStore, masterGameRepo);
        //var clock = SystemClock.Instance;
        //var interLeagueService = new InterLeagueService(fantasyCriticRepo, masterGameRepo, clock);
        //var gameSearchingService = new GameSearchingService(interLeagueService, clock);
        //var discordRepo = new MySQLDiscordRepo(repositoryConfiguration, fantasyCriticRepo);
        //var parameterParser = new DiscordParameterParser();
        //var discordFormatter = new DiscordFormatter(discordSettings);
        //var interactionService = serviceProvider.GetRequiredService<InteractionService>();


        //var discordBotService = new DiscordBotService(
        //    botToken,
        //    fantasyCriticRepo,
        //    discordRepo,
        //    clock,
        //    gameSearchingService,
        //    parameterParser,
        //    discordFormatter,
        //    interactionService,
        //    serviceProvider, baseAddress);
        //await discordBotService.InitializeBot();



        await Task.Delay(Timeout.Infinite);
    }

    public async Task RunAsync()
    {
        //var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

        //client.Log += LogAsync;

        // Here we can initialize the service that will register and execute our commands
        await _serviceProvider
            .GetRequiredService<DiscordBotService>()
            .InitializeBotAsync();

        // Bot token can be provided from the Configuration object we set up earlier
        //await client.LoginAsync(TokenType.Bot, token);
        //await client.StartAsync();

        // Never quit the program until manually forced to.
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
