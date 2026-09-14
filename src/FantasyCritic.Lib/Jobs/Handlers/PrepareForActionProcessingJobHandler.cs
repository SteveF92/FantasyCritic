using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PrepareForActionProcessingJobHandler : IFantasyCriticJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;

    public PrepareForActionProcessingJobHandler(InterLeagueService interLeagueService, AdminService adminService)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.PrepareForActionProcessing;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //Mode first, so nothing changes between the snapshot and processing.
        await _interLeagueService.SetActionProcessingMode(true);
        await context.UpdateDetailedStatus("Action processing mode on. Snapshotting database.");

        await _adminService.SnapshotDatabase();
        await context.UpdateDetailedStatus("Action processing mode on. Database snapshot started.");
        return Result.Success();
    }
}
