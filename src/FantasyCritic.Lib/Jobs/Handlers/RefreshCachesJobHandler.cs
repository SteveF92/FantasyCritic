using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshCachesJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public RefreshCachesJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshCaches;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.RefreshCaches();
        return Result.Success();
    }
}
