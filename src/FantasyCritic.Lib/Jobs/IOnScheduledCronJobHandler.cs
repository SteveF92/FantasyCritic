namespace FantasyCritic.Lib.Jobs;

//A cron handler with work that must happen at its slot's time, not whenever the single runner reaches the job.
//The scheduler calls this once per slot, right after it enqueues the slot's job. A slot that was already enqueued, by an earlier
//attempt or another scheduler, doesn't get it again: the job may have run since, and this must not undo what it did.
//The scheduler constructs the handler to call it, so keep it cheap. Running the job manually skips it.
public interface IOnScheduledCronJobHandler : IFantasyCriticCronJobHandler
{
    Task OnScheduled();
}
