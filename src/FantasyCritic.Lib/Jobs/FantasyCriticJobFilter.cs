namespace FantasyCritic.Lib.Jobs;

/// <summary>
/// Which jobs a listing should contain. An empty list places no restriction, and a dimension's two lists combine:
/// job types in <paramref name="JobTypes"/> (or every type, when that is empty) minus those in <paramref name="ExcludedJobTypes"/>.
/// </summary>
public record FantasyCriticJobFilter(
    IReadOnlyList<FantasyCriticJobType> JobTypes,
    IReadOnlyList<FantasyCriticJobType> ExcludedJobTypes,
    IReadOnlyList<FantasyCriticJobStatus> Statuses,
    IReadOnlyList<FantasyCriticJobStatus> ExcludedStatuses)
{
    public static FantasyCriticJobFilter Everything { get; } = new FantasyCriticJobFilter([], [], [], []);
}
