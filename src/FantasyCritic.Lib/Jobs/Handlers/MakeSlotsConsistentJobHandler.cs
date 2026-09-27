using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class MakeSlotsConsistentJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public MakeSlotsConsistentJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.MakeSlotsConsistent;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.MakePublisherSlotsConsistent();
        return Result.Success();
    }
}
