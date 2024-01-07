using System.Reflection;
using Dapper.NodaTime;
using Faithlife.Utility.Dapper;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NodaTime;

namespace FantasyCritic.DBUtility;

class Program
{

    private static string _connectionString = null!;
    private static readonly IClock _clock = SystemClock.Instance;

    public static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        _connectionString = configuration["ConnectionString"]!;

        DapperNodaTimeSetup.Register();
        await BackfillTopBidsAndDrops();
    }

    private static async Task BackfillTopBidsAndDrops()
    {
        RepositoryConfiguration repoConfig = new RepositoryConfiguration(_connectionString, _clock);
        IFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(repoConfig);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(repoConfig, userStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(repoConfig, userStore, masterGameRepo);

        Console.WriteLine("Gathering data...");
        List<PickupBid> allProcessedPickups = new List<PickupBid>();
        List<DropRequest> allProcessedDrops = new List<DropRequest>();
        var relevantYears = new List<int>() { 2022, 2023, 2024 };
        foreach (var year in relevantYears)
        {
            var allLeagueYears = await fantasyCriticRepo.GetLeagueYears(year);
            var standardLeagueIDs = allLeagueYears.Where(x => x.League.AffectsStats).Select(x => x.League.LeagueID).ToHashSet();
            var processedPickups = await fantasyCriticRepo.GetProcessedPickupBids(year, allLeagueYears);
            var processedDrops = await fantasyCriticRepo.GetProcessedDropRequests(year, allLeagueYears);

            allProcessedPickups.AddRange(processedPickups.Where(x => x.ProcessSetID.HasValue && standardLeagueIDs.Contains(x.LeagueYear.League.LeagueID)));
            allProcessedDrops.AddRange(processedDrops.Where(x => x.ProcessSetID.HasValue && standardLeagueIDs.Contains(x.LeagueYear.League.LeagueID)));
        }

        Console.WriteLine("Gathering action processing sets...");
        var actionProcessingSets = (await fantasyCriticRepo.GetActionProcessingSets()).OrderBy(x => x.ProcessTime).ToList();
        IReadOnlyList<ActionSetGrouping> groupings = GetGroupings(actionProcessingSets);

        List<TopBidsAndDropsEntity> finalEntities = new List<TopBidsAndDropsEntity>();

        Console.WriteLine("Parsing data...");
        foreach (var actionProcessingGroup in groupings)
        {
            var processingSetIDs = actionProcessingGroup.ProcessingSets.Select(x => x.ProcessSetID).ToHashSet();
            var pickupBidsForSet = allProcessedPickups.Where(x => processingSetIDs.Contains(x.ProcessSetID!.Value)).ToList();
            var dropsForSet = allProcessedDrops.Where(x => processingSetIDs.Contains(x.ProcessSetID!.Value)).ToList();

            var pickupBidsByYear = pickupBidsForSet.GroupToDictionary(x => x.LeagueYear.Year);
            var dropsByYear = dropsForSet.GroupToDictionary(x => x.LeagueYear.Year);

            var yearsInGroup = pickupBidsByYear.Keys.Concat(dropsByYear.Keys).Distinct().ToList();
            foreach (var year in yearsInGroup)
            {
                var standardBidsForYear = pickupBidsByYear.GetValueOrDefault(year, new List<PickupBid>()).Where(x => !x.CounterPick).ToList();
                var counterPickBidsForYear = pickupBidsByYear.GetValueOrDefault(year, new List<PickupBid>()).Where(x => x.CounterPick).ToList();
                var dropsForYear = dropsByYear.GetValueOrDefault(year, new List<DropRequest>());

                var standardBidsByMasterGame = standardBidsForYear.GroupToDictionary(x => x.MasterGame);
                var counterPickBidsByMasterGame = counterPickBidsForYear.GroupToDictionary(x => x.MasterGame);
                var dropsByMasterGame = dropsForYear.GroupToDictionary(x => x.MasterGame);
                var allMasterGames = standardBidsByMasterGame.Keys.Concat(counterPickBidsByMasterGame.Keys).Concat(dropsByMasterGame.Keys).Distinct().ToList();

                foreach (var masterGame in allMasterGames)
                {
                    var standardBidsForMasterGame = standardBidsByMasterGame.GetValueOrDefault(masterGame, new List<PickupBid>());
                    var counterPickBidsForMasterGame = counterPickBidsByMasterGame.GetValueOrDefault(masterGame, new List<PickupBid>());
                    var dropsForMasterGame = dropsByMasterGame.GetValueOrDefault(masterGame, new List<DropRequest>());

                    finalEntities.Add(new TopBidsAndDropsEntity()
                    {
                        ProcessDate = actionProcessingGroup.ProcessDate,
                        MasterGameID = masterGame.MasterGameID,
                        Year = year,

                        TotalStandardBidCount = standardBidsForMasterGame.Count,
                        SuccessfulStandardBids = standardBidsForMasterGame.Count(x => x.Successful.HasValue && x.Successful.Value),
                        FailedStandardBids = standardBidsForMasterGame.Count(x => x.Successful.HasValue && !x.Successful.Value),
                        TotalStandardBidLeagues = standardBidsForMasterGame.Select(x => x.LeagueYear.Key).Distinct().Count(),
                        TotalStandardBidAmount = (int)standardBidsForMasterGame.Sum(x => x.BidAmount),

                        TotalCounterPickBidCount = counterPickBidsForMasterGame.Count,
                        SuccessfulCounterPickBids = counterPickBidsForMasterGame.Count(x => x.Successful.HasValue && x.Successful.Value),
                        FailedCounterPickBids = counterPickBidsForMasterGame.Count(x => x.Successful.HasValue && !x.Successful.Value),
                        TotalCounterPickBidLeagues = counterPickBidsForMasterGame.Select(x => x.LeagueYear.Key).Distinct().Count(),
                        TotalCounterPickBidAmount = (int)counterPickBidsForMasterGame.Sum(x => x.BidAmount),

                        TotalDropCount = dropsForMasterGame.Count,
                        SuccessfulDrops = dropsForMasterGame.Count(x => x.Successful.HasValue && x.Successful.Value),
                        FailedDrops = dropsForMasterGame.Count(x => x.Successful.HasValue && !x.Successful.Value),
                    });
                }
            }
        }

        Console.WriteLine("Saving results...");
        await SaveResults(finalEntities);
    }

    private static IReadOnlyList<ActionSetGrouping> GetGroupings(IReadOnlyList<ActionProcessingSetMetadata> actionProcessingSets)
    {
        List<ActionSetGrouping> groupings = new List<ActionSetGrouping>();

        List<ActionProcessingSetMetadata> currentList = new List<ActionProcessingSetMetadata>();
        for (var index = 0; index < actionProcessingSets.Count; index++)
        {
            var actionSet = actionProcessingSets[index];
            currentList.Add(actionSet);
            if (actionSet.ProcessName.Contains("Drop/Bid Processing"))
            {
                var processDate = actionSet.ProcessTime.ToEasternDate();
                groupings.Add(new ActionSetGrouping(processDate, currentList));
                currentList = new List<ActionProcessingSetMetadata>();
            }
        }

        return groupings;
    }

    private static async Task SaveResults(IReadOnlyList<TopBidsAndDropsEntity> finalEntities)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.BulkInsertAsync(finalEntities, "tbl_caching_topbidsanddrops", 500, transaction);

        await transaction.CommitAsync();
    }
}
