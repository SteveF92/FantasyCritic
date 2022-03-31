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
using NLog;
using NodaTime;
using FantasyCritic.AWS;
using FantasyCritic.MySQL.Entities;
using MySqlConnector;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.DependencyInjection;

namespace FantasyCritic.BetaSync;

public static class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static string _awsRegion = null!;
    private static string _betaBucket = null!;
    private static string _productionReadOnlyConnectionString = null!;
    private static string _betaConnectionString = null!;
    private static readonly IClock _clock = SystemClock.Instance;

    static async Task Main(string[] args)
    {
        _awsRegion = ConfigurationManager.AppSettings["awsRegion"]!;
        _betaBucket = ConfigurationManager.AppSettings["betaBucket"]!;
        _productionReadOnlyConnectionString = ConfigurationManager.AppSettings["productionConnectionString"]!;
        _betaConnectionString = ConfigurationManager.AppSettings["betaConnectionString"]!;
        var productionRDSName = ConfigurationManager.AppSettings["productionRDSName"]!;
        var betaRDSName = ConfigurationManager.AppSettings["betaRDSName"]!;

        DapperNodaTimeSetup.Register();

        RDSRefresher rdsRefresher = new RDSRefresher(productionRDSName, betaRDSName);
        await rdsRefresher.CopySourceToDestination();

        MySQLFantasyCriticUserStore productionUserStore = new MySQLFantasyCriticUserStore(_productionReadOnlyConnectionString, _clock);
        MySQLFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(_betaConnectionString, _clock);
        MySQLMasterGameRepo productionMasterGameRepo = new MySQLMasterGameRepo(_productionReadOnlyConnectionString, productionUserStore);
        MySQLMasterGameRepo betaMasterGameRepo = new MySQLMasterGameRepo(_betaConnectionString, betaUserStore);
        MySQLBetaCleaner cleaner = new MySQLBetaCleaner(_betaConnectionString);
        AdminService betaAdminService = GetAdminService();

        _logger.Info("Cleaning emails/passwords for non-beta users");
        var allUsers = await betaUserStore.GetAllUsers();
        var betaUsers = await betaUserStore.GetUsersInRoleAsync("BetaTester", CancellationToken.None);
        await cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);

        _logger.Info("Getting master games from production");
        var productionMasterGameTags = await productionMasterGameRepo.GetMasterGameTags();
        var productionMasterGames = await productionMasterGameRepo.GetMasterGames();
        var betaMasterGameTags = await betaMasterGameRepo.GetMasterGameTags();
        var betaMasterGames = await betaMasterGameRepo.GetMasterGames();
        IReadOnlyList<MasterGameHasTagEntity> productionGamesHaveTagEntities = await GetProductionGamesHaveTagEntities();
        await cleaner.UpdateMasterGames(productionMasterGameTags, productionMasterGames, betaMasterGameTags, betaMasterGames, productionGamesHaveTagEntities);
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
        IFantasyCriticUserStore betaUserStore = new MySQLFantasyCriticUserStore(_betaConnectionString, _clock);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_betaConnectionString, betaUserStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_betaConnectionString, betaUserStore, masterGameRepo);
        InterLeagueService interLeagueService = new InterLeagueService(fantasyCriticRepo, masterGameRepo);
        LeagueMemberService leagueMemberService = new LeagueMemberService(null!, fantasyCriticRepo, _clock);
        GameAcquisitionService gameAcquisitionService = new GameAcquisitionService(fantasyCriticRepo, masterGameRepo, leagueMemberService, _clock);
        PublisherService publisherService = null!;
        ActionProcessingService actionProcessingService = null!;
        FantasyCriticService fantasyCriticService = new FantasyCriticService(gameAcquisitionService, leagueMemberService, publisherService, interLeagueService, fantasyCriticRepo, _clock, actionProcessingService);
        IOpenCriticService openCriticService = null!;
        IGGService ggService = null!;
        PatreonService patreonService = null!;
        IRDSManager rdsManager = null!;
        RoyaleService royaleService = null!;
        IHypeFactorService hypeFactorService = new LambdaHypeFactorService(_awsRegion, _betaBucket);

        AdminServiceConfiguration configuration = new AdminServiceConfiguration(true);
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var realHypeConstantsEnvironments = new List<string>() { "STAGING", "PRODUCTION" };
        if (environment is not null && realHypeConstantsEnvironments.Contains(environment.ToUpper()))
        {
            configuration = new AdminServiceConfiguration(false);
        }

        return new AdminService(fantasyCriticService, userManager, fantasyCriticRepo, masterGameRepo, interLeagueService,
            openCriticService, ggService, patreonService, _clock, rdsManager, royaleService, hypeFactorService, configuration);
    }
}
