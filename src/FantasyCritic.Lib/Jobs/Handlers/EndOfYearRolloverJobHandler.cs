using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class EndOfYearRolloverJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.EndOfYearRollover;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Cron("0 0 1 1 *");

    private readonly AdminService _adminService;

    public EndOfYearRolloverJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.RunEndOfYearRollover();
        return Result.Success();
    }
}
