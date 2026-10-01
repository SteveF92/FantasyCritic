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
            return Result.Failure(string.Join(" ", reasonsNotToProcess));
        }

        await _actionProcessingRunner.ProcessActions(context);

        await context.UpdateDetailedStatus("Processed actions for all active years.");
        return Result.Success();
    }
}
