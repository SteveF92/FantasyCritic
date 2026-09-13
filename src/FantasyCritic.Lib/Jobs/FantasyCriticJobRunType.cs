namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobRunType : TypeSafeEnum<FantasyCriticJobRunType>
{

    // Define values here.
    public static readonly FantasyCriticJobRunType Cron = new FantasyCriticJobRunType("Cron", false, true);
    public static readonly FantasyCriticJobRunType ManualOrCron = new FantasyCriticJobRunType("ManualOrCron", true, true);
    public static readonly FantasyCriticJobRunType Manual = new FantasyCriticJobRunType("Manual", true, false);
    public static readonly FantasyCriticJobRunType Disabled = new FantasyCriticJobRunType("Disabled", false, false);

    // Constructor is private: values are defined within this class only!
    private FantasyCriticJobRunType(string value, bool allowsManual, bool allowsCron)
        : base(value)
    {
        AllowsManual = allowsManual;
        AllowsCron = allowsCron;
    }

    public bool AllowsManual { get; }
    public bool AllowsCron { get; }

    public override string ToString() => Value;
}
