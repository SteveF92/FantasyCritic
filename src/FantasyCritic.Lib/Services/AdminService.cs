using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Services;

public class AdminService
{
    private static readonly IReadOnlyList<IsoDayOfWeek> AcceptableActionProcessingDays = [IsoDayOfWeek.Saturday, IsoDayOfWeek.Sunday];

    private readonly IRDSManager _rdsManager;
    private readonly DiscordPushService _discordPushService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly EnvironmentConfiguration _environmentConfiguration;

    public AdminService(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, InterLeagueService interLeagueService, IClock clock,
        IRDSManager rdsManager, DiscordPushService discordPushService, EnvironmentConfiguration environmentConfiguration)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
        _rdsManager = rdsManager;
        _discordPushService = discordPushService;
        _environmentConfiguration = environmentConfiguration;
    }

    public Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year)
    {
        return _fantasyCriticRepo.GetLeagueYears(year);
    }

    public Task ClearMasterGameEditDiscordQueue()
    {
        return _masterGameRepo.ClearPendingMasterGameEdits();
    }

    public Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentDatabaseSnapshots()
    {
        return _rdsManager.GetRecentSnapshots();
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
}
