using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

public class CacheRefresher
{
    private readonly IClock _clock;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IHypeFactorService _hypeFactorService;
    private readonly DiscordPushService _discordPushService;
    private readonly RoyaleService _royaleService;
    private readonly ILogger<CacheRefresher> _logger;

    public CacheRefresher(IClock clock, IMasterGameRepo masterGameRepo, IFantasyCriticRepo fantasyCriticRepo,
        IHypeFactorService hypeFactorService, DiscordPushService discordPushService, RoyaleService royaleService, ILogger<CacheRefresher> logger)
    {
        _clock = clock;
        _masterGameRepo = masterGameRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _hypeFactorService = hypeFactorService;
        _discordPushService = discordPushService;
        _royaleService = royaleService;
        _logger = logger;
    }

    //Progress goes in a temporary status as each sub-step starts, so a cancelled run shows which one it was on.
    //The one clause appended at the end says which years were recalculated.
    public async Task RefreshCaches(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refreshing caches.");

        LocalDate today = _clock.GetToday();
        LocalDate tomorrow = today.PlusDays(1);
        cancellationToken.ThrowIfCancellationRequested();
        await context.AddTemporaryStatus("Caches: updating code-based tags.");
        await UpdateCodeBasedTags(today);
        cancellationToken.ThrowIfCancellationRequested();
        await context.AddTemporaryStatus("Caches: updating release date estimates.");
        await _masterGameRepo.UpdateReleaseDateEstimates(tomorrow);

        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var cachedSystemWideValueYears = (await _fantasyCriticRepo.GetCachedSystemWideValueYears()).ToHashSet();
        List<int> systemWideValueYears = [];
        foreach (var supportedYear in supportedYears)
        {
            if (!YearNeedsSystemWideValuesRefresh(supportedYear, today, cachedSystemWideValueYears))
            {
                _logger.LogDebug("System-wide values for {Year} are cached and final; not recalculating.", supportedYear.Year);
                continue;
            }

            await context.AddTemporaryStatus($"Caches: updating system-wide values for {supportedYear.Year}.");
            IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
            cancellationToken.ThrowIfCancellationRequested();
            await UpdateSystemWideValuesForYear(supportedYear.Year, leagueYears);
            systemWideValueYears.Add(supportedYear.Year);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await context.AddTemporaryStatus("Caches: aggregating system-wide values.");
        await UpdateSystemWideValues();
        HypeConstants hypeConstants = await _hypeFactorService.GetHypeConstants();
        var gameStatsYears = await UpdateGameStats(hypeConstants, context, cancellationToken);
        _masterGameRepo.ClearMasterGameCache();
        _masterGameRepo.ClearMasterGameYearCache();
        //Anything still queued here goes out with the next RefreshCaches in this process.
        cancellationToken.ThrowIfCancellationRequested();
        await context.AddTemporaryStatus("Caches: sending batched Discord game updates.");
        await _discordPushService.SendBatchedMasterGameUpdates();

        _logger.LogInformation("Refreshed caches: system-wide values for {SystemWideValueYears}, game stats for {GameStatsYears}.",
            systemWideValueYears, gameStatsYears);
        await context.AppendDetailedStatus($"Caches refreshed: system-wide values for {DescribeYears(systemWideValueYears)}; game stats for {DescribeYears(gameStatsYears)}.");
    }

    private static string DescribeYears(IReadOnlyList<int> years) => years.Count == 0 ? "no years" : string.Join(", ", years);

    private async Task UpdateCodeBasedTags(LocalDate today)
    {
        _logger.LogDebug("Updating code-based tags.");
        var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
        var allMasterGames = await _masterGameRepo.GetMasterGames();
        var masterGamesWithEarlyAccessDate = allMasterGames.Where(x => x.EarlyAccessReleaseDate.HasValue);
        var masterGamesWithInternationalDate = allMasterGames.Where(x => x.InternationalReleaseDate.HasValue);
        Dictionary<MasterGame, List<MasterGameTag>> tagsToAdd = allMasterGames.ToDictionary(x => x, _ => new List<MasterGameTag>());

        foreach (var masterGame in masterGamesWithEarlyAccessDate)
        {
            bool inEarlyAccess = today >= masterGame.EarlyAccessReleaseDate!.Value;
            if (inEarlyAccess)
            {
                tagsToAdd[masterGame].Add(tagDictionary["CurrentlyInEarlyAccess"]);
            }
            else
            {
                tagsToAdd[masterGame].Add(tagDictionary["PlannedForEarlyAccess"]);
            }
        }

        foreach (var masterGame in masterGamesWithInternationalDate)
        {
            bool releasedInternationally = today >= masterGame.InternationalReleaseDate!.Value;
            if (releasedInternationally)
            {
                tagsToAdd[masterGame].Add(tagDictionary["ReleasedInternationally"]);
            }
            else
            {
                tagsToAdd[masterGame].Add(tagDictionary["WillReleaseInternationallyFirst"]);
            }
        }

        await _masterGameRepo.UpdateCodeBasedTags(tagsToAdd.SealDictionary());
    }

    private static bool YearNeedsSystemWideValuesRefresh(SupportedYear supportedYear, LocalDate today, HashSet<int> cachedYears)
    {
        if (!supportedYear.Finished)
        {
            return true;
        }

        LocalDate yearEndGracePeriodEnd = new LocalDate(supportedYear.Year, 12, 31).PlusDays(30);
        if (yearEndGracePeriodEnd >= today)
        {
            return true;
        }

        return !cachedYears.Contains(supportedYear.Year);
    }

    private async Task UpdateSystemWideValues()
    {
        _logger.LogDebug("Aggregating system-wide values from the year cache.");

        var systemWideValues = await _fantasyCriticRepo.BuildSystemWideValuesFromYearCache();
        await _fantasyCriticRepo.UpdateSystemWideValues(systemWideValues);
    }

    private async Task UpdateSystemWideValuesForYear(int year, IReadOnlyList<LeagueYear> leagueYears)
    {
        _logger.LogDebug("Updating system-wide values for {Year}.", year);

        var leaguesToCount = leagueYears.Where(x => x.League.AffectsStats && x.IsFirstDraftFinished).ToList();
        var publisherGames = leaguesToCount.SelectMany(x => x.Publishers).SelectMany(x => x.PublisherGames);
        var gamesWithPoints = publisherGames.Where(x => x.FantasyPoints.HasValue && !x.ManualCriticScore.HasValue).ToList();

        var allStandardGamesWithPoints = gamesWithPoints.Where(x => !x.CounterPick).ToList();
        var allPickupOnlyStandardGamesWithPoints = allStandardGamesWithPoints.Where(x => !x.OverallPickNumber.HasValue).ToList();
        var allCounterPicksWithPoints = gamesWithPoints.Where(x => x.CounterPick).ToList();

        var averageStandardPoints = allStandardGamesWithPoints.Select(x => x.FantasyPoints!.Value).DefaultIfEmpty(0m).Average();
        var averagePickupOnlyStandardPoints = allPickupOnlyStandardGamesWithPoints.Select(x => x.FantasyPoints!.Value).DefaultIfEmpty(0m).Average();
        var averageCounterPickPoints = allCounterPicksWithPoints.Select(x => x.FantasyPoints!.Value).DefaultIfEmpty(0m).Average();

        Dictionary<int, List<decimal>> pointsForPosition = [];
        Dictionary<uint, List<decimal>> pointsForBidAmount = [];
        foreach (var leagueYear in leagueYears)
        {
            var publishers = leagueYear.Publishers;
            var orderedGames = publishers.SelectMany(x => x.PublisherGames).Where(x => !x.CounterPick & x.FantasyPoints.HasValue && !x.ManualCriticScore.HasValue).OrderBy(x => x.Timestamp).ToList();
            for (var index = 0; index < orderedGames.Count; index++)
            {
                var game = orderedGames[index];
                var pickPosition = index + 1;
                if (!pointsForPosition.ContainsKey(pickPosition))
                {
                    pointsForPosition[pickPosition] = [];
                }

                pointsForPosition[pickPosition].Add(game.FantasyPoints!.Value);
            }

            var pickupGames = orderedGames.Where(x => x.BidAmount.HasValue).ToList();
            foreach (var game in pickupGames)
            {
                if (!pointsForBidAmount.ContainsKey(game.BidAmount!.Value))
                {
                    pointsForBidAmount[game.BidAmount!.Value] = [];
                }

                pointsForBidAmount[game.BidAmount!.Value].Add(game.FantasyPoints!.Value);
            }
        }

        var averageStandardGamePointsByPickPosition = pointsForPosition.Select(position => new AveragePickPositionPoints(position.Key, position.Value.Count, position.Value.Average())).ToList();
        var averageStandardGamePointsByBidAmount = pointsForBidAmount.Select(bidAmount => new AverageBidAmountPoints(bidAmount.Key, bidAmount.Value.Count, bidAmount.Value.Average())).ToList();
        var systemWideValues = new SystemWideValues(averageStandardPoints, averagePickupOnlyStandardPoints, averageCounterPickPoints,
            averageStandardGamePointsByPickPosition, averageStandardGamePointsByBidAmount);
        await _fantasyCriticRepo.UpdateSystemWideValuesForYear(year, systemWideValues, allStandardGamesWithPoints.Count,
            allPickupOnlyStandardGamesWithPoints.Count, allCounterPicksWithPoints.Count);
    }

    //Returns the years it updated.
    private async Task<IReadOnlyList<int>> UpdateGameStats(HypeConstants hypeConstants, FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        List<int> updatedYears = [];
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        var currentDate = _clock.GetToday();
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished)
            {
                continue;
            }

            _logger.LogDebug("Updating game stats for {Year}.", supportedYear.Year);
            await context.AddTemporaryStatus($"Caches: updating game stats for {supportedYear.Year}.");
            IReadOnlyList<MasterGame> cleanMasterGames = await _masterGameRepo.GetMasterGames();
            IReadOnlyList<MasterGameYear> cachedMasterGames = await _masterGameRepo.GetMasterGameYears(supportedYear.Year);

            IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
            IReadOnlyList<PickupBid> processedBids = await _fantasyCriticRepo.GetProcessedPickupBids(supportedYear.Year, leagueYears);
            var royalePublishers = await _royaleService.GetAllPublishers(supportedYear.Year);
            _logger.LogDebug("All data retrieved for game stats for {Year}.", supportedYear.Year);

            var calculatedStats = CalculateStatsForGames(supportedYear, leagueYears, cleanMasterGames, cachedMasterGames, processedBids, royalePublishers, hypeConstants, currentDate);
            cancellationToken.ThrowIfCancellationRequested();
            await _masterGameRepo.UpdateCalculatedStats(calculatedStats, supportedYear.Year);
            updatedYears.Add(supportedYear.Year);
        }

