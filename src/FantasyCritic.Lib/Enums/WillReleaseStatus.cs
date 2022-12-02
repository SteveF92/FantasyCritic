namespace FantasyCritic.Lib.Enums;
public class WillReleaseStatus : TypeSafeEnum<WillReleaseStatus>
{

    // Define values here.
    public static readonly WillReleaseStatus WillNotRelease = new WillReleaseStatus("WillNotRelease", "Will Not Release", false);
    public static readonly WillReleaseStatus MightRelease = new WillReleaseStatus("MightRelease", "Might Release", true);
    public static readonly WillReleaseStatus WillRelease = new WillReleaseStatus("WillRelease", "Will Release", true);

    // Constructor is private: values are defined within this class only!
    private WillReleaseStatus(string value, string readableName, bool countAsWillRelease)
        : base(value)
    {
        ReadableName = readableName;
        CountAsWillRelease = countAsWillRelease;
    }

    public string ReadableName { get; }
    public bool CountAsWillRelease { get; }
    public override string ToString() => Value;
}
