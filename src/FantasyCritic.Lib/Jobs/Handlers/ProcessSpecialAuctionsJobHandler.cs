using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessSpecialAuctionsJobHandler : IConditionalCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessSpecialAuctions;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    private readonly AdminService _adminService;
    private readonly ILogger<ProcessSpecialAuctionsJobHandler> _logger;

    public ProcessSpecialAuctionsJobHandler(AdminService adminService, ILogger<ProcessSpecialAuctionsJobHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<bool> ShouldSchedule()
    {
        return await _adminService.AnyUnprocessedSpecialAuctions();
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.ProcessSpecialAuctions();
        return Result.Success();
    }
}
