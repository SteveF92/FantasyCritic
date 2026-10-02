using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Services;

public class AdminService
{
    private static readonly IReadOnlyList<IsoDayOfWeek> AcceptableActionProcessingDays = [IsoDayOfWeek.Saturday, IsoDayOfWeek.Sunday];

    private readonly IRDSManager _rdsManager;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly EnvironmentConfiguration _environmentConfiguration;

    public AdminService(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, InterLeagueService interLeagueService, IClock clock,
        IRDSManager rdsManager, EnvironmentConfiguration environmentConfiguration)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
        _rdsManager = rdsManager;
        _environmentConfiguration = environmentConfiguration;
    }

    public Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year)
    {
        return _fantasyCriticRepo.GetLeagueYears(year);
    }

    public Task<PendingMasterGameUpdates> GetPendingMasterGameUpdates()
    {
        return _masterGameRepo.GetPendingMasterGameUpdates();
    }

    public Task<bool> DeletePendingMasterGameUpdate(Guid masterGameUpdateID)
    {
        return _masterGameRepo.DeletePendingMasterGameUpdate(masterGameUpdateID);
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

    //Empty means go. A manual run is checked when the button is pressed, for immediate feedback, and again when the job runs, since either can change while it waits in the queue.
    //The day check is about processingTime, so a pre-check of the next automated run doesn't report the day it was pressed.
    public async Task<IReadOnlyList<string>> GetReasonsNotToProcessActions(bool isAutomatedRun, Instant processingTime)
    {
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        List<string> reasons = [];
        if (isAutomatedRun)
        {
            if (!_environmentConfiguration.IsProduction)
            {
                reasons.Add($"This is {_environmentConfiguration.EnvironmentName}, not production. Outside production, actions are only processed by hand.");
            }

            if (!systemWideSettings.EnableAutomatedActionProcessing)
            {
                reasons.Add("Automated action processing is turned off.");
            }
        }
        //The automated job turns action processing mode on itself.
        else if (!systemWideSettings.ActionProcessingMode)
        {
            reasons.Add("Turn on action processing mode first.");
        }

        var processingDay = processingTime.ToEasternDate().DayOfWeek;
        if (_environmentConfiguration.IsProduction && !AcceptableActionProcessingDays.Contains(processingDay))
        {
            reasons.Add($"You probably didn't mean to process actions on a {processingDay}.");
        }

        //Now, not processingTime: an auction that locks before the next bid time is processed by its own job long before then,
        //so only one that is locked now and still waiting is a problem.
        var now = _clock.GetCurrentInstant();
        var allSpecialAuctions = await _fantasyCriticRepo.GetAllActiveSpecialAuctions();
        if (allSpecialAuctions.Any(x => x.IsLocked(now)))
        {
            reasons.Add("There are special auctions that need to be processed.");
        }

        var gamesWithPendingCorrections = await _masterGameRepo.GetGamesWithPendingBidsOrDropsThatHavePendingCorrections();
        if (gamesWithPendingCorrections.Any())
        {
            var gameNames = string.Join(", ", gamesWithPendingCorrections.Select(x => x.GameName));
            reasons.Add($"Before running actions, pending corrections for the following games must be actioned: {gameNames}");
        }

        return reasons;
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
