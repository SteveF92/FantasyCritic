using CsvHelper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Statistics;
using FantasyCritic.Lib.Utilities;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Royale;
using MoreLinq;
using NLog;

namespace FantasyCritic.Lib.Services
{
    public class AdminService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IRDSManager _rdsManager;
        private readonly RoyaleService _royaleService;
        private readonly IHypeFactorService _hypeFactorService;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IMasterGameRepo _masterGameRepo;
        private readonly InterLeagueService _interLeagueService;
        private readonly IOpenCriticService _openCriticService;
        private readonly IClock _clock;
        private readonly AdminServiceConfiguration _configuration;

        public AdminService(FantasyCriticService fantasyCriticService, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo,
            InterLeagueService interLeagueService, IOpenCriticService openCriticService, IClock clock, IRDSManager rdsManager,
            RoyaleService royaleService, IHypeFactorService hypeFactorService, AdminServiceConfiguration configuration)
        {
            _fantasyCriticService = fantasyCriticService;
            _fantasyCriticRepo = fantasyCriticRepo;
            _masterGameRepo = masterGameRepo;
            _interLeagueService = interLeagueService;
            _openCriticService = openCriticService;
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
            foreach (var masterGame in masterGames)
            {
                if (!masterGame.OpenCriticID.HasValue)
                {
                    continue;
                }

                if (masterGame.DoNotRefreshAnything)
                {
                    continue;
                }

                if (masterGame.IsReleased(currentDate) && masterGame.ReleaseDate.HasValue)
                {
                    var year = masterGame.ReleaseDate.Value.Year;
                    var supportedYear = supportedYears.SingleOrDefault(x => x.Year == year);
                    if (supportedYear != null && supportedYear.Finished)
                    {
                        continue;
                    }
                }

                var openCriticGame = await _openCriticService.GetOpenCriticGame(masterGame.OpenCriticID.Value);
                if (openCriticGame.HasValue)
                {
                    await _interLeagueService.UpdateCriticStats(masterGame, openCriticGame.Value);
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

                    if (subGameOpenCriticGame.HasValue)
                    {
                        await _interLeagueService.UpdateCriticStats(subGame, subGameOpenCriticGame.Value);
                    }
                }
            }

            _logger.Info("Done refreshing critic scores");
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

            var mostRecentFinishedYear = supportedYears.Where(x => x.Finished).MaxBy(x => x.Year).SingleOrDefault();
            if (mostRecentFinishedYear is not null)
            {
                var calculatedStats = await _fantasyCriticService.GetCalculatedStatsForYear(mostRecentFinishedYear.Year);
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

            var latestQuarter = supportedQuarters.MaxBy(x => x.YearQuarter).Single();
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

        private async Task UpdateSystemWideValues()
        {
            _logger.Info("Updating system wide values");

            List<PublisherGame> allGamesWithPoints = new List<PublisherGame>();
            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                var leagueYears = await _fantasyCriticRepo.GetLeagueYears(supportedYear.Year);
                var publishers = await _fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, leagueYears);
                var publisherGames = publishers.SelectMany(x => x.PublisherGames);
                var gamesWithPoints = publisherGames.Where(x => x.FantasyPoints.HasValue).ToList();
                allGamesWithPoints.AddRange(gamesWithPoints);
            }

            var averageStandardPoints = allGamesWithPoints.Where(x => !x.CounterPick).Select(x => x.FantasyPoints.Value).DefaultIfEmpty(0m).Average();
            var averageCounterPickPoints = allGamesWithPoints.Where(x => x.CounterPick).Select(x => x.FantasyPoints.Value).DefaultIfEmpty(0m).Average();

            var systemWideValues = new SystemWideValues(averageStandardPoints, averageCounterPickPoints);
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
                IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, leagueYears);
                IReadOnlyList<Publisher> publishersInRealLeagues = allPublishers.Where(x => !x.LeagueYear.League.TestLeague).ToList();
                IReadOnlyList<Publisher> publishersInCompleteLeagues = publishersInRealLeagues.Where(x => x.LeagueYear.PlayStatus.DraftFinished).ToList();
                IReadOnlyList<PublisherGame> publisherGames = publishersInCompleteLeagues.SelectMany(x => x.PublisherGames).Where(x => x.MasterGame.HasValue).ToList();
                IReadOnlyList<PickupBid> processedBids = await _fantasyCriticRepo.GetProcessedPickupBids(supportedYear.Year, leagueYears, allPublishers);
                ILookup<MasterGame, PickupBid> bidsByGame = processedBids.ToLookup(x => x.MasterGame);
                IReadOnlyDictionary<MasterGame, long> totalBidAmounts = bidsByGame.ToDictionary(x => x.Key, y => y.Sum(x => x.BidAmount));

