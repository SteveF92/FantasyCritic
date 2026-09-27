using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class FullDataRefreshJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.FullDataRefresh;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTwoHours;

    private readonly AdminService _adminService;
    private readonly ILogger<FullDataRefreshJobHandler> _logger;

    public FullDataRefreshJobHandler(AdminService adminService, ILogger<FullDataRefreshJobHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.FullDataRefresh();
        return Result.Success();
    }
}
