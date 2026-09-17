using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SetTimeFlagsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SetTimeFlags;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    private readonly AdminService _adminService;
    private readonly ILogger<SetTimeFlagsJobHandler> _logger;

    public SetTimeFlagsJobHandler(AdminService adminService, ILogger<SetTimeFlagsJobHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.SetTimeFlags();
        return Result.Success();
    }
}
