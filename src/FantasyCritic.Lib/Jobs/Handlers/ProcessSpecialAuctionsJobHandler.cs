using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessSpecialAuctionsJobHandler : IFantasyCriticCronJobHandler
{
    private readonly AdminService _adminService;

    public ProcessSpecialAuctionsJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessSpecialAuctions;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.ProcessSpecialAuctions();
        return Result.Success();
    }
}
