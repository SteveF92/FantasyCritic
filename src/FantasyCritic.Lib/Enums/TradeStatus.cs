namespace FantasyCritic.Lib.Enums;

public class TradeStatus : TypeSafeEnum<TradeStatus>
{
    // Define values here.
    public static readonly TradeStatus Proposed = new TradeStatus("Proposed", true, "Proposed");
    public static readonly TradeStatus Accepted = new TradeStatus("Accepted", true, "Accepted");
    public static readonly TradeStatus Rescinded = new TradeStatus("Rescinded", false, "Rescinded");
    public static readonly TradeStatus RejectedByCounterParty = new TradeStatus("RejectedByCounterParty", false, "Rejected by Counter Party");
    public static readonly TradeStatus RejectedByManager = new TradeStatus("RejectedByManager", false, "Rejected by League Manager");
    public static readonly TradeStatus Executed = new TradeStatus("Executed", false, "Executed");
    public static readonly TradeStatus Expired = new TradeStatus("Expired", false, "Expired");

    // Constructor is private: values are defined within this class only!
    private TradeStatus(string value, bool isActive, string readableName)
        : base(value)
    {
        IsActive = isActive;
        ReadableName = readableName;
    }

    public bool IsActive { get; }
    public string ReadableName { get; }

    public override string ToString() => Value;
}
