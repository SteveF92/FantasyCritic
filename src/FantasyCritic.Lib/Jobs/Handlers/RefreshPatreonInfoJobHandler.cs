using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshPatreonInfoJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshPatreonInfo;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    private readonly AdminService _adminService;
    private readonly ILogger<RefreshPatreonInfoJobHandler> _logger;

    public RefreshPatreonInfoJobHandler(AdminService adminService, ILogger<RefreshPatreonInfoJobHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.UpdatePatreonRoles();
        return Result.Success();
    }
}
