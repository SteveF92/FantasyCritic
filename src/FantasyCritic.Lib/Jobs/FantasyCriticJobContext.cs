using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Jobs;

//What a handler gets besides its own dependencies. The runner owns the job's status transitions; a handler only reports progress.
public class FantasyCriticJobContext
{
    private readonly IJobRepo _jobRepo;
    private readonly List<string> _completedStatusParts = [];

    public FantasyCriticJobContext(FantasyCriticJob job, IJobRepo jobRepo)
    {
        Job = job;
        _jobRepo = jobRepo;
    }

    public FantasyCriticJob Job { get; }

    //For a handler that reports once, or replaces its whole status each time.
    public Task UpdateDetailedStatus(string detailedStatus) => _jobRepo.UpdateDetailedStatusForJob(Job, detailedStatus);

    //For a handler that does several things in sequence: each finished step adds its clause, so if a later step throws or is cancelled,
    //the row still says which steps finished.
    public Task AppendDetailedStatus(string part)
    {
        _completedStatusParts.Add(part);
        return UpdateDetailedStatus(string.Join(" ", _completedStatusParts));
    }

    //Progress within the current step, shown after the finished steps. The next call to either method replaces it.
    public Task AddTemporaryStatus(string progress)
    {
        return UpdateDetailedStatus(string.Join(" ", _completedStatusParts.Append(progress)));
    }

    public static FantasyCriticJobContext FakeContext => new FantasyCriticJobContext(FantasyCriticJob.FakeJob, new NoOpJobRepo());
}
