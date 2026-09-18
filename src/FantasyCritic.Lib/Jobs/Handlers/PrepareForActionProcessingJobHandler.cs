using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PrepareForActionProcessingJobHandler : IFantasyCriticCronJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;
    private readonly IClock _clock;

    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(TimeExtensions.ActionProcessingDay, TimeExtensions.ActionProcessingTime);

    public PrepareForActionProcessingJobHandler(InterLeagueService interLeagueService, AdminService adminService, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.PrepareForActionProcessing;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //Mode first, so nothing changes between the refresh, the snapshot, and processing.
        //The scheduler skips FullDataRefresh's own slot in this wake, since this is the refresh.
        //A cancellation leaves action processing mode on, like every other way this job can stop partway.
        await _interLeagueService.SetActionProcessingMode(true);
        await context.UpdateDetailedStatus("Action processing mode on. Refreshing data.");
        cancellationToken.ThrowIfCancellationRequested();

        await _adminService.FullDataRefresh();
        await context.UpdateDetailedStatus("Action processing mode on. Data refreshed. Snapshotting database.");
        cancellationToken.ThrowIfCancellationRequested();

        await DatabaseSnapshotJobUtilities.SnapshotDatabaseAndWait(_adminService, _clock, context,
            "Action processing mode on. Data refreshed. ", cancellationToken);
        return Result.Success();
    }

}
