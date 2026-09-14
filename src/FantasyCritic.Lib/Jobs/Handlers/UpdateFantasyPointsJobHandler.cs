namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateFantasyPointsJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateFantasyPoints;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
