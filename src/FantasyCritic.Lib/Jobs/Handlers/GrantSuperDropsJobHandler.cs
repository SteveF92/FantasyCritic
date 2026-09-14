using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class GrantSuperDropsJobHandler : IFantasyCriticCronJobHandler
{
    private readonly AdminService _adminService;

    public GrantSuperDropsJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.GrantSuperDrops;

    //Every ten minutes, deliberately, rather than once a year: after the grant date it is an idempotent catch-up
    //for leagues whose first draft finishes late. An annual cron would deny those leagues super drops.
    //The guard only affects the schedule; the admin button has always granted immediately.
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes.WithCalendarGuard(x => x.ShouldGrantSuperDrops());

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.GrantSuperDrops();
        return Result.Success();
    }
}
