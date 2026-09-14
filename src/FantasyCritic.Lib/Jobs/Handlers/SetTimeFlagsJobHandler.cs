using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SetTimeFlagsJobHandler : IFantasyCriticCronJobHandler
{
    private readonly AdminService _adminService;

    public SetTimeFlagsJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.SetTimeFlags;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.SetTimeFlags();
        return Result.Success();
    }
}
