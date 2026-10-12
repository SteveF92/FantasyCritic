namespace FantasyCritic.Lib.Domain.LeagueActions;

public class ValidPickupBid
{
    public ValidPickupBid(PickupBid pickupBid, LeagueSlot slot)
    {
        PickupBid = pickupBid;
        Slot = slot;
    }

    public PickupBid PickupBid { get; }
    public LeagueSlot Slot { get; }

    public override string ToString() => PickupBid.ToString();
}
