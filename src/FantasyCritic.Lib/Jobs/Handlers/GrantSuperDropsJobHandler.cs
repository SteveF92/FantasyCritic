using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class GrantSuperDropsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.GrantSuperDrops;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly.WithCalendarGuard(instant => instant.ShouldGrantSuperDrops());

    private readonly AdminService _adminService;
    private readonly ILogger<GrantSuperDropsJobHandler> _logger;

    public GrantSuperDropsJobHandler(AdminService adminService, ILogger<GrantSuperDropsJobHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.GrantSuperDrops();
        return Result.Success();
    }
}
