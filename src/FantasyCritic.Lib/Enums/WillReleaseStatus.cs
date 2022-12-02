namespace FantasyCritic.Lib.Enums;
public class WillReleaseStatus : TypeSafeEnum<WillReleaseStatus>
{

    // Define values here.
    public static readonly WillReleaseStatus WillNotRelease = new WillReleaseStatus("WillNotRelease", "Will Not Release", "will NOT release", false);
    public static readonly WillReleaseStatus MightRelease = new WillReleaseStatus("MightRelease", "Might Release", "MIGHT release", true);
    public static readonly WillReleaseStatus WillRelease = new WillReleaseStatus("WillRelease", "Will Release", "WILL release", true);

    // Constructor is private: values are defined within this class only!
    private WillReleaseStatus(string value, string readableName, string discordText, bool countAsWillRelease)
        : base(value)
    {
        ReadableName = readableName;
        DiscordText = discordText;
        CountAsWillRelease = countAsWillRelease;
    }

    public string ReadableName { get; }
    public string DiscordText { get; }
    public bool CountAsWillRelease { get; }
    public override string ToString() => Value;
}
