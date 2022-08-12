using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Threading;
using Dapper;
using Dapper.NodaTime;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using NodaTime;
using FantasyCritic.AWS;
using FantasyCritic.MySQL.Entities;
using MySqlConnector;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.DependencyInjection;
using Serilog;

namespace FantasyCritic.BetaSync;

public static class Program
{
    private static string _awsRegion = null!;
    private static string _betaBucket = null!;
    private static string _productionReadOnlyConnectionString = null!;
    private static string _betaConnectionString = null!;
    private static readonly IClock _clock = SystemClock.Instance;

    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        _awsRegion = ConfigurationManager.AppSettings["awsRegion"]!;
        _betaBucket = ConfigurationManager.AppSettings["betaBucket"]!;
        _productionReadOnlyConnectionString = ConfigurationManager.AppSettings["productionConnectionString"]!;
        _betaConnectionString = ConfigurationManager.AppSettings["betaConnectionString"]!;

        await RefreshAndCleanDatabase();
        //await UpdateMasterGames();
    }

    private static async Task RefreshAndCleanDatabase()
    {
        var productionRDSName = ConfigurationManager.AppSettings["productionRDSName"]!;
        var betaRDSName = ConfigurationManager.AppSettings["betaRDSName"]!;

        DapperNodaTimeSetup.Register();

        RDSRefresher rdsRefresher = new RDSRefresher(productionRDSName, betaRDSName);
        await rdsRefresher.CopySourceToDestination();
        RepositoryConfiguration betaRepoConfig = new RepositoryConfiguration(_betaConnectionString, _clock);
        MySQLFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(betaRepoConfig);
        MySQLBetaCleaner cleaner = new MySQLBetaCleaner(_betaConnectionString);

        Log.Information("Cleaning emails/passwords for non-beta users");
        var allUsers = await betaUserStore.GetAllUsers();
        var betaUsers = await betaUserStore.GetUsersInRoleAsync("BetaTester", CancellationToken.None);
        await cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
    }

    private static async Task UpdateMasterGames()
    {
        _awsRegion = ConfigurationManager.AppSettings["awsRegion"]!;
        _betaBucket = ConfigurationManager.AppSettings["betaBucket"]!;
        _productionReadOnlyConnectionString = ConfigurationManager.AppSettings["productionConnectionString"]!;
        _betaConnectionString = ConfigurationManager.AppSettings["betaConnectionString"]!;

        DapperNodaTimeSetup.Register();

        RepositoryConfiguration productionRepoConfig = new RepositoryConfiguration(_productionReadOnlyConnectionString, _clock);
        RepositoryConfiguration betaRepoConfig = new RepositoryConfiguration(_betaConnectionString, _clock);
        MySQLFantasyCriticUserStore productionUserStore = new MySQLFantasyCriticUserStore(productionRepoConfig);
        MySQLFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(betaRepoConfig);
        MySQLMasterGameRepo productionMasterGameRepo = new MySQLMasterGameRepo(productionRepoConfig, productionUserStore);
        MySQLMasterGameRepo betaMasterGameRepo = new MySQLMasterGameRepo(betaRepoConfig, betaUserStore);
        MySQLBetaCleaner cleaner = new MySQLBetaCleaner(_betaConnectionString);
        AdminService betaAdminService = GetAdminService();

        Log.Information("Getting master games from production");
        var productionMasterGameTags = await productionMasterGameRepo.GetMasterGameTags();
        var productionMasterGames = await productionMasterGameRepo.GetMasterGames();
        var betaMasterGameTags = await betaMasterGameRepo.GetMasterGameTags();
        var betaMasterGames = await betaMasterGameRepo.GetMasterGames();
        IReadOnlyList<MasterGameHasTagEntity> productionGamesHaveTagEntities = await GetProductionGamesHaveTagEntities();
        await cleaner.UpdateMasterGames(productionMasterGameTags, productionMasterGames, betaMasterGameTags,
            betaMasterGames, productionGamesHaveTagEntities);
        await betaAdminService.RefreshCaches();
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
        RepositoryConfiguration betaRepoConfig = new RepositoryConfiguration(_betaConnectionString, _clock);
        IFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(betaRepoConfig);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(betaRepoConfig, betaUserStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(betaRepoConfig, betaUserStore, masterGameRepo);
        InterLeagueService interLeagueService = new InterLeagueService(fantasyCriticRepo, masterGameRepo);
        LeagueMemberService leagueMemberService = new LeagueMemberService(null!, fantasyCriticRepo, _clock);
        GameAcquisitionService gameAcquisitionService = new GameAcquisitionService(fantasyCriticRepo, masterGameRepo, leagueMemberService, _clock);
        ActionProcessingService actionProcessingService = new ActionProcessingService(gameAcquisitionService);
        FantasyCriticService fantasyCriticService = new FantasyCriticService(leagueMemberService, interLeagueService, fantasyCriticRepo, _clock);
        IOpenCriticService openCriticService = null!;
        IGGService ggService = null!;
        PatreonService patreonService = null!;
        IRDSManager rdsManager = null!;
        RoyaleService royaleService = null!;
        IHypeFactorService hypeFactorService = new LambdaHypeFactorService(_awsRegion, _betaBucket, "");

        AdminServiceConfiguration configuration = new AdminServiceConfiguration(true);
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var realHypeConstantsEnvironments = new List<string>() { "STAGING", "PRODUCTION" };
        if (environment is not null && realHypeConstantsEnvironments.Contains(environment.ToUpper()))
        {
            configuration = new AdminServiceConfiguration(false);
        }

        return new AdminService(fantasyCriticService, userManager, fantasyCriticRepo, masterGameRepo, interLeagueService,
            openCriticService, ggService, patreonService, _clock, rdsManager, royaleService, hypeFactorService, configuration, actionProcessingService);
    }
}