                var publisherGamesByMasterGame = publisherGames.ToLookup(x => x.MasterGame.Value.MasterGame.MasterGameID);
                Dictionary<LeagueYear, HashSet<MasterGame>> standardGamesByLeague = new Dictionary<LeagueYear, HashSet<MasterGame>>();
                Dictionary<LeagueYear, HashSet<MasterGame>> counterPicksByLeague = new Dictionary<LeagueYear, HashSet<MasterGame>>();
                foreach (var publisher in publishersInCompleteLeagues)
                {
                    if (!standardGamesByLeague.ContainsKey(publisher.LeagueYear))
                    {
                        standardGamesByLeague[publisher.LeagueYear] = new HashSet<MasterGame>();
                    }

                    if (!counterPicksByLeague.ContainsKey(publisher.LeagueYear))
                    {
                        counterPicksByLeague[publisher.LeagueYear] = new HashSet<MasterGame>();
                    }

                    foreach (var game in publisher.PublisherGames)
                    {
                        if (game.MasterGame.HasNoValue)
                        {
                            continue;
                        }

                        if (game.CounterPick)
                        {
                            counterPicksByLeague[publisher.LeagueYear].Add(game.MasterGame.Value.MasterGame);
                        }
                        else
                        {
                            standardGamesByLeague[publisher.LeagueYear].Add(game.MasterGame.Value.MasterGame);
                        }
                    }
                }

                var masterGameCacheLookup = cachedMasterGames.ToDictionary(x => x.MasterGame.MasterGameID, y => y);
                var allLeagueYears = publishersInCompleteLeagues.Select(x => x.LeagueYear).Distinct().ToList();
                double totalLeagueCount = allLeagueYears.Count();

                foreach (var masterGame in cleanMasterGames)
                {
                    var gameIsCached = masterGameCacheLookup.TryGetValue(masterGame.MasterGameID, out var cachedMasterGame);
                    if (masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate < new LocalDate(supportedYear.Year, 1, 1) && gameIsCached)
                    {
                        calculatedStats.Add(new MasterGameCalculatedStats(masterGame, cachedMasterGame));
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
                        adjustedPercentCounterPick = (double)leaguesWithCounterPickGame / (double) leaguesWithGame;
                    }

                    var bidsForGame = bidsByGame[masterGame];
                    int numberOfBids = bidsForGame.Count();
                    bool hasBids = totalBidAmounts.TryGetValue(masterGame, out long totalBidAmount);
                    if (!hasBids)
                    {
                        totalBidAmount = 0;
                    }

                    var gamesWithMoreBidTotal = totalBidAmounts.Where(x => x.Value > totalBidAmount);
                    double percentageGamesWithHigherBidTotal = gamesWithMoreBidTotal.Count() / (double) cleanMasterGames.Count;
                    double bidPercentile = 100 - (percentageGamesWithHigherBidTotal * 100);
                    double? averageDraftPosition = publisherGamesForMasterGame.Average(x => x.OverallDraftPosition);
                    double? averageWinningBid = bidsByGame[masterGame].Where(x => x.Successful.HasValue && x.Successful.Value).Select(x => (double) x.BidAmount).DefaultIfEmpty(0.0).Average();

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

                    if (masterGame.CriticScore.HasValue && gameIsCached)
                    {
                        calculatedStats.Add(new MasterGameCalculatedStats(masterGame, supportedYear.Year, percentStandardGame, percentCounterPick, eligiblePercentStandardGame,
                            adjustedPercentCounterPick, numberOfBids, (int)totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor,
                            dateAdjustedHypeFactor, cachedMasterGame.LinearRegressionHypeFactor));
                        continue;
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
                        adjustedPercentCounterPick, numberOfBids, (int) totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor, 
                        dateAdjustedHypeFactor, linearRegressionHypeFactor));
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
                bool inEarlyAccess = today >= masterGame.EarlyAccessReleaseDate.Value;
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
                bool releasedInternationally = today >= masterGame.InternationalReleaseDate.Value;
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
}
