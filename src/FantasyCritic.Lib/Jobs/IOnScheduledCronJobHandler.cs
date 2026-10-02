namespace FantasyCritic.Lib.Jobs;

//A cron handler with work that must happen at its slot's time, not whenever the single runner reaches the job.
//The scheduler calls this before enqueueing each due slot, even one another scheduler already enqueued, so it must be idempotent.
//The scheduler constructs the handler to call it, so keep it cheap. Running the job manually skips it.
public interface IOnScheduledCronJobHandler : IFantasyCriticCronJobHandler
{
    Task OnScheduled();
}
