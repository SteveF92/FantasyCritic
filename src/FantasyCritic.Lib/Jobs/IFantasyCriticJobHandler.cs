namespace FantasyCritic.Lib.Jobs;

//One class per job type, holding its domain logic. Handlers are resolved from DI per run, so they take their dependencies in the constructor.
//JobType is static so the registry can know which handler serves which type without constructing it, or any of its dependencies.
public interface IFantasyCriticJobHandler : IJobHandler
{
    static abstract FantasyCriticJobType JobType { get; }
}
