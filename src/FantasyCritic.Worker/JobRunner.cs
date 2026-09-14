using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Worker;

public class JobRunner
{
    //Scoped, so this is the job's own scope: the handler and everything it depends on live exactly as long as one run.
    private readonly IServiceProvider _serviceProvider;
    private readonly IJobRepo _jobRepo;

    public JobRunner(IServiceProvider serviceProvider, IJobRepo jobRepo)
    {
        _serviceProvider = serviceProvider;
        _jobRepo = jobRepo;
    }

    public Task RunJob(FantasyCriticJob job, CancellationToken cancellationToken)
    {
        var handler = _serviceProvider.GetRequiredKeyedService<IJobHandler>(job.Type);
        var context = new FantasyCriticJobContext(job, _jobRepo);
        return handler.Run(context, cancellationToken);
    }
}
