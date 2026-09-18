using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SnapshotDatabaseJobHandler : IFantasyCriticJobHandler
{
    private readonly AdminService _adminService;
    private readonly IClock _clock;

    public SnapshotDatabaseJobHandler(AdminService adminService, IClock clock)
    {
        _adminService = adminService;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.SnapshotDatabase;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var snapshotName = DatabaseSnapshotNames.Admin(_clock.GetCurrentInstant());
        await DatabaseSnapshotJobUtilities.SnapshotDatabaseAndWait(_adminService, _clock, context, snapshotName, "", cancellationToken);
        return Result.Success();
    }
}
