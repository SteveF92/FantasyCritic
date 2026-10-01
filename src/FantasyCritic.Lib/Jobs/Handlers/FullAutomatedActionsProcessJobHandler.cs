using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class FullAutomatedActionsProcessJobHandler : IFantasyCriticCronJobHandler
{
    private const string ActionProcessingModeStillOn = "Action processing mode is still on, so the site stays locked until actions are processed by hand or the mode is turned off.";

    private readonly AdminService _adminService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IRDSManager _rdsManager;
    private readonly FullDataRefresher _fullDataRefresher;
    private readonly ActionProcessingRunner _actionProcessingRunner;
    private readonly EmailSendingService _emailSendingService;
    private readonly IClock _clock;

    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(TimeExtensions.ActionProcessingDay, TimeExtensions.ActionProcessingTime);

    public FullAutomatedActionsProcessJobHandler(AdminService adminService, InterLeagueService interLeagueService, IRDSManager rdsManager,
        FullDataRefresher fullDataRefresher, ActionProcessingRunner actionProcessingRunner, EmailSendingService emailSendingService, IClock clock)
    {
        _adminService = adminService;
        _interLeagueService = interLeagueService;
        _rdsManager = rdsManager;
        _fullDataRefresher = fullDataRefresher;
        _actionProcessingRunner = actionProcessingRunner;
        _emailSendingService = emailSendingService;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.FullAutomatedActionsProcess;

    //Nobody watches this run, so any way it stops short of processing actions sends Steve an email, a cancellation included.
    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await RunSteps(context, cancellationToken);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            await SendStoppedShortEmail("Automated action processing cancelled", ex,
            [
                $"Job {context.Job.JobID} was cancelled before finishing.",
                ActionProcessingModeStillOn,
            ]);
            throw;
        }
        catch (Exception ex)
        {
            await SendStoppedShortEmail("Automated action processing failed", ex,
            [
                $"Job {context.Job.JobID} threw before finishing: {ex.GetType().Name}: {ex.Message}",
                "Some years' actions may already be processed. Check the job's status and the league histories before processing by hand.",
                ActionProcessingModeStillOn,
            ]);
            throw;
        }
    }

    private async Task<Result> RunSteps(FantasyCriticJobContext context, CancellationToken cancellationToken)
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
            await _emailSendingService.SendAdminNotification("Automated action processing stopped",
                [.. reasonsNotToProcess, ActionProcessingModeStillOn]);
            return Result.Failure(string.Join(" ", reasonsNotToProcess));
        }

        await _actionProcessingRunner.ProcessActions(context, cancellationToken);
        await _interLeagueService.SetActionProcessingMode(false);

        await context.UpdateDetailedStatus("Processed actions for all active years. Action processing mode off.");
        return Result.Success();
    }

    //If the email fails too, both errors go on the job, so the original isn't lost behind the email's.
    private async Task SendStoppedShortEmail(string subject, Exception ex, IReadOnlyList<string> lines)
    {
        try
        {
            await _emailSendingService.SendAdminNotification(subject, lines);
        }
        catch (Exception emailException)
        {
            throw new AggregateException("The automated action processing job stopped short, and the email about it failed.", ex, emailException);
        }
    }
}
