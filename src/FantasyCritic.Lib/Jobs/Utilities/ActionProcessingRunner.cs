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

    //Callers check AdminService.GetReasonsNotToProcessActions first.
    public async Task ProcessActions(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var systemWideValues = await _interLeagueService.GetSystemWideValues();
        var supportedYears = await _interLeagueService.GetSupportedYears();

        //Cancellable until the first year's results are saved, which can't be taken back. After that, the run has to finish.
        var yearCancellationToken = cancellationToken;
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished || !supportedYear.OpenForPlay)
            {
                continue;
            }

            await context.UpdateDetailedStatus($"Processing actions for {supportedYear.Year}.");
            await ProcessActionsForYear(systemWideValues, supportedYear.Year, yearCancellationToken);
            yearCancellationToken = CancellationToken.None;
        }

        //After every year, since a December run processes two and their sets share one top bids and drops week.
        await _topBidsAndDropsUpdater.UpdateTopBidsAndDropsForMostRecentWeek(CancellationToken.None);
    }

    private async Task ProcessActionsForYear(SystemWideValues systemWideValues, int year, CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        IReadOnlyList<LeagueYear> allLeagueYears = await _fantasyCriticRepo.GetLeagueYears(year);
        var results = await _adminService.GetActionProcessingDryRun(systemWideValues, year, now, allLeagueYears);
        cancellationToken.ThrowIfCancellationRequested();
        await _fantasyCriticRepo.SaveProcessedActionResults(results);
        var leagueActionSets = results.GetLeagueActionSets();
        await _discordPushService.SendActionProcessingSummary(leagueActionSets);
    }
}
