namespace FantasyCritic.Lib.Royale;

public class RoyaleGroupType : TypeSafeEnum<RoyaleGroupType>
{
    public static readonly RoyaleGroupType Manual = new RoyaleGroupType("Manual", "Manual");
    public static readonly RoyaleGroupType RulesBased = new RoyaleGroupType("RulesBased", "Rules Based");
    public static readonly RoyaleGroupType LeagueTied = new RoyaleGroupType("LeagueTied", "League Tied");
    public static readonly RoyaleGroupType ConferenceTied = new RoyaleGroupType("ConferenceTied", "Conference Tied");

    private RoyaleGroupType(string value, string readableName)
        : base(value)
    {
        ReadableName = readableName;
    }

    public string ReadableName { get; }

    public override string ToString() => Value;
}
