using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SnapshotDatabaseJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;

    public SnapshotDatabaseJobHandler(AdminService adminService)
    {
        _adminService = adminService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.SnapshotDatabase;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _adminService.SnapshotDatabase();
        return Result.Success();
    }
}
