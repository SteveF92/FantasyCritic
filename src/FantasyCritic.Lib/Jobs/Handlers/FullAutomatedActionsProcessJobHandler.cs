using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class FullAutomatedActionsProcessJobHandler : IFantasyCriticCronJobHandler
{
    private readonly AdminService _adminService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IRDSManager _rdsManager;
    private readonly FullDataRefresher _fullDataRefresher;
    private readonly ActionProcessingRunner _actionProcessingRunner;
    private readonly IClock _clock;

    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(TimeExtensions.ActionProcessingDay, TimeExtensions.ActionProcessingTime);

    public FullAutomatedActionsProcessJobHandler(AdminService adminService, InterLeagueService interLeagueService, IRDSManager rdsManager,
        FullDataRefresher fullDataRefresher, ActionProcessingRunner actionProcessingRunner, IClock clock)
    {
        _adminService = adminService;
        _interLeagueService = interLeagueService;
        _rdsManager = rdsManager;
        _fullDataRefresher = fullDataRefresher;
        _actionProcessingRunner = actionProcessingRunner;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.FullAutomatedActionsProcess;

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
        await DatabaseSnapshotJobUtilities.SnapshotDatabaseAndWait(_rdsManager, _clock, context, snapshotName, cancellationToken);

        var reasonsNotToProcess = await _adminService.GetReasonsNotToProcessActions(true, _clock.GetCurrentInstant());
        if (reasonsNotToProcess.Any())
        {
            //TODO Send admin email
            return Result.Failure(string.Join(" ", reasonsNotToProcess));
        }

        await _actionProcessingRunner.ProcessActions(context);

        await context.UpdateDetailedStatus("Processed actions for all active years.");
        return Result.Success();
    }
}
