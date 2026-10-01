using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class ActionProcessingRunner
{
    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly TopBidsAndDropsUpdater _topBidsAndDropsUpdater;
    private readonly IClock _clock;

    public ActionProcessingRunner(InterLeagueService interLeagueService, AdminService adminService, IFantasyCriticRepo fantasyCriticRepo,
        DiscordPushService discordPushService, TopBidsAndDropsUpdater topBidsAndDropsUpdater, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _topBidsAndDropsUpdater = topBidsAndDropsUpdater;
        _clock = clock;
    }

    //Callers check AdminService.CanProcessActions first.
    public async Task ProcessActions(FantasyCriticJobContext context)
    {
        var systemWideValues = await _interLeagueService.GetSystemWideValues();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished || !supportedYear.OpenForPlay)
            {
                continue;
            }

            await context.UpdateDetailedStatus($"Processing actions for {supportedYear.Year}.");
            await ProcessActionsForYear(systemWideValues, supportedYear.Year);
        }
    }

    private async Task ProcessActionsForYear(SystemWideValues systemWideValues, int year)
    {
        var now = _clock.GetCurrentInstant();
        var allSpecialAuctions = await _fantasyCriticRepo.GetAllActiveSpecialAuctions();
        if (allSpecialAuctions.Any(x => x.IsLocked(now)))
        {
            throw new Exception("There are special auctions that need to be processed.");
        }
        IReadOnlyList<LeagueYear> allLeagueYears = await _fantasyCriticRepo.GetLeagueYears(year);
        var results = await _adminService.GetActionProcessingDryRun(systemWideValues, year, now, allLeagueYears);
        await _fantasyCriticRepo.SaveProcessedActionResults(results);
        var leagueActionSets = results.GetLeagueActionSets();
        await _discordPushService.SendActionProcessingSummary(leagueActionSets);

        await _topBidsAndDropsUpdater.UpdateTopBidsAndDropsForMostRecentWeek();
    }
}
