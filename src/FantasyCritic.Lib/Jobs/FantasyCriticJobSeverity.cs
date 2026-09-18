namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobSeverity : TypeSafeEnum<FantasyCriticJobSeverity>
{
    // Define values here.
    public static readonly FantasyCriticJobSeverity Info = new FantasyCriticJobSeverity("Info");
    public static readonly FantasyCriticJobSeverity Warning = new FantasyCriticJobSeverity("Warning");
    public static readonly FantasyCriticJobSeverity Danger = new FantasyCriticJobSeverity("Danger");

    // Constructor is private: values are defined within this class only!
    private FantasyCriticJobSeverity(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}
