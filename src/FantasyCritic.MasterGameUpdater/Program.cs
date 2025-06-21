using System.ComponentModel.Design;
using System.Reflection;
using Dapper.NodaTime;
using DiscordDotNetUtilities;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using NodaTime;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using FantasyCritic.Lib.Domain;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.SharedSerialization.API;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.MasterGameUpdater;

public static class Program
{
    private static string _localConnectionString = null!;
    private static string _baseAddress = null!;
    private static Guid _addedByUserIDOverride;

    private static readonly IClock _clock = SystemClock.Instance;

    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        _localConnectionString = configuration["localConnectionString"]!;
        _baseAddress = configuration["baseAddress"]!;
        _addedByUserIDOverride = Guid.Parse(configuration["addedByUserIDOverride"]!);

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        DapperNodaTimeSetup.Register();

        await UpdateMasterGames();
    }

    private static async Task UpdateMasterGames()
    {
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLMasterGameRepo localMasterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore, _clock);
        MySQLMasterGameUpdater gameUpdater = new MySQLMasterGameUpdater(_localConnectionString);
        AdminService localAdminService = GetAdminService();

        Log.Information("Getting master games from production");
        var productionMasterGameTags = await GetTagsFromAPI();
        var productionMasterGames = await GetMasterGamesFromAPI(productionMasterGameTags);
        var localMasterGameTags = await localMasterGameRepo.GetMasterGameTags();
        var localMasterGames = await localMasterGameRepo.GetMasterGames();
        await gameUpdater.UpdateMasterGames(productionMasterGameTags, productionMasterGames, localMasterGameTags, localMasterGames, _addedByUserIDOverride);
        await localAdminService.RefreshCaches();
    }

    private static async Task<IReadOnlyList<MasterGameTag>> GetTagsFromAPI()
    {
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var tagsString = await client.GetStringAsync("api/Game/GetMasterGameTags");
        var objects = JsonConvert.DeserializeObject<List<MasterGameTagViewModel>>(tagsString)!;
        var domains = objects.Select(x => x.ToDomain()).ToList();
        return domains;
    }

    private static async Task<IReadOnlyList<MasterGame>> GetMasterGamesFromAPI(IReadOnlyList<MasterGameTag> tags)
    {
        var tagDictionary = tags.ToDictionary(x => x.Name);
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var gamesString = await client.GetStringAsync("api/Game/MasterGame");
        var objects = JsonConvert.DeserializeObject<List<MasterGameViewModel>>(gamesString)!;
        var domains = objects.Select(x => x.ToDomain(tagDictionary)).ToList();
        return domains;
    }

    private static AdminService GetAdminService()
    {
        FantasyCriticUserManager userManager = null!;
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        IFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore, _clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(localRepoConfig);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(localRepoConfig, localUserStore, masterGameRepo, combinedDataRepo);
        IConferenceRepo conferenceRepo = new MySQLConferenceRepo(localRepoConfig, localUserStore, masterGameRepo, combinedDataRepo);
        IDiscordRepo discordRepo = new MySQLDiscordRepo(localRepoConfig, fantasyCriticRepo, masterGameRepo, conferenceRepo, combinedDataRepo, _clock);
        IRoyaleRepo royaleRepo = new MySQLRoyaleRepo(localRepoConfig, localUserStore, masterGameRepo);
        DiscordPushService discordPushService = new DiscordPushService(new FantasyCriticDiscordConfiguration("", _baseAddress, true, null), _clock, new ServiceContainer(), new DiscordFormatter());
        InterLeagueService interLeagueService = new InterLeagueService(fantasyCriticRepo, combinedDataRepo, masterGameRepo, _clock, discordPushService);
        LeagueMemberService leagueMemberService = new LeagueMemberService(null!, fantasyCriticRepo, combinedDataRepo);
        GameAcquisitionService gameAcquisitionService = new GameAcquisitionService(fantasyCriticRepo, masterGameRepo, _clock, discordPushService);
        FantasyCriticService fantasyCriticService = new FantasyCriticService(leagueMemberService, interLeagueService, discordPushService, gameAcquisitionService, fantasyCriticRepo, combinedDataRepo, discordRepo, _clock);
        IOpenCriticService openCriticService = null!;
        IGGService ggService = null!;
        PatreonService patreonService = null!;
        IRDSManager rdsManager = null!;
        RoyaleService royaleService = new RoyaleService(royaleRepo, _clock, masterGameRepo);
        IHypeFactorService hypeFactorService = new DefaultHypeFactorService();

        return new AdminService(fantasyCriticService, userManager, fantasyCriticRepo, masterGameRepo, interLeagueService,
            openCriticService, ggService, patreonService, _clock, rdsManager, royaleService, hypeFactorService, discordPushService, discordRepo);
    }
}
