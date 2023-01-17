namespace FantasyCritic.Lib.Discord.Models;

public class GameNewsSetting : TypeSafeEnum<GameNewsSetting>
{
    // Define values here.
    public static readonly GameNewsSetting All = new GameNewsSetting("All");
    public static readonly GameNewsSetting WillReleaseInYear = new GameNewsSetting("WillReleaseInYear");
    public static readonly GameNewsSetting MightReleaseInYear = new GameNewsSetting("MightReleaseInYear");

    // Constructor is private: values are defined within this class only!
    private GameNewsSetting(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}
