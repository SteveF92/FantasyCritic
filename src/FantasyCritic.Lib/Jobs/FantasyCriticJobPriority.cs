namespace FantasyCritic.Lib.Jobs;

//Orders jobs queued at the same moment, which in practice means in the same scheduler wake. Lower runs first.
//Never persisted, so the values can change freely.
public enum FantasyCriticJobPriority
{
    TimeCritical = 0,
    DependedUpon = 1,
    DependantAndDependedUpon = 2,
    StrictlyDependant = 3,
    Independent = 4,
}
