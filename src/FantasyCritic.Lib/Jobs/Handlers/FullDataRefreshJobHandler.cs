using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class FullDataRefreshJobHandler : IFantasyCriticCronJobHandler
{
    private readonly AdminService _adminService;

    public FullDataRefreshJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.FullDataRefresh;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Cron("0 */2 * * *");

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.FullDataRefresh();
        return Result.Success();
    }
}
