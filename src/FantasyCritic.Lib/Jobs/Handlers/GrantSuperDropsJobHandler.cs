using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class GrantSuperDropsJobHandler : IFantasyCriticCronJobHandler
{
    private readonly AdminService _adminService;
    private readonly IClock _clock;

    public GrantSuperDropsJobHandler(AdminService adminService, IClock clock)
    {
        _adminService = adminService;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.GrantSuperDrops;

    //Every ten minutes, deliberately, rather than once a year: after the grant date it is an idempotent catch-up
    //for leagues whose first draft finishes late. An annual cron would deny those leagues super drops.
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //The date guard is for the schedule only. The admin button has always granted immediately.
        if (context.Job.IsCronRun && !_clock.ShouldGrantSuperDrops())
        {
            await context.UpdateDetailedStatus($"Not granted: super drops are granted from {_clock.GetSuperDropsGrantTime()}.");
            return Result.Success();
        }

        await _adminService.GrantSuperDrops();
        return Result.Success();
    }
}
