using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateFantasyPointsJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public UpdateFantasyPointsJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateFantasyPoints;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.UpdateFantasyPoints();
        return Result.Success();
    }
}
