namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobStatus : TypeSafeEnum<FantasyCriticJobStatus>
{

    // Define values here.
    public static readonly FantasyCriticJobStatus Queued = new FantasyCriticJobStatus("Queued");
    public static readonly FantasyCriticJobStatus Running = new FantasyCriticJobStatus("Running");
    public static readonly FantasyCriticJobStatus Complete = new FantasyCriticJobStatus("Complete");
    public static readonly FantasyCriticJobStatus Error = new FantasyCriticJobStatus("Error");
    public static readonly FantasyCriticJobStatus Cancelling = new FantasyCriticJobStatus("Cancelling");
    public static readonly FantasyCriticJobStatus Cancelled = new FantasyCriticJobStatus("Cancelled");
    public static readonly FantasyCriticJobStatus CancelledInProgress = new FantasyCriticJobStatus("CancelledInProgress");


    // Constructor is private: values are defined within this class only!
    private FantasyCriticJobStatus(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}
