using Discord;
using FantasyCritic.Lib.Domain.AllTimeStats;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Services;
public class AllTimeStatsService
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly ICombinedDataRepo _combinedDataRepo;

    public AllTimeStatsService(IFantasyCriticRepo fantasyCriticRepo, ICombinedDataRepo combinedDataRepo)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _combinedDataRepo = combinedDataRepo;
    }

    public async Task<LeagueAllTimeStats> GetLeagueAllTimeStats(League league, LocalDate currentDate)
    {
        var leagueYears = new List<LeagueYear>();
        foreach (var year in league.Years)
        {
            var leagueYear = await _combinedDataRepo.GetLeagueYear(league.LeagueID, year);
            if (leagueYear is not null && leagueYear.SupportedYear.Finished)
            {
                leagueYears.Add(leagueYear);
            }
        }

        var leagueYearDictionary = leagueYears.ToDictionary(x => x.Key);
        var playerAllTimeStats = new List<LeaguePlayerAllTimeStats>();
        var groupedByPlayer = leagueYears.SelectMany(x => x.Publishers).GroupBy(x => x.User);
        foreach (var playerGroup in groupedByPlayer)
        {
            var player = new VeryMinimalFantasyCriticUser(playerGroup.Key.UserID, playerGroup.Key.DisplayName);
            var yearsPlayedIn = playerGroup.Count();
            var yearsWon = leagueYears.Where(x => x.WinningUser is not null && x.WinningUser.UserID == player.UserID).Select(x => x.Year).ToList();

            var totalFantasyPoints = playerGroup.Sum(x => x.GetTotalFantasyPoints(leagueYearDictionary[x.LeagueYearKey].SupportedYear, leagueYearDictionary[x.LeagueYearKey].Options));
            var allPublisherGames = playerGroup.SelectMany(x => x.PublisherGames).ToList();
            var gamesReleased = allPublisherGames
                .Where(x => !x.CounterPick)
                .Where(x => x.MasterGame is not null)
                .Where(x => x.MasterGame!.MasterGame.IsReleased(currentDate))
                .ToList();

            var timesCounterPicked = 0;
            var allFinishRankings = new List<int>();
            foreach (var publisher in playerGroup)
            {
                var leagueYear = leagueYearDictionary[publisher.LeagueYearKey];
                var ranking = leagueYear.Publishers.Count(y => y.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options) >
                                                               publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options)) + 1;
                allFinishRankings.Add(ranking);

                var timesCounterPickedInYear = GameUtilities.GetTimesCounterPicked(leagueYear, publisher);
                timesCounterPicked += timesCounterPickedInYear;
            }

            var averageFinishRanking = allFinishRankings.Average();
            var averageGamesReleased = (double)gamesReleased.Count / yearsPlayedIn;
            var averageFantasyPoints = totalFantasyPoints / yearsPlayedIn;
            var averageCriticScore = gamesReleased.Where(x => x.MasterGame!.MasterGame.CriticScore.HasValue).Select(x => x.MasterGame!.MasterGame.CriticScore).Average() ?? 0m;


            playerAllTimeStats.Add(new LeaguePlayerAllTimeStats(player, yearsPlayedIn, yearsWon, totalFantasyPoints, gamesReleased.Count,
                averageFinishRanking, averageGamesReleased, averageFantasyPoints, averageCriticScore, timesCounterPicked));
        }

        var systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
        var hallOfFameGameLists = GetHallOfFameLists(leagueYears, leagueYearDictionary, systemWideValues, currentDate);

        return new LeagueAllTimeStats(leagueYears, playerAllTimeStats, hallOfFameGameLists);
    }

    private static IReadOnlyList<HallOfFameGameList> GetHallOfFameLists(IReadOnlyList<LeagueYear> leagueYears, IReadOnlyDictionary<LeagueYearKey, LeagueYear> leagueYearDictionary,
        SystemWideValues systemWideValues, LocalDate currentDate)
    {
        var publisherDictionary = leagueYears.SelectMany(x => x.Publishers).ToDictionary(x => x.PublisherID);

        var publisherGamesWithMasterGames = leagueYears
            .SelectMany(x => x.Publishers)
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToList();

        var calculatedAllTimeStats = new List<AllTimeStatsCalculation>();
        foreach (var game in publisherGamesWithMasterGames)
        {
            var publisher = publisherDictionary[game.PublisherID];
            var leagueYear = leagueYearDictionary[publisher.LeagueYearKey];
            var leagueOptions = leagueYear.Options;
            var scoringSystem = leagueOptions.ScoringSystem;

            calculatedAllTimeStats.Add(new AllTimeStatsCalculation(game.MasterGame!, publisher, game.CounterPick, game.OverallDraftPosition, game.BidAmount, game.MasterGame!.MasterGame.CriticScore, game.FantasyPoints)
            {
                CountedForPointsInYear = game.MasterGame.IsReleasedAndReleasedInYear(currentDate) ||
                                         (leagueOptions.ReleaseSystem.Equals(ReleaseSystem.OnlyNeedsScore) && game.MasterGame.MasterGame.CriticScore.HasValue &&
                                         game.MasterGame.MasterGame.FirstCriticScoreTimestamp.HasValue &&
                                         game.MasterGame.MasterGame.FirstCriticScoreTimestamp.Value.ToEasternDate().Year == game.MasterGame.Year),
                SleeperFactor = game.GetSleeperFactor(scoringSystem),
                FlopFactor = game.GetFlopFactor(scoringSystem),
                DraftValue = game.GetDraftValue(leagueYear),
                BidValue = game.GetBidValue(scoringSystem),
                DollarsPerPoint = game.GetDollarsPerPoint(scoringSystem)
            });
        }

        //Build Hall of Fame
        var mostPoints = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.FantasyPoints.HasValue)
            .OrderByDescending(x => x.FantasyPoints)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Fantasy Points", new HallOfFameGameStat(x.FantasyPoints ?? 0, "Score")}
            }))
            .ToList();

        var leastPoints = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.FantasyPoints.HasValue)
            .OrderBy(x => x.FantasyPoints)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Fantasy Points", new HallOfFameGameStat(x.FantasyPoints ?? 0, "Score")}
            }))
            .ToList();

        var bestCounterPicks = calculatedAllTimeStats
            .Where(x => x.CounterPick)
            .OrderByDescending(x => x.FantasyPoints)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Fantasy Points", new HallOfFameGameStat(x.FantasyPoints ?? 0, "Score")}
            }))
            .ToList();

        var worstCounterPicks = calculatedAllTimeStats
            .Where(x => x.CounterPick && x.FantasyPoints.HasValue)
            .OrderBy(x => x.FantasyPoints)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Fantasy Points", new HallOfFameGameStat(x.FantasyPoints ?? 0, "Score")}
            }))
            .ToList();

        var mostDollarsSpentOn = calculatedAllTimeStats
            .Where(x => x.BidAmount.HasValue)
            .OrderByDescending(x => x.BidAmount)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Spent", new HallOfFameGameStat(x.BidAmount!.Value, "Budget")}
            }))
            .ToList();

        var highestSleeperFactor = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.FantasyPoints.HasValue && x.PickedBy.LeagueYearKey.Year > 2018)
            .OrderByDescending(x => x.SleeperFactor)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Hype Factor", new HallOfFameGameStat(x.Game.DateAdjustedHypeFactor, "Factor")},
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Sleeper Factor", new HallOfFameGameStat(x.SleeperFactor, "Factor")}
            }))
            .ToList();

        var highestFlopFactor = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.CountedForPointsInYear && x.FantasyPoints.HasValue && x.PickedBy.LeagueYearKey.Year > 2018)
            .OrderByDescending(x => x.FlopFactor)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Hype Factor", new HallOfFameGameStat(x.Game.DateAdjustedHypeFactor, "Factor")},
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Flop Factor",  new HallOfFameGameStat(x.FlopFactor, "Factor")}
            }))
            .ToList();

        var highestDraftValue = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.FantasyPoints.HasValue && x.OverallDraftPosition.HasValue)
            .OrderByDescending(x => x.DraftValue)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Drafted", new HallOfFameGameStat(x.OverallDraftPosition!.Value, "Position")},
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Draft Value",  new HallOfFameGameStat(x.DraftValue, "Factor")}
            }))
            .ToList();

        var lowestDraftValue = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.OverallDraftPosition.HasValue)
            .OrderBy(x => x.DraftValue)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Drafted", new HallOfFameGameStat(x.OverallDraftPosition!.Value, "Position")},
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Draft Value",  new HallOfFameGameStat(x.DraftValue, "Factor")}
            }))
            .ToList();

        var highestBidValue = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.BidAmount.HasValue && x.FantasyPoints.HasValue)
            .OrderByDescending(x => x.BidValue)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Spent", new HallOfFameGameStat(x.BidAmount!.Value, "Budget")},
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Bid Value",  new HallOfFameGameStat(x.BidValue, "Factor")}
            }))
            .ToList();

        var worstBidValue = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.BidAmount.HasValue)
            .OrderBy(x => x.BidValue)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Spent", new HallOfFameGameStat(x.BidAmount!.Value, "Budget")},
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Bid Value",  new HallOfFameGameStat(x.BidValue, "Factor")}
            }))
            .ToList();

        var worstDollarsPerPoint = calculatedAllTimeStats
            .Where(x => !x.CounterPick && x.BidAmount.HasValue && x.FantasyPoints.HasValue)
            .OrderByDescending(x => x.DollarsPerPoint)
            .Take(10)
            .Select(x => new HallOfFameGame(x.Game, x.PickedBy, new Dictionary<string, HallOfFameGameStat>()
            {
                {"Spent",  new HallOfFameGameStat(x.BidAmount!.Value, "Budget")},
                {"Critic Score", new HallOfFameGameStat(x.FormattedCriticScore, "Score")},
                {"Dollars Per Point",  new HallOfFameGameStat(x.DollarsPerPoint, "Budget")}
            }))
            .ToList();

        var hallOfFameGameLists = new List<HallOfFameGameList>()
        {
            new HallOfFameGameList("Highest Scoring Games", mostPoints),
            new HallOfFameGameList("Lowest Scoring Games", leastPoints),
            new HallOfFameGameList("Best Counter Picks", bestCounterPicks),
            new HallOfFameGameList("Worst Counter Picks", worstCounterPicks),
            new HallOfFameGameList("Most Budget Spent On", mostDollarsSpentOn),
            new HallOfFameGameList("Sleeper Factor", highestSleeperFactor),
            new HallOfFameGameList("Flop Factor", highestFlopFactor),

            new HallOfFameGameList("Highest Value Draft Picks", highestDraftValue),
            new HallOfFameGameList("Lowest Value Draft Picks", lowestDraftValue),
            new HallOfFameGameList("Highest Bid Value", highestBidValue),
            new HallOfFameGameList("Worst Bid Value", worstBidValue),
            new HallOfFameGameList("Worst Dollars Per Point", worstDollarsPerPoint),
        };
        return hallOfFameGameLists;
    }
}
