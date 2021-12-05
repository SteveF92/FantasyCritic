namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class ValidPickupBid
    {
        public ValidPickupBid(PickupBid pickupBid, int slotNumber)
        {
            PickupBid = pickupBid;
            SlotNumber = slotNumber;
        }

        public PickupBid PickupBid { get; }
        public int SlotNumber { get; }
    }
}
