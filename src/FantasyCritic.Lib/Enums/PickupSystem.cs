namespace FantasyCritic.Lib.Enums;

public class PickupSystem : TypeSafeEnum<PickupSystem>
{
    // Define values here.
    public static readonly PickupSystem SecretBidding = new PickupSystem("SecretBidding", "Secret Bidding");
    public static readonly PickupSystem SemiPublicBidding = new PickupSystem("SemiPublicBidding", "Public Bidding");
    public static readonly PickupSystem SemiPublicBiddingSecretCounterPicks = new PickupSystem("SemiPublicBiddingSecretCounterPicks", "Public Bidding, Secret Counter Picks");
    public static readonly PickupSystem SemiPublicBiddingSemiPublicCounterPicks = new PickupSystem("SemiPublicBiddingSemiPublicCounterPicks", "Public Bidding, Semi-Public Counter Picks");

    // Constructor is private: values are defined within this class only!
    private PickupSystem(string value, string readableName)
        : base(value)
    {
        ReadableName = readableName;
    }

    public string ReadableName { get; }

    public bool HasPublicBiddingWindow => Value == SemiPublicBidding.Value || Value == SemiPublicBiddingSecretCounterPicks.Value ||
                                          Value == SemiPublicBiddingSemiPublicCounterPicks.Value;

    /// <summary>
    /// True when the games being bid on as counter picks are left out of the revealed public bidding list, rather than being listed
    /// by name alongside the standard bids.
    /// </summary>
    public bool CounterPickGamesAreHiddenDuringPublicBidding => Value == SemiPublicBiddingSecretCounterPicks.Value ||
                                                                Value == SemiPublicBiddingSemiPublicCounterPicks.Value;

    /// <summary>
    /// True when the public bidding reveal answers one yes/no question about counter picks - is anything being counter picked at all -
    /// without revealing which games or how many. Because the answer is a yes/no and not a count, a "yes" cannot be changed by anyone
    /// placing another counter pick bid, which is what lets counter pick bidding stay open for the rest of the week.
    /// </summary>
    public bool RevealsWhetherAnyCounterPicksExist => Value == SemiPublicBiddingSemiPublicCounterPicks.Value;

    /// <summary>
    /// True when the public bidding reveal says nothing whatsoever about counter picks, so counter pick bids can still be placed and
    /// cancelled after the reveal without contradicting anything the league has been shown.
    /// </summary>
    public bool CounterPickBidsAreUnaffectedByPublicBidding => Value == SemiPublicBiddingSecretCounterPicks.Value;

    public override string ToString() => Value;
}
