using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateDailyPublisherStatisticsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateDailyPublisherStatistics;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.AtTenPmEastern;

    private readonly AdminService _adminService;
    private readonly ILogger<UpdateDailyPublisherStatisticsJobHandler> _logger;

    public UpdateDailyPublisherStatisticsJobHandler(AdminService adminService, ILogger<UpdateDailyPublisherStatisticsJobHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.UpdateDailyStats();
        return Result.Success();
    }
}
