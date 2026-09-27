using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateRoyaleWinnersJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public RecalculateRoyaleWinnersJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateRoyaleWinners;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.RecalculateRoyaleWinners();
        return Result.Success();
    }
}
