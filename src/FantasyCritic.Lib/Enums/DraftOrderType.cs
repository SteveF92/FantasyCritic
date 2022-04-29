namespace FantasyCritic.Lib.Enums;

public class DraftOrderType : TypeSafeEnum<DraftOrderType>
{
    // Define values here.
    public static readonly DraftOrderType Manual = new DraftOrderType("Manual", "Manually set draft order to:");
    public static readonly DraftOrderType Random = new DraftOrderType("Random", "Randomly set draft order to:");
    public static readonly DraftOrderType InverseStandings = new DraftOrderType("InverseStandings", "Set draft order to inverse of last year's standings:");

    // Constructor is private: values are defined within this class only!
    private DraftOrderType(string value, string actionDescription)
        : base(value)
    {
        ActionDescription = actionDescription;
    }

    public string ActionDescription { get; }

    public override string ToString() => Value;
}
