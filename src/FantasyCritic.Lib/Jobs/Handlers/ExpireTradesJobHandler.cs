using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ExpireTradesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ExpireTrades;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    private readonly AdminService _adminService;
    private readonly ILogger<ExpireTradesJobHandler> _logger;

    public ExpireTradesJobHandler(AdminService adminService, ILogger<ExpireTradesJobHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Expiring trades.");
        await _adminService.ExpireTrades();
        return Result.Success();
    }
}
