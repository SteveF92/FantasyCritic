namespace FantasyCritic.Lib.Jobs;

//Everything known about a job type without constructing its handler: which class runs it, and its schedule if it has one.
public record FantasyCriticJobDefinition(FantasyCriticJobType JobType, Type HandlerType, FantasyCriticJobSchedule? Schedule)
{
    public static FantasyCriticJobDefinition For<THandler>() where THandler : IFantasyCriticJobHandler
    {
        return new FantasyCriticJobDefinition(THandler.JobType, typeof(THandler), Schedule: null);
    }

    public static FantasyCriticJobDefinition ForCron<THandler>() where THandler : IFantasyCriticCronJobHandler
    {
        return new FantasyCriticJobDefinition(THandler.JobType, typeof(THandler), THandler.Schedule);
    }
}
