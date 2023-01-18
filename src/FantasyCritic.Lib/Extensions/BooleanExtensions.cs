namespace FantasyCritic.Lib.Extensions;

public static class BooleanExtensions
{
    public static string ToYesNoString(this bool value)
    {
        return value ? "Yes" : "No";
    }

    public static string ToOnOffString(this bool value)
    {
        return value ? "On" : "Off";
    }
}
