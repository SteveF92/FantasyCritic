using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshCriticScoresJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public RefreshCriticScoresJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshCriticScores;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.RefreshCriticInfo();
        return Result.Success();
    }
}
