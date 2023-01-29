namespace FantasyCritic.Lib.Identity;

public record GeneralUserSettings(bool ShowDecimalPoints)
{
    public static GeneralUserSettings Default => new GeneralUserSettings(false);
}
