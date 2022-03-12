namespace FantasyCritic.Lib.Enums;

public class PickupSystem : TypeSafeEnum<PickupSystem>
{

    // Define values here.
    public static readonly PickupSystem SecretBidding = new PickupSystem("SecretBidding", "Secret Bidding");
    public static readonly PickupSystem SemiPublicBidding = new PickupSystem("SemiPublicBidding", "Public Bidding");
    public static readonly PickupSystem SemiPublicBiddingSecretCounterPicks = new PickupSystem("SemiPublicBiddingSecretCounterPicks", "Public Bidding, Secret Counter Picks");

    // Constructor is private: values are defined within this class only!
    private PickupSystem(string value, string readableName)
        : base(value)
    {
        ReadableName = readableName;
    }

    public string ReadableName { get; }

    public bool HasPublicBiddingWindow => Value == SemiPublicBidding.Value || Value == SemiPublicBiddingSecretCounterPicks.Value;

    public override string ToString() => Value;
}