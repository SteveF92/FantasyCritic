namespace FantasyCritic.Lib.Jobs;

//What the runner resolves and calls. Kept free of static abstract members, because an interface with those can't be a DI service type.
public interface IJobHandler
{
    //A failure is an expected refusal, like processing actions with action processing mode off, and is recorded as Error with its message.
    //Anything unexpected should throw as usual; the runner records that as Error too, with the stack trace.
    Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken);
}
