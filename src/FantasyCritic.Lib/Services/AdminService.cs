using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord;
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
