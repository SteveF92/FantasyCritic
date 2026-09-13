using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Worker;

public class JobRunner
{
    private readonly IJobRepo _jobRepo;

    public JobRunner(IJobRepo jobRepo)
    {
        _jobRepo = jobRepo;
    }

    public Task<FantasyCriticJob> RunJob(FantasyCriticJob job, CancellationToken cancellationToken)
    {
        //var detailedStatusFunc = new Func<string, Task>((detailedStatus) => _jobRepo.UpdateDetailedStatusForJob(job, detailedStatus));
        throw new NotImplementedException();
    }
}
