namespace FantasyCritic.Lib.Enums;

public class TradeStatus : TypeSafeEnum<TradeStatus>
{
    // Define values here.
    public static readonly TradeStatus Proposed = new TradeStatus("Proposed", true);
    public static readonly TradeStatus Accepted = new TradeStatus("Accepted", true);
    public static readonly TradeStatus Rescinded = new TradeStatus("Rescinded", false);
    public static readonly TradeStatus RejectedByCounterParty = new TradeStatus("RejectedByCounterParty", false);
    public static readonly TradeStatus RejectedByManager = new TradeStatus("RejectedByManager", false);
    public static readonly TradeStatus Executed = new TradeStatus("Executed", false);

    // Constructor is private: values are defined within this class only!
    private TradeStatus(string value, bool isActive)
        : base(value)
    {
        IsActive = isActive;
    }

    public bool IsActive { get; }

    public override string ToString() => Value;
}