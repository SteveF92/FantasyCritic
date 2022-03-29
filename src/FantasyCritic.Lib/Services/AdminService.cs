using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.GG;
using NLog;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.DependencyInjection;

namespace FantasyCritic.Lib.Services;

public class AdminService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IRDSManager _rdsManager;
    private readonly RoyaleService _royaleService;
    private readonly IHypeFactorService _hypeFactorService;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly FantasyCriticUserManager _userManager;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IOpenCriticService _openCriticService;
    private readonly IGGService _ggService;
    private readonly PatreonService _patreonService;
    private readonly IClock _clock;
    private readonly AdminServiceConfiguration _configuration;

    public AdminService(FantasyCriticService fantasyCriticService, FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo,
        InterLeagueService interLeagueService, IOpenCriticService openCriticService, IGGService ggService, PatreonService patreonService, IClock clock, IRDSManager rdsManager,
        RoyaleService royaleService, IHypeFactorService hypeFactorService, AdminServiceConfiguration configuration)
    {
        _fantasyCriticService = fantasyCriticService;
        _userManager = userManager;
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _interLeagueService = interLeagueService;
        _openCriticService = openCriticService;
        _ggService = ggService;
        _patreonService = patreonService;
        _clock = clock;
        _rdsManager = rdsManager;
        _royaleService = royaleService;
        _hypeFactorService = hypeFactorService;
        _configuration = configuration;
    }

    public async Task FullDataRefresh()
    {
        await RefreshCriticInfo();
        await Task.Delay(1000);
        await RefreshGGInfo();

        await Task.Delay(1000);
        await RefreshCaches();
        await Task.Delay(1000);

        await UpdateFantasyPoints();
    }

    public async Task RefreshCriticInfo()
    {
        _logger.Info("Refreshing critic scores");
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.RefreshOpenCritic)
        {
            _logger.Info("Not refreshing Open Critic scores as the flag is turned off.");
            return;
        }

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var masterGames = await _interLeagueService.GetMasterGames();

        var currentDate = _clock.GetToday();
        var masterGamesToUpdate = masterGames.Where(x => x.OpenCriticID.HasValue && !x.DoNotRefreshAnything).ToList();
        foreach (var masterGame in masterGamesToUpdate)
        {
            if (masterGame.IsReleased(currentDate) && masterGame.ReleaseDate.HasValue)
            {
                var year = masterGame.ReleaseDate.Value.Year;
                var supportedYear = supportedYears.SingleOrDefault(x => x.Year == year);
                if (supportedYear != null && supportedYear.Finished)
                {
                    continue;
                }
            }

            var openCriticGame = await _openCriticService.GetOpenCriticGame(masterGame.OpenCriticID!.Value);
            if (openCriticGame is not null)
            {
                await _interLeagueService.UpdateCriticStats(masterGame, openCriticGame);
            }
            else
            {
                _logger.Warn($"Getting an open critic game failed (empty return): {masterGame.GameName} | [{masterGame.OpenCriticID.Value}]");
            }

            foreach (var subGame in masterGame.SubGames)
            {
                if (!subGame.OpenCriticID.HasValue)
                {
                    continue;
                }

                var subGameOpenCriticGame = await _openCriticService.GetOpenCriticGame(subGame.OpenCriticID.Value);

                if (subGameOpenCriticGame is not null)
                {
                    await _interLeagueService.UpdateCriticStats(subGame, subGameOpenCriticGame);
                }
            }
        }

        _logger.Info("Done refreshing critic scores");
    }

    public async Task RefreshGGInfo()
    {
        var masterGames = await _interLeagueService.GetMasterGames();

        var masterGamesToUpdate = masterGames.Where(x => x.GGToken is not null && !x.DoNotRefreshAnything).ToList();
        foreach (var masterGame in masterGamesToUpdate)
        {
            if (!string.IsNullOrWhiteSpace(masterGame.GGCoverArtFileName))
            {
                continue;
            }

            var ggGame = await _ggService.GetGGGame(masterGame.GGToken!);
            if (ggGame is not null)
            {
                await _interLeagueService.UpdateGGStats(masterGame, ggGame);
            }
            else
            {
                _logger.Warn($"Getting an GG| game failed (empty return): {masterGame.GameName} | [{masterGame.GGToken}]");
            }
        }
    }

    public async Task UpdateFantasyPoints()
    {
        _logger.Info("Updating fantasy points");

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);
        foreach (var activeYear in activeYears)
        {
            var calculatedStats = await _fantasyCriticService.GetCalculatedStatsForYear(activeYear.Year);
            await _fantasyCriticRepo.UpdatePublisherGameCalculatedStats(calculatedStats.PublisherGameCalculatedStats);
        }

        var finishedYears = supportedYears.Where(x => x.Finished);
        foreach (var finishedYear in finishedYears)
        {
            var calculatedStats = await _fantasyCriticService.GetCalculatedStatsForYear(finishedYear.Year);
            await _fantasyCriticRepo.UpdateLeagueWinners(calculatedStats.WinningUsers);
        }

        _logger.Info("Done updating fantasy points");
        _logger.Info("Updating royale fantasy points");

        var supportedQuarters = await _royaleService.GetYearQuarters();
        foreach (var supportedQuarter in supportedQuarters)
        {
            if (supportedQuarter.Finished || !supportedQuarter.OpenForPlay)
            {
                continue;
            }

            await _royaleService.UpdateFantasyPoints(supportedQuarter.YearQuarter);
        }

        _logger.Info("Done updating royale fantasy points");
    }

    public async Task RefreshCaches()
    {
        _logger.Info("Refreshing caches");

        LocalDate today = _clock.GetToday();
        LocalDate tomorrow = today.PlusDays(1);
        await UpdateCodeBasedTags(today);
        await _masterGameRepo.UpdateReleaseDateEstimates(tomorrow);

        await UpdateSystemWideValues();
        HypeConstants hypeConstants;
        if (_configuration.DefaultHypeConstants)
        {
            _logger.Info("Using default hype constants");
            hypeConstants = HypeConstants.GetReasonableDefaults();
        }
        else
        {
            hypeConstants = await GetHypeConstants();
        }
        await UpdateGameStats(hypeConstants);
        _logger.Info("Done refreshing caches");
    }

    public Task SnapshotDatabase()
    {
        Instant time = _clock.GetCurrentInstant();
        return _rdsManager.SnapshotRDS(time);
    }

    public Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentDatabaseSnapshots()
    {
        return _rdsManager.GetRecentSnapshots();
    }

    public async Task SetTimeFlags()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();

        var now = _clock.GetCurrentInstant();
        var nycNow = now.InZone(TimeExtensions.EasternTimeZone);

        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished)
            {
                continue;
            }

            var endDate = new LocalDate(supportedYear.Year, 12, 31);
            if (nycNow.Date > endDate)
            {
                _logger.Info($"Automatically setting {supportedYear} as finished because date/time is: {nycNow}");
                await _interLeagueService.FinishYear(supportedYear);
            }
        }

        var supportedQuarters = await _royaleService.GetYearQuarters();
        foreach (var supportedQuarter in supportedQuarters)
        {
            if (supportedQuarter.Finished)
            {
                continue;
            }

            var endDate = supportedQuarter.YearQuarter.LastDateOfQuarter;
            if (nycNow.Date > endDate)
            {
                _logger.Info($"Automatically setting {supportedQuarter} as finished because date/time is: {nycNow}");
                await _royaleService.FinishQuarter(supportedQuarter);
            }
        }

        var latestQuarter = supportedQuarters.WhereMax(x => x.YearQuarter).Single();
        var nextQuarter = latestQuarter.YearQuarter.NextQuarter;
        var dayToStartNextQuarter = nextQuarter.FirstDateOfQuarter.Minus(Period.FromDays(5));
        if (nycNow.Date > dayToStartNextQuarter)
        {
            await _royaleService.StartNewQuarter(nextQuarter);
        }

        var dayOfWeek = nycNow.DayOfWeek;
        var timeOfDay = nycNow.TimeOfDay;
        var earliestTimeToSet = new LocalTime(19, 59);
        var latestTimeToSet = new LocalTime(20, 59);
        if (dayOfWeek == TimeExtensions.ActionProcessingDay && timeOfDay > earliestTimeToSet && timeOfDay < latestTimeToSet)
        {
            _logger.Info($"Automatically setting action processing mode = true because date/time is: {nycNow}");
            await _interLeagueService.SetActionProcessingMode(true);
            _logger.Info("Snapshotting database");
            await _rdsManager.SnapshotRDS(now);
        }
    }

    public async Task UpdatePatreonRoles()
    {
        var patreonUsers = await _userManager.GetAllPatreonUsers();
        var patronInfo = await _patreonService.GetPatronInfo(patreonUsers);
        await _userManager.UpdatePatronInfo(patronInfo);
    }

    private async Task UpdateSystemWideValues()
    {
        _logger.Info("Updating system wide values");

        List<PublisherGame> allGamesWithPoints = new List<PublisherGame>();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var allLeagueYears = new List<LeagueYear>();
        foreach (var supportedYear in supportedYears)
        {
            var leagueYears = await _fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
            var leaguesToCount = leagueYears.Where(x => !x.League.TestLeague && x.PlayStatus.DraftFinished).ToList();
            allLeagueYears.AddRange(leaguesToCount);
            var publishers = leaguesToCount.SelectMany(x => x.Publishers).ToList();
            var publisherGames = publishers.SelectMany(x => x.PublisherGames);
            var gamesWithPoints = publisherGames.Where(x => x.FantasyPoints.HasValue).ToList();
            allGamesWithPoints.AddRange(gamesWithPoints);
        }

        var allStandardGamesWithPoints = allGamesWithPoints.Where(x => !x.CounterPick).ToList();
        var allCounterPicksWithPoints = allGamesWithPoints.Where(x => x.CounterPick).ToList();

        var averageStandardPoints = allStandardGamesWithPoints.Select(x => x.FantasyPoints!.Value).DefaultIfEmpty(0m).Average();
        var averagePickupOnlyStandardPoints = allStandardGamesWithPoints.Where(x => !x.OverallDraftPosition.HasValue).Select(x => x.FantasyPoints!.Value).DefaultIfEmpty(0m).Average();
        var averageCounterPickPoints = allCounterPicksWithPoints.Select(x => x.FantasyPoints!.Value).DefaultIfEmpty(0m).Average();

        Dictionary<int, List<decimal>> pointsForPosition = new Dictionary<int, List<decimal>>();
        foreach (var leagueYear in allLeagueYears)
        {
            var publishers = leagueYear.Publishers;
            var orderedGames = publishers.SelectMany(x => x.PublisherGames).Where(x => !x.CounterPick & x.FantasyPoints.HasValue).OrderBy(x => x.Timestamp).ToList();
            for (var index = 0; index < orderedGames.Count; index++)
            {
                var game = orderedGames[index];
                var pickPosition = index + 1;
                if (!pointsForPosition.ContainsKey(pickPosition))
                {
                    pointsForPosition[pickPosition] = new List<decimal>();
                }

                pointsForPosition[pickPosition].Add(game.FantasyPoints!.Value);
            }
        }

        var averageStandardGamePointsByPickPosition = pointsForPosition.Select(position => new AveragePickPositionPoints(position.Key, position.Value.Count, position.Value.Average())).ToList();
        var systemWideValues = new SystemWideValues(averageStandardPoints, averagePickupOnlyStandardPoints, averageCounterPickPoints, averageStandardGamePointsByPickPosition);
        await _interLeagueService.UpdateSystemWideValues(systemWideValues);
    }

    private async Task<HypeConstants> GetHypeConstants()
    {
        _logger.Info("Getting Hype Constants");
        var supportedYears = await _interLeagueService.GetSupportedYears();
        List<MasterGameYear> allMasterGameYears = new List<MasterGameYear>();

        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Year < 2019)
            {
                continue;
            }

            var masterGamesForYear = await _masterGameRepo.GetMasterGameYears(supportedYear.Year);
            var relevantGames = masterGamesForYear.Where(x => x.IsRelevantInYear(supportedYear));
            allMasterGameYears.AddRange(relevantGames);
        }

        var hypeConstants = await _hypeFactorService.GetHypeConstants(allMasterGameYears);
        _logger.Info($"Hype Constants: {hypeConstants}");

        return hypeConstants;
    }

    private async Task UpdateGameStats(HypeConstants hypeConstants)
    {
        _logger.Info("Updating game stats.");

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var currentDate = _clock.GetToday();
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished)
            {
                continue;
            }

            List<MasterGameCalculatedStats> calculatedStats = new List<MasterGameCalculatedStats>();
            IReadOnlyList<MasterGame> cleanMasterGames = await _masterGameRepo.GetMasterGames();
            IReadOnlyList<MasterGameYear> cachedMasterGames = await _masterGameRepo.GetMasterGameYears(supportedYear.Year);

            IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
            var leagueYearDictionary = leagueYears.ToDictionary(x => x.Key);
            IReadOnlyList<Publisher> allPublishers = leagueYears.SelectMany(x => x.Publishers).ToList();
            List<Publisher> publishersInCompleteLeagues = new List<Publisher>();
            foreach (var publisher in allPublishers)
            {
                var leagueYear = leagueYearDictionary[publisher.LeagueYearKey];
                if (leagueYear.League.TestLeague || !leagueYear.PlayStatus.DraftFinished)
                {
                    continue;
                }

                publishersInCompleteLeagues.Add(publisher);
            }

            var leagueYearsToCount = publishersInCompleteLeagues.Select(x => x.LeagueYearKey).ToHashSet();
            IReadOnlyList<PublisherGame> publisherGames = publishersInCompleteLeagues.SelectMany(x => x.PublisherGames).Where(x => x.MasterGame is not null).ToList();
            IReadOnlyList<PickupBid> processedBids = await _fantasyCriticRepo.GetProcessedPickupBids(supportedYear.Year, leagueYears);
            var bidsToCount = processedBids.Where(x => leagueYearsToCount.Contains(x.LeagueYear.Key)).ToList();
            ILookup<MasterGame, PickupBid> bidsByGame = bidsToCount.ToLookup(x => x.MasterGame);
            IReadOnlyDictionary<MasterGame, long> totalBidAmounts = bidsByGame.ToDictionary(x => x.Key, y => y.Sum(x => x.BidAmount));

            var publisherGamesByMasterGame = publisherGames.ToLookup(x => x.MasterGame!.MasterGame.MasterGameID);
            Dictionary<LeagueYearKey, HashSet<MasterGame>> standardGamesByLeague = new Dictionary<LeagueYearKey, HashSet<MasterGame>>();
            Dictionary<LeagueYearKey, HashSet<MasterGame>> counterPicksByLeague = new Dictionary<LeagueYearKey, HashSet<MasterGame>>();
            foreach (var publisher in publishersInCompleteLeagues)
            {
                if (!standardGamesByLeague.ContainsKey(publisher.LeagueYearKey))
                {
                    standardGamesByLeague[publisher.LeagueYearKey] = new HashSet<MasterGame>();
                }

                if (!counterPicksByLeague.ContainsKey(publisher.LeagueYearKey))
                {
                    counterPicksByLeague[publisher.LeagueYearKey] = new HashSet<MasterGame>();
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
            double totalLeagueCount = allLeagueYears.Count();

            foreach (var masterGame in cleanMasterGames)
            {
                if (masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year < supportedYear.Year)
                {
                    continue;
                }

                //Basic Stats
                var publisherGamesForMasterGame = publisherGamesByMasterGame[masterGame.MasterGameID];
                var leaguesWithGame = standardGamesByLeague.Count(x => x.Value.Contains(masterGame));
                var leaguesWithCounterPickGame = counterPicksByLeague.Count(x => x.Value.Contains(masterGame));
                List<LeagueYear> leaguesWhereEligible = allLeagueYears.Where(x => x.GameIsEligibleInAnySlot(masterGame, currentDate)).ToList();

                List<LeagueYear> timeAdjustedLeagues;
                var scoreOrReleaseTime = masterGame.FirstCriticScoreTimestamp ?? masterGame.ReleaseDate?.AtStartOfDayInZone(TimeExtensions.EasternTimeZone).ToInstant();
                if (scoreOrReleaseTime.HasValue)
                {
                    timeAdjustedLeagues = leaguesWhereEligible.Where(x =>
                            x.DraftStartedTimestamp.HasValue &&
                            x.DraftStartedTimestamp <= scoreOrReleaseTime)
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
                bool hasBids = totalBidAmounts.TryGetValue(masterGame, out long totalBidAmount);
                if (!hasBids)
                {
                    totalBidAmount = 0;
                }

                var gamesWithMoreBidTotal = totalBidAmounts.Where(x => x.Value > totalBidAmount);
                double percentageGamesWithHigherBidTotal = gamesWithMoreBidTotal.Count() / (double)cleanMasterGames.Count;
                double bidPercentile = 100 - (percentageGamesWithHigherBidTotal * 100);
                double? averageDraftPosition = publisherGamesForMasterGame.Average(x => x.OverallDraftPosition);
                double? averageWinningBid = bidsByGame[masterGame].Where(x => x.Successful.HasValue && x.Successful.Value).Select(x => (double)x.BidAmount).DefaultIfEmpty(0.0).Average();

                double notNullAverageDraftPosition = averageDraftPosition ?? 0;

                double percentStandardGameToUse = eligiblePercentStandardGame;
                double percentCounterPickToUse = adjustedPercentCounterPick ?? percentCounterPick;
                if (masterGame.EligibilityChanged || eligiblePercentStandardGame > 1)
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

                if (masterGameCacheLookup.TryGetValue(masterGame.MasterGameID, out var cachedMasterGame))
                {
                    if (cachedMasterGame.PeakHypeFactor > peakHypeFactor)
                    {
                        peakHypeFactor = cachedMasterGame.PeakHypeFactor;
                    }

                    if (masterGame.CriticScore.HasValue)
                    {
                        calculatedStats.Add(new MasterGameCalculatedStats(masterGame, supportedYear.Year, percentStandardGame, percentCounterPick, eligiblePercentStandardGame,
                            adjustedPercentCounterPick, numberOfBids, (int)totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor,
                            dateAdjustedHypeFactor, peakHypeFactor, cachedMasterGame.LinearRegressionHypeFactor));
                        continue;
                    }
                }

                //Linear Regression
                double standardGameCalculation = percentStandardGameToUse * hypeConstants.StandardGameConstant;
                double counterPickCalculation = percentCounterPickToUse * hypeConstants.CounterPickConstant;
                double hypeFactorCalculation = dateAdjustedHypeFactor * hypeConstants.HypeFactorConstant;

                double linearRegressionHypeFactor = hypeConstants.BaseScore
                                                    + standardGameCalculation
                                                    + counterPickCalculation
                                                    + hypeFactorCalculation;

                linearRegressionHypeFactor = FixDouble(linearRegressionHypeFactor);

                calculatedStats.Add(new MasterGameCalculatedStats(masterGame, supportedYear.Year, percentStandardGame, percentCounterPick, eligiblePercentStandardGame,
                    adjustedPercentCounterPick, numberOfBids, (int)totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor,
                    dateAdjustedHypeFactor, peakHypeFactor, linearRegressionHypeFactor));
            }

            await _masterGameRepo.UpdateCalculatedStats(calculatedStats, supportedYear.Year);
        }
    }

    private async Task UpdateCodeBasedTags(LocalDate today)
    {
        _logger.Info("Updating Code Based Tags");
        var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
        var allMasterGames = await _masterGameRepo.GetMasterGames();
        var masterGamesWithEarlyAccessDate = allMasterGames.Where(x => x.EarlyAccessReleaseDate.HasValue);
        var masterGamesWithInternationalDate = allMasterGames.Where(x => x.InternationalReleaseDate.HasValue);
        Dictionary<MasterGame, List<MasterGameTag>> tagsToAdd = allMasterGames.ToDictionary(x => x, y => new List<MasterGameTag>());

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

    private double FixDouble(double num)
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

    private double? FixDouble(double? num)
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
