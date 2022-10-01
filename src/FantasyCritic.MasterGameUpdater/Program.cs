using System.Reflection;
using Dapper;
using Dapper.NodaTime;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using NodaTime;
using MySqlConnector;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.SharedSerialization;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace FantasyCritic.MasterGameUpdater;

public static class Program
{
    private static string _awsRegion = null!;
    private static string _betaBucket = null!;
    private static string _productionReadOnlyConnectionString = null!;
    private static string _betaConnectionString = null!;
    private static string _localConnectionString = null!;
    private static string _productionRDSName = null!;
    private static string _betaRDSName = null!;
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

        _awsRegion = configuration["awsRegion"];
        _betaBucket = configuration["betaBucket"];
        _productionReadOnlyConnectionString = configuration["productionConnectionString"];
        _betaConnectionString = configuration["betaConnectionString"];
        _localConnectionString = configuration["localConnectionString"];
        _productionRDSName = configuration["productionRDSName"];
        _betaRDSName = configuration["betaRDSName"];
        _addedByUserIDOverride = Guid.Parse(configuration["addedByUserIDOverride"]);

        DapperNodaTimeSetup.Register();

        await UpdateMasterGames();
    }

    private static async Task UpdateMasterGames()
    {
        RepositoryConfiguration productionRepoConfig = new RepositoryConfiguration(_productionReadOnlyConnectionString, _clock);
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        MySQLFantasyCriticUserStore productionUserStore = new MySQLFantasyCriticUserStore(productionRepoConfig);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLMasterGameRepo productionMasterGameRepo = new MySQLMasterGameRepo(productionRepoConfig, productionUserStore);
        MySQLMasterGameRepo localMasterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore);
        MySQLMasterGameUpdater gameUpdater = new MySQLMasterGameUpdater(_localConnectionString);
        AdminService localAdminService = GetAdminService();

        Log.Information("Getting master games from production");
        var productionMasterGameTags = await productionMasterGameRepo.GetMasterGameTags();
        var productionMasterGames = await productionMasterGameRepo.GetMasterGames();
        var localMasterGameTags = await localMasterGameRepo.GetMasterGameTags();
        var localMasterGames = await localMasterGameRepo.GetMasterGames();
        IReadOnlyList<MasterGameHasTagEntity> productionGamesHaveTagEntities = await GetProductionGamesHaveTagEntities();
        await gameUpdater.UpdateMasterGames(productionMasterGameTags, productionMasterGames, localMasterGameTags,
            localMasterGames, productionGamesHaveTagEntities, _addedByUserIDOverride);
        await localAdminService.RefreshCaches();
    }

    private static async Task<IReadOnlyList<MasterGameHasTagEntity>> GetProductionGamesHaveTagEntities()
    {
        await using var connection = new MySqlConnection(_productionReadOnlyConnectionString);
        var masterGameTagResults = await connection.QueryAsync<MasterGameHasTagEntity>("select * from tbl_mastergame_hastag;");
        return masterGameTagResults.ToList();
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
