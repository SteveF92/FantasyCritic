namespace FantasyCritic.Lib.Jobs;

//A handler the scheduler may enqueue. Whether it actually does is tbl_job_type.RunType's decision, not the code's.
public interface IFantasyCriticCronJobHandler : IFantasyCriticJobHandler
{
    static abstract FantasyCriticJobSchedule Schedule { get; }
}
