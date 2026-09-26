namespace FantasyCritic.Lib.Jobs;

//A cron handler whose due slots are only enqueued when there is work to do, so its job history holds only runs that did something.
//The scheduler constructs the handler to ask, so keep the check cheap. Running the job manually skips the check.
public interface IConditionalCronJobHandler : IFantasyCriticCronJobHandler
{
    Task<bool> ShouldSchedule();
}
