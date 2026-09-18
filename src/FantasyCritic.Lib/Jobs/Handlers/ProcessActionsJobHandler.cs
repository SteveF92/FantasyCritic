using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessActionsJobHandler : IFantasyCriticJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;

    public ProcessActionsJobHandler(InterLeagueService interLeagueService, AdminService adminService)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessActions;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var canProcessActions = await _adminService.CanProcessActions();
        if (canProcessActions.IsFailure)
        {
            return canProcessActions;
        }

        var systemWideValues = await _interLeagueService.GetSystemWideValues();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        foreach (var supportedYear in supportedYears)
        {
            if (supportedYear.Finished || !supportedYear.OpenForPlay)
            {
                continue;
            }

            await context.UpdateDetailedStatus($"Processing actions for {supportedYear.Year}.");
            await _adminService.ProcessActions(systemWideValues, supportedYear.Year);
        }

        await context.UpdateDetailedStatus("Processed actions for all active years.");
        return Result.Success();
    }
}
