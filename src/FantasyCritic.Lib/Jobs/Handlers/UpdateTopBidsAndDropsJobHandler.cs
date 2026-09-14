using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateTopBidsAndDropsJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public UpdateTopBidsAndDropsJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateTopBidsAndDrops;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.UpdateTopBidsAndDropsForMostRecentWeek();
        return Result.Success();
    }
}
