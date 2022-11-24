namespace FantasyCritic.Lib.Enums;

public class DiscordGameNewsSetting : TypeSafeEnum<DiscordGameNewsSetting>
{
    // Define values here.
    public static readonly DiscordGameNewsSetting Off = new DiscordGameNewsSetting("Off");
    public static readonly DiscordGameNewsSetting Relevant = new DiscordGameNewsSetting("Relevant");
    public static readonly DiscordGameNewsSetting On = new DiscordGameNewsSetting("On");

    // Constructor is private: values are defined within this class only!
    private DiscordGameNewsSetting(string value)
        : base(value)
    {
    }

    public override string ToString() => Value;
}
