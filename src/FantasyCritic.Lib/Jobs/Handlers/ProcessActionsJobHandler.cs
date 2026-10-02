using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessActionsJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;
    private readonly ActionProcessingRunner _actionProcessingRunner;
    private readonly IClock _clock;

    public ProcessActionsJobHandler(AdminService adminService, ActionProcessingRunner actionProcessingRunner, IClock clock)
    {
        _adminService = adminService;
        _actionProcessingRunner = actionProcessingRunner;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessActions;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var reasonsNotToProcess = await _adminService.GetReasonsNotToProcessActions(false, _clock.GetCurrentInstant());
        if (reasonsNotToProcess.Any())
        {
            var stopReasons = string.Join(" ", reasonsNotToProcess);
            await context.AppendDetailedStatus($"Stopped: {stopReasons}");
            return Result.Failure(stopReasons);
        }

        await _actionProcessingRunner.ProcessActions(context, cancellationToken);
        return Result.Success();
    }
}
