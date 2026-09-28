using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Patreon;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Utilities;
using Serilog;

namespace FantasyCritic.Lib.Services;

public class AdminService
{
    private static readonly ILogger _logger = Log.ForContext<AdminService>();
    private static readonly IReadOnlyList<IsoDayOfWeek> AcceptableActionProcessingDays = [IsoDayOfWeek.Saturday, IsoDayOfWeek.Sunday];

    private readonly IRDSManager _rdsManager;
    private readonly RoyaleService _royaleService;
    private readonly DiscordPushService _discordPushService;
    private readonly IDailyStatsRepo _dailyStatsRepo;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly FantasyCriticUserManager _userManager;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly PatreonService _patreonService;
    private readonly IClock _clock;
    private readonly EnvironmentConfiguration _environmentConfiguration;

    public AdminService(FantasyCriticService fantasyCriticService, FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo,
        InterLeagueService interLeagueService, PatreonService patreonService, IClock clock, IRDSManager rdsManager,
        RoyaleService royaleService, DiscordPushService discordPushService, IDailyStatsRepo dailyStatsRepo,
        EnvironmentConfiguration environmentConfiguration)
    {
        _fantasyCriticService = fantasyCriticService;
        _userManager = userManager;
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _interLeagueService = interLeagueService;
        _patreonService = patreonService;
        _clock = clock;
        _rdsManager = rdsManager;
        _royaleService = royaleService;
        _discordPushService = discordPushService;
        _dailyStatsRepo = dailyStatsRepo;
        _environmentConfiguration = environmentConfiguration;
    }

