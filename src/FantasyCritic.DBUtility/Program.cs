using System.Configuration;
using Dapper;
using Dapper.NodaTime;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Enums;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MySqlConnector;
using NodaTime;

namespace FantasyCritic.DBUtility;

class Program
{
    private static readonly string _connectionString = ConfigurationManager.AppSettings["ConnectionString"]!;
    private static readonly IClock _clock = SystemClock.Instance;

    static async Task Main(string[] args)
    {
        DapperNodaTimeSetup.Register();
        await FindBadBudgets();
    }

    private static async Task FindBadBudgets()
    {
        MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
        MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
        MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);

        var supportedYears = await fantasyCriticRepo.GetSupportedYears();
        var tradeYears = supportedYears.Where(x => x.Year >= 2022).ToList();
        foreach (var supportedYear in tradeYears)
        {
            var leagueYearsWithProblemTrades = await GetLeagueYearsWithProblemTrades(supportedYear);
            foreach (var leagueYearKey in leagueYearsWithProblemTrades)
            {
                var league = await fantasyCriticRepo.GetLeague(leagueYearKey.LeagueID);
                var leagueYear = await fantasyCriticRepo.GetLeagueYear(league!, leagueYearKey.Year);
                var trades = await fantasyCriticRepo.GetTradesForLeague(leagueYear);
                var executedTrades = trades.Where(x => x.Status.Equals(TradeStatus.Executed));
                var leagueActions = await fantasyCriticRepo.GetLeagueActions(leagueYear);
                var nonDraftActions = leagueActions.Where(x => !x.Description.ToLower().Contains("draft")).ToList();
                var processedBids = await fantasyCriticRepo.GetProcessedPickupBids(leagueYear);
                var successBids = processedBids.Where(x => x.Successful.HasValue && x.Successful.Value).ToList();
                var publisherIDs = executedTrades.SelectMany(x => x.GetUpdatedPublishers()).Select(x => x.PublisherID).Distinct().ToList();
                foreach (var publisherID in publisherIDs)
                {
                    var publisher = leagueYear.GetPublisherByID(publisherID);
                    var leagueActionsForPublisher = nonDraftActions.Where(x => x.Publisher.PublisherID == publisher.PublisherID).ToList();
                    var tradesInvolvingPublisher = executedTrades.Where(x => x.GetUpdatedPublishers().Select(x => x.PublisherID).Contains(publisher.PublisherID)).OrderBy(x => x.CompletedTimestamp).ToList();
                    var successfulBidsForPublisher = successBids.Where(x => x.Publisher.PublisherID == publisher.PublisherID).OrderBy(x => x.Timestamp).ToList();
                    PrintStatsForPublisher(leagueYear, publisher, leagueActionsForPublisher, tradesInvolvingPublisher, successfulBidsForPublisher);
                }
            }
        }
    }

    private static void PrintStatsForPublisher(LeagueYear leagueYear, Publisher publisher, IReadOnlyList<LeagueAction> leagueActionsForPublisher, IReadOnlyList<Trade> tradesInvolvingPublisher, IReadOnlyList<PickupBid> successfulBidsForPublisher)
    {
        List<(Instant, object)> rawBudgetEvents = new List<(Instant, object)>();
        rawBudgetEvents.AddRange(leagueActionsForPublisher.Where(x => x.ActionType == "Publisher Edited" && x.Description.StartsWith("Changed budget")).Select(x => (x.Timestamp, x as object)));
        rawBudgetEvents.AddRange(successfulBidsForPublisher.Select(x => (x.Timestamp, x as object)));
        rawBudgetEvents.AddRange(tradesInvolvingPublisher.Select(x => (x.CompletedTimestamp!.Value, x as object)));

        var sortedRawEvents = rawBudgetEvents.OrderBy(x => x.Item1);
        List<BudgetEvent> budgetEvents = new List<BudgetEvent>();
        long currentBudget = 100;
        foreach (var rawEvent in sortedRawEvents)
        {
            switch (rawEvent.Item2)
            {
                case LeagueAction action:
                    budgetEvents.Add(new BudgetEvent(currentBudget, action));
                    break;
                case PickupBid bid:
                    budgetEvents.Add(new BudgetEvent(currentBudget, bid));
                    break;
                case Trade trade:
                    bool isProposer = trade.Proposer.PublisherID == publisher.PublisherID;
                    budgetEvents.Add(new BudgetEvent(currentBudget, trade, isProposer));
                    break;
            }

            currentBudget = budgetEvents.Last().NewBudget;
        }

        //if (currentBudget == publisher.Budget)
        //{
        //    return;
        //}

        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine($"League: {leagueYear.League.LeagueName} | Publisher: {publisher.PublisherName}");
        Console.WriteLine($"https://www.fantasycritic.games/league/{leagueYear.League.LeagueID}/{leagueYear.Year}");
        Console.WriteLine($"https://www.fantasycritic.games/publisher/{publisher.PublisherID}");
        Console.WriteLine("Starting: 100");

        foreach (var budgetEvent in budgetEvents)
        {
            Console.WriteLine($"Event: {budgetEvent.Description} - {budgetEvent.Timestamp}");
            Console.WriteLine($"\tChange: {budgetEvent.PreviousBudget}->{budgetEvent.NewBudget} ({budgetEvent.Change})");
        }

        Console.WriteLine($"Expected Budget: {currentBudget}");
        Console.WriteLine($"Actual Budget: {publisher.Budget}");
        if (currentBudget != publisher.Budget)
        {
            Console.WriteLine("Mismatch!");
        }
        Console.WriteLine("-------------------------------------------------------------");
    }

    private static async Task<IReadOnlyList<LeagueYearKey>> GetLeagueYearsWithProblemTrades(SupportedYear year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var keys = await connection.QueryAsync<LeagueYearKeyEntity>("select distinct LeagueID, Year from tbl_league_trade where Status = 'Executed' AND (ProposerBudgetSendAmount <> 0 OR CounterPartyBudgetSendAmount <> 0) AND Year = @year;", new { year = year.Year });
        return keys.Select(x => new LeagueYearKey(x.LeagueID, x.Year)).ToList();
    }
}
