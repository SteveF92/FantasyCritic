using System.ComponentModel.Design;
using System.Reflection;
using System.Text.Json;
using DiscordDotNetUtilities;
using FantasyCritic.Lib;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.SharedSerialization.API;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.DapperTypeMaps;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.MySQL.SyncingRepos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;
using Serilog;
using Serilog.Extensions.Logging;

namespace FantasyCritic.LocalDatabaseTool;

public static class Program
{
    private static string _localConnectionString = null!;
    private static string _baseAddress = null!;
    private static Guid _addedByUserIDOverride;

    private static readonly IClock _clock = SystemClock.Instance;

    private static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables()
            .Build();

        _localConnectionString = configuration["LocalConnectionString"]!;
        _baseAddress = configuration["BaseAddress"]!;
        _addedByUserIDOverride = Guid.Parse(configuration["AddedByUserIdOverride"]!);

        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();

        await EnsureLocalAdminUser();
        await UpdateSupportedYears();
        await UpdateRoyaleQuarters();
        await UpdateMasterGames();
        await RefreshCaches();
    }

    private static async Task EnsureLocalAdminUser()
    {
        Log.Information("Ensuring local admin user exists");
        var syncer = new MySQLLocalSetupSyncer(_localConnectionString);
        await syncer.EnsureLocalAdminUser(_clock);
    }

    private static async Task UpdateMasterGames()
    {
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLMasterGameRepo localMasterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore, _clock);
        MySQLMasterGameUpdater gameUpdater = new MySQLMasterGameUpdater(_localConnectionString);

        Log.Information("Getting master games from production");
        var productionMasterGameTags = await GetTagsFromAPI();
        var productionMasterGames = await GetMasterGamesFromAPI(productionMasterGameTags);
        var localMasterGameTags = await localMasterGameRepo.GetMasterGameTags();
        var localMasterGames = await localMasterGameRepo.GetMasterGames();
        await gameUpdater.UpdateMasterGames(productionMasterGameTags, productionMasterGames, localMasterGameTags, localMasterGames, _addedByUserIDOverride);
    }

    private static async Task RefreshCaches()
    {
        Log.Information("Refreshing caches");
        CacheRefresher localCacheRefresher = GetCacheRefresher();
        await localCacheRefresher.RefreshCaches(FantasyCriticJobContext.FakeContext, CancellationToken.None);
    }

    private static async Task<IReadOnlyList<MasterGameTag>> GetTagsFromAPI()
    {
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var tagsString = await client.GetStringAsync("api/Game/GetMasterGameTags");
        var objects = JsonSerializer.Deserialize<List<MasterGameTagViewModel>>(tagsString, FantasyCriticJsonOptions.Default)!;
        var domains = objects.Select(x => x.ToDomain()).ToList();
        return domains;
    }

    private static async Task<IReadOnlyList<MasterGame>> GetMasterGamesFromAPI(IReadOnlyList<MasterGameTag> tags)
    {
        var tagDictionary = tags.ToDictionary(x => x.Name);
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var gamesString = await client.GetStringAsync("api/Game/MasterGame");
        var objects = JsonSerializer.Deserialize<List<MasterGameViewModel>>(gamesString, FantasyCriticJsonOptions.Default)!;
        var domains = objects.Select(x => x.ToDomain(tagDictionary)).ToList();
        return domains;
    }

    private static CacheRefresher GetCacheRefresher()
    {
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        IFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore, _clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(localRepoConfig, localUserStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(localRepoConfig, localUserStore, masterGameRepo, combinedDataRepo);
        IRoyaleRepo royaleRepo = new MySQLRoyaleRepo(localRepoConfig, localUserStore, masterGameRepo);
        DiscordPushService discordPushService = new DiscordPushService(new FantasyCriticDiscordConfiguration("", _baseAddress, true), _clock, new ServiceContainer(), new DiscordFormatter());
        InterLeagueService interLeagueService = new InterLeagueService(fantasyCriticRepo, combinedDataRepo, masterGameRepo, _clock, discordPushService);
        RoyaleService royaleService = new RoyaleService(royaleRepo, _clock, masterGameRepo);
        IHypeFactorService hypeFactorService = new HypeFactorService(masterGameRepo, interLeagueService);

        var logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<CacheRefresher>();

        return new CacheRefresher(_clock, masterGameRepo, fantasyCriticRepo, hypeFactorService, discordPushService, royaleService, logger);
    }

    private static async Task UpdateSupportedYears()
    {
        Log.Information("Getting supported years from production");
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var json = await client.GetStringAsync("api/Game/SupportedYears");
        var responses = JsonSerializer.Deserialize<List<SupportedYearResponse>>(json, FantasyCriticJsonOptions.Default)!;

        var entities = responses.Select(r => new SupportedYearEntity
        {
            Year = r.Year,
            OpenForCreation = r.OpenForCreation,
            OpenForPlay = r.OpenForPlay,
            OpenForBetaUsers = false,
            StartDate = LocalDate.FromDateTime(r.StartDate),
            Finished = r.Finished
        }).ToList();

        var syncer = new MySQLLocalSetupSyncer(_localConnectionString);
        await syncer.UpsertSupportedYears(entities);
    }

    private static async Task UpdateRoyaleQuarters()
    {
        Log.Information("Getting royale year quarters from production");
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var json = await client.GetStringAsync("api/Royale/RoyaleQuarters");
        var responses = JsonSerializer.Deserialize<List<RoyaleQuarterResponse>>(json, FantasyCriticJsonOptions.Default)!;

        var entities = responses
            .Select(r => new RoyaleYearQuarter(new YearQuarter(r.Year, r.Quarter), r.OpenForPlay, r.Finished, null))
            .ToList();

        var syncer = new MySQLLocalSetupSyncer(_localConnectionString);
        await syncer.UpsertRoyaleYearQuarters(entities);
    }

    private record SupportedYearResponse(int Year, bool OpenForCreation, bool OpenForPlay,
        DateTime StartDate, bool Finished);

    private record RoyaleQuarterResponse(int Year, int Quarter, bool OpenForPlay, bool Finished);
}
