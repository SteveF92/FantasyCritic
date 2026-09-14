namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SnapshotDatabaseJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SnapshotDatabase;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
