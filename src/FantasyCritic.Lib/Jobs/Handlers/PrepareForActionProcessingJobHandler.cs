using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PrepareForActionProcessingJobHandler : IFantasyCriticCronJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;
    private readonly FullDataRefresher _fullDataRefresher;
    private readonly IClock _clock;

    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(TimeExtensions.ActionProcessingDay, TimeExtensions.ActionProcessingTime);

    public PrepareForActionProcessingJobHandler(InterLeagueService interLeagueService, AdminService adminService, FullDataRefresher fullDataRefresher, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
        _fullDataRefresher = fullDataRefresher;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.PrepareForActionProcessing;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //Mode first, so nothing changes between the refresh, the snapshot, and processing.
        //The scheduler skips FullDataRefresh's own slot in this wake, since this is the refresh.
        //A cancellation leaves action processing mode on, like every other way this job can stop partway.
        await _interLeagueService.SetActionProcessingMode(true);
        await context.AppendDetailedStatus("Action processing mode on.");
        cancellationToken.ThrowIfCancellationRequested();

        //Each refresh step appends its own clause.
        await _fullDataRefresher.FullDataRefresh(context, cancellationToken);
        await context.AddTemporaryStatus("Snapshotting database.");
        cancellationToken.ThrowIfCancellationRequested();

        var snapshotName = DatabaseSnapshotNames.PreActionProcessing(_clock.GetCurrentInstant());
        await DatabaseSnapshotJobUtilities.SnapshotDatabaseAndWait(_adminService, _clock, context, snapshotName, cancellationToken);
        return Result.Success();
    }

}
