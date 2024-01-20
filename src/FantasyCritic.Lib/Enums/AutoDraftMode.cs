namespace FantasyCritic.Lib.Enums;

public class AutoDraftMode : TypeSafeEnum<AutoDraftMode>
{

    // Define values here.
    public static readonly AutoDraftMode Off = new AutoDraftMode("Off");
    public static readonly AutoDraftMode StandardGamesOnly = new AutoDraftMode("StandardGamesOnly");
    public static readonly AutoDraftMode All = new AutoDraftMode("All");

    // Constructor is private: values are defined within this class only!
    private AutoDraftMode(string value)
        : base(value)
    {
        
    }

    public override string ToString() => Value;
}
