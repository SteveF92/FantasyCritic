using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class AdvanceRoyaleQuartersJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.AdvanceRoyaleQuarters;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.AtOnePastMidnightEastern;

    private readonly AdminService _adminService;

    public AdvanceRoyaleQuartersJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.AdvanceRoyaleQuarters();
        return Result.Success();
    }
}
