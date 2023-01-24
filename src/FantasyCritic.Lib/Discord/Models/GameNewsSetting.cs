namespace FantasyCritic.Lib.Discord.Models;

public class GameNewsSetting : TypeSafeEnum<GameNewsSetting>
{
    // Define values here.
    public static readonly GameNewsSetting All = new GameNewsSetting("All");
    public static readonly GameNewsSetting WillReleaseInYear = new GameNewsSetting("WillReleaseInYear");
    public static readonly GameNewsSetting MightReleaseInYear = new GameNewsSetting("MightReleaseInYear");
    public static readonly GameNewsSetting Off = new GameNewsSetting("Off");

    // Constructor is private: values are defined within this class only!
    private GameNewsSetting(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}

//This is different from the setting above because some of these settings aren't "real", they can't be persisted to the DB.
public class RequestedGameNewsSetting  : TypeSafeEnum<RequestedGameNewsSetting>
{
    // Define values here.
    public static readonly RequestedGameNewsSetting  All = new RequestedGameNewsSetting ("All", GameNewsSetting.All);
    public static readonly RequestedGameNewsSetting  WillReleaseInYear = new RequestedGameNewsSetting ("WillReleaseInYear", GameNewsSetting.WillReleaseInYear);
    public static readonly RequestedGameNewsSetting  MightReleaseInYear = new RequestedGameNewsSetting ("MightReleaseInYear", GameNewsSetting.MightReleaseInYear);
    public static readonly RequestedGameNewsSetting  Recommended = new RequestedGameNewsSetting ("Recommended");
    public static readonly RequestedGameNewsSetting  LeagueGamesOnly = new RequestedGameNewsSetting ("LeagueGamesOnly");
    public static readonly RequestedGameNewsSetting  Off = new RequestedGameNewsSetting ("Off", GameNewsSetting.Off);

    // Constructor is private: values are defined within this class only!
    private RequestedGameNewsSetting (string value, GameNewsSetting? normalSetting = null)
        : base(value)
    {
        NormalSetting = normalSetting;
    }

    public GameNewsSetting? NormalSetting { get; }

    public override string ToString() => Value;

    public GameNewsSetting ToNormalSetting()
    {
        if (NormalSetting == null)
        {
            throw new Exception($"{Value} cannot be converted to a normal setting");
        }

        return NormalSetting;
    }
}

