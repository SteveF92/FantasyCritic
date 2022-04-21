using System.Configuration;
using Dapper;
using Dapper.NodaTime;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.Entities;
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
                var leagueActions = await fantasyCriticRepo.GetLeagueActions(leagueYear);
                var nonDraftActions = leagueActions.Where(x => !x.Description.ToLower().Contains("draft")).ToList();
                var processedBids = await fantasyCriticRepo.GetProcessedPickupBids(leagueYear);
                var successBids = processedBids.Where(x => x.Successful.HasValue && x.Successful.Value).ToList();
                var publisherIDs = trades.SelectMany(x => x.GetUpdatedPublishers()).Select(x => x.PublisherID).ToList();
                foreach (var publisherID in publisherIDs)
                {
                    var publisher = leagueYear.GetPublisherByID(publisherID);
                    var leagueActionsForPublisher = nonDraftActions.Where(x => x.Publisher.PublisherID == publisher.PublisherID).ToList();
                    var tradesInvolvingPublisher = trades.Where(x => x.GetUpdatedPublishers().Select(x => x.PublisherID).Contains(publisher.PublisherID)).OrderBy(x => x.CompletedTimestamp).ToList();
                    var successfulBidsForPublisher = successBids.Where(x => x.Publisher.PublisherID == publisher.PublisherID).OrderBy(x => x.Timestamp).ToList();
                    PrintStatsForPublisher(leagueYear, publisher, leagueActionsForPublisher, tradesInvolvingPublisher, successfulBidsForPublisher); 
                }
            }
        }
    }

    private static void PrintStatsForPublisher(LeagueYear leagueYear, Publisher publisher, IReadOnlyList<LeagueAction> leagueActionsForPublisher, IReadOnlyList<Trade> tradesInvolvingPublisher, IReadOnlyList<PickupBid> successfulBidsForPublisher)
    {
        List<string> stringsToPrint = new List<string>();
        stringsToPrint.Add("-------------------------------------------------------------");
        stringsToPrint.Add($"League: {leagueYear.League.LeagueName} | Publisher: {publisher.PublisherName}");
        stringsToPrint.Add($"https://www.fantasycritic.games/league/{leagueYear.League.LeagueID}/{leagueYear.Year}");
        stringsToPrint.Add($"https://www.beta.fantasycritic.games/publisher/{publisher.PublisherID}");
        stringsToPrint.Add("Starting: 100");

        long expectedBudget = 100;
        foreach (var action in leagueActionsForPublisher.Where(x => x.ActionType == "Publisher Edited"))
        {
            stringsToPrint.Add(action.Description);
        }
        foreach (var bid in successfulBidsForPublisher)
        {
            expectedBudget -= bid.BidAmount;
            stringsToPrint.Add($"Won {bid.MasterGame.GameName} with bid of {bid.BidAmount}");
        }
        foreach (var trade in tradesInvolvingPublisher)
        {
            long budgetChange = 0;
            if (trade.Proposer.PublisherID == publisher.PublisherID)
            {
                budgetChange = trade.ProposerBudgetSendAmount * -1;
                budgetChange += trade.CounterPartyBudgetSendAmount;
            }
            else
            {
                budgetChange = trade.CounterPartyBudgetSendAmount * -1;
                budgetChange += trade.ProposerBudgetSendAmount;
            }

            expectedBudget += budgetChange;
            stringsToPrint.Add($"Budget from trade: {budgetChange}");
        }

        stringsToPrint.Add($"Expected Budget: {expectedBudget}");
        stringsToPrint.Add($"Actual Budget: {publisher.Budget}");
        stringsToPrint.Add("-------------------------------------------------------------");
        if (expectedBudget != publisher.Budget)
        {
            foreach (var stringPrint in stringsToPrint)
            {
                Console.WriteLine(stringPrint);
            }
        }
    }

    private static async Task<IReadOnlyList<LeagueYearKey>> GetLeagueYearsWithProblemTrades(SupportedYear year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var keys = await connection.QueryAsync<LeagueYearKeyEntity>("select distinct LeagueID, Year from tbl_league_trade where Status = 'Executed' AND (ProposerBudgetSendAmount <> 0 OR CounterPartyBudgetSendAmount <> 0) AND Year = @year;", new {year = year.Year});
        return keys.Select(x => new LeagueYearKey(x.LeagueID, x.Year)).ToList();
    }
}