    public Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year)
    {
        return _fantasyCriticRepo.GetLeagueYears(year);
    }

    public async Task RecalculateWinners()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var mostRecentFinishedYear = supportedYears.Where(x => x.Finished).OrderByDescending(x => x.Year).First();
        IReadOnlyList<LeagueYear> leagueYears = await _fantasyCriticRepo.GetLeagueYears(mostRecentFinishedYear.Year);
        var calculatedStats = _fantasyCriticService.GetCalculatedStatsForYear(mostRecentFinishedYear.Year, leagueYears, true);
        await _fantasyCriticRepo.UpdateLeagueWinners(calculatedStats.WinningUsers, true);
    }

    public async Task RecomputeRulesBasedRoyaleGroups()
    {
        var rulesBasedGroups = await _royaleService.GetAllRoyaleGroupsByType(RoyaleGroupType.RulesBased);
        foreach (var group in rulesBasedGroups)
        {
            var memberIDs = await ComputeRulesBasedMembers(group);
            await _royaleService.SetRoyaleGroupMembers(group.GroupID, memberIDs);
            _logger.Information("Recomputed rules-based Royale group {GroupName} with {Count} members.", group.GroupName, memberIDs.Count);
        }
    }

    private async Task<IReadOnlyList<Guid>> ComputeRulesBasedMembers(RoyaleGroup group)
    {
        return group.RuleSetType switch
        {
            "PreviousWinners" => await ComputePreviousWinners(),
            _ => new List<Guid>()
        };
    }

    private async Task<IReadOnlyList<Guid>> ComputePreviousWinners()
    {
        var quarters = await _royaleService.GetYearQuarters();
        return quarters
            .Where(q => q.WinningUser is not null)
            .Select(q => q.WinningUser!.UserID)
            .Distinct()
            .ToList();
    }

    public void ClearMasterGameEditDiscordQueue()
    {
        _discordPushService.ClearMasterGameEditQueue();
    }

    //Returns once RDS accepts the request, while the snapshot is still being created. Poll GetDatabaseSnapshot to know when it's usable.
    public Task StartDatabaseSnapshot(string snapshotName, CancellationToken cancellationToken)
    {
        return _rdsManager.SnapshotRDS(snapshotName, cancellationToken);
    }

    public Task<DatabaseSnapshotInfo> GetDatabaseSnapshot(string snapshotName, CancellationToken cancellationToken)
    {
        return _rdsManager.GetSnapshot(snapshotName, cancellationToken);
    }

    public Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentDatabaseSnapshots()
    {
        return _rdsManager.GetRecentSnapshots();
    }

    public async Task UpdatePatreonRoles()
    {
        var patreonUsers = await _userManager.GetAllPatreonUsers();
        var patronInfo = await _patreonService.GetPatronInfo(patreonUsers);
        await _userManager.UpdatePatronInfo(patronInfo);
    }

    public async Task<FinalizedActionProcessingResults> GetActionProcessingDryRun(SystemWideValues systemWideValues, int year, Instant processingTime, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> leaguesAndBids = await _fantasyCriticRepo.GetActivePickupBids(year, allLeagueYears);
        IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> leaguesAndDropRequests = await _fantasyCriticRepo.GetActiveDropRequests(year, allLeagueYears);

        var publishersInLeagues = leaguesAndBids
            .Where(x => x.Value.Any()).SelectMany(x => x.Key.Publishers)
            .Concat(leaguesAndDropRequests.Where(x => x.Value.Any()).SelectMany(x => x.Key.Publishers))
            .Distinct();

        var masterGameYears = await _interLeagueService.GetMasterGameYears(year);
        var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);
        var allTags = await _masterGameRepo.GetMasterGameTags();

        var currentDate = _clock.GetToday();

        var actionProcessor = new ActionProcessor(systemWideValues, processingTime, currentDate, masterGameYearDictionary, allTags);
        FinalizedActionProcessingResults results = actionProcessor.ProcessActions(leaguesAndBids, leaguesAndDropRequests, publishersInLeagues);
        return results;
    }

    public async Task<bool> AnyUnprocessedSpecialAuctions()
    {
        var allSpecialAuctions = await _fantasyCriticRepo.GetAllActiveSpecialAuctions();
        var now = _clock.GetCurrentInstant();
        return allSpecialAuctions.Any(x => x.IsLocked(now));
    }

    public async Task<FinalizedActionProcessingResults> GetSpecialAuctionResults(SystemWideValues systemWideValues, int year, Instant processingTime, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        var allSpecialAuctions = await _fantasyCriticRepo.GetAllActiveSpecialAuctions();
        var specialAuctionsToProcess = allSpecialAuctions.Where(x => !x.Processed && x.IsLocked(processingTime));
        var groupedByLeagueYear = specialAuctionsToProcess.GroupBy(x => x.LeagueYearKey);
        var leagueYearDictionary = allLeagueYears.ToDictionary(x => x.Key);
        IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> leaguesAndBids = await _fantasyCriticRepo.GetActivePickupBids(year, allLeagueYears);
        List<LeagueYearSpecialAuctionSet> specialAuctionSets = [];
        foreach (var leagueYearGroup in groupedByLeagueYear)
        {
            var leagueYear = leagueYearDictionary[leagueYearGroup.Key];
            var bidsForLeagueYear = leaguesAndBids[leagueYear];
            List<SpecialAuctionWithBids> specialAuctionsWithBids = [];
            foreach (var specialAuction in leagueYearGroup)
            {
                var bidsForGame = bidsForLeagueYear.Where(x => x.MasterGame.Equals(specialAuction.MasterGameYear.MasterGame)).ToList();
                specialAuctionsWithBids.Add(new SpecialAuctionWithBids(specialAuction, bidsForGame));
            }

            specialAuctionSets.Add(new LeagueYearSpecialAuctionSet(leagueYear, specialAuctionsWithBids));
        }

        var masterGameYears = await _interLeagueService.GetMasterGameYears(year);
        var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);

        var currentDate = _clock.GetToday();

        var allTags = await _masterGameRepo.GetMasterGameTags();
        var actionProcessor = new ActionProcessor(systemWideValues, processingTime, currentDate, masterGameYearDictionary, allTags);
        FinalizedActionProcessingResults results = actionProcessor.ProcessSpecialAuctions(specialAuctionSets);
        return results;
    }

    //Checked when the button is pressed, for immediate feedback, and again when the job runs, since either can change while it waits in the queue.
    public async Task<Result> CanProcessActions()
    {
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.ActionProcessingMode)
        {
            return Result.Failure("Turn on action processing mode first.");
        }

        var today = _clock.GetToday();
        if (_environmentConfiguration.IsProduction && !AcceptableActionProcessingDays.Contains(today.DayOfWeek))
        {
            return Result.Failure($"You probably didn't mean to process pickups on a {today.DayOfWeek}.");
        }

        return Result.Success();
    }

    public async Task ProcessActions(SystemWideValues systemWideValues, int year)
    {
        var now = _clock.GetCurrentInstant();
        var allSpecialAuctions = await _fantasyCriticRepo.GetAllActiveSpecialAuctions();
        if (allSpecialAuctions.Any(x => x.IsLocked(now)))
        {
            throw new Exception("There are special auctions that need to be processed.");
        }
        IReadOnlyList<LeagueYear> allLeagueYears = await GetLeagueYears(year);
        var results = await GetActionProcessingDryRun(systemWideValues, year, now, allLeagueYears);
        await _fantasyCriticRepo.SaveProcessedActionResults(results);
        var leagueActionSets = results.GetLeagueActionSets();
        await _discordPushService.SendActionProcessingSummary(leagueActionSets);

        await UpdateTopBidsAndDropsForMostRecentWeek();
    }

    public async Task UpdateTopBidsAndDropsForMostRecentWeek()
    {
        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();
        var weeks = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets);
        if (weeks.Count == 0)
        {
            return;
        }

        var mostRecentWeek = weeks.Last();
        await UpdateTopBidsAndDropsForWeek(mostRecentWeek);
    }

    public async Task UpdateTopBidsAndDropsForWeek(LocalDate processDate)
    {
        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();
        var week = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets)
            .SingleOrDefault(x => x.ProcessDate == processDate);
        if (week is null)
        {
            return;
        }

        await UpdateTopBidsAndDropsForWeek(week);
    }

    private async Task UpdateTopBidsAndDropsForWeek(ActionProcessingWeek week)
    {
        var existingProcessDates = await _masterGameRepo.GetProcessingDatesForTopBidsAndDrops();
        if (existingProcessDates.Contains(week.ProcessDate))
        {
            return;
        }

        var bidsAndDrops = await _fantasyCriticRepo.GetPickupBidsAndDropsForProcessingSets(week.ProcessingSets);
        var yearsInGroup = bidsAndDrops.Bids.Select(x => x.LeagueYear.Key.Year).Concat(bidsAndDrops.Drops.Select(x => x.LeagueYear.Key.Year)).Distinct().ToList();

        var allMasterGameYears = new List<MasterGameYear>();
        foreach (var year in yearsInGroup)
        {
            var masterGameYears = await _masterGameRepo.GetMasterGameYears(year);
            allMasterGameYears.AddRange(masterGameYears);
        }

        var topBidsAndDrops = TopBidsAndDropsFunctions.CalculateTopBidsAndDrops(week.ProcessDate, bidsAndDrops, yearsInGroup, allMasterGameYears);
        await _fantasyCriticRepo.InsertTopBidsAndDrops(topBidsAndDrops);
    }

    public async Task ProcessSpecialAuctions()
    {
        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished || !supportedYear.OpenForPlay)
            {
                continue;
            }

            await ProcessSpecialAuctionsForYear(systemWideValues, supportedYear.Year);
        }
    }

    private async Task ProcessSpecialAuctionsForYear(SystemWideValues systemWideValues, int year)
    {
        _logger.Information($"Processing special auctions for {year}.");
        var now = _clock.GetCurrentInstant();
        IReadOnlyList<LeagueYear> allLeagueYears = await GetLeagueYears(year);
        var results = await GetSpecialAuctionResults(systemWideValues, year, now, allLeagueYears);
        if (results.IsEmpty())
        {
            return;
        }

        await _fantasyCriticRepo.SaveProcessedActionResults(results);
        await _discordPushService.SendActionProcessingSummary(results.GetLeagueActionSets());
    }

    public async Task MakePublisherSlotsConsistent()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MaxBy(x => x.Year);
        await _fantasyCriticRepo.ManualMakePublisherGameSlotsConsistent(currentYear!.Year);
    }

    public async Task GrantSuperDrops()
    {
        _logger.Information("Granting super drops.");
        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var now = _clock.GetCurrentInstant();
        var currentDate = now.ToEasternDate();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MinBy(x => x.Year);
        IReadOnlyList<LeagueYear> allLeagueYears = await GetLeagueYears(currentYear!.Year);
        var leagueYearsWithSuperDrops = allLeagueYears.Where(x => x.IsFirstDraftFinished && x.Options.GrantSuperDrops).ToList();

        var allLeagueActions = await _fantasyCriticRepo.GetLeagueActions(currentDate.Year);
        var automatedGrantActions = allLeagueActions.Where(x => x.ActionType == "Granted Super Drop");
        var publishersAlreadyGranted = automatedGrantActions.Select(x => x.Publisher.PublisherID).ToHashSet();

        List<Publisher> publishersToGrantSuperDrop = [];
        List<LeagueAction> superDropActions = [];
        foreach (var leagueYear in leagueYearsWithSuperDrops)
        {
            if (!leagueYear.Publishers.Any())
            {
                continue;
            }

            var superDropPointCutoff = leagueYear.GetSuperDropPointCuttoff(systemWideValues);
            if (!superDropPointCutoff.HasValue)
            {
                continue;
            }

            var publishersWithProjectedPoints = leagueYear.Publishers.ToDictionary(x => x, y => y.GetProjectedFantasyPoints(leagueYear, systemWideValues));
            var publishersWithLowScores = publishersWithProjectedPoints.Where(x => x.Value < superDropPointCutoff).ToList();
            List<LeagueAction> actions = [];
            foreach (var publisher in publishersWithLowScores)
            {
                if (publishersAlreadyGranted.Contains(publisher.Key.PublisherID))
                {
                    continue;
                }

                actions.Add(new LeagueAction(publisher.Key, now, "Granted Super Drop", "Granted one super drop due to league standings.", false));
                publishersToGrantSuperDrop.Add(publisher.Key);
            }
            superDropActions.AddRange(actions);
        }

        await _fantasyCriticRepo.GrantSuperDrops(publishersToGrantSuperDrop, superDropActions);
        await _discordPushService.SendSuperDropMessages(publishersToGrantSuperDrop);
    }

    public async Task ExpireTrades()
    {
        _logger.Information("Expiring trades.");
        var now = _clock.GetCurrentInstant();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var currentYear = supportedYears.Where(x => !x.Finished && x.OpenForPlay).MaxBy(x => x.Year);
        IReadOnlyList<Trade> trades = await _fantasyCriticRepo.GetTradesForYear(currentYear!.Year);

        var activeTrades = trades.Where(x => x.Status.IsActive).ToList();
        List<Trade> tradesToExpire = [];
        foreach (var trade in activeTrades)
        {
            var tradeExpirationTime = trade.GetExpirationTime();
            if (!tradeExpirationTime.HasValue)
            {
                continue;
            }

            if (now > tradeExpirationTime)
            {
                tradesToExpire.Add(trade);
            }
        }

        await _fantasyCriticRepo.ExpireTrades(tradesToExpire, now);
    }

    public Task LinkToOpenCritic(MasterGame masterGame, int openCriticID)
    {
        return _masterGameRepo.LinkToOpenCritic(masterGame, openCriticID);
    }

    public Task LinkToGG(MasterGame masterGame, string ggToken)
    {
        return _masterGameRepo.LinkToGG(masterGame, ggToken);
    }

    public Task MergeMasterGame(MasterGame removeMasterGame, MasterGame mergeIntoMasterGame)
    {
        return _fantasyCriticRepo.MergeMasterGame(removeMasterGame, mergeIntoMasterGame);
    }

    public async Task UpdateDailyStats()
    {
        _logger.Information("Updating daily statistics.");
        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var today = _clock.GetToday();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => !x.Finished && x.OpenForPlay).ToList();
        var supportedQuarters = await _royaleService.GetYearQuarters();

        await _dailyStatsRepo.UpdateDailyStats(activeYears, supportedQuarters, today, systemWideValues);
    }
}
