namespace FantasyCritic.Lib.Discord.Models;

public class GameNewsSettingOld : TypeSafeEnum<GameNewsSettingOld>
{
    // Define values here.
    public static readonly GameNewsSettingOld All = new GameNewsSettingOld("All");
    public static readonly GameNewsSettingOld WillReleaseInYear = new GameNewsSettingOld("WillReleaseInYear");
    public static readonly GameNewsSettingOld MightReleaseInYear = new GameNewsSettingOld("MightReleaseInYear");
    public static readonly GameNewsSettingOld Off = new GameNewsSettingOld("Off");

    // Constructor is private: values are defined within this class only!
    private GameNewsSettingOld(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}

//This is different from the setting above because some of these settings aren't "real", they can't be persisted to the DB.
public class RequestedGameNewsSetting  : TypeSafeEnum<RequestedGameNewsSetting>
{
    // Define values here.
    public static readonly RequestedGameNewsSetting All = new RequestedGameNewsSetting ("All", GameNewsSettingOld.All);
    public static readonly RequestedGameNewsSetting WillReleaseInYear = new RequestedGameNewsSetting ("WillReleaseInYear", GameNewsSettingOld.WillReleaseInYear);
    public static readonly RequestedGameNewsSetting MightReleaseInYear = new RequestedGameNewsSetting ("MightReleaseInYear", GameNewsSettingOld.MightReleaseInYear);
    public static readonly RequestedGameNewsSetting Recommended = new RequestedGameNewsSetting ("Recommended");
    public static readonly RequestedGameNewsSetting LeagueGamesOnly = new RequestedGameNewsSetting ("LeagueGamesOnly");
    public static readonly RequestedGameNewsSetting Off = new RequestedGameNewsSetting ("Off", GameNewsSettingOld.Off);

    // Constructor is private: values are defined within this class only!
    private RequestedGameNewsSetting (string value, GameNewsSettingOld? normalSetting = null)
        : base(value)
    {
        NormalSetting = normalSetting;
    }

    public GameNewsSettingOld? NormalSetting { get; }

    public override string ToString() => Value;

    public GameNewsSettingOld ToNormalSetting()
    {
        if (NormalSetting == null)
        {
            throw new Exception($"{Value} cannot be converted to a normal setting");
        }

        return NormalSetting;
    }
}

