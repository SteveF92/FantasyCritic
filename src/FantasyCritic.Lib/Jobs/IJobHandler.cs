namespace FantasyCritic.Lib.Jobs;

//What the runner resolves and calls. Kept free of static abstract members, because an interface with those can't be a DI service type.
public interface IJobHandler
{
    Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken);
}
