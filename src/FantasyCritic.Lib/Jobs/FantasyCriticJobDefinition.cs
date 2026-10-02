namespace FantasyCritic.Lib.Jobs;

//Everything known about a job type without constructing its handler: which class runs it, its priority, and its schedule if it has one.
public record FantasyCriticJobDefinition(FantasyCriticJobType JobType, Type HandlerType, FantasyCriticJobPriority Priority, FantasyCriticJobSchedule? Schedule)
{
    public static FantasyCriticJobDefinition For<THandler>() where THandler : IFantasyCriticJobHandler
    {
        return new FantasyCriticJobDefinition(THandler.JobType, typeof(THandler), THandler.Priority, Schedule: null);
    }

    public static FantasyCriticJobDefinition ForCron<THandler>() where THandler : IFantasyCriticCronJobHandler
    {
        return new FantasyCriticJobDefinition(THandler.JobType, typeof(THandler), THandler.Priority, THandler.Schedule);
    }
}
