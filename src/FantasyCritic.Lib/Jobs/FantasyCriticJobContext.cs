using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Jobs;

//What a handler gets besides its own dependencies. The runner owns the job's status transitions; a handler only reports progress.
public class FantasyCriticJobContext
{
    private readonly IJobRepo _jobRepo;

    public FantasyCriticJobContext(FantasyCriticJob job, IJobRepo jobRepo)
    {
        Job = job;
        _jobRepo = jobRepo;
    }

    public FantasyCriticJob Job { get; }

    //For a handler that does several things in sequence: if a later step throws, the row still says which steps finished.
    public Task UpdateDetailedStatus(string detailedStatus) => _jobRepo.UpdateDetailedStatusForJob(Job, detailedStatus);
}
