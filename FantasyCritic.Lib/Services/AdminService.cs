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

namespace FantasyCritic.Lib.Services
{
    public class AdminService
    {
        private readonly IRDSManager _rdsManager;
        private readonly RoyaleService _royaleService;
        private readonly PythonRunner _pythonRunner;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IMasterGameRepo _masterGameRepo;
        private readonly InterLeagueService _interLeagueService;
        private readonly IOpenCriticService _openCriticService;
        private readonly IClock _clock;
        private readonly ILogger<AdminService> _logger;

        public AdminService(FantasyCriticService fantasyCriticService, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo,
            InterLeagueService interLeagueService, IOpenCriticService openCriticService, IClock clock, ILogger<AdminService> logger, IRDSManager rdsManager,
            RoyaleService royaleService, PythonRunner pythonRunner)
        {
            _fantasyCriticService = fantasyCriticService;
            _fantasyCriticRepo = fantasyCriticRepo;
            _masterGameRepo = masterGameRepo;
            _interLeagueService = interLeagueService;
            _openCriticService = openCriticService;
            _clock = clock;
            _logger = logger;
            _rdsManager = rdsManager;
            _royaleService = royaleService;
            _pythonRunner = pythonRunner;
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
            _logger.LogInformation("Refreshing critic scores");
            var supportedYears = await _interLeagueService.GetSupportedYears();
            var masterGames = await _interLeagueService.GetMasterGames();
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

                if (masterGame.IsReleased(_clock.GetCurrentInstant()) && masterGame.ReleaseDate.HasValue)
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
                    _logger.LogWarning($"Getting an open critic game failed (empty return): {masterGame.GameName} | [{masterGame.OpenCriticID.Value}]");
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

            _logger.LogInformation("Done refreshing critic scores");
        }

        public async Task UpdateFantasyPoints()
        {
            _logger.LogInformation("Updating fantasy points");

            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                if (supportedYear.Finished || !supportedYear.OpenForPlay)
                {
                    continue;
                }

                await _fantasyCriticService.UpdateFantasyPoints(supportedYear.Year);
            }

            _logger.LogInformation("Done updating fantasy points");
            _logger.LogInformation("Updating royale fantasy points");

            var supportedQuarters = await _royaleService.GetYearQuarters();
            foreach (var supportedQuarter in supportedQuarters)
            {
                if (supportedQuarter.Finished || !supportedQuarter.OpenForPlay)
                {
                    continue;
                }

                await _royaleService.UpdateFantasyPoints(supportedQuarter.YearQuarter);
            }

            _logger.LogInformation("Done updating royale fantasy points");
        }

