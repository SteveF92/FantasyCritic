using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Microsoft.Extensions.Configuration;
using Serilog;

namespace FantasyCritic.BetaSync;

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

    static async Task Main(string[] args)
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

        Console.WriteLine("Select a mode:");
        Console.WriteLine("1) Update Beta with new Production Snapshot");
        Console.WriteLine("2) Update Local Master Games");
        string? selection = Console.ReadLine();

        switch (selection)
        {
            case "1":
                await RefreshAndCleanDatabase();
                break;
            case "2":
                await UpdateMasterGames();
                break;
            default:
                throw new Exception("Invalid Selection.");
        }

    }

    private static async Task RefreshAndCleanDatabase()
    {
        RDSRefresher rdsRefresher = new RDSRefresher(_productionRDSName, _betaRDSName);
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
        RepositoryConfiguration productionRepoConfig = new RepositoryConfiguration(_productionReadOnlyConnectionString, _clock);
        RepositoryConfiguration localRepoConfig = new RepositoryConfiguration(_localConnectionString, _clock);
        MySQLFantasyCriticUserStore productionUserStore = new MySQLFantasyCriticUserStore(productionRepoConfig);
        MySQLFantasyCriticUserStore localUserStore = new MySQLFantasyCriticUserStore(localRepoConfig);
        MySQLMasterGameRepo productionMasterGameRepo = new MySQLMasterGameRepo(productionRepoConfig, productionUserStore);
        MySQLMasterGameRepo localMasterGameRepo = new MySQLMasterGameRepo(localRepoConfig, localUserStore);
        MySQLBetaCleaner cleaner = new MySQLBetaCleaner(_localConnectionString);
        AdminService localAdminService = GetAdminService();

        Log.Information("Getting master games from production");
        var productionMasterGameTags = await productionMasterGameRepo.GetMasterGameTags();
        var productionMasterGames = await productionMasterGameRepo.GetMasterGames();
        var localMasterGameTags = await localMasterGameRepo.GetMasterGameTags();
        var localMasterGames = await localMasterGameRepo.GetMasterGames();
        IReadOnlyList<MasterGameHasTagEntity> productionGamesHaveTagEntities = await GetProductionGamesHaveTagEntities();
        await cleaner.UpdateMasterGames(productionMasterGameTags, productionMasterGames, localMasterGameTags,
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