        return updatedYears;
    }

    private static IReadOnlyList<MasterGameCalculatedStats> CalculateStatsForGames(SupportedYear supportedYear, IReadOnlyList<LeagueYear> leagueYears,
        IReadOnlyList<MasterGame> cleanMasterGames, IReadOnlyList<MasterGameYear> cachedMasterGames, IReadOnlyList<PickupBid> processedBids,
        IReadOnlyList<RoyalePublisher> royalePublishers, HypeConstants hypeConstants, LocalDate currentDate)
    {
        List<MasterGameCalculatedStats> calculatedStats = [];
        var publisherMasterGames = new HashSet<MasterGame>();
        foreach (var leagueYear in leagueYears)
        {
            foreach (var publisher in leagueYear.Publishers)
            {
                var currentGames = publisher.MyMasterGames;
                var formerGames = publisher.FormerPublisherGames.Where(x => x.PublisherGame.MasterGame is not null)
                    .Select(x => x.PublisherGame.MasterGame!.MasterGame);
                publisherMasterGames.UnionWith(currentGames);
                publisherMasterGames.UnionWith(formerGames);
            }
        }

        var royalePublisherMasterGames = royalePublishers.SelectMany(x => x.PublisherGames.Select(x => x.MasterGame.MasterGame)).ToHashSet();
        var leagueYearDictionary = leagueYears.ToDictionary(x => x.Key);
        IReadOnlyList<Publisher> allPublishers = leagueYears.SelectMany(x => x.Publishers).ToList();
        List<Publisher> publishersInCompleteLeagues = [];
        foreach (var publisher in allPublishers)
        {
            var leagueYear = leagueYearDictionary[publisher.LeagueYearKey];
            if (!leagueYear.League.AffectsStats || !leagueYear.IsFirstDraftFinished)
            {
                continue;
            }

            publishersInCompleteLeagues.Add(publisher);
        }

        var leagueYearsToCount = publishersInCompleteLeagues.Select(x => x.LeagueYearKey).ToHashSet();
        IReadOnlyList<PublisherGame> publisherGames = publishersInCompleteLeagues.SelectMany(x => x.PublisherGames).Where(x => x.MasterGame is not null).ToList();
        var bidsToCount = processedBids.Where(x => leagueYearsToCount.Contains(x.LeagueYear.Key)).ToList();
        ILookup<MasterGame, PickupBid> bidsByGame = bidsToCount.ToLookup(x => x.MasterGame);
        IReadOnlyDictionary<MasterGame, long> totalBidAmounts = bidsByGame.ToDictionary(x => x.Key, y => y.Sum(x => x.BidAmount));

        var publisherGamesByMasterGame = publisherGames.ToLookup(x => x.MasterGame!.MasterGame.MasterGameID);
        Dictionary<LeagueYearKey, HashSet<MasterGame>> standardGamesByLeague = [];
        Dictionary<LeagueYearKey, HashSet<MasterGame>> counterPicksByLeague = [];
        foreach (var publisher in publishersInCompleteLeagues)
        {
            if (!standardGamesByLeague.ContainsKey(publisher.LeagueYearKey))
            {
                standardGamesByLeague[publisher.LeagueYearKey] = [];
            }

            if (!counterPicksByLeague.ContainsKey(publisher.LeagueYearKey))
            {
                counterPicksByLeague[publisher.LeagueYearKey] = [];
            }

            foreach (var game in publisher.PublisherGames)
            {
                if (game.MasterGame is null)
                {
                    continue;
                }

                if (game.CounterPick)
                {
                    counterPicksByLeague[publisher.LeagueYearKey].Add(game.MasterGame.MasterGame);
                }
                else
                {
                    standardGamesByLeague[publisher.LeagueYearKey].Add(game.MasterGame.MasterGame);
                }
            }
        }

        var masterGameCacheLookup = cachedMasterGames.ToDictionary(x => x.MasterGame.MasterGameID, y => y);
        var completeLeagueYearKeys = publishersInCompleteLeagues.Select(x => x.LeagueYearKey).ToHashSet();
        var allLeagueYears = leagueYears.Where(x => completeLeagueYearKeys.Contains(x.Key)).ToList();
        double totalLeagueCount = allLeagueYears.Count;
        var crossDraftPickNumberCache = CrossDraftPickNumberCache.Build(allLeagueYears);

        foreach (var masterGame in cleanMasterGames)
        {
            bool releasedBeforeYear = masterGame.ReleaseDate.HasValue &&
                                      masterGame.ReleaseDate.Value.Year < supportedYear.Year;

            //Basic Stats
            var publisherGamesForMasterGame = publisherGamesByMasterGame[masterGame.MasterGameID];
            var leaguesWithGame = standardGamesByLeague.Count(x => x.Value.Contains(masterGame));
            var leaguesWithCounterPickGame = counterPicksByLeague.Count(x => x.Value.Contains(masterGame));
            List<LeagueYear> leaguesWhereEligible = allLeagueYears.Where(x => x.GameIsEligibleInAnySlot(masterGame, currentDate)).ToList();

            bool wasEverPartOfYear = publisherMasterGames.Contains(masterGame) || royalePublisherMasterGames.Contains(masterGame);
            if (releasedBeforeYear && !wasEverPartOfYear)
            {
                continue;
            }

            List<LeagueYear> timeAdjustedLeagues;
            var scoreOrReleaseTime = masterGame.FirstCriticScoreTimestamp ?? masterGame.ReleaseDate?.AtStartOfDayInZone(TimeExtensions.EasternTimeZone).ToInstant();
            var hadScoreBeforeYear = scoreOrReleaseTime.HasValue && scoreOrReleaseTime.Value.ToEasternDate() < new LocalDate(supportedYear.Year, 1, 1);
            if (scoreOrReleaseTime.HasValue && !hadScoreBeforeYear)
            {
                timeAdjustedLeagues = leaguesWhereEligible.Where(x =>
                        x.FirstDraft.DraftStartedTimestamp.HasValue &&
                        x.FirstDraft.DraftStartedTimestamp <= scoreOrReleaseTime)
                    .ToList();
            }
            else
            {
                timeAdjustedLeagues = leaguesWhereEligible;
            }

            double leaguesWhereEligibleCount = timeAdjustedLeagues.Count;
            double percentStandardGame = leaguesWithGame / totalLeagueCount;
            double eligiblePercentStandardGame = leaguesWithGame / leaguesWhereEligibleCount;

            double percentCounterPick = leaguesWithCounterPickGame / totalLeagueCount;
            double? adjustedPercentCounterPick = null;
            if (leaguesWithGame >= 3)
            {
                adjustedPercentCounterPick = (double)leaguesWithCounterPickGame / (double)leaguesWithGame;
            }

            var bidsForGame = bidsByGame[masterGame];
            int numberOfBids = bidsForGame.Count();
            long totalBidAmount = totalBidAmounts.GetValueOrDefault(masterGame);

            var gamesWithMoreBidTotal = totalBidAmounts.Where(x => x.Value > totalBidAmount);
            double percentageGamesWithHigherBidTotal = gamesWithMoreBidTotal.Count() / (double)cleanMasterGames.Count;
            double bidPercentile = 100 - (percentageGamesWithHigherBidTotal * 100);
            double? averageDraftPosition = publisherGamesForMasterGame.Average(x => crossDraftPickNumberCache.GetPickNumber(x));
            double? averageWinningBid = bidsByGame[masterGame].Where(x => x.Successful.HasValue && x.Successful.Value).Select(x => (double)x.BidAmount).DefaultIfEmpty(0.0).Average();

            double notNullAverageDraftPosition = averageDraftPosition ?? 0;

            double percentStandardGameToUse = eligiblePercentStandardGame;
            double percentCounterPickToUse = adjustedPercentCounterPick ?? percentCounterPick;
            if (masterGame.UseSimpleEligibility || eligiblePercentStandardGame > 1)
            {
                percentStandardGameToUse = percentStandardGame;
                percentCounterPickToUse = percentCounterPick;
            }

            //Derived Stats
            double hypeFactor = (101 - notNullAverageDraftPosition) * percentStandardGame;
            double dateAdjustedHypeFactor = (101 - notNullAverageDraftPosition) * percentStandardGameToUse;

            percentStandardGame = FixDouble(percentStandardGame);
            percentCounterPick = FixDouble(percentCounterPick);
            eligiblePercentStandardGame = FixDouble(eligiblePercentStandardGame);
            adjustedPercentCounterPick = FixDouble(adjustedPercentCounterPick);
            bidPercentile = FixDouble(bidPercentile);
            hypeFactor = FixDouble(hypeFactor);
            dateAdjustedHypeFactor = FixDouble(dateAdjustedHypeFactor);
            double peakHypeFactor = hypeFactor;

            //Linear Regression
            double standardGameCalculation = percentStandardGameToUse * hypeConstants.StandardGameConstant;
            double counterPickCalculation = percentCounterPickToUse * hypeConstants.CounterPickConstant;
            double hypeFactorCalculation = dateAdjustedHypeFactor * hypeConstants.HypeFactorConstant;

            double linearRegressionHypeFactor = hypeConstants.BaseScore
                                                + standardGameCalculation
                                                + counterPickCalculation
                                                + hypeFactorCalculation;

            linearRegressionHypeFactor = FixDouble(linearRegressionHypeFactor);

            var cachedMasterGame = masterGameCacheLookup.GetValueOrDefault(masterGame.MasterGameID);
            if (cachedMasterGame is not null)
            {
                var bigEnoughSampleSize = leaguesWithGame > 5 && allLeagueYears.Count > 20;
                if (bigEnoughSampleSize && cachedMasterGame.PeakHypeFactor > peakHypeFactor)
                {
                    peakHypeFactor = cachedMasterGame.PeakHypeFactor;
                }

                var linearRegressionHypeFactorToUse = cachedMasterGame.LinearRegressionHypeFactor;
                if (linearRegressionHypeFactorToUse == 0)
                {
                    linearRegressionHypeFactorToUse = linearRegressionHypeFactor;
                }

                if (masterGame.CriticScore.HasValue)
                {
                    calculatedStats.Add(new MasterGameCalculatedStats(masterGame, supportedYear.Year, percentStandardGame, percentCounterPick, eligiblePercentStandardGame,
                        adjustedPercentCounterPick, numberOfBids, (int)totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor,
                        dateAdjustedHypeFactor, peakHypeFactor, linearRegressionHypeFactorToUse));
                    continue;
                }
            }

            calculatedStats.Add(new MasterGameCalculatedStats(masterGame, supportedYear.Year, percentStandardGame, percentCounterPick, eligiblePercentStandardGame,
                adjustedPercentCounterPick, numberOfBids, (int)totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor,
                dateAdjustedHypeFactor, peakHypeFactor, linearRegressionHypeFactor));
        }

        return calculatedStats;
    }

    private static double FixDouble(double num)
    {
        if (double.IsNaN(num))
        {
            return 0;
        }

        if (double.IsInfinity(num))
        {
            return 1;
        }

        return num;
    }

    private static double? FixDouble(double? num)
    {
        if (!num.HasValue)
        {
            return null;
        }
        if (double.IsNaN(num.Value))
        {
            return 0;
        }

        if (double.IsInfinity(num.Value))
        {
            return 1;
        }

        return num;
    }
}
