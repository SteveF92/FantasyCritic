namespace FantasyCritic.Lib.Enums;

public class ReleaseSystem : TypeSafeEnum<ReleaseSystem>
{

    // Define values here.
    public static readonly ReleaseSystem MustBeReleased = new ReleaseSystem("MustBeReleased", "Must Be Released");
    public static readonly ReleaseSystem OnlyNeedsScore = new ReleaseSystem("OnlyNeedsScore", "Only Needs Score");

    // Constructor is private: values are defined within this class only!
    private ReleaseSystem(string value, string readableName)
        : base(value)
    {
        ReadableName = readableName;
    }

    public string ReadableName { get; }
    public override string ToString() => Value;
}
