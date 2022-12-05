namespace FantasyCritic.Lib.Enums;

public class TiebreakSystem : TypeSafeEnum<TiebreakSystem>
{

    // Define values here.
    public static readonly TiebreakSystem LowestProjectedPoints = new TiebreakSystem("LowestProjectedPoints", "Lowest Projected Points");
    public static readonly TiebreakSystem EarliestBid = new TiebreakSystem("EarliestBid", "Earliest Bid");

    // Constructor is private: values are defined within this class only!
    private TiebreakSystem(string value, string readableName)
        : base(value)
    {
        ReadableName = readableName;
    }

    public string ReadableName { get; }

    public override string ToString() => Value;
}
