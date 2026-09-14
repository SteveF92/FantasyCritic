namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecomputeRulesBasedRoyaleGroupsJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecomputeRulesBasedRoyaleGroups;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
