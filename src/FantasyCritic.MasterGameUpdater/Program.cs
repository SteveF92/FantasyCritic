using System.Reflection;
using Dapper.NodaTime;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using NodaTime;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.SharedSerialization;
using Microsoft.Extensions.Configuration;
using Serilog;
using FantasyCritic.Lib.Domain;
using Newtonsoft.Json;
using FantasyCritic.SharedSerialization.API;

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

        _localConnectionString = configuration["localConnectionString"];
        _baseAddress = configuration["baseAddress"];
        _addedByUserIDOverride = Guid.Parse(configuration["addedByUserIDOverride"]);

        DapperNodaTimeSetup.Register();

        await UpdateMasterGames();
    }

    private static async Task UpdateMasterGames()
    {
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLMasterGameRepo localMasterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore);
        MySQLMasterGameUpdater gameUpdater = new MySQLMasterGameUpdater(_localConnectionString);
        AdminService localAdminService = GetAdminService();

        Log.Information("Getting master games from production");
        var productionMasterGameTags = await GetTagsFromAPI();
        var productionMasterGames = await GetMasterGamesFromAPI();
        var localMasterGameTags = await localMasterGameRepo.GetMasterGameTags();
        var localMasterGames = await localMasterGameRepo.GetMasterGames();
        await gameUpdater.UpdateMasterGames(productionMasterGameTags, productionMasterGames, localMasterGameTags, localMasterGames, _addedByUserIDOverride);
        await localAdminService.RefreshCaches();
    }

    private static async Task<IReadOnlyList<MasterGameTag>> GetTagsFromAPI()
    {
        HttpClient client = new HttpClient() { BaseAddress = new Uri(_baseAddress) };
        var tagsString = await client.GetStringAsync("api/Game/GetMasterGameTags");
        var objects = JsonConvert.DeserializeObject<List<MasterGameTagViewModel>>(tagsString);
        var domains = objects!.Select(x => x.ToDomain()).ToList();
        return domains;
    }

    private static async Task<IReadOnlyList<MasterGame>> GetMasterGamesFromAPI()
    {
        return new List<MasterGame>();
    }

    private static AdminService GetAdminService()
    {
        FantasyCriticUserManager userManager = null!;
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        IFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(localRepoConfig, localUserStore, masterGameRepo);
        InterLeagueService interLeagueService = new InterLeagueService(fantasyCriticRepo, masterGameRepo, _clock);
        LeagueMemberService leagueMemberService = new LeagueMemberService(null!, fantasyCriticRepo);
        FantasyCriticService fantasyCriticService = new FantasyCriticService(leagueMemberService, interLeagueService, fantasyCriticRepo, _clock);
        IOpenCriticService openCriticService = null!;
        IGGService ggService = null!;
        PatreonService patreonService = null!;
        IRDSManager rdsManager = null!;
        RoyaleService royaleService = null!;
        IHypeFactorService hypeFactorService = new DefaultHypeFactorService();

        return new AdminService(fantasyCriticService, userManager, fantasyCriticRepo, masterGameRepo, interLeagueService,
            openCriticService, ggService, patreonService, _clock, rdsManager, royaleService, hypeFactorService);
    }
}
