using System.Reflection;
using FantasyCritic.Discord.Bot;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Dapper.NodaTime;
using FantasyCritic.Discord;

namespace FantasyCritic.DiscordBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        // Bot Dependencies
        var botToken = configuration["BotToken"];
        var repositoryConfiguration = new RepositoryConfiguration(configuration["ConnectionString"], SystemClock.Instance);
        var userStore = new MySQLFantasyCriticUserStore(repositoryConfiguration);
        var masterGameRepo = new MySQLMasterGameRepo(repositoryConfiguration, userStore);
        var fantasyCriticRepo = new MySQLFantasyCriticRepo(repositoryConfiguration, userStore, masterGameRepo);
        var discordRepo = new MySQLDiscordRepo(repositoryConfiguration, fantasyCriticRepo);
        var clock = SystemClock.Instance;
        var parameterParser = new ParameterParser();

        DapperNodaTimeSetup.Register();

        var discordBotService = new DiscordBotService(botToken, fantasyCriticRepo, discordRepo, clock, parameterParser);
        await discordBotService.InitializeBot();

        await Task.Delay(-1);
    }
}