        public async Task RefreshCaches()
        {
            _logger.LogInformation("Refreshing caches");

            LocalDate today = _clock.GetToday();
            LocalDate tomorrow = today.PlusDays(1);
            await UpdateCodeBasedTags(today);
            await _masterGameRepo.UpdateReleaseDateEstimates(tomorrow);

            await UpdateSystemWideValues();
            var hypeConstants = await GetHypeConstants();
            await UpdateGameStats(hypeConstants);
            _logger.LogInformation("Done refreshing caches");
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

        private async Task UpdateSystemWideValues()
        {
            List<PublisherGame> allGamesWithPoints = new List<PublisherGame>();
            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                var publishers = await _fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year);
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
            _logger.LogInformation("Getting Hype Constants");
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

            var outputModels = allMasterGameYears.Select(x => new MasterGameYearScriptInput(x));
            
            string scriptsFolder = "C:\\FantasyCritic\\Scripts\\";
            string scriptFileName = "regression_model.py";
            string scriptPath = Path.Combine(scriptsFolder, scriptFileName);

            string dataFileName = Guid.NewGuid() + ".csv";
            string dataPath = Path.Combine(scriptsFolder, dataFileName);
            using (var writer = new StreamWriter(dataPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(outputModels);
            }

            var result = _pythonRunner.RunPython(scriptPath, dataPath);
            var splitString = result.Split(' ');

            File.Delete(dataPath);

            double baseScore = double.Parse(splitString[2]);
            double standardGameConstant = double.Parse(splitString[4]);
            double counterPickConstant = double.Parse(splitString[8]);
            double hypeFactorConstant = double.Parse(splitString[12]);
            double averageDraftPositionConstant = double.Parse(splitString[16]);
            double totalBidAmountConstant = double.Parse(splitString[20]);
            double bidPercentileConstant = double.Parse(splitString[24]);

            HypeConstants hypeConstants = new HypeConstants(baseScore, standardGameConstant, counterPickConstant,
                hypeFactorConstant, averageDraftPositionConstant, totalBidAmountConstant, bidPercentileConstant);

            _logger.LogInformation($"Hype Constants: {hypeConstants}");

            return hypeConstants;
        }

        private async Task UpdateGameStats(HypeConstants hypeConstants)
        {
            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                if (supportedYear.Finished)
                {
                    continue;
                }

                List<MasterGameCalculatedStats> calculatedStats = new List<MasterGameCalculatedStats>();
                IReadOnlyList<MasterGame> cleanMasterGames = await _masterGameRepo.GetMasterGames();
                IReadOnlyList<MasterGameYear> cachedMasterGames = await _masterGameRepo.GetMasterGameYears(supportedYear.Year);
                IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year);
                IReadOnlyList<Publisher> publishersInRealLeagues = allPublishers.Where(x => !x.LeagueYear.League.TestLeague).ToList();
                IReadOnlyList<Publisher> publishersInCompleteLeagues = publishersInRealLeagues.Where(x => x.LeagueYear.PlayStatus.DraftFinished).ToList();
                IReadOnlyList<PublisherGame> publisherGames = publishersInCompleteLeagues.SelectMany(x => x.PublisherGames).Where(x => x.MasterGame.HasValue).ToList();
                IReadOnlyList<PickupBid> processedBids = await _fantasyCriticRepo.GetProcessedPickupBids(supportedYear.Year);
                ILookup<MasterGame, PickupBid> bidsByGame = processedBids.ToLookup(x => x.MasterGame);
                IReadOnlyDictionary<MasterGame, long> totalBidAmounts = bidsByGame.ToDictionary(x => x.Key, y => y.Sum(x => x.BidAmount));
                IReadOnlyList<MasterGame> allGamesWithBids = bidsByGame.Select(x => x.Key).ToList();

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
                    List<LeagueYear> leaguesWhereEligible = allLeagueYears.Where(x => x.GameIsEligible(masterGame)).ToList();

                    List<LeagueYear> timeAdjustedLeagues;
                    if (masterGame.FirstCriticScoreTimestamp.HasValue)
                    {
                        timeAdjustedLeagues = leaguesWhereEligible.Where(x =>
                                x.DraftStartedTimestamp.HasValue &&
                                x.DraftStartedTimestamp <= masterGame.FirstCriticScoreTimestamp.Value)
                            .ToList();
                    }
                    else
                    {
                        timeAdjustedLeagues = leaguesWhereEligible;
                    }

                    double leaguesWhereEligibleCount = timeAdjustedLeagues.Count;
                    double percentStandardGame = leaguesWithGame / totalLeagueCount;
                    double percentCounterPick = leaguesWithCounterPickGame / totalLeagueCount;
                    double eligiblePercentStandardGame = leaguesWithGame / leaguesWhereEligibleCount;
                    double eligiblePercentCounterPick = leaguesWithCounterPickGame / leaguesWhereEligibleCount;
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
                    double percentCounterPickToUse = eligiblePercentCounterPick;
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
                    eligiblePercentCounterPick = FixDouble(eligiblePercentCounterPick);
                    bidPercentile = FixDouble(bidPercentile);
                    hypeFactor = FixDouble(hypeFactor);
                    dateAdjustedHypeFactor = FixDouble(dateAdjustedHypeFactor);

                    if (masterGame.CriticScore.HasValue && gameIsCached)
                    {
                        calculatedStats.Add(new MasterGameCalculatedStats(masterGame, supportedYear.Year, percentStandardGame, percentCounterPick, eligiblePercentStandardGame,
                            eligiblePercentCounterPick, numberOfBids, (int)totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor,
                            dateAdjustedHypeFactor, cachedMasterGame.LinearRegressionHypeFactor));
                        continue;
                    }

                    //Linear Regression
                    double standardGameCalculation = percentStandardGameToUse * hypeConstants.StandardGameConstant;
                    double counterPickCalculation = percentCounterPickToUse * hypeConstants.CounterPickConstant;
                    double hypeFactorCalculation = dateAdjustedHypeFactor * hypeConstants.HypeFactorConstant;
                    double averageDraftPositionCalculation = notNullAverageDraftPosition * hypeConstants.AverageDraftPositionConstant;
                    double totalBidCalculation = totalBidAmount * hypeConstants.TotalBidAmountConstant;
                    double bidPercentileCalculation = bidPercentile * hypeConstants.BidPercentileConstant;

                    double linearRegressionHypeFactor = hypeConstants.BaseScore
                                                        + standardGameCalculation
                                                        + counterPickCalculation
                                                        + hypeFactorCalculation
                                                        + averageDraftPositionCalculation
                                                        + totalBidCalculation
                                                        + bidPercentileCalculation;

                    linearRegressionHypeFactor = FixDouble(linearRegressionHypeFactor);

                    calculatedStats.Add(new MasterGameCalculatedStats(masterGame, supportedYear.Year, percentStandardGame, percentCounterPick, eligiblePercentStandardGame, 
                        eligiblePercentCounterPick, numberOfBids, (int) totalBidAmount, bidPercentile, averageDraftPosition, averageWinningBid, hypeFactor, 
                        dateAdjustedHypeFactor, linearRegressionHypeFactor));
                }

                await _masterGameRepo.UpdateCalculatedStats(calculatedStats, supportedYear.Year);
            }
        }

        private async Task UpdateCodeBasedTags(LocalDate today)
        {
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
    }
}
