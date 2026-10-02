using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SnapshotDatabaseJobHandler : IFantasyCriticJobHandler
{
    private readonly IRDSManager _rdsManager;
    private readonly IClock _clock;

    public SnapshotDatabaseJobHandler(IRDSManager rdsManager, IClock clock)
    {
        _rdsManager = rdsManager;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.SnapshotDatabase;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.Independent;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var snapshotName = DatabaseSnapshotNames.Admin(_clock.GetCurrentInstant());
        await DatabaseSnapshotJobUtilities.SnapshotDatabaseAndWait(_rdsManager, _clock, context, snapshotName, cancellationToken);
        return Result.Success();
    }
}
