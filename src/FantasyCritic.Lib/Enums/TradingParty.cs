namespace FantasyCritic.Lib.Enums;

public class TradingParty : TypeSafeEnum<TradingSystem>
{
    // Define values here.
    public static readonly TradingParty Proposer = new TradingParty("Proposer");
    public static readonly TradingParty CounterParty = new TradingParty("CounterParty");

    // Constructor is private: values are defined within this class only!
    private TradingParty(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}