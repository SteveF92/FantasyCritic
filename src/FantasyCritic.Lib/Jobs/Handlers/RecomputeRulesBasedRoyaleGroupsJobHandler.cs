using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecomputeRulesBasedRoyaleGroupsJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public RecomputeRulesBasedRoyaleGroupsJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecomputeRulesBasedRoyaleGroups;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.RecomputeRulesBasedRoyaleGroups();
        return Result.Success();
    }
}
