using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Web.Models.Requests.JobManager;

/// <summary>
/// The query string of GetJobs. Each list is sent as a repeated key, for example
/// <c>?page=1&amp;count=10&amp;excludeJobTypes=ProcessSpecialAuctions&amp;statuses=Error&amp;statuses=CancelledInProgress</c>.
/// </summary>
/// <param name="Page">One-based. Jobs are always newest first.</param>
/// <param name="Count">Jobs per page.</param>
/// <param name="JobTypes">Only these job types. Null or empty means every type.</param>
/// <param name="ExcludeJobTypes">Every job type but these. Applied on top of <paramref name="JobTypes"/> when both are sent.</param>
/// <param name="Statuses">Only these statuses. Null or empty means every status.</param>
/// <param name="ExcludeStatuses">Every status but these. Applied on top of <paramref name="Statuses"/> when both are sent.</param>
public record GetJobsRequest(int Page, int Count, List<string>? JobTypes, List<string>? ExcludeJobTypes, List<string>? Statuses, List<string>? ExcludeStatuses)
{
    /// <returns>Failure, naming the value, if any job type or status is not one the system knows.</returns>
    public Result<FantasyCriticJobFilter> ToDomain()
    {
        var jobTypes = Parse(JobTypes, FantasyCriticJobType.TryFromValue, "job type");
        var excludedJobTypes = Parse(ExcludeJobTypes, FantasyCriticJobType.TryFromValue, "job type");
        var statuses = Parse(Statuses, FantasyCriticJobStatus.TryFromValue, "status");
        var excludedStatuses = Parse(ExcludeStatuses, FantasyCriticJobStatus.TryFromValue, "status");

        var combined = Result.Combine(jobTypes, excludedJobTypes, statuses, excludedStatuses);
        if (combined.IsFailure)
        {
            return Result.Failure<FantasyCriticJobFilter>(combined.Error);
        }

        return new FantasyCriticJobFilter(jobTypes.Value, excludedJobTypes.Value, statuses.Value, excludedStatuses.Value);
    }

    private static Result<IReadOnlyList<T>> Parse<T>(List<string>? values, Func<string, T?> tryParse, string description) where T : class
    {
        var parsed = new List<T>();
        foreach (var value in values ?? [])
        {
            var parsedValue = tryParse(value);
            if (parsedValue is null)
            {
                return Result.Failure<IReadOnlyList<T>>($"Unknown {description} '{value}'.");
            }

            parsed.Add(parsedValue);
        }

        return parsed;
    }
}
