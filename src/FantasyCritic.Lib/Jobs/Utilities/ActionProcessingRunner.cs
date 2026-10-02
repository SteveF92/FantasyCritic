using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class ActionProcessingRunner
{
    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly TopBidsAndDropsUpdater _topBidsAndDropsUpdater;
    private readonly IClock _clock;
    private readonly ILogger<ActionProcessingRunner> _logger;

    public ActionProcessingRunner(InterLeagueService interLeagueService, AdminService adminService, IFantasyCriticRepo fantasyCriticRepo,
        DiscordPushService discordPushService, TopBidsAndDropsUpdater topBidsAndDropsUpdater, IClock clock, ILogger<ActionProcessingRunner> logger)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _topBidsAndDropsUpdater = topBidsAndDropsUpdater;
        _clock = clock;
        _logger = logger;
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

            await ProcessActionsForYear(systemWideValues, supportedYear.Year, context, yearCancellationToken);
            yearCancellationToken = CancellationToken.None;
        }

        //After every year, since a December run processes two and their sets share one top bids and drops week.
        await _topBidsAndDropsUpdater.UpdateTopBidsAndDropsForMostRecentWeek(context, CancellationToken.None);
    }

    private async Task ProcessActionsForYear(SystemWideValues systemWideValues, int year, FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await context.AddTemporaryStatus($"{year}: processing actions.");
        var now = _clock.GetCurrentInstant();
        IReadOnlyList<LeagueYear> allLeagueYears = await _fantasyCriticRepo.GetLeagueYears(year);
        var results = await _adminService.GetActionProcessingDryRun(systemWideValues, year, now, allLeagueYears);
        cancellationToken.ThrowIfCancellationRequested();
        await _fantasyCriticRepo.SaveProcessedActionResults(results);
        var leagueActionSets = results.GetLeagueActionSets();
        await _discordPushService.SendActionProcessingSummary(leagueActionSets);

        var successBidCount = results.Results.SuccessBids.Count;
        var bidCount = successBidCount + results.Results.FailedBids.Count;
        var successDropCount = results.Results.SuccessDrops.Count;
        var dropCount = successDropCount + results.Results.FailedDrops.Count;
        var leagueCount = leagueActionSets.Count;
        _logger.LogInformation("Processed actions for {Year}: {SuccessBidCount} of {BidCount} bids and {SuccessDropCount} of {DropCount} drops succeeded in {LeagueCount} leagues.",
            year, successBidCount, bidCount, successDropCount, dropCount, leagueCount);
        if (bidCount == 0 && dropCount == 0)
        {
            await context.AppendDetailedStatus($"{year}: no bids or drops.");
            return;
        }

        var leagueWord = leagueCount == 1 ? "league" : "leagues";
        await context.AppendDetailedStatus($"{year}: {successBidCount} of {bidCount} bids and {successDropCount} of {dropCount} drops succeeded in {leagueCount} {leagueWord}.");
    }
}
