using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessActionsJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;
    private readonly ActionProcessingRunner _actionProcessingRunner;

    public ProcessActionsJobHandler(AdminService adminService, ActionProcessingRunner actionProcessingRunner)
    {
        _adminService = adminService;
        _actionProcessingRunner = actionProcessingRunner;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessActions;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var canProcessActions = await _adminService.CanProcessActions();
        if (canProcessActions.IsFailure)
        {
            return canProcessActions;
        }

        await _actionProcessingRunner.ProcessActions(context);

        await context.UpdateDetailedStatus("Processed actions for all active years.");
        return Result.Success();
    }
}
