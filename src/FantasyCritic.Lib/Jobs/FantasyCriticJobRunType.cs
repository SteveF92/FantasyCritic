namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobRunType : TypeSafeEnum<FantasyCriticJobRunType>
{

    // Define values here.
    public static readonly FantasyCriticJobRunType Cron = new FantasyCriticJobRunType("Cron");
    public static readonly FantasyCriticJobRunType ManualOrCron = new FantasyCriticJobRunType("ManualOrCron");
    public static readonly FantasyCriticJobRunType Manual = new FantasyCriticJobRunType("Manual");
    public static readonly FantasyCriticJobRunType Disabled = new FantasyCriticJobRunType("Disabled");

    // Constructor is private: values are defined within this class only!
    private FantasyCriticJobRunType(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}
